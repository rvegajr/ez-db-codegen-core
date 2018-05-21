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
        public string Name { get; set; }
        public bool Ignore { get; set; } = false;
        public Overrides Overrides { get; set; } = new Overrides();
    }

    public class PluralSingle
    {
        public string SingleWord { get; set; } = "";
        public string PluralWord { get; set; } = "";
    }

    public class CodeGenConfiguration
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();
        public List<PluralSingle> PluralizerCrossReference { get; set; } = new List<PluralSingle>();
        public static CodeGenConfiguration FromFile(string FileName)
        {
            return JsonConvert.DeserializeObject<CodeGenConfiguration>(File.ReadAllText(FileName));
        }
        public bool IsIgnoredEntity(string entityNameToCheck)
        {
            var ignoreEntity = false;
            var configEntityFound = Entities.Find(e => e.Name == entityNameToCheck);
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
                        ignoreEntity = Regex.IsMatch(entityNameToCheck, "^" + Regex.Escape(entity.Name).Replace("\\?", ".").Replace("\\*", ".*") + "$");
                        if (ignoreEntity) break;
                    }
                }
            }
            return ignoreEntity;
        }
    }
}
