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
            var val3 = settings.FindValue("/configuration/connectionStrings/add[@name='DatabaseContext']/@connectionString");
            Assert.True(val3.Contains("initial catalog=DATABASENAME"), "Could not find catalog=DATABASENAME address in connection string");
        }

        [Fact]
        public void SettingsResolverTests()
        {
            SettingsHelper settings = new SettingsHelper();
            settings.AppSettingsFileName = this.WebSettingsSample;
            var val = ("@" + WebSettingsSample + ">/configuration/connectionStrings/add[@name='DatabaseContext']/@connectionString").SettingResolution();
            Assert.True(val.Contains("initial catalog=DATABASENAME"), "Could not find catalog=DATABASENAME address in connection string");
            var val2 = ("@" + HostSettingsSample + ">/root/Environments/LOCAL/Settings/ConnectionString").SettingResolution();
            Assert.True(val2.Contains("127.0.0.1"), "Could not find loopback address in connection string");
            var sameTest = HostSettingsSample + ">/root/Environments/LOCAL/Settings/ConnectionString";
            var val3 = sameTest.SettingResolution();
            Assert.True(val3.Equals(sameTest), "String should have not changed");

            //Using Connection string Shortcut Capital "CS" will translate to /configuration/connectionStrings/add[@name='DatabaseContext']/@connectionString
            var val4 = ("@" + WebSettingsSample + ">CS[DatabaseContext]").SettingResolution();
            Assert.True(val4.Contains("initial catalog=DATABASENAME"), "Could not find catalog=DATABASENAME address in connection string");

            /*
             * XML:
             * Using Connection string Shortcut Capital "AS" will translate to /configuration/appSettings/add[@key='DatabaseContext']/@value
             * JSON:
             * Using Connection string Shortcut Capital "AS" will translate to /root/DefaultSettings/Settings/XXXX where XXXX = ConnectionString
             */
            var val5 = ("@" + WebSettingsSample + ">AS[UnobtrusiveJavaScriptEnabled]").SettingResolution();
            Assert.True(val5.Contains("true"), "Could not true in appSettings var UnobtrusiveJavaScriptEnabled");
            var val6 = ("@" + HostSettingsSample + ">AS[ConnectionString]").SettingResolution();
            Assert.True(val6.Contains("127.0.0.1"), "Could not find loopback address in connection string");

        }

    }
}
