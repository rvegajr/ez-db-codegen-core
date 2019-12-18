<#

#START COPY HERE
Use the beginning of this script to copy an paste to your init.ps1 script.  

A possible use case is to create your nuget package with the following code in init.ps1
--START
param($installPath, $toolsPath, $package, $project)
Write-Host "EMPTY :("
--END

Then copy from 'START COPY to END COPY' at the top of the init.ps1 commenting out the param

Some sample directory settings are 
$installPath="C:\Dev\Noctusoft\NuGetCommandTestHarness\packages\EzDbCodeGen.1.1.51"
$toolsPath="C:\Dev\Noctusoft\NuGetCommandTestHarness\packages\EzDbCodeGen.1.1.51\tools"
$package="NuGet.PackageManagement.VisualStudio.ScriptPackage"
$projectFileName="C:\Dev\Noctusoft\NuGetCommandTestHarness\NugetCommandTest\NugetCommandTest.csproj"
$PSScriptRoot="C:\Dev\Noctusoft\NuGetCommandTestHarness\packages\EzDbCodeGen.1.1.51\tools"
$projectPath="C:\Dev\Noctusoft\NuGetCommandTestHarness\NugetCommandTest\"

We are assuming that this file is in packages\<nuget package name>\tools path


if ($psISE)
{
    $scriptPath=Split-Path -Path $psISE.CurrentFile.FullPath        
}
else
{
    $scriptPath=$global:PSScriptRoot
}
$project = [System.Runtime.InteropServices.Marshal]::GetActiveObject("VisualStudio.DTE.16.0")

$package="NuGet.PackageManagement.VisualStudio.ScriptPackage"
$toolsPath=$scriptPath
$installPath= Split-Path -parent $scriptPath;
$packagesPath= Split-Path -parent $installPath;
$projectRoot = Split-Path -parent $packagesPath;
$projectPath = (Join-Path $projectRoot "\NugetCommandTest")
$projectFileName = (Join-Path $projectPath "\NugetCommandTest.csproj")
$solutionFileName = (Join-Path $projectRoot "\NugetCommandTest.sln")

Write-Output '$PSScriptRoot    ='+$PSScriptRoot
Write-Output '$installPath	   ='+$installPath
Write-Output '$toolsPath	   ='+$toolsPath
Write-Output '$packagesPath	   ='+$packagesPath
Write-Output '$projectRoot	   ='+$projectRoot
Write-Output '$packagesPath	   ='+$packagesPath
Write-Output '$projectPath     ='+$projectPath
Write-Output '$projectFileName ='+$projectFileName

$dte.Solution.Open($solutionFileName)

Write-Host $dte.Application.FileName
## END OF SCAFFOLDING
<#
param($installPath, $toolsPath, $package, $project)
Write-Output "init.ps1: Executing Post NuGet Install scripts "

$path = [System.IO.Path]
$projectPath = Join-Path $path::GetDirectoryName($project.FileName) ""
$projectFileName = $path::GetFullPath($project.FileName)
$contentPath = Join-Path $installPath "content"
$payloadPath = Join-Path $installPath "payload"
# >
# #>
param($installPath, $toolsPath, $package, $project)
Write-Output "init.ps1: Executing Post NuGet Install scripts "

<#
$path = [System.IO.Path]
#$projectPath = Join-Path $path::GetDirectoryName($project.FileName) ""
#$projectFileName = $path::GetFullPath($project.FileName)
$contentPath = Join-Path $installPath "content"
$payloadPath = Join-Path $installPath "payload"
#>
$path = [System.IO.Path]
$projectPath = Join-Path $path::GetDirectoryName($project.FileName) ""
$projectFileName = $path::GetFullPath($project.FileName)
$contentPath = Join-Path $installPath "content"
$payloadPath = Join-Path $installPath "payload"

$ReadMeTextFileName = Join-Path $contentPath "readme.txt"

$EzDbCodeGenPathSource = Join-Path $payloadPath "EzDbCodeGen"
$EzDbCodeGenPathTarget = Join-Path $projectPath "EzDbCodeGen"

$EzDbTemplatePathSource = Join-Path $EzDbCodeGenPathSource "Templates"
$EzDbTemplatePathTarget = Join-Path $EzDbCodeGenPathTarget "Templates"

$EzDbRenderSource = Join-Path $toolsPath "ezdbcodegen.ps1"
$EzDbRenderTarget = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.ps1"

