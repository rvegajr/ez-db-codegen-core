$ConnectionString = 'Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa'
$path = [System.IO.Path]

$DllName = 'EzDbCodeGen.Cli.dll'
$StdOut = $NULL
$StdErr = $NULL
$ExitCode = $NULL


$BinPath = Join-Path $PSScriptRoot "bin"
$TemplatePath = Join-Path $PSScriptRoot "EzDbTemplates"
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
Function Execute-Command ($commandTitle, $commandPath, $commandArguments)
{
  Try {
  Write-Output "commandPath=$commandPath" 
  Write-Output "commandTitle=$commandTitle" 
  Write-Output "commandArguments=$commandArguments" 

    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = $commandTitle
    $pinfo.WorkingDirectory = $commandPath
    $pinfo.RedirectStandardError = $true
    $pinfo.RedirectStandardOutput = $true
    $pinfo.UseShellExecute = $false
    $pinfo.CreateNoWindow = $false

    $pinfo.Arguments = $commandArguments
    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $pinfo
    $p.Start() | Out-Null
    $p.WaitForExit()
    $res = [pscustomobject]@{
        commandTitle = $commandTitle
        stdout = $p.StandardOutput.ReadToEnd()
        stderr = $p.StandardError.ReadToEnd()
        ExitCode = $p.ExitCode  
    }
    Write-Output '$stdout=' $res.stdout
  }
  Catch {
     $ErrorMessage = $_.Exception.Message
     $FailedItem = $_.Exception.ItemName
     Write-Output 'ErrorMessage=' $ErrorMessage
     exit
  }
}

$CliCallArguments = $DllName + ' -t "' + $TemplatePath + '" -sc "' + $ConnectionString + '" -cf "' + $ConfigFileName + '" -v'
Execute-Command 'dotnet' $DllPath $CliCallArguments

