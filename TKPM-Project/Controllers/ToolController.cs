using Microsoft.AspNetCore.Mvc;
using TKPM_Project.Models.Tools;
using TKPM_Project.Services;

namespace TKPM_Project.Controllers;

public class ToolController : Controller
{
    private readonly ToolService _toolService;
    private readonly ILogger<ToolController> _logger;

    public ToolController(ToolService toolService, ILogger<ToolController> logger)
    {
        _toolService = toolService;
        _logger = logger;
    }

    // Hỗ trợ cả GET và POST
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

        // Xử lý POST request để thực thi công cụ
        if (Request.Method == "POST" && !string.IsNullOrEmpty(inputs))
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            try
            {
                object[] inputArray = inputs.Split(',').Select(i => i.Trim() as object).ToArray();
                var result = await tool.ExecuteAsync(inputArray);
                _logger.LogInformation($"ExecuteAsync result: {result?.ToString() ?? "null"}");
                ViewBag.Result = result?.ToString() ?? "No result returned.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing tool {toolName}: {ex.Message}");
                ViewBag.Error = $"Error executing tool: {ex.Message}";
            }
        }

        // Thiết lập template tùy chỉnh (cho cả GET và POST)
        if (!string.IsNullOrEmpty(tool.CustomViewTemplate))
        {
            _logger.LogInformation($"Custom view template found for {toolName}");
            ViewBag.CustomViewTemplate = tool.CustomViewTemplate;
        }
        else
        {
            _logger.LogInformation($"No custom view template for {toolName}, using default view");
        }

        ViewBag.Inputs = inputs; // Giữ lại inputs để hiển thị trong form
        return View("Detail", tool);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Execute(string toolName, string inputs)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model state invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        _logger.LogInformation($"POST request to Execute with toolName: {toolName}, inputs: {inputs ?? "none"}");
        var tool = _toolService.GetToolByName(toolName);
        if (tool == null)
        {
            _logger.LogWarning($"Tool not found: {toolName}");
            return NotFound();
        }

        try
        {
            // Tách chuỗi inputs thành mảng
            object[] inputArray = string.IsNullOrEmpty(inputs)
                ? new object[0]
                : inputs.Split(',').Select(i => i.Trim() as object).ToArray();

            var result = await tool.ExecuteAsync(inputArray);
            _logger.LogInformation($"ExecuteAsync result: {result?.ToString() ?? "null"}");
            ViewBag.Result = result?.ToString() ?? "No result returned.";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error executing tool {toolName}: {ex.Message}");
            ViewBag.Error = $"Error executing tool: {ex.Message}";
        }

        if (!string.IsNullOrEmpty(tool.CustomViewTemplate))
        {
            ViewBag.CustomViewTemplate = tool.CustomViewTemplate;
        }
        ViewBag.Inputs = inputs;
        return View("Detail", tool);
    }
}