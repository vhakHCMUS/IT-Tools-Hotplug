using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models.Tools;
using TKPM_Project.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace TKPM_Project.Controllers
{
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
        public async Task<IActionResult> Detail(string toolName)
        {
            _logger.LogInformation($"{Request.Method} request to Detail with toolName: {toolName}");

            var tool = await _toolRepository.GetByNameAsync(toolName);
            if (tool == null)
            {
                _logger.LogWarning($"Tool not found: {toolName}");
                return NotFound();
            }

            if (!tool.IsAvailable)
            {
                _logger.LogWarning($"Tool {toolName} is not available.");
                return NotFound();
            }

            if (tool.IsPremium && !User.IsInRole("Premium"))
            {
                _logger.LogWarning($"Non-premium user attempted to access premium tool: {toolName}");
                return RedirectToAction("PremiumRequest", "User");
            }

            // Execute tool if it's a POST request with inputs
            if (Request.Method == "POST" && Request.HasFormContentType)
            {
                try
                {
                    // Process the form collection directly
                    var formData = await Request.ReadFormAsync();
                    var result = await _toolService.ExecuteToolAsync(toolName, formData);
                    ViewBag.Result = result?.ToString() ?? "No result returned.";
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error executing tool {toolName}: {ex.Message}");
                    ViewBag.Error = $"Error executing tool: {ex.Message}";
                }
            }

            // Setup view template
            SetupCustomViewTemplate(tool, toolName);
            return View("Detail", tool);
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

                // Load the tool into ToolService
                try
                {
                    _toolService.LoadToolFromDll(filePath);
                    _logger.LogInformation($"Successfully loaded tool from {dllFile.FileName} into ToolService.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to load tool from {dllFile.FileName} into ToolService.");
                    return StatusCode(500, $"Error loading tool: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning("No valid DLL file provided for import.");
                return BadRequest("Please upload a valid DLL file.");
            }

            return RedirectToAction("ToolManager");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTool(string toolName)
        {
            try
            {
                var tool = _toolService.GetToolByName(toolName);
                if (tool == null)
                {
                    _logger.LogWarning($"Tool {toolName} not found.");
                    return RedirectToAction("ToolManager");
                }

                _toolService.UnloadTool(toolName);
                _logger.LogInformation($"Unloaded tool {toolName} from memory.");

                GC.Collect();
                GC.WaitForPendingFinalizers();
                _logger.LogInformation("Garbage collection completed.");

                return RedirectToAction("ToolManager");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting tool {toolName}");
                return RedirectToAction("ToolManager");
            }
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
            var tools = await _toolRepository.GetAllAvailableAsync();
            return View(tools);
        }

        [HttpGet]
        public async Task<IActionResult> SearchTools(string name, string category, string premium, string sortBy)
        {
            try
            {
                var toolsQuery = _dbContext.Tools.AsQueryable();

                // Always filter by available tools
                toolsQuery = toolsQuery.Where(t => t.IsAvailable);

                if (!string.IsNullOrEmpty(name))
                {
                    toolsQuery = toolsQuery.Where(t => t.Name.Contains(name));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    toolsQuery = toolsQuery.Where(t => t.Category == category);
                }

                // Check if user is authenticated before applying premium filter
                if (!User.Identity.IsAuthenticated)
                {
                    toolsQuery = toolsQuery.Where(t => !t.IsPremium);
                }
                else if (!User.IsInRole("Premium"))
                {
                    toolsQuery = toolsQuery.Where(t => !t.IsPremium);
                }
                else if (!string.IsNullOrEmpty(premium) && bool.TryParse(premium, out bool isPremium))
                {
                    toolsQuery = toolsQuery.Where(t => t.IsPremium == isPremium);
                }

                // Apply sorting
                switch (sortBy)
                {
                    case "name_asc":
                        toolsQuery = toolsQuery.OrderBy(t => t.Name);
                        break;
                    case "name_desc":
                        toolsQuery = toolsQuery.OrderByDescending(t => t.Name);
                        break;
                    case "date_asc":
                        toolsQuery = toolsQuery.OrderBy(t => t.CreatedAt);
                        break;
                    case "date_desc":
                        toolsQuery = toolsQuery.OrderByDescending(t => t.CreatedAt);
                        break;
                    default:
                        toolsQuery = toolsQuery.OrderBy(t => t.Name); // Default sort by name ascending
                        break;
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
            return View(tools);
        }
    }
}