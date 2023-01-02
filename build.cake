#tool nuget:?package=vswhere
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var IncrementMinorVersion = false;
var NuGetReleaseNotes = new [] {"Udpated to .net 7.0"};

DirectoryPath vsLatest  = VSWhereLatest();
FilePath msBuildPathX64 = (vsLatest==null)
                            ? null
                            : vsLatest.CombineWithFilePath("./MSBuild/Current/bin/msbuild.exe");
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "net6.0");
var runtime = Argument("runtime", "Portable");

var binDir = Directory("./bin") ;
var thisDir = System.IO.Path.GetFullPath(".") + System.IO.Path.DirectorySeparatorChar;
var coreProjectFile = thisDir + "Src/EzDbCodeGen.Core/EzDbCodeGen.Core.csproj";
var cliProjectFile = thisDir + "Src/EzDbCodeGen.Cli/EzDbCodeGen.Cli.csproj";
var solutionFile = thisDir + "Src/ez-db-codegen-core.sln";

public int MAJOR = 0; public int MINOR = 1; public int REVISION = 2; public int BUILD = 3; //Version Segments
var VersionInfoText = System.IO.File.ReadAllText(thisDir + "Src/VersionInfo.cs");
var AssemblyFileVersionAttribute = Pluck(VersionInfoText, "AssemblyFileVersionAttribute(\"", "\")]");
var CurrentAssemblyVersionAttribute = Pluck(VersionInfoText, "System.Reflection.AssemblyVersionAttribute(\"", "\")]");
var deployPath = thisDir + "artifacts" + System.IO.Path.DirectorySeparatorChar;
var publishDir = deployPath + System.IO.Path.DirectorySeparatorChar + "publish" + System.IO.Path.DirectorySeparatorChar;

var AssemblyVersionAttribute = CurrentAssemblyVersionAttribute;
var CurrentNugetVersion = VersionStringParts(AssemblyVersionAttribute, MAJOR, MINOR, REVISION);
var NugetVersion = CurrentNugetVersion;
if (IncrementMinorVersion) {	
	AssemblyVersionAttribute = VersionStringIncrement(CurrentAssemblyVersionAttribute, REVISION);
	NugetVersion = VersionStringParts(AssemblyVersionAttribute, MAJOR, MINOR, REVISION);
	AssemblyFileVersionAttribute = NugetVersion + ".*";
}

Information("	  AssemblyVersionAttribute: {0}... Next: {1}", CurrentAssemblyVersionAttribute, AssemblyVersionAttribute);
Information("	       CliVersionAttribute: {0}... Next: {1}", GetVersionInProjectFile(cliProjectFile), AssemblyVersionAttribute);
Information("	      CoreVersionAttribute: {0}... Next: {1}", GetVersionInProjectFile(coreProjectFile), AssemblyVersionAttribute);
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
		UpdateVersionInProjectFile(cliProjectFile, AssemblyVersionAttribute);
		UpdateVersionInProjectFile(coreProjectFile, AssemblyVersionAttribute);
		
		//AssemblyVersionNumber = GetVersionSettingInProjectFile(cliProjectFile, "AssemblyVersion");
		UpdateVersionSettingInProjectFile(cliProjectFile, AssemblyVersionAttribute, "AssemblyVersion");
		//FileVersionNumber = GetVersionSettingInProjectFile(cliProjectFile, "FileVersion");
		UpdateVersionSettingInProjectFile(cliProjectFile, AssemblyVersionAttribute, "FileVersion");

		//AssemblyVersionNumber = GetVersionSettingInProjectFile(coreProjectFile, "AssemblyVersion");
		UpdateVersionSettingInProjectFile(coreProjectFile, AssemblyVersionAttribute, "AssemblyVersion");
		//FileVersionNumber = GetVersionSettingInProjectFile(coreProjectFile, "FileVersion");
		UpdateVersionSettingInProjectFile(coreProjectFile, AssemblyVersionAttribute, "FileVersion");
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
		Information("Building using MSBuild at " + msBuildPathX64);
		
		MSBuild(solutionFile, new MSBuildSettings {
			  ToolPath = msBuildPathX64
			, Configuration = configuration
		});
    }
    else
    {
      // Use XBuild
      XBuild(solutionFile, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
{
	 DoPackage("EzDbCodeGen.Cli", "net6.0", NugetVersion, "portable");
});

Task("Run-Unit-Tests")
    .IsDependentOn("publish")
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
	DotNetCorePack(solutionFile, new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = "./artifacts/"
	});
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


