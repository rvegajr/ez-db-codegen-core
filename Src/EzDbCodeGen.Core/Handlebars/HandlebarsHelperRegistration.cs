using System;
using HandlebarsDotNet;

namespace EzDbCodeGen.Core.Handlebars
{
    /// <summary>
    /// Provides methods to register all templating helpers with Handlebars
    /// </summary>
    public static class HandlebarsHelperRegistration
    {
        /// <summary>
        /// Registers all templating helpers with the Handlebars context
        /// </summary>
        /// <param name="context">The Handlebars context</param>
        public static void RegisterAllHelpers(IHandlebars context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Register formatting helpers
            FormatEz.RegisterHelper(context);
            ConvertTypeEz.RegisterHelper(context);
            DocEz.RegisterHelper(context);
            LayoutEz.RegisterHelper(context);
            CodeFormatEz.RegisterHelper(context);
        }
    }
}
