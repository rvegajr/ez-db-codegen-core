﻿using System;
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
        public void MyConnectionTest()
        {
            try
            {
                var codeGenerator = new CodeGenerator();
                ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=logicbyter.lan;Database=CPPE;user id=localsysadmin;password=localsysadmin");
                var config = Configuration.FromFile(@"C:\Dev\PXD\cem-rest-api\PLEXEzDbCodeGen\EzDbCodeGen\CppeDb.WebApi.config.json");
                //ITemplateInput template = new TemplateInputDatabaseConnecton(@"Server=localhost;Database=WideWorldImportersDW;user id=User;password=Server@Database");
                var database = template.LoadSchema(config);
                var OutputPath = System.IO.Path.GetTempPath() + "MySchemaNameRender.txt";
                if (File.Exists(OutputPath)) File.Delete(OutputPath);
                codeGenerator.ConfigurationFileName = @"C:\Dev\PXD\cem-rest-api\PLEXEzDbCodeGen\EzDbCodeGen\CppeDb.WebApi.config.json";
                codeGenerator.ProcessTemplate((@"C:\Dev\PXD\cem-rest-api\PLEXEzDbCodeGen\EzDbCodeGen\Templates\Ef6ModelsTemplate.hbs").ResolvePathVars(), template, OutputPath);
                Assert.True(true, string.Format("Template Rendered Output file {0} was not created", codeGenerator.OutputPath));
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
