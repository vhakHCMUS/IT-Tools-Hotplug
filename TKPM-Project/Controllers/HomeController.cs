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
    private readonly ToolService _toolService;

    public HomeController(ILogger<HomeController> logger, ToolService toolService)
    {
        _logger = logger;
        _toolService = toolService;
    }

    public IActionResult Index()
    {
        var tools = _toolService.GetTools();
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
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteTool(string toolName)
    {
        try
        {
            // Find the tool to delete
            var tool = _toolService.GetToolByName(toolName);
            if (tool == null)
            {
                _logger.LogWarning($"Tool {toolName} not found.");
                return RedirectToAction("Index");
            }

            // Get the .dll file name from ToolService
            var dllFileName = _toolService.GetDllFileName(toolName);
            if (string.IsNullOrEmpty(dllFileName))
            {
                _logger.LogWarning($"No .dll file name found for tool {toolName}.");
                return RedirectToAction("Index");
            }

            // Construct the full file path
            _logger.LogInformation($"Current directory: {Directory.GetCurrentDirectory()}");
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", dllFileName);
            _logger.LogInformation($"Attempting to delete .dll file at: {filePath}");

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning($"File {dllFileName} does not exist at {filePath} for tool {toolName}.");
                // Still unload the tool even if the file isn't found
                _toolService.UnloadTool(toolName);
                return RedirectToAction("Index");
            }

            // Unload the tool first
            _toolService.UnloadTool(toolName);
            _logger.LogInformation($"Unloaded tool {toolName} from memory.");

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _logger.LogInformation("Garbage collection completed.");

            // Delete the file (should work now since the file is not locked)
            System.IO.File.Delete(filePath);
            _logger.LogInformation($"Successfully deleted .dll file for tool {toolName} at {filePath}");

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting tool {toolName}");
            return RedirectToAction("Index");
        }
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}