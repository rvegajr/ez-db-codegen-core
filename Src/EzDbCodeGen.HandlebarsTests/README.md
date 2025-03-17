# EzDbCodeGen.HandlebarsTests

This project contains unit tests for the Handlebars utility functionality in the EzDbCodeGen project.

## Overview

The tests in this project validate the functionality of the HandlebarsUtility class, which provides string manipulation and template helper functions for use with Handlebars.Net templates.

## Features Tested

- Basic template compilation
- String comparison helpers (eq, ne, lt, gt, le, ge)
- String manipulation helpers (concat, lower, upper, pascal, camel)
- Pluralization helpers (plural, singular)
- String transformation helpers (replace, substring)

## Dependencies

- Handlebars.Net (v2.1.6)
- Pluralize.NET (v1.0.2)
- xUnit for testing

## Running the Tests

To run the tests, use the following command:

```bash
dotnet test
```

## Implementation Notes

This implementation is a standalone version of the HandlebarsUtility class that can be used for testing without dependencies on the rest of the EzDbCodeGen project. It provides the same functionality as the original HandlebarsUtility class but with a simplified implementation.

The tests validate that all string helpers work correctly with various inputs and edge cases.
