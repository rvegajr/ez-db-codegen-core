#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var IncrementMinorVersion = true;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var binDir = Directory("./bin") ;
var thisDir = System.IO.Path.GetFullPath(".") + System.IO.Path.DirectorySeparatorChar;
var publishDir = binDir + System.IO.Path.DirectorySeparatorChar + "publish" + System.IO.Path.DirectorySeparatorChar;
var projectFile = thisDir + "Src/EzDbCodeGen.Core/EzDbCodeGen.Core.csproj";
var cliProjectFile = thisDir + "Src/EzDbCodeGen.Cli/EzDbCodeGen.Cli.csproj";
var solutionFile = thisDir + "Src/ez-db-codegen-core.sln";

public int MAJOR = 0; public int MINOR = 1; public int REVISION = 2; public int BUILD = 3; //Version Segments
var VersionInfoText = System.IO.File.ReadAllText(thisDir + "Src/VersionInfo.cs");
var AssemblyFileVersionAttribute = Pluck(VersionInfoText, "AssemblyFileVersionAttribute(\"", "\")]");
var CurrentAssemblyVersionAttribute = Pluck(VersionInfoText, "System.Reflection.AssemblyVersionAttribute(\"", "\")]");
var AssemblyVersionAttribute = CurrentAssemblyVersionAttribute;
var CurrentNugetVersion = VersionStringParts(AssemblyVersionAttribute, MAJOR, MINOR, REVISION);
var NugetVersion = CurrentNugetVersion;
if (IncrementMinorVersion) {	
	AssemblyVersionAttribute = VersionStringIncrement(CurrentAssemblyVersionAttribute, REVISION);
	NugetVersion = VersionStringParts(AssemblyVersionAttribute, MAJOR, MINOR, REVISION);
	AssemblyFileVersionAttribute = NugetVersion + ".*";
}

Information("	  AssemblyVersionAttribute: {0}... Next: {1}", CurrentAssemblyVersionAttribute, AssemblyVersionAttribute);
Information("        		 Nuget version: {0}... Next: {1}", CurrentNugetVersion, NugetVersion);
Information("AssemblyFileVersionAttribute : {0}", AssemblyFileVersionAttribute);

