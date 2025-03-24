using System.Runtime.Loader;
using TKPM_Project.Models.Tools;
using System.Reflection;
using System.Runtime.Loader;

namespace TKPM_Project.Services
{
    public class ToolService
    {
        private readonly Dictionary<string, ITool> _tools = new();
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
            var context = new CustomAssemblyLoadContext();
            var assembly = context.LoadFromAssemblyPath(dllPath);

            var toolType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (toolType != null)
            {
                var tool = Activator.CreateInstance(toolType) as ITool;
                if (tool != null)
                {
                    _tools[tool.Name] = tool;
                    _contexts[tool.Name] = context;
                }
            }
        }

        // Xóa công cụ
        public void UnloadTool(string toolName)
        {
            if (_tools.TryGetValue(toolName, out var tool))
            {
                tool.Dispose(); // Gọi Dispose để giải phóng tài nguyên
                _tools.Remove(toolName);

                if (_contexts.TryGetValue(toolName, out var context))
                {
                    context.Unload(); // Xóa assembly khỏi bộ nhớ
                    _contexts.Remove(toolName);
                }
            }
        }

        public List<ITool> GetTools() => _tools.Values.ToList();
        public ITool GetToolByName(string name) => _tools.GetValueOrDefault(name);
    }

    // Custom AssemblyLoadContext để hỗ trợ unload
    public class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public CustomAssemblyLoadContext() : base(isCollectible: true) { }
    }
}
