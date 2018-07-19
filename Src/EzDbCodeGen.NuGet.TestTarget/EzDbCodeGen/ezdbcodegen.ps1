$ConnectionString = 'Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa'
$path = [System.IO.Path]

$DllName = 'EzDbCodeGen.Cli.dll'
$BinPath = Join-Path $PSScriptRoot "bin"
$TemplatePath = Join-Path $PSScriptRoot "Templates"
$ConfigFileName = Join-Path $PSScriptRoot "ezdbcodegen.config.json"

$dllLocation = (Get-ChildItem -Path $BinPath -Filter $DllName -Recurse -ErrorAction SilentlyContinue -Force).FullName
If (-NOT( $dllLocation ) ) {
    $ErrorMessage = "Cli Application dll [" + $DllName + "] could not be found :(  Have you compiled the application yet?."
    Write-Error -Message $ErrorMessage -ErrorAction Stop
}
foreach ($dll in $dllLocation)
{
    $DllPath = Join-Path $path::GetDirectoryName($dll) ""
    break
}

$folder = Get-ChildItem $DllPath -Directory -ErrorAction SilentlyContinue
If (-NOT( $Folder ) ) {
    $ErrorMessage = "Cli Application dll [" + $DllPath + "] does not exist :(  Have you compiled the application yet?."
    Write-Error -Message $ErrorMessage -ErrorAction Stop
}
Write-Output '       DllPath=' $DllPath
Write-Output 'ConfigFileName=' $ConfigFileName

$CliCallArguments = $DllName + ' -t "' + $TemplatePath + '" -sc "' + $ConnectionString + '" -cf "' + $ConfigFileName + '" -v'
$FullCallArguments = 'dotnet ' + $DllName + ' -t "' + $TemplatePath + '" -sc "' + $ConnectionString + '" -cf "' + $ConfigFileName + '" -v ; pause'
<# We have to double escape the string so the powershell invoke Start-Process arguements are properly escaped #>
Start-Process "powershell" -WorkingDirectory $DllPath -wait -ArgumentList $FullCallArguments.Replace('"','""').Replace('"','""')