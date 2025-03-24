using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models;
using TKPM_Project.Services; // Thêm dòng này để import namespace của ToolService

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
            _logger.LogInformation($"Imported tool from {dllFile.FileName}");
        }
        else
        {
            _logger.LogWarning("Invalid file uploaded. Only .dll files are allowed.");
        }
        return RedirectToAction("Index");
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