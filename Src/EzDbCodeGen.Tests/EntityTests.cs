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
    [Collection("DatabaseTest")]
    public class EntityTests
    {
        string SchemaFileName = "";
        DatabaseFixture fixture;
        public EntityTests(DatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }

        [Fact]
        public void ServerConnectionTest()
        {
            Console.WriteLine(string.Format("Database to connect to {0}", fixture.ServerName()));
            Assert.True(fixture.ServerName()!="Unknown", "Database should not be 'Unknown'");
        }

        [Fact]
        public void ConfigDb()
        {

			ITemplateInput template = new TemplateInputFileSource(SchemaFileName);
			var database = template.LoadSchema(Internal.AppSettings.Instance.Configuration);
        }


        [Fact]
        public void CompareObjectTestsAreEqual()
        {
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var list = db1.CompareTo(db2);
            Assert.True(list.Count==0, "Both schemas should equal");
        }

        [Fact]
        public void CompareObjectTestsAreNotEqual()
        {
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var Entity = db2.Entities[db2.Keys[0]];
            Entity.Name = Entity.Name + "_CHANGED";
            var list = db1.CompareTo(db2);
            Assert.True(list.Count > 0, "Both schemas should not equal");
        }

        [Fact]
        public void AliasPatternTests()
        {
            var cfg = Internal.AppSettings.Instance.Configuration;
            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + Configuration.OBJECT_NAME;
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema(cfg);
            var e1 = db1.FindEntity("SalesLT.Product");
            Assert.True((e1!=null), "Should find entity SalesLT.Product");
            Assert.True((e1.Alias.Equals("SalesLTProduct")), "Alias of entity should equal SalesLTProduct");

            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + "____" + Configuration.OBJECT_NAME;
            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema(cfg);
            var e2 = db2.FindEntity("SalesLT.Product");
            Assert.True((e2 != null), "Should find entity SalesLT.Product");
            Assert.True((e2.Alias.Equals("SalesLT____Product")), "Alias of entity should equal SalesLT____Product");
        }

        [Fact]
        public void PrimaryKeyReasignTests()
        {
            var c = new Core.Config.Entity
            {
                Name = "SalesLT.Product"
            };
            c.AddPKOverride("Name");
            c.AddPKOverride("ProductNumber");
            Internal.AppSettings.Instance.Configuration.Entities.Add(c);

            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var lst = db1.FindEntities("SalesLT.Product");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'SalesLT.Product'");

            Assert.True((lst[0].PrimaryKeys[0].Name.Equals("Name")), "First Primary Key Name should be 'Name'");
            Assert.True((lst[0].PrimaryKeys[1].Name.Equals("ProductNumber")), "Second Primary Key Name should be 'ProductNumber'");

            c.ClearPKOverrides();
            c.AddPKOverride("Name");
            c.AddPKOverride("ProductNumber");
            c.AddPKOverride("Size");

            var db2 = new TemplateInputFileSource(SchemaFileName).LoadSchema(Internal.AppSettings.Instance.Configuration);
            var lst2 = db2.FindEntities("SalesLT.Product");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'SalesLT.Product'");

            Assert.True((lst2[0].PrimaryKeys[0].Name.Equals("Name")), "First Primary Key Name should be 'Name'");
            Assert.True((lst2[0].PrimaryKeys[1].Name.Equals("ProductNumber")), "Second Primary Key Name should be 'ProductNumber'");
            Assert.True((lst2[0].PrimaryKeys[2].Name.Equals("Size")), "Third Primary Key Name should be 'Size'");


        }


    }
}
