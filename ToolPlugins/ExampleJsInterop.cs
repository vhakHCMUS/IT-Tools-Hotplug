using System;
using System.Text;
using System.Threading.Tasks;
using TKPM_Project.Models.Tools;

namespace ToolPlugin
{
    public class CaesarCipherTool : BaseTool
    {
        public override string Name => "CaesarCipherTool";
        public override string Description => "Encrypts or decrypts a string using a Caesar Cipher with a specified shift value.";
        public override string Category => "Encryption";
        public override bool IsPremium => false;

        // Embedded custom view template
        public override string CustomViewTemplate => @"
<div class=""text-center"">
    <h3>Use Caesar Cipher Tool</h3>
    <form asp-action=""Detail"" asp-controller=""Tool"" method=""post"">
        <input type=""hidden"" name=""toolName"" value=""CaesarCipherTool"" />
        <div class=""form-group mb-3"">
            <label for=""inputText"">Input Text:</label>
            <input type=""text"" name=""inputs"" class=""form-control"" placeholder=""Enter text to encrypt/decrypt"" required />
        </div>
        <div class=""form-group mb-3"">
            <label for=""shift"">Shift Value (1-25):</label>
            <input type=""number"" name=""inputs"" class=""form-control"" min=""1"" max=""25"" value=""3"" required />
        </div>
        <div class=""form-group mb-3"">
            <label for=""operation"">Operation:</label>
            <select name=""inputs"" class=""form-control"">
                <option value=""encrypt"">Encrypt</option>
                <option value=""decrypt"">Decrypt</option>
            </select>
        </div>
        <button type=""submit"" class=""btn btn-primary"">Process</button>
    </form>
</div>";

        public override async Task<object> ExecuteAsync(params object[] inputs)
        {
            // Validate inputs
            if (inputs == null || inputs.Length != 3)
            {
                throw new ArgumentException("Three inputs are required: text, shift value, and operation (encrypt/decrypt).");
            }

            string text = inputs[0]?.ToString();
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Input text cannot be null or empty.");
            }

            if (!int.TryParse(inputs[1]?.ToString(), out int shift) || shift < 1 || shift > 25)
            {
                throw new ArgumentException("Shift value must be a number between 1 and 25.");
            }

            string operation = inputs[2]?.ToString()?.ToLower();
            if (operation != "encrypt" && operation != "decrypt")
            {
                throw new ArgumentException("Operation must be 'encrypt' or 'decrypt'.");
            }

            // Decryption is just encryption with the negative shift
            if (operation == "decrypt")
            {
                shift = -shift;
            }

            // Process the text
            string result = ApplyCaesarCipher(text, shift);
            return result;
        }

        private string ApplyCaesarCipher(string text, int shift)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    // Determine the base (A for uppercase, a for lowercase)
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    // Apply the shift, wrapping around the alphabet (mod 26)
                    int shifted = (c - baseChar + shift) % 26;
                    // Ensure positive modulus (C# can return negative for negative shifts)
                    if (shifted < 0)
                    {
                        shifted += 26;
                    }
                    // Convert back to a character
                    result.Append((char)(baseChar + shifted));
                }
                else
                {
                    // Non-letters remain unchanged
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public override void Dispose()
        {
            // Clean up any resources if needed
            base.Dispose();
        }
    }
}