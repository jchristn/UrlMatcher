namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Xunit;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    /// <summary>
    /// Runs every UrlMatcher descriptor sequentially in a single xUnit fact.
    /// </summary>
    public sealed class UrlMatcherFactTests : TouchstoneFactBase
    {
        /// <summary>
        /// All UrlMatcher suites.
        /// </summary>
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return UrlMatcherSuites.All; }
        }

        /// <summary>
        /// Run all descriptors.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
