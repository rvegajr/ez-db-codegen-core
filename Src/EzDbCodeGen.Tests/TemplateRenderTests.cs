using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Tests
{
    public class TemplateRenderTests : IClassFixture<DatabaseFixture>
    {
        readonly string SchemaFileName = "";
        DatabaseFixture fixture;
        public TemplateRenderTests(DatabaseFixture fixture)
        {
            this.fixture = fixture;
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }

        [Fact]
        public void RenderTemplateFileTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator(Internal.AppSettings.Instance.ConfigurationFileName);
                ITemplateInput inputSource = new TemplateInputFileSource(SchemaFileName);
                var database = inputSource.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), inputSource, OutputPath);
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
                var codeGenerator = new CodeGenerator(Internal.AppSettings.Instance.ConfigurationFileName);
                ITemplateInput inputSource = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                var database = inputSource.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                //database.ToJsonFile(@"C:\Dev\Noctusoft\ez-db-codegen-core\Src\EzDbCodeGen.Tests\Resources\MySchemaName.db.json");
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), inputSource, OutputPath);
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
                var codeGenerator = new CodeGenerator(Internal.AppSettings.Instance.ConfigurationFileName);
                ITemplateInput inputSource = new TemplateInputDatabaseConnecton(fixture.ConnectionString);
                var database = inputSource.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRender.hbs").ResolvePathVars(), inputSource, OutputPath);
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
                var codeGenerator = new CodeGenerator(Internal.AppSettings.Instance.ConfigurationFileName);
                ITemplateInput inputSource = new TemplateInputFileSource(SchemaFileName);
                var database = inputSource.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                var OutputPath = System.IO.Path.GetTempPath() + "EzDbCodeGenTest" + Path.DirectorySeparatorChar;
                if (Directory.Exists(OutputPath)) Directory.Delete(OutputPath, true);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRenderAsFiles.hbs").ResolvePathVars(), inputSource, OutputPath);
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
                var codeGenerator = new CodeGenerator(Internal.AppSettings.Instance.ConfigurationFileName);
                ITemplateInput inputSource = new TemplateInputFileSource(SchemaFileName);
                var database = inputSource.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRenderAsFiles.hbs").ResolvePathVars(), inputSource);
                Assert.True(Directory.Exists(codeGenerator.OutputPath), string.Format("Template Rendered Output files in path {0} was not created", codeGenerator.OutputPath));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}
