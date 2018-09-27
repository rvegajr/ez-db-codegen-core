using System;
using Xunit;
using EzDbCodeGen.Core;
using EzDbCodeGen.Internal;
using System.IO;
using EzDbCodeGen.Core.Extentions.Strings;
using EzDbCodeGen.Core.Config;
using EzDbCodeGen.Core.Classes;

namespace EzDbCodeGen.Tests
{
    public class AppSettingsTests
    {
        readonly string HostSettingsSample = "";
        readonly string WebSettingsSample = "";
        public AppSettingsTests()
        {
            this.HostSettingsSample = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"hostappsettings.json").ResolvePathVars();
            this.WebSettingsSample = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + @"Web.sample.config").ResolvePathVars();
        }

        [Fact]
        public void HostSettingsJsonTests()
        {
            SettingsHelper settings = new SettingsHelper();
            settings.AppSettingsFileName = this.HostSettingsSample;
            var val = settings.FindValue("/root/Environments/LOCAL/Settings/ConnectionString");
            Assert.True(val.Contains("127.0.0.1"), "Could not find loopback address in connection string");
            var val2 = settings.FindValue("/root/Environments/PROD/Settings/ConnectionString");
            Assert.True(val2.Contains("PROD_SQL_SERVER"), "Could not find PROD_SQL_SERVER address in connection string");
        }

        [Fact]
        public void WebSettingsJsonTests()
        {
            SettingsHelper settings = new SettingsHelper();
            settings.AppSettingsFileName = this.WebSettingsSample;
            var val = settings.FindValue("/configuration/connectionStrings/add[@name='DatabaseContext']", "connectionString");
            Assert.True(val.Contains("initial catalog=DATABASENAME"), "Could not find catalog=DATABASENAME address in connection string");
            var val2 = settings.FindValue("/configuration/appSettings/add[@key='UnobtrusiveJavaScriptEnabled']", "value");
            Assert.True(val2.Contains("true"), "Could not true in appSettings var UnobtrusiveJavaScriptEnabled");
        }
    }
}
