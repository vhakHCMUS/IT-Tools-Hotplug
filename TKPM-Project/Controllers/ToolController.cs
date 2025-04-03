using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models.Tools;
using TKPM_Project.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TKPM_Project.Controllers;

public class ToolController : Controller
{
    private readonly ToolService _toolService;
    private readonly ApplicationDbContext _dbContext; // Added for DB access
    private readonly ILogger<ToolController> _logger;

    public ToolController(ToolService toolService, ApplicationDbContext dbContext, ILogger<ToolController> logger)
    {
        _toolService = toolService;
        _dbContext = dbContext;
        _logger = logger;
    }

    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> Detail(string toolName, string inputs = null)
    {
        _logger.LogInformation($"{Request.Method} request to Detail with toolName: {toolName}, inputs: {inputs ?? "none"}");

        var tool = _toolService.GetToolByName(toolName);
        if (tool == null)
        {
            _logger.LogWarning($"Tool not found: {toolName}");
            return NotFound();
        }

        if (tool is Tool dbTool && !dbTool.IsAvailable)
        {
            _logger.LogWarning($"Tool {toolName} is not available.");
            return StatusCode(403, "This tool is currently unavailable.");
        }

        if (tool.IsPremium && !User.IsInRole("PremiumUser"))
        {
            _logger.LogWarning($"Non-premium user attempted to access premium tool: {toolName}");
            return StatusCode(403, "This tool requires a premium subscription.");
        }

        if (Request.Method == "POST" && !string.IsNullOrEmpty(inputs))
        {
            await ExecuteToolWithLogging(tool, inputs);
        }

        SetupCustomViewTemplate(tool, toolName);
        ViewBag.Inputs = inputs;
        return View("Detail", tool);
    }

    private async Task ExecuteToolWithLogging(ITool tool, string inputs)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state invalid: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            ViewBag.Error = "Invalid input model state.";
            return;
        }

        try
        {
            object[] inputArray = inputs.Split(',')
                .Select(i => i.Trim() as object)
                .ToArray();

            var result = await tool.ExecuteAsync(inputArray);

            _logger.LogInformation($"ExecuteAsync result: {result?.ToString() ?? "null"}");
            ViewBag.Result = result?.ToString() ?? "No result returned.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing tool {tool.Name}: {ex.Message}");
            ViewBag.Error = $"Error executing tool: {ex.Message}";
        }
    }

    private void SetupCustomViewTemplate(ITool tool, string toolName)
    {
        if (!string.IsNullOrEmpty(tool.CustomViewTemplate))
        {
            _logger.LogInformation($"Custom view template found for {toolName}");
            ViewBag.CustomViewTemplate = tool.CustomViewTemplate;
        }
        else
        {
            _logger.LogInformation($"No custom view template for {toolName}, using default view");
        }
    }

    public IActionResult Index(string searchTerm = null, string categoryFilter = null)
    {
        var tools = _dbContext.Tools.ToList(); // Direct DB access since ToolService is unchanged
        if (!string.IsNullOrEmpty(searchTerm))
        {
            tools = tools.Where(t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        if (!string.IsNullOrEmpty(categoryFilter))
        {
            tools = tools.Where(t => t.Category == categoryFilter).ToList();
        }
        ViewBag.SearchTerm = searchTerm;
        ViewBag.CategoryFilter = categoryFilter;
        return View(tools);
    }

    [HttpPost]
    public IActionResult ImportTool(IFormFile dllFile)
    {
        if (dllFile != null && dllFile.Length > 0)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", dllFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                dllFile.CopyTo(stream);
            }
            // Placeholder: Original ToolService doesn't have ImportTool
            _logger.LogInformation($"Imported tool from {dllFile.FileName} (not fully implemented in original ToolService)");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteTool(string toolName)
    {
        var tool = _dbContext.Tools.FirstOrDefault(t => t.Name == toolName);
        if (tool != null)
        {
            _dbContext.Tools.Remove(tool);
            _dbContext.SaveChanges();
            _logger.LogInformation($"Deleted tool {toolName}");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult TogglePremium(string toolName, bool isPremium)
    {
        var tool = _dbContext.Tools.FirstOrDefault(t => t.Name == toolName);
        if (tool != null)
        {
            tool.IsPremium = isPremium;
            _dbContext.SaveChanges();
            _logger.LogInformation($"Set {toolName} premium status to {isPremium}");
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult ToggleAvailability(string toolName, bool isAvailable)
    {
        var tool = _dbContext.Tools.FirstOrDefault(t => t.Name == toolName);
        if (tool != null)
        {
            tool.IsAvailable = isAvailable;
            _dbContext.SaveChanges();
            _logger.LogInformation($"Set {toolName} availability to {isAvailable}");
        }
        return RedirectToAction("Index");
    }
}

// Assuming this is defined elsewhere
public class ApplicationDbContext : DbContext
{
    public DbSet<Tool> Tools { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
}