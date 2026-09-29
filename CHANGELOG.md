# Change Log

## Current Version

v3.1.0

- Catch-all segments: `{*name}` as the entire final segment matches zero or more remaining URL segments and captures the raw remainder of the URL (repeated and trailing slashes kept, not decoded, query and fragment removed)
- New `UrlPattern` class: parse a pattern once with `new UrlPattern`, `UrlPattern.Parse`, or `UrlPattern.TryParse`, and inspect `Segments`, `SegmentCount`, `FixedSegmentCount`, `LiteralCount`, `ParameterCount`, `LiteralPrefixCount`, `IsCatchAll`, and `CatchAllName`
- New `UrlPatternSegment` class and `SegmentTypeEnum` enum describing each parsed segment
- New overloads: `Matcher.Match(UrlPattern, out vals)`, `Matcher.Match(string, UrlPattern, out vals)`, and `Matcher.Match(Uri, UrlPattern, out vals)`
- Fix: a failed match now always returns an empty collection.  In 3.0.2, values captured before a failing literal segment (for example `version` when matching `/v1/users/42` against `/{version}/admins/{id}`) were left in the collection
- XML documentation now lists thrown exceptions and the thread-safety guarantee
- Tests migrated to Touchstone (`Test.Shared`, `Test.Automated`, `Test.Xunit`, `Test.Nunit`), expanded from 61 to 358 cases, and run in CI on .NET 8.0 and 10.0.  The interactive `Test` project and the `AutomatedTest` console project were removed

### Compatibility notes

These behavior changes only affect patterns that use an asterisk inside braces:

- `{*name}` was an ordinary single-segment parameter named `*name`.  It is now a catch-all named `name`
- `{*name}` in any position other than the entire last segment, or more than one catch-all, now throws `ArgumentException` (from `Match` and from `UrlPattern`).  Routers should parse patterns with `UrlPattern.Parse` when routes are registered so the error surfaces once
- `{*}` was a parameter named `*`.  It is now a literal, consistent with `{}`

## Previous Versions

v3.0.2

- Performance optimizations: ~30% faster with reduced memory allocations
- Security fix: `Parts` property now returns a copy to prevent external mutation of internal state
- Improved query string and fragment stripping logic using `IndexOf` instead of `Split`
- Optimized `ExtractParameter` method to return parameter names without braces directly
- Enhanced XML documentation clarifying case-sensitivity, URL-encoding behavior, and parameter handling
- Modern C# syntax improvements (expression-bodied members, `StringComparison.Ordinal`)
- Removed unnecessary array allocations in `Split` calls
- Fixed logic inconsistency: `NameValueCollection` is now empty (not null) on match failure

## Previous Versions

v3.0.1

- XML documentation file generation
- Retargeting updates

v3.0.0

- Refactor to support both static methods and instances
- Instances take either a `Uri` or `string` (URL)

v2.0.x

- Migrate to `NameValueCollection` instead of `Dictionary`

v1.0.x

- Initial release