public string GetVersionInProjectFile(string projectFileName) {
	var _VersionInfoText = System.IO.File.ReadAllText(projectFileName);
	var _AssemblyFileVersionAttribute = Pluck(_VersionInfoText, "<Version>", "</Version>");
	return _AssemblyFileVersionAttribute;
}

public bool UpdateVersionInProjectFile(string projectFileName, string NewVersion)
{
	var _VersionInfoText = System.IO.File.ReadAllText(projectFileName);
	var _AssemblyFileVersionAttribute = Pluck(_VersionInfoText, "<Version>", "</Version>");
	var VersionPattern = "<Version>{0}</Version>";
	var _AssemblyFileVersionAttributeTextOld = string.Format(VersionPattern, _AssemblyFileVersionAttribute);
	var _AssemblyFileVersionAttributeTextNew = string.Format(VersionPattern, NewVersion);
	var newText = _VersionInfoText.Replace(_AssemblyFileVersionAttributeTextOld, _AssemblyFileVersionAttributeTextNew);

	System.IO.File.WriteAllText(projectFileName, newText);	
	return true;
}

public string GetVersionSettingInProjectFile(string projectFileName, string Name) {
	var _VersionInfoText = System.IO.File.ReadAllText(projectFileName);
	var _AssemblyFileVersionAttribute = Pluck(_VersionInfoText, "<"+Name+">", "</"+Name+">");
	return _AssemblyFileVersionAttribute;
}

public bool UpdateVersionSettingInProjectFile(string projectFileName, string NewVersion, string Name)
{
Information("projectFileName : {0}", projectFileName);

	var _VersionInfoText = System.IO.File.ReadAllText(projectFileName);
	var _AssemblyFileVersionAttribute = Pluck(_VersionInfoText, "<"+Name+">", "</"+Name+">");
//Information("_AssemblyFileVersionAttribute : {0}", _AssemblyFileVersionAttribute);
	var VersionPattern = "<"+Name+">{0}</"+Name+">";
//Information("VersionPattern : {0}", VersionPattern);
	var _AssemblyFileVersionAttributeTextOld = string.Format(VersionPattern, _AssemblyFileVersionAttribute);
//Information("_AssemblyFileVersionAttributeTextOld : {0}", _AssemblyFileVersionAttributeTextOld);
	var _AssemblyFileVersionAttributeTextNew = string.Format(VersionPattern, NewVersion);
//Information("_AssemblyFileVersionAttributeTextNew : {0}", _AssemblyFileVersionAttributeTextNew);
	var newText = _VersionInfoText.Replace(_AssemblyFileVersionAttributeTextOld, _AssemblyFileVersionAttributeTextNew);
//Information("newText : {0}", newText);

	System.IO.File.WriteAllText(projectFileName, newText);	
	return true;
}
 
private void DoPackage(string project, string framework, string NugetVersion, string runtimeId = null)
{
    var publishedTo = System.IO.Path.Combine(publishDir, project, framework);
    var projectDir = System.IO.Path.Combine("./Src", project);
    var packageId = $"{project}";
    var nugetPackProperties = new Dictionary<string,string>();
    var publishSettings = new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = publishedTo,
        Framework = framework,
		ArgumentCustomization = args => args.Append($"/p:Version={NugetVersion}").Append($"--verbosity normal")
    };
    if (!string.IsNullOrEmpty(runtimeId))
    {
        publishedTo = System.IO.Path.Combine(publishedTo, runtimeId);
        publishSettings.OutputDirectory = publishedTo;
        // "portable" is not an actual runtime ID. We're using it to represent the portable .NET core build.
        publishSettings.Runtime = (runtimeId != null && runtimeId != "portable") ? runtimeId : null;
        packageId = $"{project}.{runtimeId}";
        nugetPackProperties.Add("runtimeId", runtimeId);
    }
    DotNetCorePublish(projectDir, publishSettings);
}

