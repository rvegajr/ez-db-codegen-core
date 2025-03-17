using EzDbCodeGen.Handlebars.Basic;
using EzDbCodeGen.Handlebars.Property;
using EzDbCodeGen.Handlebars.Entity;

namespace EzDbCodeGen.Handlebars;

/// <summary>
/// Provides utility functions for registering and managing Handlebars helpers
/// </summary>
public static class HandlebarsUtility
{
    /// <summary>
    /// Registers all standard helpers with the provided Handlebars instance
    /// </summary>
    /// <param name="handlebars">The Handlebars instance to register helpers with</param>
    public static void RegisterHelpers(IHandlebars? handlebars = null)
    {
        handlebars ??= HandlebarsDotNet.Handlebars.Create();

        BasicHelpers.Register(handlebars);
        PropertyHelpers.Register(handlebars);
        EntityHelpers.Register(handlebars);
    }
}
