using System;
using System.Collections.Generic;
using System.Linq;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Handlebars helper that provides enhanced entity selection capabilities with
    /// path expressions and filtering.
    /// </summary>
    public class SelectEzHelper
    {
        private readonly SelectEz _selectEz;

        public SelectEzHelper()
        {
            _selectEz = new SelectEz();
        }

        /// <summary>
        /// Registers the SelectEz helper with the Handlebars instance.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register with.</param>
        public void Register(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("SelectEz", (context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    throw new HandlebarsException("{{SelectEz}} helper requires at least one argument (the selection expression)");
                }

                string expression = arguments[0].ToString();
                object source = context.Value;
                
                try
                {
                    if (source == null)
                    {
                        throw new ArgumentNullException(nameof(source), "Context value cannot be null when evaluating SelectEz expression");
                    }

                    // Create a context dictionary for parent/root navigation
                    var contextDict = new Dictionary<string, object>();
                    
                    // Add the current value as CurrentEntity
                    contextDict["CurrentEntity"] = source;
                    
                    // Add Entities collection if available
                    if (arguments.Length > 1 && arguments[1] != null)
                    {
                        contextDict["Entities"] = arguments[1];
                    }
                    
                    // Perform the selection
                    var result = _selectEz.Select(expression, source, contextDict);
                    
                    // Return the result as an array that Handlebars can iterate over
                    return result.ToArray();
                }
                catch (Exception ex)
                {
                    throw new HandlebarsException($"Error in SelectEz helper: {ex.Message}", ex);
                }
            });

            // Register a block helper version for iteration
            handlebars.RegisterHelper("EachSelected", (writer, options, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    throw new HandlebarsException("{{#EachSelected}} helper requires at least one argument (the selection expression)");
                }

                string expression = arguments[0].ToString();
                object source = context.Value;
                
                try
                {
                    if (source == null)
                    {
                        throw new ArgumentNullException(nameof(source), "Context value cannot be null when evaluating EachSelected expression");
                    }

                    // Create a context dictionary for parent/root navigation
                    var contextDict = new Dictionary<string, object>();
                    
                    // Add the current value as CurrentEntity
                    contextDict["CurrentEntity"] = source;
                    
                    // Add Entities collection if available
                    if (arguments.Length > 1 && arguments[1] != null)
                    {
                        contextDict["Entities"] = arguments[1];
                    }
                    
                    // Perform the selection
                    var selectedItems = _selectEz.Select(expression, source, contextDict).ToList();
                    
                    if (selectedItems.Any())
                    {
                        foreach (var item in selectedItems)
                        {
                            options.Template(writer, item);
                        }
                    }
                    else
                    {
                        try
                        {
                            options.Inverse(writer, context);
                        }
                        catch (NullReferenceException)
                        {
                            // No inverse block provided, do nothing
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HandlebarsException($"Error in EachSelected helper: {ex.Message}", ex);
                }
            });
        }
    }
}
