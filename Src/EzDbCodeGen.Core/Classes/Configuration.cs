using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = Newtonsoft.Json.JsonException;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Interfaces;
using EzDbSchema.Core.Extentions;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;

namespace EzDbCodeGen.Core.Config
{
    public class Field
    {
        public required string FieldName { get; set; }
        public required string ColumnAttributeTypeName { get; set; }
        public bool? Nullable { get; set; } = null;
        public required string DisplayName { get; set; }
        public required string PlaceholderText { get; set; }
        public required string HelpText { get; set; }
        public required string InputType { get; set; }
        public bool? IsRequired { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool? IsReadOnly { get; set; }
        public bool? IsHidden { get; set; }
        public bool? IsVisible { get; set; }
        public bool? IsEditable { get; set; }
    }

    public class PrimaryKey
    {
        public required string FieldName { get; set; }
    }

    public class Overrides
    {
        public List<PrimaryKey> PrimaryKey { get; set; } = new List<PrimaryKey>();
        public List<Field> Fields { get; set; } = new List<Field>();
    }

    public class Entity
    {
        public string Name { get; set; } = "";
        public bool Ignore { get; set; } = false;
        public string AliasRenameTo { get; set; } = "";
        public Overrides Overrides { get; set; } = new Overrides();
        public string[] ObjectFilters = (new List<string>()).ToArray();
        public Dictionary<object, object> Misc { get; set; } = new Dictionary<object, object>();

        public void ClearPKOverrides()
        {
            this.Overrides.PrimaryKey.Clear();
        }
        public Entity AddPKOverride(string ColumnToAddAsPKOverride)
        {
            this.Overrides.PrimaryKey.Add(new PrimaryKey() { FieldName = ColumnToAddAsPKOverride });
            return this;
        }
    }

    public class TemplateItem
    {
        public string Name { get; set; } = "";

        public string[] Include = (new List<string>()).ToArray();
        public string[] Exclude = (new List<string>()).ToArray();
    }

    public class PluralSingle
    {
        public string SingleWord { get; set; } = "";
        public string PluralWord { get; set; } = "";
    }

    public class DataTypeMap
    {
        public string DataType { get; set; } = "";
        public string TargetDataType { get; set; } = "";
    }

    public class Database
    {
        public string DefaultSchema { get; set; } = "dbo";
        public string AliasNamePattern { get; set; } = Configuration.OBJECT_NAME;
        public bool FilterEntitiesWithNoKey { get; set; } = false;
        public bool AutoAddKeysIfNoPK { get; set; } = false;
        public string SchemaName { get; set; } = "";
        public string PropertyObjectNameCollisionSuffix { get; set; } = "Value";
        public string InverseFKTargetNameCollisionSuffix { get; set; } = "Item";
        public Dictionary<object, object> Misc { get; set; } = new Dictionary<object, object>();

        public string[] ColumnNameFilters = (new List<string>()).ToArray();

        public string[] ColumnNameComputed = (new List<string>()).ToArray();

        public string[] ColumnNameNotMapped = (new List<string>()).ToArray();
        
        public bool DeleteObjectOnFilter { get; set; } = true;

    }

    public static class ConfigurationExtentions
    {
        /// <summary>
        /// This will search for a string and see if it matches in a list of items To Search
        /// </summary>
        /// <param name="itemsToSearch">This list can be literal or file cards on either or both ends</param>
        /// <param name="objectNameToCheck">String to see if it exists in @itemsToSearch</param>
        /// <returns></returns>
        public static bool IsInWildcardList(this string[] itemsToSearch, string objectNameToCheck)
        {
            var isFiltered = false;
            foreach(var itemToSearch in itemsToSearch)
            {
                if (itemToSearch== objectNameToCheck)
                {
                    isFiltered = true;
                    break;
                } else {
                    if (itemToSearch.Contains(@"*")) //contains wildcard?
                    {
                        var isMatched = Regex.IsMatch(objectNameToCheck, "^" + Regex.Escape(itemToSearch).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                        if (isMatched) isFiltered = true;
                        if (isFiltered) break;
                    }
                }
            }
            return isFiltered;
        }
    }

