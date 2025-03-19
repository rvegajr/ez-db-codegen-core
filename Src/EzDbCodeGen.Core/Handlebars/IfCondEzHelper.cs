using System;
using System.IO;
using System.Threading.Tasks;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Handlebars helper that provides enhanced conditional expressions with schema navigation
    /// and equation processing capabilities.
    /// </summary>
    public class IfCondEzHelper
    {
        private readonly IfCondEz _ifCondEz;

        public IfCondEzHelper()
        {
            _ifCondEz = new IfCondEz();
        }

        /// <summary>
        /// Registers the IfCondEz helper with the Handlebars instance.
        /// </summary>
        /// <param name="handlebars">The Handlebars instance to register with.</param>
        public void Register(IHandlebars handlebars)
        {
            handlebars.RegisterHelper("IfCondEz", (writer, options, context, arguments) =>
            {
                if (arguments.Length < 1)
                {
                    throw new HandlebarsException("{{#IfCondEz}} helper requires at least one argument (the condition)");
                }

                string expression = arguments[0].ToString();
                object entity = context.Value;

                try
                {
                    if (entity == null)
                    {
                        throw new ArgumentNullException(nameof(entity), "Context value cannot be null when evaluating IfCondEz condition");
                    }

                    bool result = _ifCondEz.Evaluate(expression, entity);

                    if (result)
                    {
                        options.Template(writer, context);
                    }
                    else
                    {
                        options.Inverse(writer, context);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new HandlebarsException($"Error evaluating condition in {{#IfCondEz}}: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new HandlebarsException($"Unexpected error in {{#IfCondEz}}: {ex.Message}", ex);
                }
            });
        }
    }
}
