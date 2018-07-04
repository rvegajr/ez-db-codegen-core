using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Internal;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Compare;
namespace EzDbCodeGen.Tests
{
    public class EntityTests 
    {
        string SchemaFileName = "";
        public EntityTests()
        {
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }
        [Fact]
        public void ConfigDb()
        {
			ITemplateInput template = new TemplateInputFileSource(SchemaFileName);
			var database = template.LoadSchema().Filter();
        }


        [Fact]
        public void CompareObjectTestsAreEqual()
        {
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var list = db1.CompareTo(db2);
            Assert.True(list.Count==0, "Both schemas should equal");
        }

        [Fact]
        public void CompareObjectTestsAreNotEqual()
        {
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var Entity = db2.Entities[db2.Keys[0]];
            Entity.Name = Entity.Name + "_CHANGED";
            var list = db1.CompareTo(db2);
            Assert.True(list.Count > 0, "Both schemas should not equal");
        }

        [Fact]
        public void AliasPatternTests()
        {
            var cfg = Configuration.Instance;
            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + Configuration.OBJECT_NAME;
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var e1 = db1.FindEntity("Dimension.City");
            Assert.True((e1!=null), "Should find entity Dimension.City");
            Assert.True((e1.Alias.Equals("DimensionCity")), "Alias of entity should equal DimensionCity");

            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + "____" + Configuration.OBJECT_NAME;
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var e2 = db2.FindEntity("Dimension.City");
            Assert.True((e2 != null), "Should find entity Dimension.City");
            Assert.True((e2.Alias.Equals("Dimension____City")), "Alias of entity should equal Dimension____City");
        }

        [Fact]
        public void PrimaryKeyReasignTests()
        {
            var c = new Core.Config.Entity();
            c.Name = "Dimension.City";
            c.AddPKOverride("City Key");
            c.AddPKOverride("WWI City ID");
            Configuration.Instance.Entities.Add(c);

            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var lst = db1.FindEntities("Dimension.City");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'Dimension.City'");

            Assert.True((lst[0].PrimaryKeys[0].Name.Equals("City Key")), "First Primary Key Name should be 'City Key'");
            Assert.True((lst[0].PrimaryKeys[1].Name.Equals("WWI City ID")), "First Primary Key Name should be 'WWI City ID'");

            c.ClearPKOverrides();
            c.AddPKOverride("Region");
            c.AddPKOverride("WWI City ID");
            c.AddPKOverride("City Key");

            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var lst2 = db2.FindEntities("Dimension.City");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'Dimension.City'");

            Assert.True((lst2[0].PrimaryKeys[0].Name.Equals("Region")), "First Primary Key Name should be 'Region'");
            Assert.True((lst2[0].PrimaryKeys[1].Name.Equals("WWI City ID")), "First Primary Key Name should be 'WWI City ID'");
            Assert.True((lst2[0].PrimaryKeys[2].Name.Equals("City Key")), "First Primary Key Name should be 'City Key'");


        }


    }
}
