<#

#START COPY HERE
Use the beginning of this script to copy an paste to your init.ps1 script.  

A possible use case is to create your nuget package with the following code in init.ps1
--START
param($installPath, $toolsPath, $package, $project)
Write-Host "EMPTY :("
--END

Then copy from 'START COPY to END COPY' at the top of the init.ps1 commenting out the param

Copy init.ps1 to <Solution Path>\packages\<Package Name>

for example:
"C:\Dev\\NuGetCommandTestHarness\packages\EzDbCodeGen.1.1.51"

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

$project.Solution.Open($solutionFileName)

Write-Host $project.Application.FileName
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

$projectName = $path::GetFileNameWithoutExtension($projectFileName)

$EzDbCodeGenPathSource = Join-Path $payloadPath "EzDbCodeGen"
$EzDbCodeGenPathTarget = Join-Path $projectPath "EzDbCodeGen"

$EzDbTemplatePathSource = Join-Path $EzDbCodeGenPathSource "Templates"
$EzDbTemplatePathTarget = Join-Path $EzDbCodeGenPathTarget "Templates"

$EzDbRenderSource = Join-Path $toolsPath "ezdbcodegen.ps1"
$EzDbRenderTarget = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.ps1"
$ProjectEzDbRenderTarget = Join-Path $EzDbCodeGenPathTarget "$projectName.codegen.ps1"

$EzDbCliPathSource = Join-Path $EzDbCodeGenPathSource "bin"
$EzDbCliPathTarget = Join-Path $EzDbCodeGenPathTarget "appbin"
$EzDbCliPathOldTarget = Join-Path $EzDbCodeGenPathTarget "bin"

$ReadMeTextFileNameSource = Join-Path $payloadPath "readme.txt"
$ReadMeTextFileNameTarget = Join-Path $EzDbCodeGenPathTarget "readme.txt"

$EzDbSampleTemplateSource = Join-Path $EzDbTemplatePathSource "SchemaRender.hbs"
$EzDbSampleTemplateTarget = Join-Path $EzDbTemplatePathTarget "SchemaRender.hbs"
$EzDbSampleTemplateFilesSource = Join-Path $EzDbTemplatePathSource "SchemaRenderAsFiles.hbs"
$EzDbSampleTemplateFilesTarget = Join-Path $EzDbTemplatePathTarget "SchemaRenderAsFiles.hbs"

$EzDbConfigFileSource = Join-Path $EzDbCodeGenPathSource "ezdbcodegen.config.json"
$EzDbConfigFileTarget = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.config.json"
$ProjectEzDbConfigFileTarget = Join-Path $EzDbCodeGenPathTarget "$projectName.config.json"

Write-Output "Project Name is '$projectName'"

Write-Output "installPath	  =$installPath"
Write-Output "toolsPath	      =$toolsPath"
Write-Output "package		  =$package"
Write-Output "project		  =$project"
Write-Output "projectFileName =$projectFileName"
Write-Output "PSScriptRoot    =$PSScriptRoot"
Write-Output "projectPath     =$projectPath"`

$DATESTRING = (Get-Date).ToString("s").Replace(":","-")
$TEMPPATH =  Join-Path $env:TEMP "EzDbCodeGen"
if(!(Test-Path -Path $TEMPPATH )){
    New-Item -ItemType directory -Path $TEMPPATH
}

Write-Output "init.ps1: Clearing out old binary files"
Get-ChildItem -Path $EzDbCliPathOldTarget  | foreach ($_) {
    Remove-Item $_.fullname -Force -Recurse
    "Removed :" + $_.fullname
}
Get-ChildItem -Path $EzDbCliPathTarget  | foreach ($_) {
    Remove-Item $_.fullname -Force -Recurse
    "Removed :" + $_.fullname
}
if((Test-Path -Path $EzDbCliPathOldTarget )) {
    Remove-Item -path $EzDbCliPathOldTarget -Force
}

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
Copy-Item -Path "$EzDbCliPathSource\*" -Destination "$EzDbCliPathTarget" -Recurse

Write-Output "init.ps1: Copying Sample Templates"
Copy-Item $EzDbSampleTemplateSource -Destination $EzDbSampleTemplateTarget -ErrorAction SilentlyContinue -Force
Copy-Item $EzDbSampleTemplateFilesSource -Destination $EzDbSampleTemplateFilesTarget -ErrorAction SilentlyContinue -Force

Write-Output "init.ps1: Copying Config and script files"
if((Test-Path -Path $EzDbConfigFileTarget )) {
    $EzDbConfigFileTargetBackup = Join-Path $TEMPPATH "ezdbcodegen.config-$DATESTRING.json"
    "WARNING! ezdbcodegen.config.json already exists, backing it up to '$EzDbConfigFileTargetBackup'" 
    Copy-Item $EzDbConfigFileTarget -Destination $EzDbConfigFileTargetBackup
}
if(!(Test-Path -Path $ProjectEzDbConfigFileTarget )) {
    "Project specific config file doesn't exist,  so '$ProjectEzDbConfigFileTarget' will be created and text 'MyDatabase' will be changed to '$projectName'" 
    (Get-Content $EzDbConfigFileSource) -replace "MyDatabase", "$projectName" | Set-Content $ProjectEzDbConfigFileTarget
}
Copy-Item $EzDbConfigFileSource -Destination $EzDbConfigFileTarget

if((Test-Path -Path $EzDbRenderTarget )) {
    $EzDbRenderTargetBackup = Join-Path $TEMPPATH "ezdbcodegen-$DATESTRING.ps1"
    "WARNING! ezdbcodegen.ps1 already exists, backing it up to '$EzDbRenderTargetBackup'" 
    Copy-Item $EzDbRenderTarget -Destination $EzDbRenderTargetBackup
}
if(!(Test-Path -Path $ProjectEzDbRenderTarget )) {
    "Project specific script file doesn't exist,  so '$ProjectEzDbRenderTarget' will be created and text 'ezdbcodegen.config.json' will be changed to '$projectName.config.json'" 
    (Get-Content $EzDbRenderSource) -replace 'ezdbcodegen.config', "$projectName.config" | Set-Content $ProjectEzDbRenderTarget

}
Copy-Item $EzDbRenderSource -Destination $EzDbRenderTarget
Copy-Item $ReadMeTextFileNameSource -Destination $ReadMeTextFileNameTarget
(Get-Content $ReadMeTextFileNameTarget) -replace '%PSPATH%', "$ProjectEzDbRenderTarget" | Set-Content $ReadMeTextFileNameTarget

#Clear out unneeded files from the appbin path
$BinTemplateConfig = Join-Path $EzDbCliPathTarget "\ezdbcodegen.config.json" 
Remove-Item $BinTemplateConfig -Force 
$BinTemplatePath = Join-Path $EzDbCliPathTarget "\Templates"
Remove-Item $BinTemplatePath -Force -Recurse
Start-Process $ReadMeTextFileNameTarget