using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzDbCodeGen.Cli
{
    internal class Settings
    {
        public string SchemaName { get; set; } = "MySchema";
        public string AppName { get; set; } = "MyApp";
        public string ConnectionString { get; set; }
        public string TemplateFileNameOrPath { get; set; }
        public bool AutoRun { get; set; } = false;
    }
}
