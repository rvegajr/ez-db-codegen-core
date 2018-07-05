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
        private static AppSettings instance;
        
		private AppSettings()
        {
			this.ConfigurationFileName = "{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars();
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
                        //Complete ghetto way to deal with working around a Newtonsoft JSON bug 
                        var appsettingsText = File.ReadAllText(configFileName);
                        var items = JsonObject.Parse(appsettingsText);
                        instance = new AppSettings();
                        foreach (JsonPair jp in items)
                        {
                            var p = instance.GetType().GetProperty(jp.Key);
                            if (p != null) p.SetValue(instance, jp.Value.AsString());
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
    }
}
