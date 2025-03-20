# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.5.4] - 2025-03-20

### Changed
- Improved error handling in connection testing
- Enhanced timeout configuration for database connections
- Updated package metadata and documentation
- Refined error messages for better user experience

## [8.5.3] - 2025-03-19

### Added
- Enhanced connection testing with detailed error messages and improved user feedback
- Interface-first design for database connection testing via `IConnectionTester`
- Comprehensive error handling for various connection failure scenarios
- Unit tests for connection testing feature

### Changed
- Improved error messages for connection string validation
- Enhanced SSL/TLS error handling with clear guidance
- Updated documentation with connection testing examples and interface usage
- Refactored connection testing to follow SOLID principles

### Fixed
- Better handling of invalid connection string formats
- More descriptive error messages for common connection issues
- Proper null handling in connection testing following .NET 8.0 guidelines

## [8.5.2] - 2025-03-19

### Added
- New CLI parameter `--test-connection` for validating database connection strings
- Comprehensive connection testing service with detailed error reporting
- Interface-based connection testing following SOLID principles
- Specific error codes for different connection failure scenarios

### Changed
- Enhanced error handling for database connections
- Improved CLI feedback with detailed error messages
- Updated to async/await pattern for better performance
- Added interface-first approach for connection testing components

### Technical Details
- Added `IConnectionTester` interface for connection validation
- Implemented SQL Server-specific connection tester
- Enhanced return codes for better error handling
- Added dependency injection support for testing

## [8.5.1] - 2025-03-19

### Changed
- Enhanced interface documentation in README.md and AI_USAGE.md
- Improved template examples for interface-based code generation
- Added comprehensive interface usage patterns and best practices
- Updated documentation for EzDbSchema.Core integration

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
