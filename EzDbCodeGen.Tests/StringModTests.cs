using System;
using System.IO;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Internal;

namespace EzDbCodeGen.Test
{
	public class StringModTests
    {
        internal void CaseTestPluralize(string singular, string plural)
        {
            Assert.True(singular.ToPlural().Equals(plural), string.Format("{0} plural should be {1}, it was {2}", singular, plural, singular.ToPlural()));
        }

        internal void CaseTestSingularize(string plural, string singular)
        {
			Assert.True(plural.ToSingular().Equals(singular), string.Format("{0} singular should be {1}, it was {2}", plural, singular, plural.ToSingular()));
        }

		[Fact]
        public void EntityIgnoreTests()
        {
            Assert.True(File.Exists(AppSettings.Instance.ConfigurationFileName), "Config file should exist at " + AppSettings.Instance.ConfigurationFileName);
            if (File.Exists(AppSettings.Instance.ConfigurationFileName))
            {
				var ezDbConfig = Core.Config.CodeGenConfiguration.FromFile(AppSettings.Instance.ConfigurationFileName);
				Assert.True(ezDbConfig.IsIgnoredEntity("ViewName"), "ViewName should marked as ignore");
				Assert.False(ezDbConfig.IsIgnoredEntity("ViewNames"), "ViewNames should NOT marked as ignore");
				Assert.True(ezDbConfig.IsIgnoredEntity("TableTemporal"), "TableTemporal should marked as ignore");
				Assert.True(ezDbConfig.IsIgnoredEntity("Table_Archive"), "Table_Archive should NOT marked as ignore");
				Assert.True(ezDbConfig.IsIgnoredEntity("TemporalHistoryFor_428378723"), "TemporalHistoryFor_428378723 should marked as ignore");
				Assert.True(ezDbConfig.IsIgnoredEntity("Table_DELETE"), "Table_DELETE should marked as ignore");
            }
        }

		[Fact]
        public void CaseChangeTests()
        {

			var EzDbConfig = AppSettings.Instance.ConfigurationFileName;

            if (File.Exists(EzDbConfig))
            {
                var ezDbConfig = Core.Config.CodeGenConfiguration.FromFile(EzDbConfig);
				foreach (var item in ezDbConfig.PluralizerCrossReference)
                {
                    Pluralizer.Instance.AddWord(item.SingleWord, item.PluralWord);
                }
            }

			CaseTestPluralize("ProductCurve", "ProductCurves");
			CaseTestSingularize("ProductCurves", "ProductCurve");
            var str = "PrioritizationCriterion".ToSafeString().ToSingular();
            var strPlural = "PrioritizationCriterion".ToSafeString().ToPlural();
            CaseTestSingularize("ConstructionStatus", "ConstructionStatus");
                     
            var pl = Pluralizer.Instance;
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

            Assert.True(pl.Pluralize("Virus").Equals("Viruses"), "Virus plural should be Viruses");
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
    }
}
