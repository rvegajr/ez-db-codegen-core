using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Extentions.Strings;
namespace EzDbCodeGen.Tests
{
    public class TemplateRenderTests 
    {
        string SchemaFileName = "";
        public TemplateRenderTests()
        {
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }
        [Fact]
        public void RenderTemplateFileTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputFileSource(SchemaFileName);
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
        public void RenderTemplateMultipleFilesTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputFileSource(SchemaFileName);
                var database = template.LoadSchema().Filter();
                var OutputPath = System.IO.Path.GetTempPath() + "EzDbCodeGenTest" + Path.DirectorySeparatorChar;
                if (Directory.Exists(OutputPath)) Directory.Delete(OutputPath, true);
                codeGenerator.ProcessTemplate((@"{ASSEMBLY_PATH}Templates" + Path.DirectorySeparatorChar + @"SchemaRenderAsFiles.hbs").ResolvePathVars(), template, OutputPath);
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
                ITemplateInput template = new TemplateInputFileSource(SchemaFileName);
                var database = template.LoadSchema().Filter();
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
