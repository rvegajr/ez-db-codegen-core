using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace EzDbCodeGen.Cli;

internal class Settings
{
    [JsonPropertyName("schemaName")]
    public string SchemaName { get; set; } = "MySchema";
    
    [JsonPropertyName("appName")]
    public string AppName { get; set; } = "MyApp";
    
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }
    
    [JsonPropertyName("templateFileNameOrPath")]
    public string? TemplateFileNameOrPath { get; set; }
    
    [JsonPropertyName("autoRun")]
    public bool AutoRun { get; set; } = false;
    
    [JsonPropertyName("verbose")]
    public bool Verbose { get; set; } = false;
    
    [JsonPropertyName("config")]
    public string Config { get; set; } = string.Empty;
}
