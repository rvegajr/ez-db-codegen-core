using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Extentions.Json;
using System;
using System.IO;
using System.Json;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using System.Runtime.CompilerServices;
using EzDbCodeGen.Core.Config;

[assembly: InternalsVisibleTo("EzDbCodeGen.Cli")]
[assembly: InternalsVisibleTo("EzDbCodeGen.Tests")]

namespace EzDbCodeGen.Internal
{
    internal class AppSettings 
    {
        /// <summary></summary>
        public string ConfigurationFileName { get; set; } = "";
        private Configuration configuration;
        public Configuration Configuration
        {
            get
            {
                if (configuration == null) configuration = EzDbCodeGen.Core.Config.Configuration.FromFile(ConfigurationFileName);
                if (configuration.SourceFileName != this.ConfigurationFileName) configuration = EzDbCodeGen.Core.Config.Configuration.FromFile(ConfigurationFileName);
                return configuration;
            }
            set 
            {
                configuration = value;
            }
        }

        /// <summary></summary>
        public bool VerboseMessages { get; set; } = false;
        public string ConnectionString { get; set; } = "";
        public string Version { get; set; } = "";
        private static AppSettings instance;
        
		internal AppSettings()
        {
			this.ConfigurationFileName = "{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars();
        }

        internal AppSettings(string configurationFileName)
        {
            this.ConfigurationFileName = configurationFileName;
        }

        internal static AppSettings LoadFrom(string configurationFileName)
        {
            var appsettingsText = File.ReadAllText(configurationFileName);
            var items = JsonObject.Parse(appsettingsText);
            var instance = new AppSettings();
            foreach (JsonPair jp in items)
            {
                var p = instance.GetType().GetProperty(jp.Key);
                if (p != null) p.SetValue(instance, jp.Value.AsString());
            }
            return instance;
        }

        internal static AppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    var configFileName = "{ASSEMBLY_PATH}appsettings.json".ResolvePathVars();
                    try
                    {
                        if (File.Exists(configFileName))
                        {
                            instance = AppSettings.LoadFrom(configFileName);
                        }
                        else
                        {
                            instance = new AppSettings();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception(string.Format("Error while parsing {0}. {1}", configFileName, ex.Message), ex);
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="VarName">the name of the variable to fetch</param>
        /// <returns></returns>
        internal static string Var(string VarName)
        {
            if (VarName.Equals("ConnectionString"))
            {
                return AppSettings.Instance.ConnectionString;
            }
            return null;
        }
    }
}
