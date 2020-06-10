using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.GitHub.GitHubTasks;
using static Nuke.GitHub.ChangeLogExtensions;
using static Nuke.Common.Tooling.ProcessTasks;
using Nuke.GitHub;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using static Nuke.CodeGeneration.CodeGenerator;
using static Nuke.Common.Tooling.ToolSettingsExtensions;


[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter] string NugetApiUrl = "https://api.nuget.org/v3/index.json";
    [Parameter] string NugetApiKey = "NOTSET";

    AbsolutePath SourceDirectory => RootDirectory / "Src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath NugetDirectory => ArtifactsDirectory / "nuget";    
    AbsolutePath ChangeLogFile => RootDirectory / "CHANGELOG.md";
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            DeleteFiles(RootDirectory, new string[] { ArtifactsDirectory+"/*.nupkg" });
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore()
                );
        });

   Target GitVersionTagUpdate => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {

            var dotnetPath = ToolPathResolver.GetPathExecutable("dotnet");

            StartProcess("GitVersion", " " +
                            "/updateprojectfiles ",
            workingDirectory: RootDirectory)
            // AssertWairForExit() instead of AssertZeroExitCode()
            // because we want to continue all tests even if some fail
            .AssertWaitForExit();
        });

    Target Pack => _ => _
        .DependsOn(GitVersionTagUpdate)
        .Executes(() =>
        {
            int commitNum = 0;
            string NuGetVersionCustom = GitVersion.NuGetVersionV2;
            Console.WriteLine(string.Format("Commits Since Version Source: {0}", GitVersion.CommitsSinceVersionSource));

            //if it's not a tagged release - append the commit number to the package version
            //tagged commits on master have versions
            // - v0.3.0-beta
            //other commits have
            // - v0.3.0-beta1
            if (Int32.TryParse(GitVersion.CommitsSinceVersionSource, out commitNum))
                NuGetVersionCustom = commitNum > 0 ? NuGetVersionCustom + $"{commitNum}" : NuGetVersionCustom;

            var changeLog = GetCompleteChangeLog(ChangeLogFile)
                .EscapeStringPropertyForMsBuild();
            var firstChangeLog = changeLog.Split(new string[] {@"##%20["},StringSplitOptions.None)[1]
                .Trim();

            DotNetPack(s => s
               .SetProject(Solution.GetProject("EzDbCodeGen.Cli"))
               .SetPackageId("EzDbCodeGenCli")
               .SetConfiguration(Configuration)
               .EnableNoBuild()
               .EnableNoRestore()
               .SetVersion(NuGetVersionCustom)
               .SetNoDependencies(true)
               .SetOutputDirectory(ArtifactsDirectory)
               .SetPackageReleaseNotes(changeLog)
            );

            DotNetPack(s => s
               .SetProject(Solution.GetProject("EzDbCodeGen.Core"))
               .SetPackageId("EzDbCodeGen")
               .SetConfiguration(Configuration)
               .EnableNoBuild()
               .EnableNoRestore()
               .SetVersion(NuGetVersionCustom)
               .SetNoDependencies(true)
               .SetOutputDirectory(ArtifactsDirectory)
               .SetPackageReleaseNotes(changeLog)
            );
        });

        Target Push => _ => _
            //.DependsOn(Pack)
            .Requires(() => NugetApiUrl)
            .Requires(() => NugetApiKey)
            .Requires(() => Configuration.Equals(Configuration.Release))
            .Executes(() =>
            {
                GlobFiles(ArtifactsDirectory, "*.nupkg")
                    .NotEmpty()
                    .Where(x => !x.EndsWith("symbols.nupkg"))
                    .ForEach(x =>
                    {
                        DotNetNuGetPush(s => s
                            .SetTargetPath(x)
                            .SetSource(NugetApiUrl)
                            .SetApiKey(NugetApiKey)
                        );
                    });
            });     

    private void DeleteDirectories(string directory, string[] globPatterns, string[] ignoreList = null)
    {
        if (ignoreList == null)
        {
            ignoreList = new string[0];
        }

        var toDelete = GlobDirectories(directory, globPatterns);
        toDelete.ForEach(_ =>
        {
            if (ignoreList.Any(_.Contains))
            {
                Console.WriteLine($"DeleteDirectories: Ignoring '{_}' directory...");
                return;
            }

            DeleteDirectory(_);
        });
    }   
    
    private void DeleteFiles(string directory, string[] globPatterns, string[] ignoreList = null)
    {
        if (ignoreList == null)
        {
            ignoreList = new string[0];
        }

        var toDelete = GlobFiles(directory, globPatterns);
        toDelete.ForEach(_ =>
        {
            if (ignoreList.Any(_.Contains))
            {
                Console.WriteLine($"DeleteFiles: Ignoring '{_}' file...");
                return;
            }

            DeleteFile(_);
        });
    }                
}
