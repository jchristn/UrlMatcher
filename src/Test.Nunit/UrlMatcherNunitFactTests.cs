namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Runs every UrlMatcher descriptor sequentially in a single NUnit test.
    /// </summary>
    [TestFixture]
    public sealed class UrlMatcherNunitFactTests : TouchstoneNunitBase
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
        [Test]
        public async Task RunAll()
        {
            await RunAllAsync().ConfigureAwait(false);
        }
    }
}
