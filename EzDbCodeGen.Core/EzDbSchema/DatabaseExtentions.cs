using System;
using EzDbCodeGen.Core.Config;
using EzDbSchema.Core.Interfaces;
using Newtonsoft.Json;

namespace EzDbCodeGen.Core
{
	public static class DatabaseExtentions
    {
		/// <summary>
        /// This will filter a schema based on the a passed configuration file.  This will remove entites that will need to be ignored and alter primary keys based
		/// on the parameters passed 
        /// </summary>
        /// <returns>An altered copy of the database</returns>
        /// <param name="database">Database.</param>
        /// <param name="config">Config.</param>
		public static IDatabase Filter(this IDatabase database, CodeGenConfiguration config)
        {
			var schemaAsJson = JsonConvert.SerializeObject(database, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });
			var ret = JsonConvert.DeserializeObject<IDatabase>(schemaAsJson);
			return ret;
        }    
	}
}