$EzDbCliPathSource = Join-Path $EzDbCodeGenPathSource "bin"
$EzDbCliPathTarget = Join-Path $EzDbCodeGenPathTarget "appbin"

$EzDbSampleTemplateSource = Join-Path $EzDbTemplatePathSource "SchemaRender.hbs"
$EzDbSampleTemplateTarget = Join-Path $EzDbTemplatePathTarget "SchemaRender.hbs"
$EzDbSampleTemplateFilesSource = Join-Path $EzDbTemplatePathSource "SchemaRenderAsFiles.hbs"
$EzDbSampleTemplateFilesTarget = Join-Path $EzDbTemplatePathTarget "SchemaRenderAsFiles.hbs"

$EzDbConfigFileSource = Join-Path $EzDbCodeGenPathSource "ezdbcodegen.config.json"
$EzDbConfigFileTarget = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.config.json"

Write-Output '$installPath	   ='+$installPath
Write-Output '$toolsPath	   ='+$toolsPath
Write-Output '$package		   ='+$package
Write-Output '$project		   ='+$project
Write-Output '$projectFileName ='+$projectFileName
Write-Output '$PSScriptRoot    ='+$PSScriptRoot
Write-Output '$projectPath     ='+$projectPath
$DateStamp = get-date -uformat "%Y-%m-%d@%H-%M-%S"

Write-Output "init.ps1: Clearing out old binary files"
Get-ChildItem -Path $EzDbCliPathTarget  | foreach ($_) {
    Remove-Item $_.fullname -Force -Recurse
    "Removed :" + $_.fullname
}
if((Test-Path -Path $EzDbConfigFileTarget )) {
    $EzDbConfigFileTargetBackup = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.config.json-$DateStamp"
    Copy-Item -Path $EzDbConfigFileTarget -Destination $EzDbConfigFileTargetBackup
    Remove-Item $EzDbConfigFileTarget -Force -Recurse
}
if((Test-Path -Path $EzDbRenderTarget )) {
    $EzDbRenderTargetBackup = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.ps1-$DateStamp"
    Copy-Item -Path $EzDbRenderTarget -Destination $EzDbRenderTargetBackup
    Remove-Item $EzDbRenderTarget -Force -Recurse
}

Remove-Item $EzDbCliPathTarget -Force -Recurse

Write-Output "init.ps1: Making sure that '$EzDbTemplatePathTarget' exists"
if(!(Test-Path -Path $EzDbTemplatePathTarget )){
    New-Item -ItemType directory -Path $EzDbTemplatePathTarget
    Write-Host "New folder created '$EzDbTemplatePathTarget'"
}

$DllName = "EzDbCodeGen.Cli.dll"
$dllLocation = (Get-ChildItem -Path $payloadPath -Filter $DllName -Recurse -ErrorAction SilentlyContinue -Force).FullName
If (-NOT( $dllLocation ) ) {
    $ErrorMessage = "Cli Application dll [" + $DllName + "] could not be found in the payload path [" + $payloadPath + "], is it even in the NuGet package?"
    Write-Error -Message $ErrorMessage -ErrorAction Stop
}
foreach ($dll in $dllLocation)
{
    $EzDbCliPathSource = Join-Path $path::GetDirectoryName($dll) ""
    break
}
Write-Output '$EzDbCliPathSource     ='+$EzDbCliPathSource
Write-Output "init.ps1: Copying Cli content application '$EzDbCliPathSource' to '$EzDbCliPathTarget'"
Copy-Item $EzDbCliPathSource -Destination $EzDbCliPathTarget -Recurse

Write-Output "init.ps1: Copying Sample Templates"
Copy-Item -Path $EzDbSampleTemplateSource -Destination $EzDbSampleTemplateTarget -ErrorAction SilentlyContinue -Force
Copy-Item -Path $EzDbSampleTemplateFilesSource -Destination $EzDbSampleTemplateFilesTarget -ErrorAction SilentlyContinue -Force

Write-Output "init.ps1: Copying '$EzDbConfigFileSource' to solution root '$EzDbConfigFileTarget'"
Copy-Item -Path $EzDbConfigFileSource -Destination $EzDbConfigFileTarget
Copy-Item -Path $EzDbRenderSource -Destination $EzDbRenderTarget

$BinTemplateConfig = Join-Path $EzDbCliPathTarget "\ezdbcodegen.config.json" 
Remove-Item $BinTemplateConfig -Force 
$BinTemplatePath = Join-Path $EzDbCliPathTarget "\Templates\" 
Remove-Item $BinTemplatePath -Force -Recurse

