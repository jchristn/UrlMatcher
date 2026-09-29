namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using Test.Shared.Suites;
    using Touchstone.Core;

    /// <summary>
    /// Every UrlMatcher test suite.
    /// </summary>
    public static class UrlMatcherSuites
    {
        /// <summary>
        /// All suites, in execution order.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    MatchingSuite.Create(),
                    ParametersSuite.Create(),
                    SegmentsSuite.Create(),
                    QueryAndFragmentSuite.Create(),
                    UriSuite.Create(),
                    CharactersSuite.Create(),
                    PartsSuite.Create(),
                    ArgumentValidationSuite.Create(),
                    MalformedPatternsSuite.Create(),
                    CatchAllSuite.Create(),
                    UrlPatternSuite.Create(),
                    WatsonCompatibilitySuite.Create(),
                    ConcurrencySuite.Create()
                };
            }
        }
    }
}
