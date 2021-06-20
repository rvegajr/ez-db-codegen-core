using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Extentions;
using EzDbSchema.Core.Interfaces;
using EzDbSchema.Core.Objects;

namespace EzDbCodeGen.Tests
{
    [Collection("DatabaseCollection")]

    public class SchemaRenderTests : TestBase
    {

        DatabaseFixture fixture;
        public SchemaRenderTests(DatabaseFixture _fixture) : base()
        {
            this.fixture = _fixture;
        }

        [Fact]
        public void RenderDatbaseConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateDataInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema(EzDbCodeGen.Core.Config.Configuration.FromFile(this.ConfigFileName));
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.TemplateFileNameFilter = "SchemaRenderAsFiles*";
                codeGenerator.ProcessTemplate(this.TemplatePath, template, OutputPath);
                //codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), template, OutputPath);
                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ArrayObjectPropertyAsStringTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateDataInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema(EzDbCodeGen.Core.Config.Configuration.FromFile(this.ConfigFileName));
                var str = ((EntityDictionary)database.Entities).ObjectPropertyAsString("#Name");
                Assert.True(str.Length>0, "Entities Name should be returned");

                var rel = (RelationshipList)database.Entities["SalesLT.Customer"].RelationshipGroups["FK_CustomerAddress_Customer_CustomerID"];
                Assert.True(rel.Count == 1, "Relationship should return 1 item");

                var relStr = rel.ObjectPropertyAsString(">Name");
                Assert.True(relStr.Equals("FK_CustomerAddress_Customer_CustomerID"), "Relationship Name should be 'FK_CustomerAddress_Customer_CustomerID'");

                var st2 = ((PrimaryKeyProperties)database.Entities["SalesLT.Product"].PrimaryKeys).ObjectPropertyAsString(">Name");
                Assert.True(st2.Equals("ProductID"), "SalesLT.Product should have returned ProductID");
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

        }
    }
}