    public class Configuration
    {
        private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new()
        {
            Formatting = Newtonsoft.Json.Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        private readonly Dictionary<string, object> _configSettings;

        public Configuration()
        {
            _configSettings = new Dictionary<string, object>();
            PluralizerCrossReference = new List<PluralSingle>();
            NotMappedColumns = new List<string>();
            NotMappedTables = new List<string>();
            NotMappedSchemas = new List<string>();
            NotMappedTypes = new List<string>();
            TypeAliases = new Dictionary<string, string>();
            Templates = new List<string>();
            TemplateFileNameFilter = new List<string>();
            VerboseMessages = false;
            AutoRun = false;
        }

        public T GetConfigValue<T>(string configKey)
        {
            if (!_configSettings.TryGetValue(configKey, out var configValue))
                throw new KeyNotFoundException($"Configuration key '{configKey}' not found");

            try
            {
                if (configValue is T typedConfigValue)
                    return typedConfigValue;

                var jsonToken = JToken.FromObject(configValue);
                return jsonToken.ToObject<T>() ?? throw new InvalidOperationException($"Failed to convert value for key '{configKey}' to type {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting value for key '{configKey}' to type {typeof(T).Name}", ex);
            }
        }

        public bool TryGetConfigValue<T>(string configKey, out T configValue)
        {
            configValue = default;

            if (!_configSettings.TryGetValue(configKey, out var rawConfigValue))
                return false;

            try
            {
                if (rawConfigValue is T typedConfigValue)
                {
                    configValue = typedConfigValue;
                    return true;
                }

                var jsonToken = JToken.FromObject(rawConfigValue);
                var converted = jsonToken.ToObject<T>();
                if (converted != null)
                {
                    configValue = converted;
                    return true;
                }
            }
            catch
            {
                // Conversion failed
            }

            return false;
        }

        public void SetConfigValue<T>(string configKey, T configValue)
        {
            _configSettings[configKey] = configValue;
        }

        public bool HasConfigValue(string configKey)
        {
            return _configSettings.ContainsKey(configKey);
        }

        public void RemoveConfigValue(string configKey)
        {
            _configSettings.Remove(configKey);
        }

        public void ClearConfig()
        {
            _configSettings.Clear();
        }

        public T GetValue<T>(string key) => GetConfigValue<T>(key);
        public bool TryGetValue<T>(string key, out T value) => TryGetConfigValue(key, out value);
        public void SetValue<T>(string key, T value) => SetConfigValue(key, value);
        public bool HasValue(string key) => HasConfigValue(key);
        public void RemoveValue(string key) => RemoveConfigValue(key);
        public void Clear() => ClearConfig();

        public static Configuration FromFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Configuration file not found: {fileName}");

            try
            {
                var jsonString = File.ReadAllText(fileName);
                var config = JsonConvert.DeserializeObject<Configuration>(jsonString, DefaultJsonSerializerSettings);
                if (config == null)
                    throw new InvalidOperationException($"Failed to deserialize configuration from {fileName}");

                config.SourceFileName = fileName;
                return config;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error parsing configuration file {fileName}: {ex.Message}", ex);
            }
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, DefaultJsonSerializerSettings);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving configuration to file: {ex.Message}", ex);
            }
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>(_configSettings);
        }

        public void Merge(Configuration other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            foreach (var kvp in other._configSettings)
            {
                _configSettings[kvp.Key] = kvp.Value;
            }
        }

