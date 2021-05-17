$confirmation = Read-Host "WARNING: This will possibly cause your project to reload as it may change your project file (caused by adding/removing files from path.. this is due to the possibility of project files bieng in older formats)\nIf you are unsure, save all your documents first before pressing 'y' and hitting enter:"
if ($confirmation -eq 'y') {
	$ConnectionString = 'Server=localhost;Database=WideWorldImportersDW;user id=sa;password=sa'

	$TemplatePath = Join-Path $PSScriptRoot "Templates"
	$ConfigFileName = Join-Path $PSScriptRoot "ezdbcodegen.config.json"

	Write-Output 'ConfigFileName=' $ConfigFileName
	Write-Output '  TemplatePath=' $TemplatePath

	$FullCallArguments = 'dotnet ezdbcg -t "' + $TemplatePath + '" -sc "' + $ConnectionString + '" -cf "' + $ConfigFileName + '" -v ; pause'
	<# We have to double escape the string so the powershell invoke Start-Process arguements are properly escaped #>
	Start-Process "powershell" -WorkingDirectory $PSScriptRoot -wait -ArgumentList $FullCallArguments.Replace('"','""').Replace('"','""')
}