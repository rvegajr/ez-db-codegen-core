﻿using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Tests
{
    public class TemplateRenderTests 
    {
        readonly string SchemaFileName = "";
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
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=Northwind;user id=sa;password=sa");
                var database = template.LoadSchema(Configuration.FromFile(@"C:\Temp\ezdbcodegen.config.json"));
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
        public void ConfigReadTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=AdventureWorksDW2017;user id=sa;password=sa");
                var database = template.LoadSchema(Configuration.FromFile(@"C:\Temp\ezdbcodegen.config.json"));
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
                var database = template.LoadSchema(Configuration.FromFile("ezdbcodegen.config.json"));
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
