namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Xunit;
    using global::Xunit.Abstractions;
    using Test.Shared;
    using Touchstone.Core;

    /// <summary>
    /// Runs each UrlMatcher descriptor as a separate xUnit theory row.
    /// </summary>
    public sealed class UrlMatcherTheoryTests
    {
        private readonly ITestOutputHelper _Output;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="output">Test output helper.</param>
        public UrlMatcherTheoryTests(ITestOutputHelper output)
        {
            _Output = output;
        }

        /// <summary>
        /// Every non-skipped descriptor.
        /// </summary>
        /// <returns>Theory data.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in UrlMatcherSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip)
                        data.Add(testCase);
                }
            }

            return data;
        }

        /// <summary>
        /// Run a single descriptor.
        /// </summary>
        /// <param name="testCase">Descriptor.</param>
        /// <returns>Task.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            _Output.WriteLine($"Running: {testCase.DisplayName}");
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
