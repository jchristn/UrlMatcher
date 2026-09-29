# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UrlMatcher is a .NET library published to NuGet that provides URL pattern matching with parameter extraction and catch-all segments, for patterns like `/{version}/users/{userId}` and `/api/{*rest}`.  Watson Webserver (`ParameterRouteManager`, `WebSocketRouteManager`) and Switchboard depend on it, so behavior changes must be checked against those callers.

## Build and Test

```bash
dotnet build src/UrlMatcher.sln
dotnet run --project src/Test.Automated -- --results results.json
dotnet test src/Test.Xunit
dotnet test src/Test.Nunit
dotnet build src/UrlMatcher/UrlMatcher.csproj -c Release   # produces .nupkg and .snupkg (GeneratePackageOnBuild)
```

## Architecture

### Library (`src/UrlMatcher/`)

- `Matcher.cs`: static and instance `Match` overloads for `string` and `Uri` URLs against `string` or `UrlPattern` patterns.  An instance splits its URL once and records each segment's start offset so a catch-all can return the raw remainder with one `Substring`.
- `UrlPattern.cs`: parses a pattern once (`new UrlPattern`, `Parse`, `TryParse`), validates catch-all placement, and exposes shape properties for route ranking.
- `UrlPatternSegment.cs` and `SegmentTypeEnum.cs`: one parsed segment (`Literal`, `Parameter`, `CatchAll`).

Matching rules: split on `/` discarding empty segments; strip `?` and `#` from the URL only; literal segments compare ordinally; parameter names are case-insensitive (`StringComparer.InvariantCultureIgnoreCase`); a failed match returns an empty collection; a catch-all `{*name}` must be the entire last segment and matches zero or more segments.  `{}`, `{*}`, `*`, `**`, and unclosed braces are literals.  The whole-segment capture for `v{x}` and first-group-only rule for `{a}{b}` are retained quirks.

### Tests (Touchstone)

- `src/Test.Shared`: all descriptors (Touchstone.Core only, no console output).  `UrlMatcherSuites.All` aggregates one static class per suite in `Suites/`.  `MatchVerifier` runs each match case through all four string entry points (or all four `Uri` entry points), so every case also proves overload equivalence.  `CaseFactory` builds `Match`, `NoMatch`, and `Throws` descriptors.
- `src/Test.Automated`: `ConsoleRunner`, supports `--results <path>`.
- `src/Test.Xunit`, `src/Test.Nunit`: fact-style and per-case adapters.  xUnit files need `using global::Xunit;` because of the `Test.Xunit` namespace.
- Test projects target `net8.0;net10.0` only (Touchstone has no .NET Framework build).  CI builds the library for every target framework in Release.

### Multi-Targeting

The library targets `netstandard2.0;netstandard2.1;net462;net48;net8.0;net10.0`.  Keep library code to C# 7.3 features and APIs available on `netstandard2.0`.  Nullable reference types are not enabled in the library.

## Code Style (from CODE_STYLE.md)

- Namespace first; `using` statements inside the namespace, System usings first, then others, each alphabetical
- XML documentation on all public members, including `<exception>` tags and thread-safety notes; none on private members
- Private fields are `_PascalCase`; no `var`; no tuples; one class or enum per file
- Specific exception types with messages that include the offending input
- Regions (`Public-Members`, `Private-Members`, `Constructors-and-Factories`, `Public-Methods`, `Private-Methods`) in library classes
- No `Console.Write*` in library or Test.Shared code
- Never use em-dashes in code, comments, or docs
- Do not change the version number unless the user explicitly asks (VERSIONING.md)
