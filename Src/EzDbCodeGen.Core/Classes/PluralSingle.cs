using System.Text.Json.Serialization;

namespace EzDbCodeGen.Core.Classes
{
    /// <summary>
    /// Represents a mapping between singular and plural forms of words
    /// </summary>
    public class PluralSingle
    {
        /// <summary>
        /// Gets or sets the singular form of the word
        /// </summary>
        [JsonPropertyName("singleWord")]
        public string SingleWord { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the plural form of the word
        /// </summary>
        [JsonPropertyName("pluralWord")]
        public string PluralWord { get; set; } = string.Empty;
    }
}