        public List<Entity> Entities { get; set; } = new List<Entity>();
        public List<TemplateItem> TemplatesList { get; set; } = new List<TemplateItem>();
        public List<PluralSingle> PluralizerCrossReferenceList { get; set; } = new List<PluralSingle>();
        public List<DataTypeMap> DataTypeMapList { get; set; } = new List<DataTypeMap>();
        public List<DataTypeMap> DataTypeMap { get; set; } = new();
        public Database? Database { get; set; }
        public string? ConnectionString { get; set; }
        public List<PluralSingle> PluralizerCrossReference { get; set; } = new();
        public List<string>? NotMappedColumns { get; set; }
        public List<string>? NotMappedTables { get; set; }
        public List<string>? NotMappedSchemas { get; set; }
        public List<string>? NotMappedTypes { get; set; }
        public Dictionary<string, string>? TypeAliases { get; set; }
        public List<string>? Templates { get; set; }
        public List<string>? TemplateFileNameFilter { get; set; }
        public string? OutputPath { get; set; }
        public bool VerboseMessages { get; set; }
        public bool AutoRun { get; set; }

        public string? SourceFileName 
        { 
            get 
            {
                if (string.IsNullOrEmpty(_sourceFileName)) 
                    throw new ArgumentNullException(nameof(SourceFileName), "Need to set SourceFileName to a valid file name");
                return _sourceFileName;
            }
            set => _sourceFileName = value;
        }
        private string? _sourceFileName;

        public const string SCHEMA_NAME = "{SCHEMANAME}";
        public const string OBJECT_NAME = "{OBJECTNAME}";
        public const string OP_UPPER_CASE = "U";
        public const string OP_LOWER_CASE = "L";
        public const string OP_PROPER_CASE = "P";
        public const string OP_STRING_REMOVE = "X"; // X'<String to remove>'
        public const string OP_STRING_REPLACE = "R"; // R'Old string'=>'New String'
        public const string OP_SINGULARIZE = "S";
        public const string OP_PLURALIZE = "M";
        public const string OP_TITLE_CASE = "T";

