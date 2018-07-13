using EzDbSchema.Core.Extentions.Json;
using EzDbSchema.Core.Extentions.Strings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using JsonPair = System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>;
using JsonPairEnumerable = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Json.JsonValue>>;

namespace EzDbCodeGen.Internal
{
	public class AppSettings 
    {
        /// <summary></summary>
        public string ConfigurationFileName { get; set; } = "";
		/// <summary></summary>
		public bool VerboseMessages { get; set; } = false;
        public string ConnectionString { get; set; } = "";
        public string Version { get; set; } = "";
        private static AppSettings instance;
        
		private AppSettings()
        {
			this.ConfigurationFileName = "{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars();
        }

        private AppSettings(string configurationFileName)
        {
            this.ConfigurationFileName = configurationFileName;
        }

        public static AppSettings LoadFrom(string configurationFileName)
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

        public static AppSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    var configFileName = "{ASSEMBLY_PATH}appsettings.json".ResolvePathVars();
                    try
                    {
                        instance = AppSettings.LoadFrom(configFileName);
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
        public static string Var(string VarName)
        {
            if (VarName.Equals("ConnectionString"))
            {
                return AppSettings.Instance.ConnectionString;
            }
            return null;
        }
    }
}
