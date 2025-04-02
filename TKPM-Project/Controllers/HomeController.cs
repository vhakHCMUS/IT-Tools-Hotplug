using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models;
using TKPM_Project.Services;

namespace TKPM_Project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ToolService _toolService; // For DLL import and delete
    private readonly IToolRepository _toolRepository; // For loading tool list from DB

    public HomeController(ILogger<HomeController> logger, ToolService toolService, IToolRepository toolRepository)
    {
        _logger = logger;
        _toolService = toolService;
        _toolRepository = toolRepository;
    }

    public async Task<IActionResult> Index()
    {
        var tools = await _toolRepository.GetAllAsync(); // Load tools from database
        ViewData["Tools"] = tools;
        return View(tools);
    }

    [HttpPost]
    public async Task<IActionResult> ImportTool(IFormFile dllFile)
    {
        if (dllFile != null && dllFile.FileName.EndsWith(".dll"))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", dllFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dllFile.CopyToAsync(stream);
            }
            _toolService.LoadToolFromDll(filePath);
            _logger.LogInformation($"Imported tool from {dllFile.FileName} at {filePath}");
        }
        else
        {
            _logger.LogWarning("Invalid file uploaded. Only .dll files are allowed.");
        }
        var tools = await _toolRepository.GetAllAsync(); // Load updated list from database
        ViewData["Tools"] = tools;
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTool(string toolName) // Keep toolName for DLL logic
    {
        try
        {
            var tool = _toolService.GetToolByName(toolName);
            if (tool == null)
            {
                _logger.LogWarning($"Tool {toolName} not found.");
                return RedirectToAction("Index");
            }

            _toolService.UnloadTool(toolName);
            _logger.LogInformation($"Unloaded tool {toolName} from memory.");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            _logger.LogInformation("Garbage collection completed.");

            var dllFileName = _toolService.GetDllFileName(toolName);
            if (!string.IsNullOrEmpty(dllFileName))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", dllFileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"Successfully deleted .dll file for tool {toolName} at {filePath}");
                }
            }

            var tools = await _toolRepository.GetAllAsync(); // Load updated list from database
            ViewData["Tools"] = tools;
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting tool {toolName}");
            return RedirectToAction("Index");
        }
    }

    public async Task<IActionResult> Privacy()
    {
        var tools = await _toolRepository.GetAllAsync(); // Load tools from database
        ViewData["Tools"] = tools;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        var tools = await _toolRepository.GetAllAsync(); // Load tools from database
        ViewData["Tools"] = tools;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}