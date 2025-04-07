using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TKPM_Project.Models.Tools
{
    public abstract class BaseTool : ITool
    {
        public virtual string Name => this.GetType().Name;
        public abstract string Description { get; }
        public virtual bool IsPremium => false;
        public abstract string Category { get; }
        public virtual string CustomViewTemplate => null; // Default no custom view

        // Base implementation that accepts a dictionary for more flexible input handling
        public virtual Task<object> ExecuteAsync(params object[] inputs)
        {
            // Default implementation for backward compatibility
            // Convert inputs to dictionary for tools that implement dictionary-based processing
            if (inputs.Length == 1 && inputs[0] is IDictionary<string, object> dict)
            {
                return ProcessInputsAsync(dict);
            }

            // For tools that still expect raw inputs
            return ProcessRawInputsAsync(inputs);
        }

        // Override this method in derived classes that want to work with named parameters
        protected virtual Task<object> ProcessInputsAsync(IDictionary<string, object> inputs)
        {
            // Default implementation just passes it through to raw processing
            // You would override this in your tool implementations
            return ProcessRawInputsAsync(ConvertDictionaryToArray(inputs));
        }

        // Override this method in derived classes that want to work with positional parameters
        protected virtual Task<object> ProcessRawInputsAsync(object[] inputs)
        {
            // Default implementation that returns an error
            // This should be overridden in tool implementations
            throw new NotImplementedException("Tool must implement either ProcessInputsAsync or ProcessRawInputsAsync.");
        }

        // Utility method to convert dictionary to array for backwards compatibility
        protected object[] ConvertDictionaryToArray(IDictionary<string, object> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                return Array.Empty<object>();

            var result = new List<object>();
            foreach (var value in inputs.Values)
            {
                result.Add(value);
            }
            return result.ToArray();
        }

        public virtual void Dispose() { }
    }
}