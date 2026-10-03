![alt tag](https://github.com/jchristn/urlmatcher/blob/main/assets/icon.ico)

# UrlMatcher

[![NuGet Version](https://img.shields.io/nuget/v/UrlMatcher.svg?style=flat)](https://www.nuget.org/packages/UrlMatcher/) [![NuGet](https://img.shields.io/nuget/dt/UrlMatcher.svg)](https://www.nuget.org/packages/UrlMatcher)

Simple URL matcher library allowing you to match based on explicit strings, parameters, and catch-all segments, targeted to .NET Standard 2.0/2.1, .NET Framework 4.6.2/4.8, and .NET 8.0/10.0.

## Help or Feedback

First things first - do you need help or have feedback?  File an issue here!  We'd love to hear from you.

## New in v3.1.x

- v3.1.1: maintenance release with updated test dependencies (Touchstone 0.2.0, NUnit 5, xUnit runner 4); no behavior changes
- v3.1.0:
  - Catch-all segments: `/api/{*rest}` matches `/api`, `/api/users`, and `/api/users/42/orders`
  - `UrlPattern` parses a pattern once for reuse, reports its shape for route ranking, and rejects invalid patterns up front
  - A failed match always returns an empty collection (3.0.2 could leave values captured before the failing segment)

See [CHANGELOG.md](CHANGELOG.md) for compatibility notes.

## Install

```
dotnet add package UrlMatcher
```

## Usage

### Static Method

Use the static method when you need to compare an input URL against a pattern once.

```csharp
using System.Collections.Specialized;
using UrlMatcher;

if (Matcher.Match("/v1.0/users/42", "/{version}/users/{userId}", out NameValueCollection vals))
{
    Console.WriteLine("version : " + vals["version"]);  // v1.0
    Console.WriteLine("userId  : " + vals["userId"]);   // 42
}
```

### Instance Method

Instantiate the class with a URL or `Uri` to parse it once, then match it against many patterns.

```csharp
Matcher matcher = new Matcher("/v1.0/users/42");

if (matcher.Match("/{version}/users/{userId}", out NameValueCollection vals))
{
    // vals["userId"] == "42"
}
else if (matcher.Match("/{version}/hobbies/{hobbyId}", out vals))
{
    // ...
}
```

### Catch-All Segments

A catch-all is written `{*name}` and must be the entire final segment of the pattern.  It matches zero or more remaining URL segments and captures them as the raw remainder of the URL.

```csharp
Matcher.Match("/api/users/42/orders", "/api/{*rest}", out NameValueCollection vals);  // true, rest = "users/42/orders"
Matcher.Match("/api", "/api/{*rest}", out vals);                                     // true, rest = ""
Matcher.Match("/v1/files/a/b.txt", "/{v}/files/{*path}", out vals);                  // true, v = "v1", path = "a/b.txt"
Matcher.Match("/other/users", "/api/{*rest}", out vals);                             // false
```

The captured value is the remainder of the URL exactly as supplied (after the query string and fragment are removed), so it is safe to substitute into a forwarded path:

| URL | Pattern | `rest` |
|---|---|---|
| `/api/a//b` | `/api/{*rest}` | `a//b` (repeated slashes kept) |
| `/api/a/b/` | `/api/{*rest}` | `a/b/` (trailing slash kept) |
| `/api/a%2Fb/c` | `/api/{*rest}` | `a%2Fb/c` (not decoded) |
| `/api/a/b?x=1#f` | `/api/{*rest}` | `a/b` (query and fragment removed) |

A catch-all that is not the last segment, appears more than once, or shares its segment with other text (for example `/{*rest}/edit`, `/{*a}/{*b}`, or `/files/v{*x}`) throws `ArgumentException`.

### Pre-Parsed Patterns

`UrlPattern` parses a pattern once.  Routers should parse patterns when routes are registered, so an invalid pattern fails immediately instead of on every request, and matching skips re-parsing.

```csharp
UrlPattern pattern = UrlPattern.Parse("/{v}/files/{*path}");   // throws ArgumentException if invalid

if (Matcher.Match("/v1/files/a/b.txt", pattern, out NameValueCollection vals)) { ... }
if (matcher.Match(pattern, out vals)) { ... }

if (!UrlPattern.TryParse(userSupplied, out UrlPattern parsed)) { /* reject the route */ }
```

`UrlPattern` also reports facts a router can use to rank routes: `SegmentCount`, `FixedSegmentCount`, `LiteralCount`, `ParameterCount`, `LiteralPrefixCount`, `IsCatchAll`, `CatchAllName`, and `Segments` (each with `Text`, `Type`, and `Name`).  The ranking policy itself is left to the router.

## Matching Rules

- URLs and patterns are split on `/` and empty segments are discarded, so leading, trailing, and repeated slashes do not matter (except inside a catch-all value).
- The query string and fragment are removed from the URL before matching.  They are not removed from the pattern.
- The URL and pattern must have the same number of segments, unless the pattern ends in a catch-all.
- Literal segments are compared ordinally (case-sensitive, no culture or Unicode normalization).
- Parameter names are case-insensitive.  Captured values keep their case and are not URL-decoded.
- A failed match returns `false` and an empty (never null) collection.
- Duplicate parameter names are joined with a comma by `NameValueCollection` (`/{id}/{id}` against `/a/b` gives `id` = `a,b`).

## Pattern Syntax

| Form | Example pattern | Matches (captured values) | Does not match |
|---|---|---|---|
| `{name}` | `/users/{id}` | `/users/42` (`id=42`) | `/users`, `/users/42/orders` |
| `{*name}` as the whole last segment | `/api/{*rest}` | `/api/users/42` (`rest=users/42`), `/api` (`rest=""`) | `/other/users`, `/API/users` |
| `{*name}` not last | `/{*rest}/edit` | throws `ArgumentException` | |
| More than one `{*name}` | `/{*a}/{*b}` | throws `ArgumentException` | |
| `{*name}` with other text in the segment | `/files/v{*x}` | throws `ArgumentException` | |
| `{*}` | `/files/{*}` | `/files/{*}` (literal) | `/files/42` |
| `{}` | `/files/{}` | `/files/{}` (literal) | `/files/42` |
| `*` or `**` | `/files/*` | `/files/*` (literal) | `/files/a`, `/files/a/b` |
| Unclosed brace | `/users/{id` | `/users/{id` (literal) | `/users/42` |
| Literal | `/Users/list` | `/Users/list` | `/users/list` |

Some brace forms are kept for compatibility with earlier versions and are best avoided:

| Form | Example pattern | Behavior |
|---|---|---|
| Braces inside a segment | `/v{version}/users` | `/v1/users` gives `version=v1` (the whole segment, not `1`) |
| Two groups in one segment | `/item/{a}{b}` | `/item/xy` gives `a=xy`; `b` is never set |
| Spaces in a name | `/users/{ id }` | The key is ` id `, so `vals["id"]` is null |
| Query text in a pattern | `/search?q={q}` | One segment; `/search?q=1` gives `q=search` |

## Thread Safety

`Matcher`, `UrlPattern`, and `UrlPatternSegment` instances are immutable after construction and safe to use from multiple threads concurrently.  Each call to `Match` returns a new collection.

## Testing

Tests use [Touchstone](https://github.com/jchristn/touchstone).  Test descriptors live in `src/Test.Shared` and run through three runners:

```
dotnet run --project src/Test.Automated -- --results results.json
dotnet test src/Test.Xunit
dotnet test src/Test.Nunit
```

## Version History

Please refer to [CHANGELOG.md](CHANGELOG.md).
