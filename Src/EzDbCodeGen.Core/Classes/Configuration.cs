using EzDbCodeGen.Core.Extentions.Strings;
using EzDbSchema.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EzDbCodeGen.Core.Config
{
    public class PrimaryKey
    {
        public string FieldName { get; set; }
    }

    public class Overrides
    {
        public List<PrimaryKey> PrimaryKey { get; set; } = new List<PrimaryKey>();
    }

    public class Entity
    {
        public string Name { get; set; } = "";
        public bool Ignore { get; set; } = false;
        public string AliasRenameTo { get; set; } = "";
        public Overrides Overrides { get; set; } = new Overrides();
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

    public class PluralSingle
    {
        public string SingleWord { get; set; } = "";
        public string PluralWord { get; set; } = "";
    }

    public class Database
    {
        public string DefaultSchema { get; set; } = "dbo";
        public string AliasNamePattern { get; set; } = Configuration.OBJECT_NAME;
        public string SchemaName { get; set; } = "";
        public string PropertyObjectNameCollisionSuffix { get; set; } = "Value";
        public string InverseFKTargetNameCollisionSuffix { get; set; } = "Item";
        public Dictionary<object, object> Misc { get; set; } = new Dictionary<object, object>();

        public string[] ColumnNameFilters = (new List<string>()).ToArray();

    }

    public class Configuration
    {
        /// <summary>
        /// A Replacement that is geared to specific templates and some simple string maniplation capabilities
        /// 
        ///  You can use the following variables here {SCHEMANAME}, {OBJECTNAME} with replace patterns after the instructions, for example:
        /// Lets say the Schema name is "CUSTOMER" and the Object name is "tbl_Address" 
        ///    You have the following one letter codes to modify the string after the name seperated by "|"
        ///        U=Upper Case, L = Lower Case, P=Proper Case, X'<String to remove>'= Clear String, R'Old string'=>'New String'
        ///  After Filtering, the following patterns will yield that following names:
        /// "{SCHEMANAME}{OBJECTNAME}" = "CUSTOMERtbl_Address"
        /// "{SCHEMANAME}{OBJECTNAME-U}" = "CUSTOMERTBL_ADDRESS"
        /// "{SCHEMANAME-L}{OBJECTNAME-L|X'tbl_'}" = "customeraddress"
        /// "{SCHEMANAME-P}{OBJECTNAME-P|X'tbl_'}" ="CustomerAddress"
        /// </summary>
        /// <param name="TemplatePattern">The template pattern which can be {##-U|L|P|X''|R''=>''} where ## can be SCHEMANAME or OBJECTNAME</param>
        /// <param name="schemaObjectName">Name of the SchemaObjectName that will rename the string.</param>
        /// <returns>The String replace with formatted</returns>
        public static string ReplaceEx(string TemplatePattern, SchemaObjectName schemaObjectName)
        {
            var returnString = TemplatePattern;
            var TemplatePatternArr = TemplatePattern.Split(new string[] { "}" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < TemplatePatternArr.Count(); i++)
            {
                TemplatePatternArr[i] = TemplatePatternArr[i] + "}";
            }
            foreach (var _template in TemplatePatternArr)
            {
                var template = _template;
                if (!template.StartsWith("{"))  //Looks like there is text between the next template token,  lets just grab the token
                {
                    template = "{" + template.Pluck("{", "}") + "}";
                } 
                var strResolved = "";
                if (template.Contains(Configuration.SCHEMA_NAME.Substring(0, Configuration.SCHEMA_NAME.Length-1)))
                {
                    strResolved = ReplaceEx(template, schemaObjectName.SchemaName);
                    returnString = returnString.Replace(template, strResolved);
                } else if (template.Contains(Configuration.OBJECT_NAME.Substring(0, Configuration.OBJECT_NAME.Length - 1)))
                {
                    strResolved = ReplaceEx(template, schemaObjectName.TableName);
                    returnString = returnString.Replace(template, strResolved);
                }
            }
            return returnString;
        }

        /// <summary>
        /// A Replacement that is geared to specific templates and some simple string maniplation capabilities
        /// 
        ///  You can use the following variables here {SCHEMANAME}, {OBJECTNAME} with replace patterns after the instructions, for example:
        /// Lets say the Schema name is "CUSTOMER" and the Object name is "tbl_Address" 
        ///    You have the following one letter codes to modify the string after the name seperated by "|"
        ///        U=Upper Case, L = Lower Case, P=Proper Case, X'<String to remove>'= Clear String, R'Old string'=>'New String'
        ///  After Filtering, the following patterns will yield that following names:
        /// "{SCHEMANAME}{OBJECTNAME}" = "CUSTOMERtbl_Address"
        /// "{SCHEMANAME}{OBJECTNAME-U}" = "CUSTOMERTBL_ADDRESS"
        /// "{SCHEMANAME-L}{OBJECTNAME-L|X'tbl_'}" = "customeraddress"
        /// "{SCHEMANAME-P}{OBJECTNAME-P|X'tbl_'}" ="CustomerAddress"
        /// </summary>
        /// <param name="TemplatePattern">The template pattern which can be {##-U|L|P|X''|R''=>''} where ## can be SCHEMANAME or OBJECTNAME</param>
        /// <param name="StringWithPatternToReplace">The string that will replace the patter when found</param>
        /// <returns>The String replace with formatted</returns>
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
                if (operandList.EndsWith("}")) operandList = operandList.Substring(0, operandList.Length - 1);
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
                        returnString = returnString.ToSingular();
                    }
                    else if (operand.StartsWith(Configuration.OP_PLURALIZE))
                    {
                        returnString = returnString.ToPlural();
                    }
                    else if (operand.StartsWith(Configuration.OP_TITLE_CASE))
                    {
                        returnString = returnString.ToTitleCase();
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REMOVE))
                    {
                        var StrToReplace = "";
                        if (!operand.StartsWith(Configuration.OP_STRING_REMOVE + "'")) throw new Exception("OP_STRING_REMOVE should have X'????' where X is immediately followed by a single quote and closed by a another single quote");
                        StrToReplace = operand.Substring(2);
                        if (operand.EndsWith("'")) StrToReplace = StrToReplace.Substring(0, StrToReplace.Length - 1);
                        returnString = Regex.Replace(returnString, StrToReplace, "", RegexOptions.IgnoreCase);
                    }
                    else if (operand.StartsWith(Configuration.OP_STRING_REPLACE))
                    {
                        var operandArr = operand.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                        operandArr[0] = operandArr[0].Substring(1);
                        returnString = Regex.Replace(returnString, operandArr[0].Unquote(), operandArr[1].Unquote(), RegexOptions.IgnoreCase);
                    }
                }
            }
            return returnString;
        }
        public const string SCHEMA_NAME = "{SCHEMANAME}";
        public const string OBJECT_NAME = "{OBJECTNAME}";
        public const string OP_UPPER_CASE = "U";
        public const string OP_LOWER_CASE = "L";
        public const string OP_PROPER_CASE = "P";
        public const string OP_TITLE_CASE = "T";
        public const string OP_STRING_REMOVE = "X"; // X'<String to remove>'
        public const string OP_STRING_REPLACE = "R"; // R'Old string'=>'New String'
        public const string OP_SINGULARIZE = "S";
        public const string OP_PLURALIZE = "M";
        public static Configuration ReloadInstance(string FileName)
        {
            _sourceFileName = FileName;
            instance = Configuration.FromFile(_sourceFileName);
            return instance;
        }
        /// <summary>
        /// Gets or sets the name of the source file.  This will also cause the reload of the config file
        /// </summary>
        /// <value>
        /// The name of the source file.
        /// </value>
        public static string SourceFileName { 
            get {
                if (string.IsNullOrEmpty(_sourceFileName)) _sourceFileName = "{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars();
                return _sourceFileName;
            }
            set
            {
                _sourceFileName = value;
                Configuration.ReloadInstance(_sourceFileName);
            }
        }
        private static string _sourceFileName = "";
        private static Configuration instance;
        private Configuration()
        {
        }
        public static Configuration Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        instance = Configuration.FromFile(SourceFileName);
                    }
                    catch (System.Exception ex)
                    {
                        throw new Exception(string.Format("Error while parsing {0}. {1}", SourceFileName, ex.Message), ex);
                    }
                }
                return instance;
            }
        }
        public List<Entity> Entities { get; set; } = new List<Entity>();
        public List<PluralSingle> PluralizerCrossReference { get; set; } = new List<PluralSingle>();
        public Database Database = new Database();
        public static Configuration FromFile(string FileName)
        {
            var ret = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(FileName));
            foreach(var e in ret.Entities)
            {
                if (e.Misc.ContainsKey("PrimaryKey"))
                {
                    var arrPK = e.Misc["PrimaryKey"].ToString().Split(',');
                    //Overrides.PrimaryKey overrides Misc,  but if it doesn't exist,  we will use the Misc Primary key list
                    if (e.Overrides.PrimaryKey.Count==0)
                    {
                        foreach(var newPk in arrPK)
                        {
                            e.AddPKOverride(newPk);
                        }
                    }
                }
            }
            if (ret.Database.AliasNamePattern.Length == 0) ret.Database.AliasNamePattern = Configuration.OBJECT_NAME;
            return ret;
        }

        public bool IsIgnoredColumn(IProperty property)
        {
            return (IsIgnoredColumn(property.Parent.Schema, property.Parent.Name, property.Alias) || IsIgnoredColumn(property.Parent.Schema, property.Parent.Name, property.Alias));
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
    }
}
