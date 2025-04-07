using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TKPM_Project.Models.Tools;

namespace ToolPlugin
{
    public class CalculatorTool : BaseTool
    {
        public override string Name => "CalculatorTool";
        public override string Description => "Performs basic arithmetic operations.";
        public override string Category => "Math";
        public override bool IsPremium => false;
        public override string CustomViewTemplate => @"
<div class=""container mt-3"">
    <h4>Calculator</h4>
    <form action=""/Tool/Detail"" method=""post"">
        <input type=""hidden"" name=""toolName"" value=""CalculatorTool"" />
        <div class=""form-group"">
            <label for=""num1"">Number 1:</label>
            <input type=""number"" name=""num1"" class=""form-control"" id=""num1"" step=""any"" required />
        </div>
        <div class=""form-group"">
            <label for=""operator"">Operator:</label>
            <select name=""operator"" class=""form-control"" id=""operator"">
                <option value=""+"">+</option>
                <option value=""-"">-</option>
                <option value=""*"">×</option>
                <option value=""/"">÷</option>
            </select>
        </div>
        <div class=""form-group"">
            <label for=""num2"">Number 2:</label>
            <input type=""number"" name=""num2"" class=""form-control"" id=""num2"" step=""any"" required />
        </div>
        <button type=""submit"" class=""btn btn-primary"">Calculate</button>
    </form>
</div>";

        // The key change is in the ExecuteAsync method to handle form inputs
        public override async Task<object> ExecuteAsync(params object[] inputs)
        {
            // Extract input values from either direct parameters or form collection
            IDictionary<string, string> inputDict = ExtractInputValues(inputs);

            if (!inputDict.TryGetValue("num1", out string num1Str) ||
                !inputDict.TryGetValue("num2", out string num2Str) ||
                !inputDict.TryGetValue("operator", out string op))
            {
                throw new ArgumentException("Required inputs are missing (num1, operator, num2).");
            }

            try
            {
                var num1 = Convert.ToDouble(num1Str);
                var num2 = Convert.ToDouble(num2Str);

                return op switch
                {
                    "+" => num1 + num2,
                    "-" => num1 - num2,
                    "*" or "×" => num1 * num2,
                    "/" or "÷" => num2 == 0 ? throw new ArgumentException("Division by zero.") : num1 / num2,
                    _ => throw new ArgumentException($"Unsupported operator: {op}")
                };
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid number format. Please ensure both numbers are valid.");
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ArgumentException($"Error performing calculation: {ex.Message}");
            }
        }

        // Helper method to extract input values from different input formats
        private IDictionary<string, string> ExtractInputValues(object[] inputs)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (inputs == null || inputs.Length == 0)
                return result;

            // Case 1: Inputs are already key-value pairs
            if (inputs.Length == 1 && inputs[0] is IDictionary<string, object> dictObj)
            {
                foreach (var kvp in dictObj)
                {
                    result[kvp.Key] = kvp.Value?.ToString();
                }
                return result;
            }

            // Case 2: Form collection or similar name-value structure
            if (inputs.Length == 1 && inputs[0] is Microsoft.AspNetCore.Http.IFormCollection form)
            {
                foreach (var key in form.Keys)
                {
                    result[key] = form[key].ToString();
                }
                return result;
            }

            // Case 3: Simple array inputs (legacy format)
            if (inputs.Length >= 3)
            {
                result["num1"] = inputs[0]?.ToString();
                result["operator"] = inputs[1]?.ToString();
                result["num2"] = inputs[2]?.ToString();
            }

            return result;
        }
    }
}