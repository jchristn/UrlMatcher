![alt tag](https://github.com/jchristn/urlmatcher/blob/main/assets/icon.ico)

# UrlMatcher

[![NuGet Version](https://img.shields.io/nuget/v/UrlMatcher.svg?style=flat)](https://www.nuget.org/packages/UrlMatcher/) [![NuGet](https://img.shields.io/nuget/dt/UrlMatcher.svg)](https://www.nuget.org/packages/UrlMatcher) 

Simple URL matcher library allowing you to match based on explicit string or parameters, targeted to .NET Core, .NET Standard, and .NET Framework.

## Help or Feedback

First things first - do you need help or have feedback?  File an issue here!  We'd love to hear from you.

## New in v3.0.0

- Refactor to support both static methods and instances
- Instances take either a `Uri` or `string` (URL)

## It's Really Easy...  I Mean, REALLY Easy

Refer to the ```Test``` project for a working example.

### Simple Static Method

Use the static method when you need to compare an input URL against a pattern once.

```csharp
using UrlMatcher;

string pattern = "/{version}/users/{userId}";
NameValueCollection vals = null;

if (Matcher.Match("/v1.0/users/42", pattern, out vals))
{
  Console.WriteLine("Match!");
  Console.WriteLine("  version : " + vals["version"]);  // v1.0
  Console.WriteLine("  userId  : " + vals["userId"]);   // 42
}
else
{
  Console.WriteLine("No match");
}
```

### Instance Method

Instantiate the class by supplying the URI or URL.  Using the instance method eliminates the need to repeatedly parse the input URI or URL.

```csharp
using UrlMatcher;

Matcher matcher = new Matcher("/v1.0/users/42");
NameValueCollection vals = null;

if (matcher.Match("/{version}/users/{userId}"))          // do something
else if (matcher.Match("/{version}/hobbies/{hobbyId}"))  // do something
else
{
  Console.WriteLine("No match");
}
```

## Version History

Please refer to CHANGELOG.md.