        public static string ReplaceEx(string TemplatePattern, SchemaObjectName schemaObjectName)
        {
            var returnString = TemplatePattern;
            var templatePattern = TemplatePattern;
            var operandList = "";
            if (templatePattern.Contains("-"))
            {
                var arr = templatePattern.Split('-');
                templatePattern = arr[0];
                operandList = arr[1];
                if (operandList.EndsWith("}")) 
                    operandList = operandList[..^1];
                var OperandArray = operandList.Split('|');
                foreach(var operand in OperandArray)
                {
                    if (operand.StartsWith(Configuration.OP_LOWER_CASE))
                    {
                        returnString = returnString.ToLower();
                    }
                    else if (operand.StartsWith(Configuration.OP_PROPER_CASE))
                    {
                        returnString = returnString.ToTitleCase();
                    }
                    else if (operand.StartsWith(Configuration.OP_UPPER_CASE))
                    {
                        returnString = returnString.ToUpper();
                    }
                    else if (operand.StartsWith(Configuration.OP_SINGULARIZE))
                    {
                        returnString = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(returnString);
                    }
                    else if (operand.StartsWith(Configuration.OP_PLURALIZE))
                    {
                        returnString = EzDbSchema.Core.Extentions.StringExtensions.ToPlural(returnString);
                    }
                    else if (operand.StartsWith(Configuration.OP_TITLE_CASE))
                    {
                        returnString = returnString.ToTitleCase();
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REMOVE))
                    {
                        if (!operand.StartsWith(Configuration.OP_STRING_REMOVE + "'")) 
                            throw new Exception("OP_STRING_REMOVE should have X'????' where X is immediately followed by a single quote and closed by a another single quote");
                        var strToReplace = operand[2..];
                        if (operand.EndsWith("'")) 
                            strToReplace = strToReplace[..^1];
                        returnString = Regex.Replace(returnString, strToReplace, "", RegexOptions.IgnoreCase);
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REPLACE))
                    {
                        var operandArr = operand.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                        operandArr[0] = operandArr[0][1..];
                        returnString = Regex.Replace(returnString, operandArr[0].Unquote(), operandArr[1].Unquote(), RegexOptions.IgnoreCase);
                    }
                }
            }
            return returnString;
        }

        public static string ReplaceEx(string TemplatePattern, string StringWithPatternToReplace)
        {
            var returnString = StringWithPatternToReplace;
            var templatePattern = TemplatePattern;
            var operandList = "";
            if (templatePattern.Contains("-"))
            {
                var arr = templatePattern.Split('-');
                templatePattern = arr[0];
                operandList = arr[1];
                if (operandList.EndsWith("}")) 
                    operandList = operandList[..^1];
                var OperandArray = operandList.Split('|');
                foreach(var operand in OperandArray)
                {
                    if (operand.StartsWith(Configuration.OP_LOWER_CASE))
                    {
                        returnString = returnString.ToLower();
                    }
                    else if (operand.StartsWith(Configuration.OP_PROPER_CASE))
                    {
                        returnString = returnString.ToTitleCase();
                    }
                    else if (operand.StartsWith(Configuration.OP_UPPER_CASE))
                    {
                        returnString = returnString.ToUpper();
                    }
                    else if (operand.StartsWith(Configuration.OP_SINGULARIZE))
                    {
                        returnString = EzDbSchema.Core.Extentions.StringExtensions.ToSingular(returnString);
                    }
                    else if (operand.StartsWith(Configuration.OP_PLURALIZE))
                    {
                        returnString = EzDbSchema.Core.Extentions.StringExtensions.ToPlural(returnString);
                    }
                    else if (operand.StartsWith(Configuration.OP_TITLE_CASE))
                    {
                        returnString = returnString.ToTitleCase();
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REMOVE))
                    {
                        if (!operand.StartsWith(Configuration.OP_STRING_REMOVE + "'")) 
                            throw new Exception("OP_STRING_REMOVE should have X'????' where X is immediately followed by a single quote and closed by a another single quote");
                        var strToReplace = operand[2..];
                        if (operand.EndsWith("'")) 
                            strToReplace = strToReplace[..^1];
                        returnString = Regex.Replace(returnString, strToReplace, "", RegexOptions.IgnoreCase);
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REPLACE))
                    {
                        var operandArr = operand.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                        operandArr[0] = operandArr[0][1..];
                        returnString = Regex.Replace(returnString, operandArr[0].Unquote(), operandArr[1].Unquote(), RegexOptions.IgnoreCase);
                    }
                }
            }
            return returnString;
        }

        public bool IsIgnoredColumn(IProperty property)
        {
            // In version 8.2.0, IsHidden and ParentEntity are not available
            // We'll check for ignored columns based on schema, table and property name
            var schema = property.GetType().GetProperty("DatabaseSchema")?.GetValue(property) as string ?? Database.DefaultSchema;
            var tableName = property.GetType().GetProperty("TableName")?.GetValue(property) as string ?? string.Empty;
            var propertyName = property.GetType().GetProperty("Name")?.GetValue(property) as string ?? string.Empty;
            
            return IsIgnoredColumn(schema, tableName, propertyName) || IsIgnoredColumn(schema, tableName, propertyName);
        }
        public bool IsIgnoredColumn(string schemaToCheck, string tableCheck, string columnNameToCheck)
        {
            var ignoreColumn = false;
            foreach (var columnNameFilterItem in Database.ColumnNameFilters)
            {
                var SchemaColumnNameFilterItem = Database.DefaultSchema; var TableColumnNameFilterItem = "*"; var ColumnColumnNameFilterItem = "*";
                var arr = columnNameFilterItem.Split('.');
                if (arr.Length == 3)
                {
                    SchemaColumnNameFilterItem = arr[0];
                    TableColumnNameFilterItem = arr[1];
                    ColumnColumnNameFilterItem = arr[2];
                }

                if (arr.Length == 2)
                {
                    SchemaColumnNameFilterItem = Database.DefaultSchema;
                    TableColumnNameFilterItem = arr[0];
                    ColumnColumnNameFilterItem = arr[1];
                }
                if (arr.Length == 1) //if there is only one parm, then we are going to assume that it affects all schemas
                {
                    SchemaColumnNameFilterItem = "*";
                    ColumnColumnNameFilterItem = arr[0];
                }
                //if (columnNameToCheck.Contains(@"*")) 
                //{
                    var isSchemaMatched = Regex.IsMatch(schemaToCheck, "^" + Regex.Escape(SchemaColumnNameFilterItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                    var isTableMatched = Regex.IsMatch(tableCheck, "^" + Regex.Escape(TableColumnNameFilterItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                    var isColumnMatched = Regex.IsMatch(columnNameToCheck, "^" + Regex.Escape(ColumnColumnNameFilterItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                    //var isMatched = Regex.IsMatch(columnNameFilterItem, "^" + Regex.Escape(columnNameToCheck).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                    if (isSchemaMatched && isTableMatched && isColumnMatched) return true;
                //}
                //else if (columnNameFilterItem.Equals(columnNameToCheck)) return true;
            }
            return ignoreColumn;
        }

        public bool IsComputedColumn(IProperty property)
        {
            if (property == null) return false;
            var parentEntity = property.GetType().GetProperty("ParentEntity")?.GetValue(property) as IEntity;
            if (parentEntity == null) return false;
            var columnAlias = property.GetType().GetProperty("ColumnAlias")?.GetValue(property) as string ?? string.Empty;
            var databaseSchema = parentEntity.GetType().GetProperty("DatabaseSchema")?.GetValue(parentEntity) as string ?? Database.DefaultSchema;
            var tableName = parentEntity.GetType().GetProperty("TableName")?.GetValue(parentEntity) as string ?? string.Empty;
            return (IsComputedColumn(databaseSchema, tableName, columnAlias) || 
                    IsComputedColumn(databaseSchema, tableName, columnAlias));
        }
        public bool IsComputedColumn(string schemaToCheck, string tableCheck, string columnNameToCheck)
        {
            var computedColumn = false;
            foreach (var columnNameComputedItem in Database.ColumnNameComputed)
            {
                var SchemaColumnNameComputedItem = Database.DefaultSchema; var TableColumnNameComputedItem = "*"; var ColumnColumnNameComputedItem = "*";
                var arr = columnNameComputedItem.Split('.');
                if (arr.Length == 3)
                {
                    SchemaColumnNameComputedItem = arr[0];
                    TableColumnNameComputedItem = arr[1];
                    ColumnColumnNameComputedItem = arr[2];
                }

                if (arr.Length == 2)
                {
                    SchemaColumnNameComputedItem = Database.DefaultSchema;
                    TableColumnNameComputedItem = arr[0];
                    ColumnColumnNameComputedItem = arr[1];
                }
                if (arr.Length == 1) //if there is only one parm, then we are going to assume that it affects all schemas
                {
                    SchemaColumnNameComputedItem = "*";
                    ColumnColumnNameComputedItem = arr[0];
                }
                //if (columnNameToCheck.Contains(@"*")) 
                //{
                var isSchemaMatched = Regex.IsMatch(schemaToCheck, "^" + Regex.Escape(SchemaColumnNameComputedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                var isTableMatched = Regex.IsMatch(tableCheck, "^" + Regex.Escape(TableColumnNameComputedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                var isColumnMatched = Regex.IsMatch(columnNameToCheck, "^" + Regex.Escape(ColumnColumnNameComputedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                //var isMatched = Regex.IsMatch(columnNameFilterItem, "^" + Regex.Escape(columnNameToCheck).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                if (isSchemaMatched && isTableMatched && isColumnMatched) return true;
                //}
                //else if (columnNameFilterItem.Equals(columnNameToCheck)) return true;
            }
            return computedColumn;
        }

        public bool IsIgnoredEntityByTemplate(string templateName, string entityNameToCheck)
        {
            var isIncluded = true;
            var isExcluded = false;
            foreach (var templateItem in this.TemplatesList)
            {
                if (entityNameToCheck.Contains("*Constraint*"))
                    Console.Write("");
                var isTemplateNameMatched = Regex.IsMatch(templateName, "^" + Regex.Escape(templateItem.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                if (isTemplateNameMatched)
                {
                    isIncluded = templateItem.Include.Length == 0;
                    foreach (var includeItem in templateItem.Include)
                    {
                        isIncluded = Regex.IsMatch(entityNameToCheck, "^" + Regex.Escape(includeItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                        if (isIncluded) break;
                    }
                    if (isIncluded)
                    {
                        foreach (var excludeListItem in templateItem.Exclude)
                        {
                            isExcluded = Regex.IsMatch(entityNameToCheck, "^" + Regex.Escape(excludeListItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                            if (isExcluded) break;
                        }
                    }
                }
            }
            return ((!isIncluded) || (isExcluded && isIncluded));
        }

        public bool IsIgnoredEntity(string entityNameToCheck)
        {
            var schemaObjectName = new SchemaObjectName(entityNameToCheck);
            var ignoreEntity = false;
            var configEntityFound = Entities.Find(e => e.Name == entityNameToCheck);
            if (configEntityFound==null ) configEntityFound = Entities.Find(e => e.Name == schemaObjectName.AsFullName());
            if (configEntityFound != null)
            {
                ignoreEntity = configEntityFound.Ignore;
            }
            if (!ignoreEntity)
            {
                foreach (var entity in this.Entities)
                {
                    if (entity.Name.Contains("dbo.DP.*"))
                        Console.Write("");
                    if (entity.Name.Contains(@"*")) //contains wildcard?
                    {
                        var isMatched = Regex.IsMatch(schemaObjectName.AsFullName(), "^" + Regex.Escape(entity.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                        if (isMatched) ignoreEntity = entity.Ignore;
                        if (ignoreEntity) break;
                    }
                }
            }
            return ignoreEntity;
        }

        /// <summary>
        /// tthis function will check and see if the entity has any Overrides.PrimaryKey declarations.
        /// </summary>
        /// <param name="entityNameToCheck">Can be a wild card to search for enntity names.  Entity names will include schemas</param>
        /// <returns></returns>
        public bool IsEntityWithConfigKeyDeclaration(string entityNameToCheck)
        {
            var schemaObjectName = new SchemaObjectName(entityNameToCheck);
            var hasConfigPrimaryKeyDeclarations = false;
            var configEntityFound = Entities.Find(e => e.Name == entityNameToCheck);
            if (configEntityFound == null) configEntityFound = Entities.Find(e => e.Name == schemaObjectName.AsFullName());
                foreach (var entity in this.Entities)
                {
                    if ((entity.Name.Contains("ScenarioScheduleGroupings")) || (entity.Name.Contains("ScenarioDatesByWell")))
                        Console.Write("");
                    var isMatched = Regex.IsMatch(schemaObjectName.AsFullName(), "^" + Regex.Escape(entity.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                    if (isMatched) hasConfigPrimaryKeyDeclarations = entity.Overrides.PrimaryKey.Count>0;
                    if (hasConfigPrimaryKeyDeclarations) break;
                }
            return hasConfigPrimaryKeyDeclarations;
        }

        /// <summary>
        /// tthis function will check and see if the entity has any Overrides.PrimaryKey declarations.
        /// </summary>
        /// <param name="entityNameToCheck">Can be a wild card to search for enntity names.  Entity names will include schemas</param>
        /// <returns></returns>
        public List<Entity> FindMatchingConfigEntities(string entityNameToCheck)
        {
            var ret = new List<Entity>();
            var schemaObjectName = new SchemaObjectName(entityNameToCheck);
            var hasConfigPrimaryKeyDeclarations = false;
            var configEntityFound = Entities.Find(e => e.Name == entityNameToCheck);
            if (configEntityFound == null) configEntityFound = Entities.Find(e => e.Name == schemaObjectName.AsFullName());
            foreach (var entity in this.Entities)
            {
                var isMatched = Regex.IsMatch(schemaObjectName.AsFullName(), "^" + Regex.Escape(entity.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                if (isMatched) ret.Add(entity);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityNameToCheck">Needs to be the full object name for the entity</param>
        /// <param name="objectNametoCheck"></param>
        /// <returns></returns>
        public bool IsObjectNameFiltered( string entityNameToCheck, string objectNametoCheck)
        {
            var schemaObjectName = new SchemaObjectName(entityNameToCheck);
            var filterObject = false;
            var configEntityFound = Entities.Find(e => e.Name == entityNameToCheck);
            if (configEntityFound == null) configEntityFound = Entities.Find(e => e.Name == schemaObjectName.AsFullName());
            if (configEntityFound != null)
            {
                filterObject = configEntityFound.ObjectFilters.IsInWildcardList(objectNametoCheck);
            }
            if (!filterObject)
            {
                foreach (var entity in this.Entities)
                {
                    if (entity.Name.Contains("dbo.DP.*"))
                        Console.Write("");
                    if (entity.Name.Contains(@"*")) //contains wildcard?
                    {
                        var isMatched = Regex.IsMatch(schemaObjectName.AsFullName(), "^" + Regex.Escape(entity.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                        if (isMatched)
                        {
                            filterObject = entity.ObjectFilters.IsInWildcardList(objectNametoCheck);
                        }
                        if (filterObject) break;
                    }
                }
            }
            return filterObject;
        }

        public bool IsNotMappedColumn(IProperty property)
        {
            if (property == null) return false;
            var parentEntity = property.GetType().GetProperty("ParentEntity")?.GetValue(property) as IEntity;
            if (parentEntity == null) return false;
            var columnAlias = property.GetType().GetProperty("ColumnAlias")?.GetValue(property) as string ?? string.Empty;
            var databaseSchema = parentEntity.GetType().GetProperty("DatabaseSchema")?.GetValue(parentEntity) as string ?? Database.DefaultSchema;
            var tableName = parentEntity.GetType().GetProperty("TableName")?.GetValue(parentEntity) as string ?? string.Empty;
            return (IsNotMappedColumn(databaseSchema, tableName, columnAlias) || 
                    IsNotMappedColumn(databaseSchema, tableName, columnAlias));
        }
        public bool IsNotMappedColumn(string schemaToCheck, string tableCheck, string columnNameToCheck)
        {
            var notMappedColumn = false;
            foreach (var columnNameNotMappedItem in Database.ColumnNameNotMapped)
            {
                var SchemaColumnNameNotMappedItem = "*"; var TableColumnNameNotMappedItem = "*"; var ColumnColumnNameNotMappedItem = "*";
                var arr = columnNameNotMappedItem.Split('.');
                if (arr.Length == 3)
                {
                    SchemaColumnNameNotMappedItem = arr[0];
                    TableColumnNameNotMappedItem = arr[1];
                    ColumnColumnNameNotMappedItem = arr[2];
                }

                if (arr.Length == 2)
                {
                    SchemaColumnNameNotMappedItem = Database.DefaultSchema;
                    TableColumnNameNotMappedItem = arr[0];
                    ColumnColumnNameNotMappedItem = arr[1];
                }
                if (arr.Length == 1) //if there is only one parm, then we are going to assume that it affects all schemas
                {
                    SchemaColumnNameNotMappedItem = "*";
                    ColumnColumnNameNotMappedItem = arr[0];
                }
                //if (columnNameToCheck.Contains(@"*")) 
                //{
                var isSchemaMatched = Regex.IsMatch(schemaToCheck, "^" + Regex.Escape(SchemaColumnNameNotMappedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                var isTableMatched = Regex.IsMatch(tableCheck, "^" + Regex.Escape(TableColumnNameNotMappedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                var isColumnMatched = Regex.IsMatch(columnNameToCheck, "^" + Regex.Escape(ColumnColumnNameNotMappedItem).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                //var isMatched = Regex.IsMatch(columnNameFilterItem, "^" + Regex.Escape(columnNameToCheck).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                if (isSchemaMatched && isTableMatched && isColumnMatched) return true;
                //}
                //else if (columnNameFilterItem.Equals(columnNameToCheck)) return true;
            }
            return notMappedColumn;
        }
    }
}
