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
    public class ProjectPathEditTests
    {
        readonly string OldFormatProjectFileName = @"Json.Comparer.Tests.csprojsample";
        readonly string NewFormatProjectFileName = @"Json.Comparer.csprojsample";
        public ProjectPathEditTests()
        {
            var TempPath = Path.GetTempPath();
            var OldFormatProjectFileNameOriginal = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + OldFormatProjectFileName).ResolvePathVars();
            var NewFormatProjectFileNameOriginal = (@"{ASSEMBLY_PATH}Resources" + Path.DirectorySeparatorChar + NewFormatProjectFileName).ResolvePathVars();

            this.OldFormatProjectFileName = TempPath + Path.DirectorySeparatorChar + this.OldFormatProjectFileName;
            this.NewFormatProjectFileName = TempPath + Path.DirectorySeparatorChar + this.NewFormatProjectFileName;

            if (File.Exists(this.OldFormatProjectFileName)) File.Delete(this.OldFormatProjectFileName);
            if (File.Exists(this.NewFormatProjectFileName)) File.Delete(this.NewFormatProjectFileName);
            File.Copy(OldFormatProjectFileNameOriginal, this.OldFormatProjectFileName);
            File.Copy(NewFormatProjectFileNameOriginal, this.NewFormatProjectFileName);
        }

        [Fact]
        public void PathOffsetTests()
        {
            var projectPath = @"C:\Users\Ricardo Vega\Documents\GitHub\ez-db-codegen-core\Src\EzDbCodeGen.Cli\EzDbCodeGen.Cli.csproj";
            Uri address1 = new Uri(projectPath);
            Uri address2 = new Uri(@"C:\Users\Ricardo Vega\Documents\GitHub\ez-db-codegen-core\Src\EzDbCodeGen.Cli\Templates\");
            var ret = (new Uri(@"C:\Users\Ricardo Vega\Documents\GitHub\ez-db-codegen-core\Src\EzDbCodeGen.Cli\EzDbCodeGen.Cli.csproj"))
                .MakeRelativeUri(new Uri(@"C:\Users\Ricardo Vega\Documents\GitHub\ez-db-codegen-core\Src\EzDbCodeGen.Cli\Templates"))
                .ToString()
                .Replace('/', Path.DirectorySeparatorChar);
            Assert.True(ret.Equals("C:\\Users\\Ricardo Vega\\AppData\\Local\\Temp\\..\\..\\"), string.Format("Directory of {0} was not expected", ret));

            var str = @"C:\Users\Ricardo Vega\AppData\";
            var ret2 = (str.PathEnds() + address2.MakeRelativeUri(address1).ToString().Replace('/', Path.DirectorySeparatorChar));
            Assert.True(ret2.Equals("C:\\Users\\Ricardo Vega\\AppData\\Local\\Temp\\..\\..\\"), string.Format("Directory of {0} was not expected", ret2));
        }

        [Fact]
        public void ChangeIncludedFilesTests()
        {

            var par = new ProjectHelpers();
            var retDA = par.ModifyClassPath(this.OldFormatProjectFileName, @"Controllers\Generated\*.cs");
            var oldtext = File.ReadAllText(this.OldFormatProjectFileName);
            Assert.False(oldtext.Contains(@"Controllers\Generated\GenericController.cs"), "GenericController should have been removed");
            Assert.True(oldtext.Contains(@"Controllers\Generated\*.cs"), "Wild card should be present");
            var retDA2 = par.ModifyClassPath(this.NewFormatProjectFileName, @"Controllers\Generated\*.cs");
            var newtext = File.ReadAllText(this.NewFormatProjectFileName);
            Assert.False(newtext.Contains(@"Controllers\Generated\*.cs"), "Wild card should not be present");

            var retDA3 = par.ModifyClassPath(this.OldFormatProjectFileName, @"TestObjects\SimpleObjectWithExtraProperty.cs");
            var newtext3 = File.ReadAllText(this.OldFormatProjectFileName);
            Assert.True(newtext3.Contains(@"TestObjects\SimpleObjectWithExtraProperty.cs"), "Class name is not present");
            Assert.False(retDA3, "Changes to the file should not have been made");

            var retDA4 = par.ModifyClassPath(this.OldFormatProjectFileName, @"TestObjects\SimpleObjectWithExtraPropertyADDED.cs");
            var newtext4 = File.ReadAllText(this.OldFormatProjectFileName);
            Assert.True(newtext4.Contains(@"TestObjects\SimpleObjectWithExtraPropertyADDED.cs"), "Class name is not present");
            Assert.True(retDA4, "Changes to the file should have been made");

        }
    }
}