# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UrlMatcher is a .NET library published to NuGet that provides URL pattern matching with parameter extraction. The library supports both static and instance methods for matching URLs/URIs against patterns like `/{version}/users/{userId}`.

## Build Commands

Build the entire solution:
```bash
dotnet build src/UrlMatcher.sln
```

Build specific projects:
```bash
dotnet build src/UrlMatcher/UrlMatcher.csproj
dotnet build src/Test/Test.csproj
```

Build for specific frameworks:
```bash
dotnet build src/UrlMatcher/UrlMatcher.csproj -f net8.0
dotnet build src/UrlMatcher/UrlMatcher.csproj -f netstandard2.1
```

Create NuGet package (happens automatically on build):
```bash
dotnet build src/UrlMatcher/UrlMatcher.csproj -c Release
```

Run the test application:
```bash
dotnet run --project src/Test/Test.csproj
```

## Architecture

### Core Library (`src/UrlMatcher/`)

The library consists of a single class `Matcher` in `Matcher.cs` with two usage patterns:

1. **Static methods** (`Matcher.Match()`): Use for one-time URL matching against a pattern
2. **Instance methods** (`new Matcher(url).Match(pattern)`): Use when matching the same URL against multiple patterns to avoid re-parsing

### Pattern Matching Logic

- URLs are split by `/` into parts (query strings and fragments removed via `Split('?', '#')`)
- Patterns use curly braces for parameters: `/{version}/users/{userId}`
- Parts are matched positionally - URL and pattern must have same number of parts
- Pattern parts without `{}` must match exactly (case-sensitive)
- Pattern parts with `{}` extract the URL value into a `NameValueCollection` (case-insensitive keys)
- Key methods:
  - `MatchInternal()` (line 140): Core matching algorithm comparing URL parts to pattern parts
  - `ExtractParameter()` (line 170): Extracts parameter name from `{param}` syntax

### Multi-Targeting

The library targets multiple frameworks defined in `UrlMatcher.csproj`:
- .NET Standard 2.0, 2.1 (broad compatibility)
- .NET Framework 4.6.2, 4.8 (legacy support)
- .NET 8.0, 10.0 (modern .NET)

The Test and AutomatedTest projects target: `net462`, `net48`, `net8.0`, `net10.0`

### NuGet Package

- Package generation is automatic on build (`GeneratePackageOnBuild`)
- Version is specified in `UrlMatcher.csproj` (currently 3.0.1)
- XML documentation is generated automatically (`GenerateDocumentationFile`)
- Strong naming is used (previous commit history)

### Test Application

`src/Test/Program.cs` provides an interactive console app that:
- Prompts for pattern and URL inputs (using Inputty library)
- Tests both static and instance methods
- Displays extracted parameter values
- Runs in an infinite loop for repeated testing

## Code Conventions

- Namespace matches project name: `UrlMatcher`, `Test`
- Region organization: `#region Public-Members`, `#region Private-Members`, `#region Constructors-and-Factories`, `#region Public-Methods`, `#region Private-Methods`
- Private fields prefixed with underscore: `_Url`, `_Parts`
- XML documentation comments required (enforced by build)
- `StringComparer.InvariantCultureIgnoreCase` used for parameter name matching