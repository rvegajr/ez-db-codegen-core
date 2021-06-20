using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Tests
{
    [Collection("DatabaseCollection")]

    public class TemplateRenderTests : TestBase
    {

        DatabaseFixture fixture;
        public TemplateRenderTests(DatabaseFixture _fixture) : base()
        {
            this.fixture = _fixture;
        }

        [Fact]
        public void RenderTemplateFileTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateDataInput template = new TemplateInputFileSource(SchemaFileName);
                var database = template.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), template, OutputPath);
                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created.", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void RenderTemplateFileUseCaseTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                var database = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                var rc = codeGenerator
                    .WithTemplate(new TemplateInputDatabaseConnecton(fixture.ConnectionString))
                    .WithConfiguration(AWLT2008Configuration)
                    .WithOutputPath(OutputPath)
                    .ProcessTemplate((@"Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs"));

                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void ConfigReadTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                var database = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                var rc = codeGenerator
                    .WithTemplate(new TemplateInputDatabaseConnecton(fixture.ConnectionString))
                    .WithConfiguration(AWLT2008Configuration)
                    .WithOutputPath(OutputPath)
                    .ProcessTemplate((@"Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs"));
                Assert.True(File.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void RenderTemplateMultipleFilesTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                var database = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
                var OutputPath = System.IO.Path.GetTempPath() + "EzDbCodeGenTest" + Path.DirectorySeparatorChar;
                if (Directory.Exists(OutputPath)) Directory.Delete(OutputPath, true);
                var rc = codeGenerator
                    .WithTemplate(new TemplateInputDatabaseConnecton(fixture.ConnectionString))
                    .WithConfiguration(AWLT2008Configuration)
                    .WithOutputPath(OutputPath)
                    .ProcessTemplate((@"Templates" + Path.DirectorySeparatorChar + @"SchemaRenderAsFiles.hbs"));
                Assert.True(Directory.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output files in path {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }

        [Fact]
        public void RenderTemplateSingleNoOutputPathTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateDataInput template = new TemplateInputFileSource(SchemaFileName);
                var database = template.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRenderAsFiles.hbs").ResolvePathVars(), template);
                Assert.True(Directory.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output files in path {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}
