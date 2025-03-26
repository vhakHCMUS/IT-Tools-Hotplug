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

        // Handle POST request to execute tool
        if (Request.Method == "POST" && !string.IsNullOrEmpty(inputs))
        {
            await ExecuteToolWithLogging(tool, inputs);
        }

        // Set up custom view template
        SetupCustomViewTemplate(tool, toolName);

        ViewBag.Inputs = inputs; // Retain inputs for form display
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
            // Convert input string to object array
            object[] inputArray = inputs.Split(',')
                .Select(i => i.Trim() as object)
                .ToArray();

            // Execute the tool
            var result = await tool.ExecuteAsync(inputArray);

            // Log and store the result
            _logger.LogInformation($"ExecuteAsync result: {result?.ToString() ?? "null"}");
            ViewBag.Result = result?.ToString() ?? "No result returned.";
        }
        catch (Exception ex)
        {
            // Log and store any execution errors
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

    // Optional input model for future extensibility
    public class ToolExecutionInput
    {
        public string[] Inputs { get; set; }
        // You can add more properties here for future input handling
    }
}