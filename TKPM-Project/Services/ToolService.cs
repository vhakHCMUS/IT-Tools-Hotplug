using System.Runtime.Loader;
using TKPM_Project.Models.Tools;
using System.Reflection;

namespace TKPM_Project.Services
{
    public class ToolService
    {
        private readonly Dictionary<string, (ITool Tool, string DllFileName)> _tools = new();
        private readonly Dictionary<string, CustomAssemblyLoadContext> _contexts = new();
        private readonly string _pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

        public ToolService()
        {
            Directory.CreateDirectory(_pluginPath); // Tạo thư mục Plugins nếu chưa có
            LoadExistingTools();
        }

        // Tải các công cụ hiện có trong thư mục Plugins
        private void LoadExistingTools()
        {
            foreach (var dll in Directory.GetFiles(_pluginPath, "*.dll"))
            {
                LoadToolFromDll(dll);
            }
        }

        // Tải công cụ từ file .dll
        public void LoadToolFromDll(string dllPath)
        {
            // Read the .dll file into memory to avoid locking the file on disk
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
                    }
                }
            }
        }

        // Xóa công cụ
        public void UnloadTool(string toolName)
        {
            if (_tools.TryGetValue(toolName, out var toolInfo))
            {
                toolInfo.Tool.Dispose(); // Gọi Dispose để giải phóng tài nguyên
                _tools.Remove(toolName);

                if (_contexts.TryGetValue(toolName, out var context))
                {
                    context.Unload(); // Xóa assembly khỏi bộ nhớ
                    _contexts.Remove(toolName);
                }
            }
        }

        public List<ITool> GetTools() => _tools.Values.Select(t => t.Tool).ToList();
        public ITool GetToolByName(string name) => _tools.TryGetValue(name, out var toolInfo) ? toolInfo.Tool : null;
        public string GetDllFileName(string toolName) => _tools.TryGetValue(toolName, out var toolInfo) ? toolInfo.DllFileName : null;
    }

    // Custom AssemblyLoadContext để hỗ trợ unload
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext() : base(isCollectible: true) { }
    }
}