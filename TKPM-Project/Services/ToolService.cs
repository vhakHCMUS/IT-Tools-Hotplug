using System.Runtime.Loader;
using TKPM_Project.Models.Tools;

namespace TKPM_Project.Services
{
    public class ToolService
    {
        private readonly Dictionary<string, (ITool Tool, string DllFileName)> _tools = new();
        private readonly Dictionary<string, CustomAssemblyLoadContext> _contexts = new();
        private readonly string _pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        private readonly IServiceProvider _serviceProvider;

        public ToolService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                        Console.WriteLine($"Loaded tool {tool.Name} from {dllPath}");

                        // Save to database using a scope
                        SaveToolToDatabase(tool);
                    }
                }
            }
        }

        private async void SaveToolToDatabase(ITool tool)
        {
            // Create a scope to resolve scoped services
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
                    Console.WriteLine($"Saved tool {tool.Name} to the database.");
                }
                else
                {
                    existingTool.Description = tool.Description;
                    existingTool.IsPremium = tool.IsPremium;
                    existingTool.Category = tool.Category;
                    existingTool.CustomViewTemplate = tool.CustomViewTemplate ?? "defaultTemplate";
                    await toolRepository.UpdateAsync(existingTool);
                    Console.WriteLine($"Updated tool {tool.Name} in the database.");
                }
            }
        }

        public void UnloadTool(string toolName)
        {
            if (_tools.TryGetValue(toolName, out var toolInfo))
            {
                toolInfo.Tool.Dispose();
                _tools.Remove(toolName);

                if (_contexts.TryGetValue(toolName, out var context))
                {
                    context.Unload();
                    _contexts.Remove(toolName);
                }
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