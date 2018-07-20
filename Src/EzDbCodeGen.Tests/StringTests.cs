using System;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Internal;
using System.IO;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Config;

namespace EzDbCodeGen.Tests
{
    public class StringTests
    {
        string SchemaFileName = "";
        public StringTests()
        {
            this.SchemaFileName = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"MySchemaName.db.json").ResolvePathVars();
        }

        internal void CaseTestPluralize(string singular, string plural)
        {
            Assert.True(singular.ToPlural().Equals(plural), string.Format("{0} plural should be {1}, it was {2}", singular, plural, singular.ToPlural()));
        }

        internal void CaseTestSingularize(string plural, string singular)
        {
            Assert.True(plural.ToSingular().Equals(singular), string.Format("{0} singular should be {1}, it was {2}", plural, singular, plural.ToSingular()));
        }
        [Fact]
        public void CaseChangeTests()
        {

            var EzDbConfig = AppSettings.Instance.ConfigurationFileName;

            if (File.Exists(EzDbConfig))
            {
                var ezDbConfig = Core.Config.Configuration.FromFile(EzDbConfig);
                foreach (var item in ezDbConfig.PluralizerCrossReference)
                {
                    Pluralizer.Instance.AddWord(item.SingleWord, item.PluralWord);
                }
            }

            CaseTestPluralize("ProductCurve", "ProductCurves");
            CaseTestSingularize("ProductCurves", "ProductCurve");
            var str = "PrioritizationCriterion".ToSingular();
            var strPlural = "PrioritizationCriterion".ToPlural();
            CaseTestSingularize("ConstructionStatus", "ConstructionStatus");

            var pl = new Pluralizer();
            Assert.True(pl.IsPlural("constructionStatuses"), "constructionStatuses should be Plural");
            Assert.True(pl.IsPlural("OpStatuses"), "OpStatuses should be Plural");
            Assert.True(pl.IsPlural("ScenarioCases"), "ScenarioCases should be Plural");
            Assert.True(pl.IsPlural("IncreaseDecreases"), "IncreaseDecreases should be Plural");
            Assert.False(pl.IsPlural("Virii"), "Virii should not be Plural");
            Assert.False(pl.IsPlural("Car"), "Car should not be Plural");
            Assert.True(pl.IsSingular("IncreaseDecrease"), "IncreaseDecrease should be Singular");
            Assert.True(pl.IsSingular("lease"), "lease should be Singular");
            Assert.True(pl.IsSingular("ScenarioCase"), "ScenarioCase should be Singular");
            Assert.True(pl.IsSingular("OpStatus"), "OpStatus should be Singular");
            Assert.True(pl.IsSingular("ConstructionStatus"), "ConstructionStatus should be Singular");
            Assert.True(pl.IsSingular("Virus"), "Virus should be Singular");
            Assert.False(pl.IsSingular("ScenarioCases"), "Virus should be not Singular");

            Assert.True(!pl.Pluralize("Virus").Equals("Viruses"), "Virus plural should not be Viruses");
            Assert.True(pl.Singularize("Viruses").Equals("Virus"), "Viruses singular should be Virus");

        }

        [Fact]
        public void PluckTest()
        {
            var s = "This is a long string to test";
            var sRest = "";
            var newStr = s.Pluck("a ", " string", out sRest);
            Assert.True(newStr.Equals("long"), "newStr should be long");
            Assert.True(sRest.Equals("This is a  string to test"), "sRest should be the rest of the string");
            var s2 = @"<FILE>TestfileName.xml</FILE>
<ENTITY_KEY>FancyKey</ENTITY_KEY>
Rest of the code
";
            var sRestTest = @"<FILE>TestfileName.xml</FILE>
<ENTITY_KEY></ENTITY_KEY>
Rest of the code
";
            var sRest2 = "";
            var newStr2 = s2.Pluck("<ENTITY_KEY>", "</ENTITY_KEY>", out sRest2);
            Assert.True(newStr2.Equals("FancyKey"), "newStr2 should be FancyKey");
            Assert.True(sRest2.Equals(sRestTest), "sRest2 should be the rest of the string");
        }

        [Fact]
        public void ConfigReplaceTests()
        {
            SchemaObjectName so = new SchemaObjectName("Customer.tbl_Addresses");
            var AliasName1 = Configuration.ReplaceEx("{SCHEMANAME-L}", so);
            Assert.True(AliasName1.Equals("customer"), "Customer should be all lower case");
            var AliasName2 = Configuration.ReplaceEx("{SCHEMANAME-U}", so);
            Assert.True(AliasName2.Equals("CUSTOMER"), "Customer should be all lower case");
            var AliasName3 = Configuration.ReplaceEx("{OBJECTNAME-L}", so);
            Assert.True(AliasName3.Equals("tbl_addresses"), "tbl_Addresses should be all lower case");
            var AliasName4 = Configuration.ReplaceEx("{OBJECTNAME-U}", so);
            Assert.True(AliasName4.Equals("TBL_ADDRESSES"), "tbl_Addresses should be all lower case");
            var AliasName5 = Configuration.ReplaceEx("{SCHEMANAME-L}{OBJECTNAME-U}", so);
            Assert.True(AliasName5.Equals("customerTBL_ADDRESSES"), "customer should be all lower and tbl_Addresses should be all upper case");
            var AliasName6 = Configuration.ReplaceEx("{SCHEMANAME-P}--{OBJECTNAME-P|X'Tbl_'}", so);
            Assert.True(AliasName6.Equals("Customer--Addresses"), "customer should be all lower and tbl_Addresses should be all upper case");
            var AliasName7 = Configuration.ReplaceEx("{SCHEMANAME-P}{OBJECTNAME-P|R'Tbl_'=>'XXX'}", so);
            Assert.True(AliasName7.Equals("CustomerXXXAddresses"), "customer should be all lower and tbl_Addresses should be all upper case");
        }

        [Fact]
        public void WlldCardSearchTest()
        {
            var db1 = new TemplateInputFileSource(SchemaFileName).LoadSchema();
            var lst = db1.FindEntities("Integration.Trans*");
            Assert.True(lst.Count==2, "Should be 2 entities that match the pattern 'Integration.Trans*'");
            var lst2 = db1.FindEntities("Dimension.*");
            Assert.True(lst2.Count == 8, "Should be 8 entities that match the pattern 'Dimension.*'");
            var lst3 = db1.FindEntities("*Stock*");
            Assert.True(lst3.Count == 4, "Should be 4 entities that match the pattern '*Stock*'");
        }

    }
}