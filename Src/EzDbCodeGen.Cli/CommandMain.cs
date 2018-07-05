using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using EzDbCodeGen.Internal;
namespace EzDbCodeGen.Cli
{
    public static class CommandMain
    {
        public static void Enable(CommandLineApplication app) {
            app.Name = "plex.codegen.cli";
            app.Description = "Plex Code Generation Utility";
            app.ExtendedHelpText = "This application will allow you to trigger code generation using the command line.  It will handle Plex and Unifier Conduit Conde Generations."
                + Environment.NewLine + "This utility will also dump the schema into a handy JSON file.";

            // Set the arguments to display the description and help text
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                app.ShowHint();
                Environment.ExitCode = 0;
                return Environment.ExitCode;
            });
        }
    }

    public static class CommandHelper<T>
    {
        public static IEnumerable<T> GetAll()
        {
            var assembly = Assembly.GetEntryAssembly();
            var assemblies = assembly.GetReferencedAssemblies();

            foreach (var assemblyName in assemblies)
            {
                assembly = Assembly.Load(assemblyName);

                foreach (var ti in assembly.DefinedTypes)
                {
                    if (ti.ImplementedInterfaces.Contains(typeof(T)))
                    {
                        yield return (T)assembly.CreateInstance(ti.FullName);
                    }
                }
            }
        }
    }
}