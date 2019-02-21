# ez-db-codegen-core
Easy code generation based on a database schema given by [EZDbSchema](https://github.com/rvegajr/ez-db-schema-core).  The template language this application uses is HandleBars. 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system. This nuget package will dump the published cli package for code generation and a powershell script to run it.  The nuget package will dump everything you need for code generation into the project you have selected under the EzDbCodeGen folder.    

Please note I owe you so much more documenation on this!! more to come! :)

### Prerequisites
* [DotNetCore] (https://www.microsoft.com/net/learn/get-started) - You will get everything you need except the sdk!  please download the latest version of this before trying to run the powershell script
* [Visual Studio 2017] (https://visualstudio.microsoft.com/) - You will get everything you need except the sdk!  please download the latest version of this before trying to run the powershell script
* You will need MSSQL with some database installed.  If you need a sample database,  feel free to look for the [World Wide Importers](https://github.com/Microsoft/sql-server-samples/releases/tag/wide-world-importers-v1.0) samples.

NOTE:  If you have not set your powershell execution remote policy first,  you will need to do this as noted in [Powershell Execution Policy](https://www.pdq.com/blog/powershell-how-to-write-your-first-powershell-script/)
* Open the powershell command prompt in administrator mode and type:
Set-ExecutionPolicy RemoteSigned

### Using this project:

Lets go through a test run of how to use this before we get into the nitty gritty:
####  From NuGet (or use the nuget package manager)

1. Install-Package EzDbCodeGen  - it will create a folder in EzDbCodeGen.NuGet.TestTarget called EzDbCodeGen
2. Update the connection string in ezdbcodegen.ps1 with the database you wish to create your templates on
3. right click on ezdbcodegen.ps1 and select "Open with PowerShell ISE" (because for whatever reason,  the direct executer just hangs for me)
4. Click the green right arrow to run the script.  You should see a powershell window come up with the script running,  EzDbCodeGen/Generated should have the results of both of the templates execution

## Deployment

This project was design to be hosted and distributed with nuget.com.

## Built With

* [.net core](https://www.microsoft.com/net/learn/get-started) - The framework used

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/rvegajr/651875c08acb76009e563db128f33e7e) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/rvegajr/tags). 

## Authors

* **Ricky Vega** - *Initial work* - [Noctusoft](https://github.com/rvegajr)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

Many thanks to the following projects that have helped in this project
* EzDBSchema 
* McMaster.Extensions.CommandLineUtils


