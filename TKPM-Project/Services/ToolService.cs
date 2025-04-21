using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TKPM_Project.Models.Tools;

namespace TKPM_Project.Services
{
    public class ToolService
    {
        private readonly Dictionary<string, (ITool Tool, string DllFileName)> _tools = new();
        private readonly Dictionary<string, CustomAssemblyLoadContext> _contexts = new();
        private readonly string _pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ToolService> _logger;

        public ToolService(IServiceProvider serviceProvider, ILogger<ToolService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            Directory.CreateDirectory(_pluginPath);
            LoadExistingTools();
        }

        private void LoadExistingTools()
        {
            foreach (var dll in Directory.GetFiles(_pluginPath, "*.dll"))
            {
                LoadToolFromDll(dll);
            }
        }

        public void LoadToolFromDll(string dllPath)
        {
            byte[] dllBytes = System.IO.File.ReadAllBytes(dllPath);
            using (var stream = new MemoryStream(dllBytes))
            {
                var context = new CustomAssemblyLoadContext();
                var assembly = context.LoadFromStream(stream);

                var toolType = assembly.GetTypes()
                    .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                if (toolType != null)
                {
                    var tool = Activator.CreateInstance(toolType) as ITool;
                    if (tool != null)
                    {
                        var dllFileName = Path.GetFileName(dllPath);
                        _tools[tool.Name] = (tool, dllFileName);
                        _contexts[tool.Name] = context;
                        _logger.LogInformation($"Loaded tool {tool.Name} from {dllPath}");

                        // Save to database using a scope
                        SaveToolToDatabase(tool);
                    }
                }
            }
        }

        private async void SaveToolToDatabase(ITool tool)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var toolRepository = scope.ServiceProvider.GetRequiredService<IToolRepository>();

                var existingTool = (await toolRepository.GetByKeywordAsync(tool.Name))
                    .FirstOrDefault(t => t.Name == tool.Name);

                if (existingTool == null)
                {
                    var newTool = new Tool
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        IsPremium = tool.IsPremium,
                        Category = tool.Category,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow,
                        CustomViewTemplate = tool.CustomViewTemplate ?? "defaultTemplate"
                    };
                    await toolRepository.AddAsync(newTool);
                    _logger.LogInformation($"Saved tool {tool.Name} to the database.");
                }
                else
                {
                    existingTool.Description = tool.Description;
                    existingTool.IsPremium = tool.IsPremium;
                    existingTool.Category = tool.Category;
                    existingTool.CustomViewTemplate = tool.CustomViewTemplate ?? "defaultTemplate";
                    await toolRepository.UpdateAsync(existingTool);
                    _logger.LogInformation($"Updated tool {tool.Name} in the database.");
                }
            }
        }

        public async Task<object> ExecuteToolAsync(string toolName, object inputs)
        {
            _logger.LogInformation($"Executing tool {toolName} with inputs: {inputs?.ToString() ?? "none"}");

            var tool = GetToolByName(toolName);
            if (tool == null)
            {
                _logger.LogWarning($"Tool not found: {toolName}");
                throw new ArgumentException($"Tool not found: {toolName}");
            }

            try
            {
                // Process inputs into appropriate format for this specific tool
                Dictionary<string, object> processedInputs = ProcessToolInputs(inputs);

                // Execute the tool with the processed inputs
                var result = await tool.ExecuteAsync(processedInputs);
                _logger.LogInformation($"ExecuteAsync result: {result?.ToString() ?? "null"}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing tool {toolName}: {ex.Message}");
                throw;
            }
        }

        private Dictionary<string, object> ProcessToolInputs(object inputs)
        {
            var result = new Dictionary<string, object>();

            if (inputs == null)
                return result;

            // Handle IFormCollection (most common web input)
            if (inputs is IFormCollection formCollection)
            {
                foreach (var key in formCollection.Keys)
                {
                    // Handle multiple values for the same key
                    var values = formCollection[key];
                    if (values.Count == 1)
                    {
                        result[key] = values[0]; // Store single value
                    }
                    else if (values.Count > 1)
                    {
                        result[key] = values.ToArray(); // Store as array
                    }
                }
                return result;
            }

            // Handle dictionary-like inputs
            if (inputs is IDictionary<string, object> dictObj)
            {
                foreach (var kvp in dictObj)
                {
                    result[kvp.Key] = kvp.Value;
                }
                return result;
            }

            // Handle plain string
            if (inputs is string str)
            {
                // Try to parse as comma-separated values with potential name=value format
                var parts = str.Split(',').Select(p => p.Trim()).ToArray();
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    if (part.Contains('='))
                    {
                        var kvp = part.Split('=', 2);
                        result[kvp[0].Trim()] = kvp[1].Trim();
                    }
                    else
                    {
                        // Use positional parameters for unnamed values
                        result[$"param{i}"] = part;
                    }
                }
                return result;
            }

            // Other object types - extract public properties
            var properties = inputs.GetType().GetProperties();
            if (properties.Length > 0)
            {
                foreach (var prop in properties)
                {
                    if (prop.CanRead)
                    {
                        result[prop.Name] = prop.GetValue(inputs);
                    }
                }
                return result;
            }

            // If nothing else matches, store the entire input as a single value
            result["value"] = inputs;
            return result;
        }

        public async void UnloadTool(string toolName)
        {
            if (_tools.TryGetValue(toolName, out var toolInfo))
            {
                try
                {
                    // Dispose of the tool
                    toolInfo.Tool.Dispose();
                    _tools.Remove(toolName);

                    // Unload the assembly context
                    if (_contexts.TryGetValue(toolName, out var context))
                    {
                        context.Unload();
                        _contexts.Remove(toolName);
                    }

                    // Delete the DLL file with retry mechanism
                    string dllPath = Path.Combine(_pluginPath, toolInfo.DllFileName);
                    if (File.Exists(dllPath))
                    {
                        int maxRetries = 5;
                        int retryDelay = 1000; // 1 second
                        
                        for (int i = 0; i < maxRetries; i++)
                        {
                            try
                            {
                                File.Delete(dllPath);
                                _logger.LogInformation($"Successfully deleted DLL file {dllPath}.");
                                break;
                            }
                            catch (IOException ex)
                            {
                                if (i == maxRetries - 1)
                                {
                                    _logger.LogError($"Failed to delete DLL file {dllPath} after {maxRetries} attempts: {ex.Message}");
                                    throw;
                                }
                                await Task.Delay(retryDelay);
                            }
                        }
                    }

                    // Remove from the database
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var toolRepository = scope.ServiceProvider.GetRequiredService<IToolRepository>();
                        var toolToDelete = (await toolRepository.GetByKeywordAsync(toolName))
                            .FirstOrDefault(t => t.Name == toolName);

                        if (toolToDelete != null)
                        {
                            await toolRepository.DeleteAsync(toolToDelete.Id);
                            _logger.LogInformation($"Deleted tool {toolName} from the database.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error unloading tool {toolName}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning($"Tool {toolName} not found for unloading.");
            }
        }

        public List<ITool> GetTools() => _tools.Values.Select(t => t.Tool).ToList();
        public ITool GetToolByName(string name) => _tools.TryGetValue(name, out var toolInfo) ? toolInfo.Tool : null;
        public string GetDllFileName(string toolName) => _tools.TryGetValue(toolName, out var toolInfo) ? toolInfo.DllFileName : null;
    }

    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext() : base(isCollectible: true) { }
    }
}