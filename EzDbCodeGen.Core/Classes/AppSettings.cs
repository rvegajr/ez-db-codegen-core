using System.IO;
using EzDbCodeGen.Core;
using Microsoft.Extensions.Configuration;
namespace EzDbCodeGen.Internal
{
	public class AppSettings 
    {
		/// <summary></summary>
		public string ConfigurationFileName { get; set; } = "";
		/// <summary></summary>
		public bool VerboseMessages { get; set; } = false;
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
					var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
					IConfigurationRoot configuration = builder.Build();
					instance = new AppSettings();
					foreach (var item in configuration.GetChildren())
					{
						var p= instance.GetType().GetProperty(item.Key);
						if (p != null) p.SetValue(instance, item.Value);
					}
				}
                return instance;
            }
        }
    }
}
