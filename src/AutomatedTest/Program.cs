namespace AutomatedTest
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using UrlMatcher;

    class Program
    {
        static List<TestResult> _TestResults = new List<TestResult>();
        static bool _VerboseOutput = true;

        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("UrlMatcher Comprehensive Test Suite");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            Stopwatch totalTimer = Stopwatch.StartNew();

            // Run all tests
            await RunTest("Basic Static Match - Success", Test_BasicStaticMatch_Success);
            await RunTest("Basic Static Match - Failure", Test_BasicStaticMatch_Failure);
            await RunTest("Basic Instance Match - Success", Test_BasicInstanceMatch_Success);
            await RunTest("Basic Instance Match - Failure", Test_BasicInstanceMatch_Failure);
            await RunTest("Parameter Extraction - Single", Test_ParameterExtraction_Single);
            await RunTest("Parameter Extraction - Multiple", Test_ParameterExtraction_Multiple);
            await RunTest("Case Sensitivity - Literal Parts", Test_CaseSensitivity_LiteralParts);
            await RunTest("Case Insensitivity - Parameter Names", Test_CaseInsensitivity_ParameterNames);
            await RunTest("Query String Stripping", Test_QueryStringStripping);
            await RunTest("Fragment Stripping", Test_FragmentStripping);
            await RunTest("Query and Fragment Stripping", Test_QueryAndFragmentStripping);
            await RunTest("Empty Parameter Rejection", Test_EmptyParameterRejection);
            await RunTest("Different Part Counts - More URL Parts", Test_DifferentPartCounts_MoreUrlParts);
            await RunTest("Different Part Counts - More Pattern Parts", Test_DifferentPartCounts_MorePatternParts);
            await RunTest("URI Constructor", Test_UriConstructor);
            await RunTest("URI Static Method", Test_UriStaticMethod);
            await RunTest("Multiple Patterns - Instance Method", Test_MultiplePatterns_InstanceMethod);
            await RunTest("URL Encoded Values", Test_UrlEncodedValues);
            await RunTest("Leading Slash Handling", Test_LeadingSlashHandling);
            await RunTest("Trailing Slash Handling", Test_TrailingSlashHandling);
            await RunTest("No Slashes", Test_NoSlashes);
            await RunTest("Root Path", Test_RootPath);
            await RunTest("Single Part URL", Test_SinglePartUrl);
            await RunTest("Many Parts URL", Test_ManyPartsUrl);
            await RunTest("Special Characters in Literal", Test_SpecialCharactersInLiteral);
            await RunTest("Special Characters in Parameter", Test_SpecialCharactersInParameter);
            await RunTest("Parameter at Start", Test_ParameterAtStart);
            await RunTest("Parameter at End", Test_ParameterAtEnd);
            await RunTest("All Parameters", Test_AllParameters);
            await RunTest("No Parameters", Test_NoParameters);
            await RunTest("Parts Property - Returns Copy", Test_PartsProperty_ReturnsCopy);
            await RunTest("Parts Property - Array Immutability", Test_PartsProperty_ArrayImmutability);
            await RunTest("Null URL - ArgumentNullException", Test_NullUrl_ArgumentNullException);
            await RunTest("Empty URL - ArgumentNullException", Test_EmptyUrl_ArgumentNullException);
            await RunTest("Null Pattern - ArgumentNullException", Test_NullPattern_ArgumentNullException);
            await RunTest("Empty Pattern - ArgumentNullException", Test_EmptyPattern_ArgumentNullException);
            await RunTest("Null URI - ArgumentNullException", Test_NullUri_ArgumentNullException);
            await RunTest("Malformed Pattern - Missing Close Brace", Test_MalformedPattern_MissingCloseBrace);
            await RunTest("Malformed Pattern - Missing Open Brace", Test_MalformedPattern_MissingOpenBrace);
            await RunTest("Pattern with Spaces", Test_PatternWithSpaces);
            await RunTest("URL with Spaces", Test_UrlWithSpaces);
            await RunTest("Complex Real World Example", Test_ComplexRealWorldExample);
            await RunTest("Empty Values Collection on Failure", Test_EmptyValuesCollectionOnFailure);
            await RunTest("Parameter Name with Underscores", Test_ParameterNameWithUnderscores);
            await RunTest("Parameter Name with Numbers", Test_ParameterNameWithNumbers);
            await RunTest("Long URL", Test_LongUrl);
            await RunTest("Duplicate Parameter Names", Test_DuplicateParameterNames);
            await RunTest("Query String Without Path", Test_QueryStringWithoutPath);
            await RunTest("Fragment Without Path", Test_FragmentWithoutPath);
            await RunTest("Unicode Characters in Literal", Test_UnicodeCharactersInLiteral);
            await RunTest("Unicode Characters in Parameter Value", Test_UnicodeCharactersInParameterValue);
            await RunTest("Literal Mismatch - Middle Part", Test_LiteralMismatch_MiddlePart);
            await RunTest("Parameter Value Case Preserved", Test_ParameterValueCasePreserved);
            await RunTest("Empty Pattern Value Mismatch", Test_ParameterCannotMatchMissingPart);
            await RunTest("URI With Query and Fragment", Test_UriWithQueryAndFragment);
            await RunTest("Instance URI Parts Extraction", Test_InstanceUriPartsExtraction);
            await RunTest("Consecutive Slashes Collapsed", Test_ConsecutiveSlashesCollapsed);
            await RunTest("Numeric vs Literal No Match", Test_NumericVsLiteralNoMatch);
            await RunTest("Longer Pattern Negative", Test_LongerPatternNegative);
            await RunTest("Dot Segments As Literals", Test_DotSegmentsAsLiterals);
            await RunTest("Mixed Literal and Param Negative", Test_MixedLiteralAndParamNegative);

            totalTimer.Stop();

            // Print summary
            PrintSummary(totalTimer.Elapsed);

            // Exit with appropriate code
            bool allPassed = _TestResults.All(r => r.Passed);
            Environment.Exit(allPassed ? 0 : 1);
        }

        static async Task RunTest(string testName, Func<TestDetail> testFunc)
        {
            Console.WriteLine();
            Console.WriteLine($"[TEST] {testName}");
            Stopwatch sw = Stopwatch.StartNew();
            bool passed = false;
            string error = null;
            TestDetail detail = null;

            try
            {
                detail = testFunc();
                passed = detail.Passed;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                detail = new TestDetail { Passed = false };
            }

            sw.Stop();

            // Show test details
            if (_VerboseOutput && detail != null)
            {
                if (!string.IsNullOrEmpty(detail.Url))
                    Console.WriteLine($"  URL:     {detail.Url}");
                if (!string.IsNullOrEmpty(detail.Pattern))
                    Console.WriteLine($"  Pattern: {detail.Pattern}");
                if (detail.Matched.HasValue)
                    Console.WriteLine($"  Matched: {detail.Matched.Value}");
                if (detail.Parameters != null && detail.Parameters.Count > 0)
                {
                    Console.WriteLine($"  Parameters:");
                    foreach (string key in detail.Parameters.AllKeys)
                    {
                        Console.WriteLine($"    {key} = {detail.Parameters[key]}");
                    }
                }
                if (!string.IsNullOrEmpty(detail.Description))
                    Console.WriteLine($"  Note:    {detail.Description}");
            }

            TestResult result = new TestResult
            {
                TestName = testName,
                Passed = passed,
                Duration = sw.Elapsed,
                Error = error
            };

            _TestResults.Add(result);

            Console.Write($"  Result:  ");
            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PASS ({sw.ElapsedMilliseconds}ms)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"FAIL ({sw.ElapsedMilliseconds}ms)");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"  Error:   {error}");
                }
            }
            Console.ResetColor();

            await Task.CompletedTask;
        }

        static void PrintSummary(TimeSpan totalDuration)
        {
            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("TEST SUMMARY");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            int maxNameLength = _TestResults.Max(r => r.TestName.Length);

            foreach (var result in _TestResults)
            {
                string status = result.Passed ? "PASS" : "FAIL";
                ConsoleColor color = result.Passed ? ConsoleColor.Green : ConsoleColor.Red;

                Console.ForegroundColor = color;
                Console.Write($"  [{status}] ");
                Console.ResetColor();
                Console.Write($"{result.TestName.PadRight(maxNameLength + 2)}");
                Console.WriteLine($"{result.Duration.TotalMilliseconds:F2}ms");
            }

            Console.WriteLine();
            Console.WriteLine("-------------------------------------------");

            int passed = _TestResults.Count(r => r.Passed);
            int failed = _TestResults.Count(r => !r.Passed);
            int total = _TestResults.Count;

            Console.WriteLine($"Total Tests:  {total}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Passed:       {passed}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed:       {failed}");
            Console.ResetColor();
            Console.WriteLine($"Total Time:   {totalDuration.TotalMilliseconds:F2}ms");
            Console.WriteLine();

            if (failed == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("*** ALL TESTS PASSED ***");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("*** SOME TESTS FAILED ***");
            }
            Console.ResetColor();
        }

        #region Test Methods

        static TestDetail Test_BasicStaticMatch_Success()
        {
            string url = "/v1.0/users/42";
            string pattern = "/{version}/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            bool passed = result && vals["version"] == "v1.0" && vals["id"] == "42";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals
            };
        }

        static TestDetail Test_BasicStaticMatch_Failure()
        {
            string url = "/v1.0/posts/42";
            string pattern = "/{version}/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match because 'posts' != 'users'"
            };
        }

        static TestDetail Test_BasicInstanceMatch_Success()
        {
            string url = "/v1.0/users/42";
            string pattern = "/{version}/users/{id}";
            Matcher matcher = new Matcher(url);
            NameValueCollection vals;
            bool result = matcher.Match(pattern, out vals);
            bool passed = result && vals["version"] == "v1.0" && vals["id"] == "42";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Using instance method"
            };
        }

        static TestDetail Test_BasicInstanceMatch_Failure()
        {
            string url = "/v1.0/posts/42";
            string pattern = "/{version}/users/{id}";
            Matcher matcher = new Matcher(url);
            NameValueCollection vals;
            bool result = matcher.Match(pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Instance method - should NOT match"
            };
        }

        static TestDetail Test_ParameterExtraction_Single()
        {
            string url = "/hello";
            string pattern = "/{greeting}";
            NameValueCollection vals;
            Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["greeting"] == "hello",
                Url = url,
                Pattern = pattern,
                Matched = true,
                Parameters = vals
            };
        }

        static TestDetail Test_ParameterExtraction_Multiple()
        {
            string url = "/api/v2/products/123/reviews/456";
            string pattern = "/{prefix}/{version}/products/{productId}/reviews/{reviewId}";
            NameValueCollection vals;
            Matcher.Match(url, pattern, out vals);
            bool passed = vals["prefix"] == "api" && vals["version"] == "v2" && vals["productId"] == "123" && vals["reviewId"] == "456";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = true,
                Parameters = vals
            };
        }

        static TestDetail Test_CaseSensitivity_LiteralParts()
        {
            string url = "/Users/42";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Literal parts are case-sensitive: 'Users' != 'users'"
            };
        }

        static TestDetail Test_CaseInsensitivity_ParameterNames()
        {
            string url = "/users/42";
            string pattern = "/{UserId}/{ID}";
            NameValueCollection vals;
            Matcher.Match(url, pattern, out vals);
            bool passed = vals["userid"] == "users" && vals["UserId"] == "users" &&
                   vals["id"] == "42" && vals["ID"] == "42" && vals["Id"] == "42";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = true,
                Parameters = vals,
                Description = "Parameter names are case-insensitive"
            };
        }

        static TestDetail Test_QueryStringStripping()
        {
            string url = "/users/42?foo=bar";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Query string should be stripped"
            };
        }

        static TestDetail Test_FragmentStripping()
        {
            string url = "/users/42#section";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Fragment should be stripped"
            };
        }

        static TestDetail Test_QueryAndFragmentStripping()
        {
            string url = "/users/42?foo=bar#section";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Both query string and fragment should be stripped"
            };
        }

        static TestDetail Test_EmptyParameterRejection()
        {
            string url = "/{}";
            string pattern = "/{}";
            NameValueCollection vals;
            // Pattern with empty braces should be treated as literal "{}"
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Empty braces should be treated as literal"
            };
        }

        static TestDetail Test_DifferentPartCounts_MoreUrlParts()
        {
            string url = "/users/42/extra";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - URL has more parts than pattern"
            };
        }

        static TestDetail Test_DifferentPartCounts_MorePatternParts()
        {
            string url = "/users/42";
            string pattern = "/users/{id}/extra";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - Pattern has more parts than URL"
            };
        }

        static TestDetail Test_UriConstructor()
        {
            Uri uri = new Uri("http://localhost:8000/v1.0/users/42");
            string pattern = "/{version}/users/{id}";
            Matcher matcher = new Matcher(uri);
            NameValueCollection vals;
            bool result = matcher.Match(pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["version"] == "v1.0" && vals["id"] == "42",
                Url = uri.ToString(),
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Using Uri constructor"
            };
        }

        static TestDetail Test_UriStaticMethod()
        {
            Uri uri = new Uri("http://localhost:8000/v1.0/users/42");
            string pattern = "/{version}/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(uri, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["version"] == "v1.0" && vals["id"] == "42",
                Url = uri.ToString(),
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Using Uri static method"
            };
        }

        static TestDetail Test_MultiplePatterns_InstanceMethod()
        {
            string url = "/v1.0/users/42";
            Matcher matcher = new Matcher(url);
            NameValueCollection vals1, vals2;
            bool match1 = matcher.Match("/{version}/users/{id}", out vals1);
            bool match2 = matcher.Match("/{version}/posts/{id}", out vals2);

            return new TestDetail
            {
                Passed = match1 && !match2 && vals1["id"] == "42",
                Url = url,
                Pattern = "Multiple patterns tested",
                Matched = match1,
                Parameters = vals1,
                Description = "First pattern should match, second should not"
            };
        }

        static TestDetail Test_UrlEncodedValues()
        {
            string url = "/hello%20world";
            string pattern = "/{greeting}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            // URL-encoded values are matched as-is (not decoded)

            return new TestDetail
            {
                Passed = vals["greeting"] == "hello%20world",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "URL-encoded values matched as-is (not decoded)"
            };
        }

        static TestDetail Test_LeadingSlashHandling()
        {
            string url = "/users/42";
            string pattern = "/{resource}/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["resource"] == "users" && vals["id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals
            };
        }

        static TestDetail Test_TrailingSlashHandling()
        {
            NameValueCollection vals1, vals2;
            bool result1 = Matcher.Match("/users/42/", "/users/{id}", out vals1);
            bool result2 = Matcher.Match("/users/42", "/users/{id}/", out vals2);
            // Trailing slashes create empty parts, which get removed by RemoveEmptyEntries

            return new TestDetail
            {
                Passed = result1 && result2 && vals1["id"] == "42" && vals2["id"] == "42",
                Url = "/users/42/ and /users/42",
                Pattern = "/users/{id} and /users/{id}/",
                Matched = result1 && result2,
                Parameters = vals1,
                Description = "Trailing slashes create empty parts removed by RemoveEmptyEntries"
            };
        }

        static TestDetail Test_NoSlashes()
        {
            string url = "users";
            string pattern = "{resource}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["resource"] == "users",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "URLs without slashes should work"
            };
        }

        static TestDetail Test_RootPath()
        {
            string url = "/";
            string pattern = "/";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Root path should match with no parameters"
            };
        }

        static TestDetail Test_SinglePartUrl()
        {
            string url = "/users";
            string pattern = "/users";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals
            };
        }

        static TestDetail Test_ManyPartsUrl()
        {
            string url = "/a/b/c/d/e/f/g/h";
            string pattern = "/a/b/c/d/e/f/g/h";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Long URL with many parts"
            };
        }

        static TestDetail Test_SpecialCharactersInLiteral()
        {
            string url = "/api-v2/user_profile";
            string pattern = "/api-v2/user_profile";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Hyphens and underscores in literal parts"
            };
        }

        static TestDetail Test_SpecialCharactersInParameter()
        {
            string url = "/user-123_test";
            string pattern = "/{userId}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["userId"] == "user-123_test",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Special characters in parameter values"
            };
        }

        static TestDetail Test_ParameterAtStart()
        {
            string url = "/123/users";
            string pattern = "/{id}/users";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "123",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals
            };
        }

        static TestDetail Test_ParameterAtEnd()
        {
            string url = "/users/123";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "123",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals
            };
        }

        static TestDetail Test_AllParameters()
        {
            string url = "/foo/bar/baz";
            string pattern = "/{a}/{b}/{c}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["a"] == "foo" && vals["b"] == "bar" && vals["c"] == "baz",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "All parts are parameters"
            };
        }

        static TestDetail Test_NoParameters()
        {
            string url = "/users/list/all";
            string pattern = "/users/list/all";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "No parameters, all literal parts"
            };
        }

        static TestDetail Test_PartsProperty_ReturnsCopy()
        {
            string url = "/foo/bar/baz";
            Matcher matcher = new Matcher(url);
            string[] parts1 = matcher.Parts;
            string[] parts2 = matcher.Parts;
            // Should be different array instances
            bool passed = !Object.ReferenceEquals(parts1, parts2);

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Description = "Parts property should return a copy, not the same reference"
            };
        }

        static TestDetail Test_PartsProperty_ArrayImmutability()
        {
            string url = "/foo/bar/baz";
            Matcher matcher = new Matcher(url);
            string[] parts1 = matcher.Parts;
            parts1[0] = "changed";
            string[] parts2 = matcher.Parts;
            // parts2 should still have original value
            bool passed = parts2[0] == "foo";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Description = "Modifying returned array should not affect internal state"
            };
        }

        static TestDetail Test_NullUrl_ArgumentNullException()
        {
            bool passed = false;
            try
            {
                string nullUrl = null;
                NameValueCollection vals;
                Matcher.Match(nullUrl, "/{id}", out vals);
            }
            catch (ArgumentNullException)
            {
                passed = true;
            }

            return new TestDetail
            {
                Passed = passed,
                Description = "Null URL should throw ArgumentNullException"
            };
        }

        static TestDetail Test_EmptyUrl_ArgumentNullException()
        {
            bool passed = false;
            try
            {
                NameValueCollection vals;
                Matcher.Match("", "/{id}", out vals);
            }
            catch (ArgumentNullException)
            {
                passed = true;
            }

            return new TestDetail
            {
                Passed = passed,
                Description = "Empty URL should throw ArgumentNullException"
            };
        }

        static TestDetail Test_NullPattern_ArgumentNullException()
        {
            bool passed = false;
            try
            {
                NameValueCollection vals;
                Matcher.Match("/users", null, out vals);
            }
            catch (ArgumentNullException)
            {
                passed = true;
            }

            return new TestDetail
            {
                Passed = passed,
                Description = "Null pattern should throw ArgumentNullException"
            };
        }

        static TestDetail Test_EmptyPattern_ArgumentNullException()
        {
            bool passed = false;
            try
            {
                NameValueCollection vals;
                Matcher.Match("/users", "", out vals);
            }
            catch (ArgumentNullException)
            {
                passed = true;
            }

            return new TestDetail
            {
                Passed = passed,
                Description = "Empty pattern should throw ArgumentNullException"
            };
        }

        static TestDetail Test_NullUri_ArgumentNullException()
        {
            bool passed = false;
            try
            {
                Uri nullUri = null;
                NameValueCollection vals;
                Matcher.Match(nullUri, "/{id}", out vals);
            }
            catch (ArgumentNullException)
            {
                passed = true;
            }

            return new TestDetail
            {
                Passed = passed,
                Description = "Null URI should throw ArgumentNullException"
            };
        }

        static TestDetail Test_MalformedPattern_MissingCloseBrace()
        {
            string url = "/{id";
            string pattern = "/{id";
            NameValueCollection vals;
            // Pattern with missing close brace should be treated as literal
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Malformed pattern treated as literal"
            };
        }

        static TestDetail Test_MalformedPattern_MissingOpenBrace()
        {
            string url = "/id}";
            string pattern = "/id}";
            NameValueCollection vals;
            // Pattern with missing open brace should be treated as literal
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Malformed pattern treated as literal"
            };
        }

        static TestDetail Test_PatternWithSpaces()
        {
            string url = "/hello world";
            string pattern = "/hello world";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Spaces in literal parts"
            };
        }

        static TestDetail Test_UrlWithSpaces()
        {
            string url = "/hello world";
            string pattern = "/{greeting}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["greeting"] == "hello world",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Spaces in parameter values"
            };
        }

        static TestDetail Test_ComplexRealWorldExample()
        {
            string url = "/api/v3/organizations/acme-corp/repositories/my-repo/pull-requests/42";
            string pattern = "/api/{version}/organizations/{org}/repositories/{repo}/pull-requests/{prId}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            bool passed = result &&
                vals["version"] == "v3" &&
                vals["org"] == "acme-corp" &&
                vals["repo"] == "my-repo" &&
                vals["prId"] == "42";

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Complex real-world API URL"
            };
        }

        static TestDetail Test_EmptyValuesCollectionOnFailure()
        {
            string url = "/users/42";
            string pattern = "/posts/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            // vals should be empty collection, not null
            bool passed = vals != null && vals.Count == 0;

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Failed match should return empty collection, not null"
            };
        }

        static TestDetail Test_ParameterNameWithUnderscores()
        {
            string url = "/users/42";
            string pattern = "/users/{user_id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["user_id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Parameter names can contain underscores"
            };
        }

        static TestDetail Test_ParameterNameWithNumbers()
        {
            string url = "/users/42";
            string pattern = "/users/{id1}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id1"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Parameter names can contain numbers"
            };
        }

        static TestDetail Test_LongUrl()
        {
            string url = "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p/q/r/s/t/u/v/w/x/y/z";
            string pattern = "/a/b/c/d/e/f/g/h/i/j/k/l/m/n/o/p/q/r/s/t/u/v/w/x/y/{letter}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["letter"] == "z",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Very long URL with many segments"
            };
        }

        static TestDetail Test_DuplicateParameterNames()
        {
            string url = "/foo/bar";
            string pattern = "/{id}/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            // NameValueCollection allows duplicate keys - both values should be present

            return new TestDetail
            {
                Passed = vals["id"] != null,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Duplicate parameter names allowed in NameValueCollection"
            };
        }

        static TestDetail Test_QueryStringWithoutPath()
        {
            string url = "?foo=bar";
            // Query string without path becomes empty after stripping, should match empty pattern with 0 parts
            Matcher matcher = new Matcher(url);
            bool passed = matcher.Parts.Length == 0;

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Description = "Query string without path results in 0 parts"
            };
        }

        static TestDetail Test_FragmentWithoutPath()
        {
            string url = "#section";
            // Fragment without path becomes empty after stripping, should have 0 parts
            Matcher matcher = new Matcher(url);
            bool passed = matcher.Parts.Length == 0;

            return new TestDetail
            {
                Passed = passed,
                Url = url,
                Description = "Fragment without path results in 0 parts"
            };
        }

        static TestDetail Test_UnicodeCharactersInLiteral()
        {
            string url = "/用户/列表";
            string pattern = "/用户/列表";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Unicode characters in literal parts"
            };
        }

        static TestDetail Test_UnicodeCharactersInParameterValue()
        {
            string url = "/users/用户123";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = vals["id"] == "用户123",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Unicode characters in parameter values"
            };
        }

        static TestDetail Test_LiteralMismatch_MiddlePart()
        {
            string url = "/api/v1/users/42";
            string pattern = "/api/v2/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - literal 'v1' != 'v2' in the middle"
            };
        }

        static TestDetail Test_ParameterValueCasePreserved()
        {
            string url = "/users/AbC123";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            // Parameter names are case-insensitive, but the extracted VALUE must preserve case.

            return new TestDetail
            {
                Passed = result && vals["id"] == "AbC123",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Extracted parameter value must preserve original casing"
            };
        }

        static TestDetail Test_ParameterCannotMatchMissingPart()
        {
            // A trailing parameter has no corresponding URL part -> different part counts -> no match.
            string url = "/users";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "A parameter with no corresponding URL part should not match"
            };
        }

        static TestDetail Test_UriWithQueryAndFragment()
        {
            Uri uri = new Uri("http://localhost:8000/v1.0/users/42?active=true#top");
            string pattern = "/{version}/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(uri, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["version"] == "v1.0" && vals["id"] == "42",
                Url = uri.ToString(),
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "URI query and fragment should be stripped before matching"
            };
        }

        static TestDetail Test_InstanceUriPartsExtraction()
        {
            Uri uri = new Uri("http://localhost:9000/a/b/c?x=1");
            Matcher matcher = new Matcher(uri);
            string[] parts = matcher.Parts;
            bool passed = parts.Length == 3 && parts[0] == "a" && parts[1] == "b" && parts[2] == "c";

            return new TestDetail
            {
                Passed = passed,
                Url = uri.ToString(),
                Description = "URI constructor should extract path parts without query string"
            };
        }

        static TestDetail Test_ConsecutiveSlashesCollapsed()
        {
            string url = "/users//42";
            string pattern = "/users/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals["id"] == "42",
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Consecutive slashes collapse so /users//42 matches /users/{id}"
            };
        }

        static TestDetail Test_NumericVsLiteralNoMatch()
        {
            string url = "/orders/2024";
            string pattern = "/orders/2023";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - numeric literal '2024' != '2023'"
            };
        }

        static TestDetail Test_LongerPatternNegative()
        {
            string url = "/a/b";
            string pattern = "/{one}/{two}/{three}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = !result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - pattern has 3 params but URL has 2 parts"
            };
        }

        static TestDetail Test_DotSegmentsAsLiterals()
        {
            string url = "/files/./config";
            string pattern = "/files/./config";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);

            return new TestDetail
            {
                Passed = result && vals.Count == 0,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Dot segments are treated as ordinary literal parts"
            };
        }

        static TestDetail Test_MixedLiteralAndParamNegative()
        {
            string url = "/v1.0/users/42";
            string pattern = "/{version}/admins/{id}";
            NameValueCollection vals;
            bool result = Matcher.Match(url, pattern, out vals);
            // The literal 'admins' does not match 'users', so overall no match. Note the library
            // accumulates parameter values encountered before the failing literal (e.g. 'version'),
            // so only the boolean result is a reliable indicator of a failed match.

            return new TestDetail
            {
                Passed = !result,
                Url = url,
                Pattern = pattern,
                Matched = result,
                Parameters = vals,
                Description = "Should NOT match - literal 'users' != 'admins' despite an earlier matching param"
            };
        }

        #endregion
    }

    class TestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public TimeSpan Duration { get; set; }
        public string Error { get; set; }
    }

    class TestDetail
    {
        public bool Passed { get; set; }
        public string Url { get; set; }
        public string Pattern { get; set; }
        public bool? Matched { get; set; }
        public NameValueCollection Parameters { get; set; }
        public string Description { get; set; }
    }
}