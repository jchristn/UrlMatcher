# UrlMatcher Improvements Plan

UrlMatcher needs two things before its next release: a catch-all segment, so a pattern like `/api/{*rest}` can match every path under a prefix, and a test suite that follows the Touchstone architecture in `c:\code\agents\requirements\BACKEND_TEST_ARCHITECTURE.md`. The tests move first. The current suite is a single 1,362 line console program, and I want its 61 cases running green under Touchstone before the matching algorithm changes, so any regression the feature causes shows up against a known baseline.

The catch-all request comes from Switchboard. Switchboard routes every proxied request through `Matcher.Match` (in `GatewayService.FindApiEndpoint` and `UrlTools.RewriteUrl`), and it has no way to express "everything under `/api`" today. Watson 7.1.0 also depends on UrlMatcher 3.0.2, so whatever ships here reaches Watson users as well.

## Where Things Stand

`Matcher` splits the URL and the pattern on `/`, drops empty segments, and compares them position by position. A segment containing `{name}` captures exactly one URL segment. Every other segment has to match exactly, and the comparison is ordinal (case-sensitive). The URL and the pattern must have the same number of segments, or the match fails before any comparison runs.

I probed 3.0.2 directly against the URL `/a/b/c`. Every candidate catch-all syntax failed: `/`, `/*`, `/**`, `/{*rest}`, `/a/*`, and `/a/{rest}` all returned false. `/{*rest}` fails only because the segment counts differ. Against a one-segment URL such as `/a`, the current code treats `{*rest}` as an ordinary parameter named `*rest` and captures `a`. That detail matters for compatibility, and the design below accounts for it.

Testing is split across two projects, and neither uses Touchstone:

| Project | What it is | Fate |
|---|---|---|
| `src/AutomatedTest` | Console program with 61 hand-rolled cases, a `TestDetail` result type, and its own summary printer. Targets `net462;net48;net8.0;net10.0`. | Ported case by case into `Test.Shared`, then deleted. |
| `src/Test` | Interactive REPL built on Inputty that prompts for a pattern and a URL. Not a test. | Deleted. It has no assertions, and a README example serves the same purpose. If you want to keep it as a playground, rename it `src/Sample` instead. |

`CLAUDE.md` is stale as well: it still says the version is 3.0.1, cites line numbers in `Matcher.cs` that have moved, and describes only the REPL. It gets rewritten in Phase 4.

## Phase 1: Migrate All Tests to Touchstone

Phase 1 changes no behavior. It ends when the same 61 cases pass through the console runner, xUnit, and NUnit, and the old projects are gone.

### Project layout

The four projects follow the requirements layout exactly, using Touchstone 0.1.12 as that document specifies:

```
src/
  UrlMatcher/            library (unchanged in this phase)
  Test.Shared/           Touchstone.Core + ProjectReference to UrlMatcher. All descriptors. No console output.
  Test.Automated/        Touchstone.Cli ConsoleRunner. Supports --results <path>.
  Test.Xunit/            Touchstone.XunitAdapter. UrlMatcherFactTests + UrlMatcherTheoryTests.
  Test.Nunit/            Touchstone.NunitAdapter. UrlMatcherNunitFactTests + UrlMatcherNunitTests.
```

Every test project uses `<TargetFrameworks>net8.0;net10.0</TargetFrameworks>`, `<ImplicitUsings>disable</ImplicitUsings>`, and `<Nullable>enable</Nullable>`, matching the templates in the requirements document. `UrlMatcher.sln` drops `Test` and `AutomatedTest` and adds the four new projects.

Two practical notes carry over from the Switchboard migration. First, a namespace called `Test.Xunit` collides with the `Xunit` namespace, so the xUnit files need `using global::Xunit;`. Second, the Touchstone packages only ship `net8.0` and `net10.0` assets, which is the one real tradeoff in this phase and is covered next.

### The .NET Framework coverage question

`AutomatedTest` currently runs on `net462` and `net48`. The Touchstone projects cannot, because Touchstone has no .NET Framework build. The library is a single file of string handling with no conditional compilation and no framework-specific APIs, so running the suite on `net8.0` and `net10.0` exercises the same logic. I am comfortable dropping runtime test coverage on .NET Framework. The library keeps all six target frameworks, and CI builds every one of them in Release so a compile break on `netstandard2.0` or `net462` still fails the pipeline.

