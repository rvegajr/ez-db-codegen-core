using Newtonsoft.Json;

namespace EzDbCodeGen.Core.Classes
{
    /// <summary>
    /// Represents a mapping between database data types and target .NET data types
    /// </summary>
    public class DataTypeMap
    {
        /// <summary>
        /// Gets or sets the source data type from the database
        /// </summary>
        [JsonProperty("dataType")]
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target .NET data type
        /// </summary>
        [JsonProperty("targetDataType")]
        public string TargetDataType { get; set; } = string.Empty;
    }
}
