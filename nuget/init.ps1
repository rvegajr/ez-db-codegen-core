param($installPath, $toolsPath, $package, $project)
Write-Output "init.ps1: Executing Post NuGet Install scripts "

$path = [System.IO.Path]
$projectPath = Join-Path $path::GetDirectoryName($project.FileName) ""
$projectFileName = $path::GetFullPath($project.FileName)
$contentPath = Join-Path $installPath "content"

$ReadMeTextFileName = Join-Path $contentPath "readme.txt"

$EzDbCodeGenPathSource = Join-Path $contentPath "EzDbCodeGen"
$EzDbCodeGenPathTarget = Join-Path $projectPath "EzDbCodeGen"

$EzDbTemplatePathSource = Join-Path $EzDbCodeGenPathSource "Templates"
$EzDbTemplatePathTarget = Join-Path $EzDbCodeGenPathTarget "Templates"

$EzDbRenderSource = Join-Path $toolsPath "ezdbcodegen.ps1"
$EzDbRenderTarget = Join-Path $EzDbCodeGenPathTarget "ezdbcodegen.ps1"

$EzDbCliPathSource = Join-Path $EzDbCodeGenPathSource "bin"
$EzDbCliPathTarget = Join-Path $EzDbCodeGenPathTarget "bin"

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

Write-Output "init.ps1: Making sure that '$EzDbTemplatePathTarget' exists"
if(!(Test-Path -Path $EzDbTemplatePathTarget )){
    New-Item -ItemType directory -Path $EzDbTemplatePathTarget
    Write-Host "New folder created"
}
Write-Output "init.ps1: Copying '$EzDbRenderSource' to target path '$EzDbRenderTarget'"
Copy-Item -Path $EzDbRenderSource -Destination $EzDbRenderTarget

Write-Output "init.ps1: Copying '$EzDbSampleTemplateSource' to solution root '$EzDbSampleTemplateTarget'"
Copy-Item -Path $EzDbSampleTemplateSource -Destination $EzDbSampleTemplateTarget

Write-Output "init.ps1: Copying '$EzDbSampleTemplateFilesSource' to solution root '$EzDbSampleTemplateFilesTarget'"
Copy-Item -Path $EzDbSampleTemplateFilesSource -Destination $EzDbSampleTemplateFilesTarget

Write-Output "init.ps1: Copying '$EzDbConfigFileSource' to solution root '$EzDbConfigFileTarget'"
Copy-Item -Path $EzDbConfigFileSource -Destination $EzDbConfigFileTarget

$DllName = "EzDbCodeGen.Cli.dll"
$dllLocation = (Get-ChildItem -Path $contentPath -Filter $DllName -Recurse -ErrorAction SilentlyContinue -Force).FullName
If (-NOT( $dllLocation ) ) {
    $ErrorMessage = "Cli Application dll [" + $DllName + "] could not be found :(  Have you compiled the application yet?."
    Write-Error -Message $ErrorMessage -ErrorAction Stop
}
foreach ($dll in $dllLocation)
{
    $EzDbCliPathSource = Join-Path $path::GetDirectoryName($dll) ""
    break
}
Write-Output "init.ps1: Copying Cli content application '$EzDbCliPathSource' to '$EzDbCliPathTarget'"
Copy-Item $EzDbCliPathSource -Destination $EzDbCliPathTarget -Recurse

<# Experimental code to update the solution file

Write-Output "Checking to see if we have to make any changes to the solution for newly added files from '$project.FileName'"

$xmldoc = [xml](get-content $projectFileName)
$xmldoc.Load($projectFileName)
$changes = 0

function Add-File-To-Solution-Xml ()
{
  param($xmldoc, $filename, $copyToOuputDirectory, [ref]$changes)
  $ret = 0
  Try {
    $root = $xmldoc.SelectSingleNode('//Project')
    $nodeName = '//None[@Update="' + $filename + '"]'
    $nod = $xmldoc.SelectSingleNode($nodeName)
    if ($nod -eq $null) {
        $ItemGroup = $xmldoc.CreateElement("ItemGroup")
        $NoneElement = $xmldoc.CreateElement("None")
        $NoneAttribUpdate = $xmldoc.CreateAttribute("Update")
        $NoneAttribUpdate.Value = $filename
        $NoneElement.Attributes.Append($NoneAttribUpdate)
        $CopyToOutputDirectoryElement = $xmldoc.CreateElement("CopyToOutputDirectory")
        $CopyToOutputDirectoryElement.InnerText = "Always"
        $NoneElement.AppendChild( $CopyToOutputDirectoryElement );
        $ItemGroup.AppendChild( $NoneElement );
        $root.AppendChild( $ItemGroup );
        $changes.Value = $changes.Value + 1 
        Write-Output "Added reference to new file '$filename'"
    } else {
        $CopyToOutputDirectoryElement = $nod.SelectSingleNode('//CopyToOutputDirectory')
    }
  }
  Catch {
     $ErrorMessage = $_.Exception.Message
     $FailedItem = $_.Exception.ItemName
     Write-Output 'ErrorMessage=' $ErrorMessage
     exit
  }
}
Add-File-To-Solution-Xml $xmldoc "ezdbcodegen.config.json" "Always" ([ref]$changes)
Add-File-To-Solution-Xml $xmldoc "EzDbTemplates\SchemaRender.hbs" "Always" ([ref]$changes)
if ( $changes -gt 0 ) {
    $xmlDoc.Save("$projectFileName")
    Write-Output "Rewriting edited '$projectFileName'.. Note that this may cause the solution to reload"
}
#>