If you would rather keep Framework runtime coverage, the fallback is a small `net48` smoke project outside the Touchstone structure. I would not build it unless a Framework-specific bug ever appears.

### Suite structure

Each suite lives in its own file with one static class, per the one-class-per-file rule in `CODE_STYLE.md`. `UrlMatcherSuites.All` aggregates them. Assertions throw a dedicated `AssertionFailedException` (its own file) instead of a bare `Exception`, and a small static `Check` class wraps the common assertions (`True`, `False`, `Equal`, `Throws<T>`) with messages that include the URL, the pattern, and the captured values. The per-case `TestDetail` diagnostics the old runner printed become the exception message, because Test.Shared must not write to the console.

The 61 existing cases map into nine suites with no case dropped:

| Suite id | Cases | Existing tests |
|---|---|---|
| `Matching` | 11 | BasicStaticMatch Success/Failure, BasicInstanceMatch Success/Failure, MultiplePatterns_InstanceMethod, NoParameters, LiteralMismatch_MiddlePart, NumericVsLiteralNoMatch, MixedLiteralAndParamNegative, CaseSensitivity_LiteralParts, ComplexRealWorldExample |
| `Parameters` | 11 | ParameterExtraction Single/Multiple, CaseInsensitivity_ParameterNames, ParameterAtStart, ParameterAtEnd, AllParameters, ParameterNameWithUnderscores, ParameterNameWithNumbers, DuplicateParameterNames, ParameterValueCasePreserved, EmptyValuesCollectionOnFailure |
| `Segments` | 13 | DifferentPartCounts MoreUrlParts/MorePatternParts, LeadingSlashHandling, TrailingSlashHandling, NoSlashes, RootPath, SinglePartUrl, ManyPartsUrl, LongUrl, ConsecutiveSlashesCollapsed, DotSegmentsAsLiterals, LongerPatternNegative, ParameterCannotMatchMissingPart |
| `QueryAndFragment` | 5 | QueryStringStripping, FragmentStripping, QueryAndFragmentStripping, QueryStringWithoutPath, FragmentWithoutPath |
| `Uri` | 4 | UriConstructor, UriStaticMethod, UriWithQueryAndFragment, InstanceUriPartsExtraction |
| `Characters` | 7 | UrlEncodedValues, SpecialCharactersInLiteral, SpecialCharactersInParameter, PatternWithSpaces, UrlWithSpaces, UnicodeCharactersInLiteral, UnicodeCharactersInParameterValue |
| `Parts` | 2 | PartsProperty ReturnsCopy/ArrayImmutability |
| `ArgumentValidation` | 5 | NullUrl, EmptyUrl, NullPattern, EmptyPattern, NullUri |
| `MalformedPatterns` | 3 | EmptyParameterRejection, MalformedPattern MissingCloseBrace/MissingOpenBrace |

Case ids stay close to the old method names (`Test_RootPath` becomes `Segments.RootPath`) so a failure in the new runner is easy to trace back to the history of the old one.

A straight port would carry over a few weak assertions, and I would fix them during the move rather than preserve them. `DuplicateParameterNames` only checks that `vals["id"]` is non-null; it should assert the actual value (`NameValueCollection` joins duplicates as `foo,bar`) so any change to that behavior gets caught. `EmptyParameterRejection` actually tests that `{}` is treated as a literal, so it becomes `MalformedPatterns.EmptyBracesAreLiteral`. Every negative case should also assert that `vals` is empty (not null), because 3.0.2 made that a documented guarantee.

### Runners and CI

`Test.Automated/Program.cs` is the template from the requirements document with `UrlMatcherSuites.All` substituted, and it returns the `ConsoleRunner` exit code (0 when everything passes, 1 on any failure). The xUnit and NUnit projects each get the fact-style class and the per-case class from the templates.

The repository has no `.github` directory today. Phase 1 adds `.github/workflows/tests.yaml` (the `.yaml` extension is required by `REPOSITORY_REQUIREMENTS.md`) based on the workflow in the test requirements: set up .NET 8 and 10, restore and build `src/UrlMatcher.sln`, run `Test.Automated` with `--results results.json`, run `dotnet test` on the xUnit and NUnit projects, and upload the results file. The build step builds the library in Release across all six target frameworks, which covers the .NET Framework compile check described above.

**Done when:** all 61 cases pass in all three runners on both TFMs, `AutomatedTest` and `Test` are deleted, and CI is green.

