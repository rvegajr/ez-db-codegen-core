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
    [Collection("DatabaseCollection")]
    public class EntityTests : TestBase
    {

        DatabaseFixture fixture;
        public EntityTests(DatabaseFixture _fixture) : base()
        {
            this.fixture = _fixture;
        }

        [Fact]
        public void ConfigDb()
        {
            ITemplateDataInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
			var database = template.LoadSchema(AWLT2008Configuration);
            Assert.True(database.Entities.Count > 0, "There should be entities in the TemplateInputDatabaseConnecton");

        }


        [Fact]
        public void CompareObjectTestsAreEqual()
        {
            var db1 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
            var SchemaFileName = this.CreateTempFile();
            db1.ToJsonFile(SchemaFileName);
            var db2 = EzDbSchema.MsSql.Database.FromJsonFile(SchemaFileName);
            var list = db1.CompareTo(db2);
            Assert.True(list.Count==0, "Both schemas should equal");
        }

        [Fact]
        public void CompareObjectTestsAreNotEqual()
        {
            var db1 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
            var SchemaFileName = this.CreateTempFile();
            db1.ToJsonFile(SchemaFileName);
            var db2 = EzDbSchema.MsSql.Database.FromJsonFile(SchemaFileName);
            var Entity = db2.Entities[db2.Keys[0]];
            Entity.Name = Entity.Name + "_CHANGED";
            var list = db1.CompareTo(db2);
            Assert.True(list.Count > 0, "Both schemas should not equal");
        }

        [Fact]
        public void AliasPatternTests()
        {
            var cfg = AWLT2008Configuration;
            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + Configuration.OBJECT_NAME;
            var db1 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(cfg);
            var e1 = db1.FindEntity("SalesLT.Address");
            Assert.True((e1!=null), "Should find entity SalesLT.Address");
            Assert.True((e1.Alias.Equals("SalesLTAddress")), "Alias of entity should equal SalesLTAddress");

            cfg.Database.AliasNamePattern = Configuration.SCHEMA_NAME + "____" + Configuration.OBJECT_NAME;
            var db2 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(cfg);
            var e2 = db2.FindEntity("SalesLT.Address");
            Assert.True((e2 != null), "Should find entity SalesLT.Address");
            Assert.True((e2.Alias.Equals("SalesLT____Address")), "Alias of entity should equal SalesLT____Address");
        }

        [Fact]
        public void PrimaryKeyReasignTests()
        {
            var c = new Core.Config.Entity
            {
                Name = "SalesLT.Address"
            };
            c.AddPKOverride("AddressLine1");
            c.AddPKOverride("AddressLine2");
            AWLT2008Configuration.Entities.Add(c);

            var db1 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
            var lst = db1.FindEntities("SalesLT.Address");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'SalesLT.Address'");

            Assert.True((lst[0].PrimaryKeys[0].Name.Equals("AddressLine1")), "First Primary Key Name should be 'AddressLine1'");
            Assert.True((lst[0].PrimaryKeys[1].Name.Equals("AddressLine2")), "First Primary Key Name should be 'AddressLine2'");

            c.ClearPKOverrides();
            c.AddPKOverride("AddressID");
            c.AddPKOverride("AddressLine1");
            c.AddPKOverride("AddressLine2");

            var db2 = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
            var lst2 = db2.FindEntities("SalesLT.Address");
            Assert.True(lst.Count == 1, "Should be 1 entity that match the pattern 'SalesLT.Address'");

            Assert.True((lst2[0].PrimaryKeys[0].Name.Equals("AddressID")), "First Primary Key Name should be 'AddressID'");
            Assert.True((lst2[0].PrimaryKeys[1].Name.Equals("AddressLine1")), "First Primary Key Name should be 'AddressLine1'");
            Assert.True((lst2[0].PrimaryKeys[2].Name.Equals("AddressLine2")), "First Primary Key Name should be 'AddressLine2'");


        }


    }
}
