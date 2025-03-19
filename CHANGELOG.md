# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.4.0] - 2025-03-17

### Changed
- Updated EzDbSchema.Core and EzDbSchema.MsSql dependencies to version 8.4.0
- Updated Microsoft.Extensions.* packages to 9.0.3
- Updated System.Text.Json to 9.0.3
- Centralized package management in Directory.Build.props
- Enhanced package metadata and documentation
- Improved build configuration with embedded debug symbols

## [8.3.3] - 2025-03-17

### Added
- Enhanced string extension methods for better .NET 8.0 compatibility
- Improved handling of null inputs in string operations

### Changed
- Updated Microsoft.Extensions.* packages to 9.0.3
- Updated Microsoft.Data.SqlClient to 6.0.1
- Updated System.Text.Json to 9.0.3
- Standardized on modern JSON libraries (System.Text.Json and Newtonsoft.Json)

### Removed
- Removed legacy JsonComparer 1.0.0 package (.NET Framework dependency)
- Removed ServiceStack.Text 4.0.60 package (.NET Framework dependency)
- Removed System.Json package (redundant with System.Text.Json)

### Security
- Eliminated .NET Framework compatibility warnings by removing legacy packages
- Updated all Microsoft.Extensions packages to latest secure versions

## [8.3.2] - 2025-03-17

### Added
- Enhanced string extension methods for better .NET 8.0 compatibility
- Improved handling of null inputs in string operations

### Changed
- Updated Microsoft.Extensions.* packages to 9.0.3
- Updated Microsoft.Data.SqlClient to 6.0.1
- Updated System.Text.Json to 9.0.3
- Standardized on modern JSON libraries (System.Text.Json and Newtonsoft.Json)

### Removed
- Removed legacy JsonComparer 1.0.0 package (.NET Framework dependency)
- Removed ServiceStack.Text 4.0.60 package (.NET Framework dependency)
- Removed System.Json package (redundant with System.Text.Json)

### Security
- Eliminated .NET Framework compatibility warnings by removing legacy packages
- Updated all Microsoft.Extensions packages to latest secure versions

## [1.2.23.X] - 2020-06-08 [Unreleased]

### Added
- Addition of legacy of handlebar template functions to handle legacy code generation
- Changelog addition

### Changed
- Now using Nuke for building instead of Cake

## [1.1.X] - 2019-01-01

### Added
- Applicaiton Created and SimVer added.  GitVersion has been properly added. Yes, I am quite late to the game on using this technology.