## Phase 2: Catch-All Segments

### Syntax and semantics

A catch-all is written `{*name}` and must be the entire final segment of the pattern. It matches zero or more remaining URL segments and captures them under `name`.

| Pattern | URL | Result |
|---|---|---|
| `/api/{*rest}` | `/api/users/42/orders` | match, `rest` = `users/42/orders` |
| `/api/{*rest}` | `/api/users` | match, `rest` = `users` |
| `/api/{*rest}` | `/api` | match, `rest` = empty string |
| `/api/{*rest}` | `/other/users` | no match |
| `/api/{*rest}` | `/API/users` | no match (literals stay case-sensitive) |
| `/{v}/files/{*path}` | `/v1/files/a/b.txt` | match, `v` = `v1`, `path` = `a/b.txt` |
| `/{*path}` | `/` | match, `path` = empty string |
| `/{*rest}/edit` | any | `ArgumentException`: catch-all must be the last segment |

I chose zero-or-more rather than one-or-more because the common use is "this prefix and everything under it". Requiring at least one segment would force users to register `/api` and `/api/{*rest}` separately to cover the prefix itself, which is the sort of footgun a catch-all is supposed to remove. A later `{+name}` variant for one-or-more is possible, and nothing here blocks it, but I would not build it until someone asks.

Bare `*` and `**` stay literal. A literal `*` segment is legal in a URL, the 3.0.2 behavior treats it as one, and the braces keep the syntax consistent with the existing `{name}` form.

### The captured value is the raw remainder

The captured value is the substring of the original URL, after the query and fragment are stripped, starting at the first unmatched segment. It is not the remaining segments rejoined with `/`. The difference shows up at the edges:

- `/api/a//b` captures `a//b` (joining would give `a/b`)
- `/api/a/b/` captures `a/b/` (joining would drop the trailing slash)
- `/api/a%2Fb/c` captures `a%2Fb/c`, undecoded, consistent with how parameters already behave

Switchboard is the reason this matters. A rewrite like `/legacy/{*rest}` to `/v2/{rest}` substitutes the captured value into the forwarded path, and a proxy that silently collapses slashes or drops a trailing slash will break origins that care about either. The literal and parameter segments before the catch-all keep the existing collapsed-slash matching, so only the captured remainder is raw.

The implementation needs the start offset of each URL segment. The constructor currently produces `_Parts` with `String.Split`. It would also record an `int[]` of segment start offsets from a single scan, so capturing the remainder is one `Substring` call at match time. The static `Match(string, string, ...)` overload computes the same offsets locally. Patterns without a catch-all never read the offsets, so their cost is one extra integer array at construction.

### Changes to `MatchInternal`

The segment-count check is the only structural change. Today it is an early `return false` whenever the counts differ. The new rule:

1. If the last pattern segment is exactly `{*name}` (with a non-empty name), treat the pattern as a catch-all with `N` fixed segments before it. The URL must have at least `N` segments.
2. Otherwise the counts must be equal, exactly as today.
3. The fixed segments match by the existing rules. The catch-all then adds `name` to `vals` with the raw remainder.

Validation throws `ArgumentException` with a message naming the pattern for a catch-all in any position other than last, and for more than one catch-all. Silently treating a misplaced catch-all as a literal would reproduce the "my route just never matches" failure this feature exists to fix.

### Compatibility

One behavior changes. In 3.0.2, `{*rest}` is an ordinary parameter named `*rest`. After this change it captures the remainder, and it throws when it is not the last segment. Anyone who deliberately named a parameter with a leading asterisk would notice. I think that population is empty, but it is still a semantic change and belongs in the changelog under its own heading.

Everything else stays as it is. `{}` and `{*}` are literals (a catch-all needs a name, consistent with empty braces today). A partial segment like `v{x}` keeps its current quirk of treating the whole segment as parameter `x`. That quirk is worth revisiting some day, but not in the same release as catch-all.

### Versioning

Per `VERSIONING.md`, I will not touch the version number. My recommendation is **3.1.0**: the feature is additive, and the only behavior change affects a parameter-naming pattern nobody should be relying on. If you would rather treat the `{*` reinterpretation as breaking, the alternative is 4.0.0. Either way, the version, `PackageReleaseNotes`, and the changelog heading change only after you approve.

### New test suite

A new `CatchAll` suite in `Test.Shared` covers:

