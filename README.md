# ez-db-codegen-cli

EzDbCodeGen now works as a local tool.  
Easy code generation based on a database schema given by [EZDbSchema](https://github.com/rvegajr/ez-db-schema-core).  The template language this application uses is HandleBars. 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system. This nuget package will dump the published cli package for code generation and a powershell script to run it.  The nuget package will dump everything you need for code generation into the project you have selected under the EzDbCodeGen folder.    

### Prerequisites
* [Net 8.0+] (https://www.microsoft.com/net/learn/get-started) - You will get everything you need except the sdk!  please download the latest version of this before trying to run the powershell script
* You will need MSSQL with some database installed.  If you need a sample database,  feel free to look for the [World Wide Importers](https://github.com/Microsoft/sql-server-samples/releases/tag/wide-world-importers-v1.0) samples.

NOTE:  If you have not set your powershell execution remote policy first,  you will need to do this as noted in [Powershell Execution Policy](https://www.pdq.com/blog/powershell-how-to-write-your-first-powershell-script/)
* Open the powershell command prompt in administrator mode and type:
Set-ExecutionPolicy RemoteSigned

### Using this project:

1. Navigate to an empty directoy where you want to install this tool at.
1. Using the command line: `dotnet new tool-manifest`
2. Once this has completed:  
 `dotnet tool install EzDbCodeGen.Cli --interactive`  
(or to update: `dotnet tool update EzDbCodeGen.Cli --interactive`)
3. You will need a database that you can run the sample templates against.  This utility will build the connection string, test it,  download the sample files from the nuget library, copy them to the proper location, then perform the code generation was instructed by the templates.  
`dotnet ezdbcg` 

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

## HandleBar Custom Functions

* `{{ ContextAsJson }}` - Will dump the current context as a JSON file on the rendered file,  useful for debugging
* `{{ Prefix $p1 }}` - Will append a string to the beginning of the string passed through $p1
* `{{ ExtractTableName $p1 }}` - Used the extract the table name from a schema.table object name format
* `{{ ExtractSchemaName  $p1 }}` - Used the extract the schema name from a schema.table object name format
* `{{ ToSingular $p1 }}` -  Will change $p1 to a singular word
* `{{ Comma }}` -  Will output a comma 
* `{{ ToPlural }}` - Will change $p1 to a plural word
* `{{ ToNetType $p1 }}` - Assuming that the string is a sql type, it will return the corresponding .net type with a ? if the property is nullable
* `{{ ToCodeFriendly $p1 }}` - Will write a string removing code unfriendly characters
* `{{ PropertyNameSuffix $p1 }}` - Will output a code friendly string 
* `{{ ToJsType }}` -  Assuming that the string is a sql type, it will return the corresponding javascript type appending "| null" if it is nullable
* `{{ AsFormattedName $p1 }}` -  Strips ID, UID, or id from $p1 
* `{{ ToSnakeCase $p1 }}` - Will turn $p1 into snake case
* `{{ ToSingularSnakeCase $p1 }}` -  Will turn $p1 into snake case singular
* `{{ ToTitleCaseSafeFileName $p1 }}` -  Will turn $p1 into Title case and safe for a file name (excellent for the <FILE/> clause )
* `{{ ToCsObjectName $p1 }}` - Will convert $p1 to a string sutable for C# Code name
* `{{ StringFormat $p1, $p2 }}` - Versitile string function that lets you apply 1 or more formatting tasks on $p1, $p2 can cantain one more of 'lower,upper,snake,title,pascal,trim,plural,single,nettype,jstype', performed in order 
* `{{ EntityCustomAttribute $p1 }}` - Upeer stirng and replacing "US_" to ""
* `{{ IfPropertyExists $p1 }}` - Will search the parent context to see of the entity name exists,  will only write the code after to {{/IfPropertyExists}} if true
* `{{ isRelationshipCount $p1 $p2 }}` - $p1 should be a comparison op >, =, ==, <, !=, <>,  $p2 should be the number compared to
    will only write the code after to {{/isRelationshipCount}} if true
* `{{ isRelationshipTypeOf $p1}}` - Should be called when the context is a Relationship or RelaitonshipList, 
    $p1 can = OneToMany, ZeroOrOneToMany, ZeroOrOneToManyOnly, ManyToOne, ManyToZeroOrOne, ManyToZeroOrOneOnly, OneToOne, OneToZeroOrOne, OneToZeroOrOneOnly, ZeroOrOneToOne, ZeroOrOneToOneOnly  
* `{{ ToTargetEntityAlias }}` -  Should be called when the context is a Relationship or RelaitonshipList, will return the Alias of what this entity is related to
* `{{ ToUniqueColumnName $p1 }}` - Will attempt to figured out a unique column name if one of the same name exists 
* `{{ ifPropertyCustomAttributeCond $p1 $p2 $p3}}` - Tests the value of a particular custom attribute of a propery, if true will write code from tag to {{/ifPropertyCustomAttributeCond}} or {{else}}. if false, it will write from {{else}} to {{/ifPropertyCustomAttributeCond}}
    $p1 = attribute name
    $p2 = should be a comparison op >, =, ==, <, !=, <> 
    $p3 = value to compare
* `{{ isNotInList $p1 $p2 \[$p3\]...\[$pn\] }}` - Will return true if $p1 does not exist in $p2(+),  will write from tag isNotInList to {{/isNotInList}} or {{else}} of it doesnt exist, if it does, it will write from {{else}} to  {{/isNotInList}}
* `{{ ifNot $p1 }} - if $p1 is false, this will write all code between this tag and {{/ifNot}} or {{else}}, if true, code to be writtent will be {{else}} to {{/ifNot}}
* `{{ ifCond $p1 \[$p2\] \[$p3\]}}` - This function requirs 1 argument or 3 arguments
if there is only $p1, then $p1 should be a boolean, otherwise 
    $p1 = 1value to compare
    $p2 = should be a comparison op >, =, ==, <, !=, <> 
    $p3 = value to
if the result of the 3 operators is true, it will write from this tag to {{else}} or {{/ifCond}}, if false then code from {{else}} to {{/ifCond}} will be written 
* `{{ IsAuditableOutput $p1 }}` - This function will output the contents if $p1 if the entity contains any auditable column (Created. CreatedBy, Updated, UpdatedBy) 
* `{{ IsNotAuditableOutput $p1 }}` - This function will output the contents if $p1 if the entity DOES NOT contain any auditable column (Created. CreatedBy, Updated, UpdatedBy) 

## Changes
V 6.0.0  - Added IsNotAuditableOutput template render directive (fixed mispelling)

V 6.0.20 - Added IsNotAuditiableOutput template render directive

V 6.0.19 - Added Data Type Override
		   Added Field Level Type Name and Nullable Overrides

V 6.0.14 - Changed names to be more inclusive

V 6.0.13 - Added tge abiltity to WhiteList/Blacklist based on template and entity file name

V 7.0.1  - Updated to .net 7.0 

V 8.0.0  - Updated to .net 8.0 