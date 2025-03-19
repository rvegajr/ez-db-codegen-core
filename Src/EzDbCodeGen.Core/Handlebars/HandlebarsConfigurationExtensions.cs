using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Extension methods for configuring Handlebars with EzDbCodeGen helpers.
    /// </summary>
    public static class HandlebarsConfigurationExtensions
    {
        /// <summary>
        /// Registers the IfCondEz helper with the Handlebars configuration.
        /// </summary>
        /// <param name="configuration">The Handlebars configuration.</param>
        /// <returns>The Handlebars configuration for method chaining.</returns>
        public static IHandlebars RegisterIfCondEzHelper(this IHandlebars configuration)
        {
            new IfCondEzHelper().Register(configuration);
            return configuration;
        }

        /// <summary>
        /// Registers the SelectEz helper with the Handlebars configuration.
        /// </summary>
        /// <param name="configuration">The Handlebars configuration.</param>
        /// <returns>The Handlebars configuration for method chaining.</returns>
        public static IHandlebars RegisterSelectEzHelper(this IHandlebars configuration)
        {
            new SelectEzHelper().Register(configuration);
            return configuration;
        }

        /// <summary>
        /// Registers all EzDbCodeGen helpers with the Handlebars configuration.
        /// </summary>
        /// <param name="configuration">The Handlebars configuration.</param>
        /// <returns>The Handlebars configuration for method chaining.</returns>
        public static IHandlebars RegisterAllEzHelpers(this IHandlebars configuration)
        {
            // Register existing helpers
            HandlebarsUtility.RegisterHelpers(configuration);
            HandlebarsModelPropertyHelpers.RegisterHelpers(configuration);
            HandlebarsSchemaHelpers.RegisterHelpers(configuration);
            HandlebarsAuditHelpers.RegisterHelpers(configuration);
            HandlebarsForeignKeyHelpers.RegisterHelpers(configuration);
            HandlebarsUnitTestHelpers.RegisterHelpers(configuration);
            
            // Register the new IfCondEz helper
            configuration.RegisterIfCondEzHelper();
            
            // Register the new SelectEz helper
            configuration.RegisterSelectEzHelper();
            
            return configuration;
        }
    }
}
