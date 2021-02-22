# UrlMatcher

[![NuGet Version](https://img.shields.io/nuget/v/UrlMatcher.svg?style=flat)](https://www.nuget.org/packages/UrlMatcher/) [![NuGet](https://img.shields.io/nuget/dt/UrlMatcher.svg)](https://www.nuget.org/packages/UrlMatcher) 

Simple URL matcher library allowing you to match based on explicit string or parameters, targeted to .NET Core, .NET Standard, and .NET Framework.

## Help or Feedback

First things first - do you need help or have feedback?  File an issue here!  We'd love to hear from you.

## New in v1.0.0

- Initial release

## It's Really Easy...  I Mean, REALLY Easy

Refer to the ```Test``` project for a working example.

```csharp
using UrlMatcher;

Matcher matcher = new Matcher();
string pattern = "/{version}/foo/{whatever}/bar";
Dictionary<string, string> vals = null;

if (parser.Match("/v1.0/foo/woohoo/bar", pattern, out vals))
{
  Console.WriteLine("Match!");
  Console.WriteLine("  version  : " + vals["version"]);  // v1.0
  Console.WriteLine("  whatever : " + vals["whatever"]); // woohoo
}
else
{
  Console.WriteLine("No match");
}
```

## Version History

Please refer to CHANGELOG.md.
