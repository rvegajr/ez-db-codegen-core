using System;
using System.IO;
using System.Text;
using HandlebarsDotNet;

namespace EzDbCodeGen.Tests.EFCore
{
    /// <summary>
    /// Extension methods for HandlebarsDotNet
    /// </summary>
    public static class HandlebarsExtensions
    {
        /// <summary>
        /// Registers all the required Handlebars helpers for the tests
        /// </summary>
        public static void RegisterHelpers()
        {
            // Register equality helper
            Handlebars.RegisterHelper("eq", (writer, context, arguments) =>
            {
                if (arguments.Length != 2)
                {
                    throw new HandlebarsException("{{eq}} helper requires exactly two arguments");
                }

                var left = arguments[0];
                var right = arguments[1];

                if (left.Equals(right))
                {
                    writer.Write(true);
                }
                else
                {
                    writer.Write(false);
                }
            });
        }

        /// <summary>
        /// Writes a safe string to the encoded text writer
        /// </summary>
        /// <param name="writer">The encoded text writer</param>
        /// <param name="value">The value to write</param>
        public static void WriteSafeString(this EncodedTextWriter writer, string value)
        {
            // EncodedTextWriter is a struct, so it can't be null
            // Just write the string directly without HTML encoding
            writer.Write(value);
        }
    }
}
