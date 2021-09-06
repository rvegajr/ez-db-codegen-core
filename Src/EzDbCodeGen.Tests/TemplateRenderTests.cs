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
                if (File.Exists(SchemaFileName)) File.Delete(SchemaFileName);
                var codeGenerator = new CodeGenerator();
                var database = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
                database.ToJsonFile(SchemaFileName);
                Assert.True(File.Exists(SchemaFileName), string.Format("Database schema file {0} was not created.", SchemaFileName));

                ITemplateDataInput template = new TemplateInputFileSource(SchemaFileName);
                var database2 = template.LoadSchema(Configuration.FromFile($"{this.ResourcesPath}ezdbcodegen.config.json"));
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate(($"{this.TemplatePath}SchemaRender.hbs"), template, OutputPath);
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
                    .ProcessTemplate($"{this.TemplatePath}SchemaRender.hbs");

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
                    .ProcessTemplate($"{this.TemplatePath}SchemaRender.hbs");
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
                    .ProcessTemplate($"{this.TemplatePath}SchemaRenderAsFiles.hbs");
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
                var database = new TemplateInputDatabaseConnecton(fixture.ConnectionString).LoadSchema(AWLT2008Configuration);
                var OutputPath = System.IO.Path.GetTempPath() + "EzDbCodeGenTest" + Path.DirectorySeparatorChar;
                var rc = codeGenerator
                    .WithTemplate(new TemplateInputDatabaseConnecton(fixture.ConnectionString))
                    .WithConfiguration(AWLT2008Configuration)
                    .WithOutputPath(OutputPath)
                    .ProcessTemplate($"{this.TemplatePath}SchemaRenderAsFilesNoOutput.hbs");
                Assert.True(Directory.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output files in path {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}
