(Get-Content GitVersion.yml) | Foreach-Object {$_ -replace 'next-version:.+','next-version: X.X.77'} | Set-Content GitVersion.yml
Write-Output "TEST" 