// Define directories.
var buildDir = Directory("./Src/EzDbCodeGen.Cli/bin") + Directory(configuration);
Console.WriteLine(string.Format("target={0}", target));
Console.WriteLine(string.Format("binDir={0}", binDir));
Console.WriteLine(string.Format("thisDir={0}", thisDir));
Console.WriteLine(string.Format("buildDir={0}", buildDir));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("SetVersion")
.IsDependentOn("Clean")
.Does(() => {
	var VersionData = string.Format(@"using System.Reflection;
[assembly: System.Reflection.AssemblyFileVersionAttribute(""{0}"")]
[assembly: System.Reflection.AssemblyVersionAttribute(""{1}"")]
", AssemblyFileVersionAttribute, AssemblyVersionAttribute);
		System.IO.File.WriteAllText(thisDir + "Src/VersionInfo.cs", VersionData);   
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("SetVersion")
    .Does(() =>
{

	var settings = new NuGetRestoreSettings()
	{
		// VSTS has old version of Nuget.exe and Automapper restore fails because of that
		ToolPath = thisDir + "nuget/nuget.exe",
		Verbosity = NuGetVerbosity.Detailed,
	};
	NuGetRestore(solutionFile, settings);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration));
		
	  MSBuild(cliProjectFile,  settings => settings.SetConfiguration(configuration)
        .UseToolVersion(MSBuildToolVersion.Default)
		.WithTarget("publish")
		.WithProperty("DeployOnBuild", "true")
	  );
    }
    else
    {
      // Use XBuild
      XBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(thisDir + "Src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

Task("NuGet-Pack")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{

   var CodeGenFiles = GetFiles(thisDir + "./**/Cake.*"); 
   var nuGetPackSettings   = new NuGetPackSettings {
		BasePath 				= thisDir,
        Id                      = @"EzDbCodeGen",
        Version                 = NugetVersion,
        Title                   = @"EzDbCodeGen - Easy Database Code Generator",
        Authors                 = new[] {"Ricardo Vega Jr."},
        Owners                  = new[] {"Ricardo Vega Jr."},
        Description             = @"A class library that allows you to point to a database and obtain a schema dump complete with columns, relationships (including fk names and multiplicity).  Some use cases require a schema of a database without the bulk of Entity power tools or Entity Framework.",
        Summary                 = @"A class library that allows you to point to a database and obtain a schema dump in xml format.",
        ProjectUrl              = new Uri(@"https://github.com/rvegajr/ez-db-codegen-core"),
        //IconUrl                 = new Uri(""),
        LicenseUrl              = new Uri(@"https://github.com/rvegajr/ez-db-codegen-core/blob/master/LICENSE"),
        Copyright               = @"Noctusoft 2018",
        ReleaseNotes            = new [] {"JSON Serilization", "XML Serilization", "Issue fixes", "Typos"},
        Tags                    = new [] {"Database ", "Schema"},
        RequireLicenseAcceptance= false,
        Symbols                 = false,
        NoPackageAnalysis       = false,
        OutputDirectory         = thisDir + "artifacts/",
		Properties = new Dictionary<string, string>
		{
			{ @"Configuration", @"Release" }
		},
		Files = new[] {
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/readme.txt", Target = "content" },

			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/net461/EzDbCodeGen.Core.dll", Target = "lib/net461" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/net461/EzDbCodeGen.Core.pdb", Target = "lib/net461" },

			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netstandard2.0/EzDbCodeGen.Core.dll", Target = "lib/netstandard2.0" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netstandard2.0/EzDbCodeGen.Core.pdb", Target = "lib/netstandard2.0" },

			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netcoreapp2.0/EzDbCodeGen.Core.dll", Target = "lib/netcoreapp2.0" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netcoreapp2.0/EzDbCodeGen.Core.pdb", Target = "lib/netcoreapp2.0" },
			
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netcoreapp2.1/EzDbCodeGen.Core.dll", Target = "lib/netcoreapp2.1" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/bin/Release/netcoreapp2.1/EzDbCodeGen.Core.pdb", Target = "lib/netcoreapp2.1" },

			new NuSpecContent { Source = thisDir + @"nuget/init.ps1", Target = "tools" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/ezdbcodegen.ps1", Target = "tools" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/ezdbcodegen.config.json", Target = "content/EzDbCodeGen" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/Templates/SchemaRender.hbs", Target = "content/EzDbCodeGen/Templates" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/Templates/SchemaRenderAsFiles.hbs", Target = "content/EzDbCodeGen/Templates" },
			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Cli/bin/Release/netcoreapp2.1/publish/**.*", Target = "content/EzDbCodeGen/bin" }
		},
		ArgumentCustomization = args => args.Append("")		
    };
    NuGetPack(thisDir + "nuget/EzDbCodeGen.nuspec", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS  - 			new NuSpecContent { Source = thisDir + @"Src/EzDbCodeGen.Core/ezdbcodegen.ps1", Target = "contentFiles" },
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("NuGet-Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

//versionSegments can equal Major, Minor, Revision, Build in format Major.Minor.Revision.Build
public string VersionStringParts(string versionString, params int[] versionSegments) {
	var vArr = versionString.Split('.');
	string newVersion = "";
	foreach ( var versionSegment in versionSegments ) {
		newVersion += (newVersion.Length>0 ? "." : "") + vArr[versionSegment].ToString();
	}
	return newVersion;
}

//segmentToIncrement can equal Major, Minor, Revision, Build in format Major.Minor.Revision.Build
public string VersionStringIncrement(string versionString, int segmentToIncrement) {
	var vArr = versionString.Split('.');
	var valAsStr = vArr[segmentToIncrement];
	int valAsInt = 0;
    int.TryParse(valAsStr, out valAsInt);	
	vArr[segmentToIncrement] = (valAsInt + 1).ToString();
	return String.Join(".", vArr);
}


public string Pluck(string str, string leftString, string rightString)
{
	try
	{
		var lpos = str.LastIndexOf(leftString);
		var rpos = str.IndexOf(rightString, lpos+1);
		if (rpos > 0)
		{
			lpos = str.LastIndexOf(leftString, rpos);
			if ((lpos > 0) && (rpos > lpos))
			{
				return str.Substring(lpos + leftString.Length, (rpos - lpos) - leftString.Length);
			}
		}
	}
	catch (Exception)
	{
		return "";
	}
	return "";
}