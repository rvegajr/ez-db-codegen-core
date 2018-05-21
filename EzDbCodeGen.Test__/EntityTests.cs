using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Internal;
using EzDbCodeGen.Core.Config;
namespace EzDbCodeGen.Test
{
    public class EntityTests
    {
		[Fact]
        public void ConfigDb(string singular, string plural)
        {
			ITemplateInput template = new TemplateInputFileSource(@"{ASSEMBLY_PATH}Resources/MySchemaName.db.json".ResolvePathVars());
			var configuration = CodeGenConfiguration.FromFile(@"{ASSEMBLY_PATH}ezdbcodegen.config.json".ResolvePathVars());
			var database = template.LoadSchema().Filter(configuration);
            Assert.True(singular.ToPlural().Equals(plural), string.Format("{0} plural should be {1}, it was {2}", singular, plural, singular.ToPlural()));
        }
    }
}
