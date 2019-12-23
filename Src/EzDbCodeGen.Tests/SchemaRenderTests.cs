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
    public class SchemaRenderTests 
    {
        string SchemaFileName = "";
        public SchemaRenderTests()
        {
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }
        [Fact]
        public void RenderDatbaseConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=AdventureWorksDW2017;user id=sa;password=sa");
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema().Filter();
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), template, OutputPath);
                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
            
        }

        [Fact]
        public void MyConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=MARS;user id=sa;password=sa");
                Configuration.FromFile(@"C:\Dev\Summit\MARSApi\Src\MARSApi.Web\EzDbCodeGen\MARSApi.Web.config.json");
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema().Filter();
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ConfigurationFileName = @"C:\Dev\Summit\MARSApi\Src\MARSApi.Web\EzDbCodeGen\MARSApi.Web.config.json";
                codeGenerator.ProcessTemplate((@"C:\Dev\Summit\MARSApi\Src\MARSApi.Web\EzDbCodeGen\Templates\Ef6ModelsTemplateAnnotations.hbs").ResolvePathVars(), template, OutputPath);
                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }

        }
    }
}
