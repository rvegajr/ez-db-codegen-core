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
    public class SchemaRenderTests : IClassFixture<DatabaseFixture>
    {
        string SchemaFileName = "";
        DatabaseFixture fixture;
        public SchemaRenderTests(DatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }

        [Fact]
        public void RenderDatbaseConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                var database = template.LoadSchema(Internal.AppSettings.Instance.Configuration);
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.TemplateFileNameFilter = "SchemaRenderAsFiles*";
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"").ResolvePathVars(), template, OutputPath);
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
                ITemplateInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema(Internal.AppSettings.Instance.Configuration);
                var str = ((EntityDictionary)database.Entities).ObjectPropertyAsString("#Name");
                var rel = (RelationshipList)database.Entities["SalesLT.Customer"].RelationshipGroups["FK_CustomerAddress_Customer_CustomerID"];
                Assert.True(rel.Count>0, string.Format("Entity {0} should havd a relationship of {1} with a count greater than 0", "SalesLT.Customer", "FK_CustomerAddress_Customer_CustomerID"));

                var relStr = rel.ObjectPropertyAsString("");

                var st2 = ((PrimaryKeyProperties)database.Entities["SalesLT.Customer"].PrimaryKeys).ObjectPropertyAsString(">Name");
                Assert.True(st2.Equals("CustomerID"), string.Format("Entity {0} should have a primary key list of value 'CustomerID'", "SalesLT.Customer"));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

        }
    }
}