- every row of the semantics table above, including the `ArgumentException` cases, with exception messages checked for the pattern text
- the three raw-remainder edges (double slash, trailing slash, encoded slash)
- query and fragment stripping ahead of the capture (`/api/a/b?x=1#f` captures `a/b`)
- case-insensitive lookup of the catch-all name (`vals["REST"]`)
- the `Uri` overloads and a reused instance matched against several patterns, one of them a catch-all
- two catch-alls in one pattern (throws)
- `{*}` treated as a literal
- a regression case asserting `Parts` is unaffected by the offset bookkeeping

All 61 Phase 1 cases must still pass unchanged. That is the whole reason they moved first.

## Phase 3: Precompiled Patterns (Recommended)

Phases 1 and 2 are enough to unblock Switchboard. Phase 3 makes the result pleasant to use in a router, and I would ship it in the same release if the time is available.

Every call to `Match` re-splits the pattern string, and a router like Switchboard's calls it once per configured route on every request. A public `UrlPattern` class (its own file) would parse once and expose what a router needs in order to rank routes:

- `UrlPattern.Parse(string pattern)`, which throws the same `ArgumentException` rules as Phase 2
- `Pattern` (original text), `SegmentCount`, `LiteralCount`, `ParameterCount`, `IsCatchAll`, and `CatchAllName`
- `Matcher.Match(UrlPattern pattern, out NameValueCollection vals)` as a new overload; the string overloads stay and delegate to it

With those properties, Switchboard can sort routes by specificity (more literals first, catch-alls last, longer literal prefixes before shorter ones) without writing its own parser. The ranking policy itself stays in Switchboard, because precedence is a routing decision, not a matching one. UrlMatcher only reports the facts. The `UrlPattern` suite would verify parse results and the new overload's equivalence with the string overload across every Matching, Parameters, and CatchAll case.

## Phase 4: Documentation and Release Preparation

- **README.md:** add a catch-all section with the semantics table, the raw-remainder behavior, and (if Phase 3 lands) a `UrlPattern` example. Replace any reference to the REPL with a short code sample.
- **CHANGELOG.md:** add an entry covering the Touchstone migration, catch-all support, the `{*` compatibility note, and `UrlPattern` if included. The version heading waits for your approval.
- **CLAUDE.md:** rewrite it to describe the current architecture, the four test projects and how to run them, and the code style rules in `CODE_STYLE.md`, which that file already asks to be recorded there. Drop the hard-coded line numbers.
- **XML docs:** document the catch-all rules, the exceptions thrown (`<exception>` tags), and the thread-safety guarantee on `Matcher` and `UrlPattern` (instances are immutable after construction, so matching from multiple threads is safe).

## Phase 5: Adoption in Switchboard

Switchboard gets UrlMatcher transitively through Watson 7.1.0. Once the new version is on NuGet, Switchboard adds an explicit `PackageReference` to it in `Switchboard.Core.csproj`. A direct reference wins over the transitive one, so there is no need to wait for a Watson release. Route precedence and any-method routes (points 2 and 3 of the Switchboard analysis) are separate work in that repository and are not part of this plan.

Watson itself also picks up the new semantics whenever it moves to the new UrlMatcher. I have not confirmed how Watson 7.x uses the matcher internally, so before Watson updates, someone should check whether any Watson parameter route could contain `{*`.

## Other Gaps Noticed

A few things in the library do not meet `CODE_STYLE.md`. None of them block the catch-all work, and I would keep them out of this change set so the diff stays reviewable. The library project does not enable nullable reference types, and turning it on for a `netstandard2.0` target means annotating the public surface carefully. Public methods lack `<exception>` documentation (Phase 4 adds it for anything touched). The repository has no `DOCKERHUB_README.md`, but it ships no container, so that requirement does not apply.

## Open Decisions

I made six calls in this plan that you may want to overrule before work starts:

1. **Zero-or-more** for `{*name}`, so `/api/{*rest}` matches `/api`.
2. **Raw remainder** for the captured value, rather than segments rejoined with `/`.
3. **Throw** on a misplaced or duplicate catch-all, rather than treating it as a literal.
4. **Drop runtime tests on .NET Framework**, keeping a Release build of every target framework in CI.
5. **Delete `src/Test`** rather than keeping it as a sample.
6. **3.1.0** as the proposed version, pending your approval.

Order matters more than any of those choices. Get the 61 cases green under Touchstone first, and the catch-all change becomes something a reviewer can trust.
