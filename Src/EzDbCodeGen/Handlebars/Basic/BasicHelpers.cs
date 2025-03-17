namespace EzDbCodeGen.Handlebars.Basic;

/// <summary>
/// Provides basic Handlebars helper functions like if/else conditions
/// </summary>
public static class BasicHelpers
{
    /// <summary>
    /// Registers basic helpers with the provided Handlebars instance
    /// </summary>
    public static void Register(IHandlebars handlebars)
    {
        RegisterIfHelper(handlebars);
    }

    private static void RegisterIfHelper(IHandlebars handlebars)
    {
        handlebars.RegisterHelper("if", (writer, options, context, arguments) =>
        {
            if (arguments.Length != 1)
            {
                throw new HandlebarsException("{{#if}} helper requires exactly one argument");
            }

            var condition = arguments[0];
            if (condition is bool boolValue)
            {
                if (boolValue)
                {
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            }
            else
            {
                if (condition != null)
                {
                    options.Template(writer, context);
                }
                else
                {
                    options.Inverse(writer, context);
                }
            }
        });
    }
}
