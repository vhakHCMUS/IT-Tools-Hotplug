using Microsoft.AspNetCore.Authorization;
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
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ToolController> _logger;
    private readonly IToolRepository _toolRepository;

    public ToolController(ToolService toolService, ApplicationDbContext dbContext, ILogger<ToolController> logger, IToolRepository toolRepository)
    {
        _toolService = toolService;
        _dbContext = dbContext;
        _logger = logger;
        _toolRepository = toolRepository;
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
        var tools = _dbContext.Tools.ToList();
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

    [Authorize]
    [HttpGet]
    [Route("tool/likedtools")]
    public IActionResult LikedTools()
    {
        _logger.LogInformation($"User {User.Identity.Name} accessed LikedTools page.");
        return View();
    }

    [HttpPost]
    public IActionResult ImportTool(IFormFile dllFile, string toolCategory = null)
    {
        if (dllFile != null && dllFile.Length > 0)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", dllFile.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                dllFile.CopyTo(stream);
            }

            // Optional: Add the tool to database with category
            if (!string.IsNullOrEmpty(toolCategory))
            {
                var tool = new Tool
                {
                    Name = Path.GetFileNameWithoutExtension(dllFile.FileName),
                    Category = toolCategory,
                    CreatedAt = DateTime.Now,
                    IsAvailable = true
                };
                _dbContext.Tools.Add(tool);
                _dbContext.SaveChanges();
            }

            _logger.LogInformation($"Imported tool from {dllFile.FileName} with category {toolCategory}");
        }
        return RedirectToAction("ToolManager");
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
        return RedirectToAction("ToolManager");
    }

    [HttpPost]
    public IActionResult TogglePremium(string toolName, bool isPremium)
    {
        _logger.LogInformation($"TogglePremium called with toolName: {toolName}, isPremium: {isPremium}");
        var tool = _dbContext.Tools.FirstOrDefault(t => t.Name == toolName);
        if (tool != null)
        {
            tool.IsPremium = isPremium;
            _dbContext.SaveChanges();
            _logger.LogInformation($"Set {toolName} premium status to {isPremium}");
            return Json(new { success = true });
        }
        _logger.LogWarning($"Tool {toolName} not found in database.");
        return Json(new { success = false, message = "Tool not found" });
    }

    [HttpPost]
    public IActionResult ToggleAvailability(string toolName, bool isAvailable)
    {
        _logger.LogInformation($"ToggleAvailability called with toolName: {toolName}, isAvailable: {isAvailable}");
        var tool = _dbContext.Tools.FirstOrDefault(t => t.Name == toolName);
        if (tool != null)
        {
            tool.IsAvailable = isAvailable;
            _dbContext.SaveChanges();
            _logger.LogInformation($"Set {toolName} availability to {isAvailable}");
            return Json(new { success = true });
        }
        _logger.LogWarning($"Tool {toolName} not found in database.");
        return Json(new { success = false, message = "Tool not found" });
    }

    [HttpGet]
    public async Task<IActionResult> Tools()
    {
        var tools = await _toolRepository.GetAllAsync();
        if (tools == null || !tools.Any())
        {
            _logger.LogWarning("No tools retrieved for Tools view.");
        }
        else
        {
            _logger.LogInformation($"Retrieved {tools.Count()} tools for Tools view.");
        }
        return View(tools);
    }

    [HttpGet]
    public async Task<IActionResult> SearchTools(string name, string category, string premium)
    {
        try
        {
            if (_dbContext == null || _dbContext.Tools == null)
            {
                _logger.LogError("Database context or Tools DbSet is null");
                return StatusCode(500, "Database configuration error");
            }

            var toolsQuery = _dbContext.Tools.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                toolsQuery = toolsQuery.Where(t => t.Name != null && t.Name.ToLower().Contains(name.ToLower()));
            }

            if (!string.IsNullOrEmpty(category))
            {
                toolsQuery = toolsQuery.Where(t => t.Category != null && t.Category == category);
            }

            if (!string.IsNullOrEmpty(premium) && bool.TryParse(premium, out bool isPremium))
            {
                toolsQuery = toolsQuery.Where(t => t.IsPremium == isPremium);
            }

            var tools = await toolsQuery
                .Select(t => new
                {
                    Id = t.Id,
                    Name = t.Name ?? "N/A",
                    Description = t.Description ?? "",
                    Category = t.Category ?? "Uncategorized",
                    IsPremium = t.IsPremium
                })
                .ToListAsync();

            _logger.LogInformation($"Search returned {tools.Count} tools for name: {name ?? "none"}, category: {category ?? "none"}, premium: {premium ?? "none"}");
            return Json(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SearchTools: {Message}", ex.Message);
            return StatusCode(500, $"Error searching tools: {ex.Message}");
        }
    }

    public async Task<IActionResult> ToolManager()
    {
        var tools = await _toolRepository.GetAllAsync();
        if (tools == null || !tools.Any())
        {
            _logger.LogWarning("No tools retrieved from ToolManager.");
        }
        else
        {
            _logger.LogInformation($"Retrieved tools for ToolManager.");
        }
        return View(tools); // Uses default view "ToolManager.cshtml"
    }

}