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

    public class SchemaRenderTests 
    {

        DatabaseFixture fixture;
        string SchemaFileName = "";
        string ConfigFileName = "";
        string TemplatePath = "";
        public SchemaRenderTests(DatabaseFixture _fixture)
        {
            this.fixture = _fixture;
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
            this.ConfigFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"AWLT2008.config.json").ResolvePathVars();
            this.TemplatePath = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"Templates" + Path.DirectorySeparatorChar + @"").ResolvePathVars();
        }

        [Fact]
        public void RenderDatbaseConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var ConfigFile = EzDbCodeGen.Core.Config.Configuration.FromFile(this.ConfigFileName);

                var database = template.LoadSchema(ConfigFile);
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
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=AdventureWorksDW2017;user id=sa;password=sa");
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema(Internal.AppSettings.Instance.Configuration);
                var str = ((EntityDictionary)database.Entities).ObjectPropertyAsString("#Name");
                var rel = (RelationshipList)database.Entities["dbo.FactSurveyResponse"].RelationshipGroups["FK_FactSurveyResponse_CustomerKey"];
                var relStr = rel.ObjectPropertyAsString("");

                var st2 = ((PrimaryKeyProperties)database.Entities["dbo.FactAdditionalInternationalProductDescription"].PrimaryKeys).ObjectPropertyAsString(">Name");

                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                //var str = database.Entities[""];


                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

        }
    }
}
