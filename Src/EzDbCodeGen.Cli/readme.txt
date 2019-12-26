Hi! Thanks for installing EzDbCodeGen!

This app uses dotnet core 3.1,  please make sure this is installed where you are running this application 

Lets assume this app is called "SuperApp"

*** WHAT YOU NEED TO DO
What you will need to do is change the connection string:
1. Set the $ConnectionString in 'SuperApp.config.ps1' the the database you want to connect to 

2. Start powershell and navigate to the path the script is located in, this should be 
Navigate to %PSPATH%
execute it by typing ./NugetCommandTest.codegen.ps1

When you make updates to the nuget package,  the script will backup existing ezdbcodegen config and ps1 files to %TEMP% path (open cmd and type echo %TEMP%), 
but will ignore project specific config and script files.


*** WHAT THIS NUGET INSTALLATION HAS DONE (if this is the first time)
If this the first time you run this (or the project specific config and script),  the init script will create 2 project specific versions
of important files. For example, lets say your app is called 'SuperApp', the script will automatically perform the following actions:

1. the script will Change line 9 in ezdbcodegen.ps1
	$ConfigFileName = Join-Path $PSScriptRoot "ezdbcodegen.config.json"
  to
	$ConfigFileName = Join-Path $PSScriptRoot "SuperApp.config.json"

2. Change line 16 in ezdbcodegen.config.json from 
    "SchemaName": "MyEntities",
  to
    "SchemaName": "SuperAppEntities",

4. Rename 'ezdbcodegen.config.json' to 'SuperApp.config.json'

5. Rename 'ezdbcodegen.ps1' to 'SuperApp.codegen.ps1'


Visit the project at https://github.com/rvegajr/ez-db-codegen-core!
Email me directly at ez@noctusoft.com!

