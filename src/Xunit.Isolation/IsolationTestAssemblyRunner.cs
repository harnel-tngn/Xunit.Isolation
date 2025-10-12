using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation;

/// <summary>
/// Assembly runner for isolated tests
/// </summary>
public class IsolationTestAssemblyRunner : XunitTestAssemblyRunner
{
    /// <summary>
    /// Constructor for IsolationTestAssemblyRunner
    /// </summary>
    public IsolationTestAssemblyRunner(
        ITestAssembly testAssembly,
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink diagnosticMessageSink,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
        : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
    {
    }

    /// <summary>
    /// Run test collection in isolated environment
    /// <summary>
    /// Runs the provided test collection, executing tests in isolation according to each test's IsolationContextConfigAttribute.
    /// When all tests share the same isolation configuration, a single IsolationContext is created and applied to the collection and its tests; when multiple configurations exist, the collection is executed once per configuration and results are aggregated.
    /// </summary>
    /// <param name="messageBus">The message bus used to report test execution messages.</param>
    /// <param name="testCollection">The test collection to run.</param>
    /// <param name="testCases">The test cases from the collection; they are grouped by isolation configuration before execution.</param>
    /// <param name="cancellationTokenSource">Cancellation token source that may be used to cancel the run.</param>
    /// <returns>A RunSummary containing aggregated results for the executed tests.</returns>
    protected override async Task<RunSummary> RunTestCollectionAsync(
        IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        var testIsolationConfigGroup = testCases
            .GroupBy(testCase => IsolationContextConfigAttribute.GetConfig(testCase.TestMethod.TestClass.Class.ToRuntimeType()))
            .Select(g => KeyValuePair.Create(
                IsolationContextConfigAttribute.GetConfig(g.First().TestMethod.TestClass.Class.ToRuntimeType()),
                g.ToArray()))
            .ToArray();

        if (testIsolationConfigGroup.Length == 1)
        {
            var (config, subTestCases) = testIsolationConfigGroup.First();
            using var context = IsolationContext.GetOrCreate(config);

            var clonedTestCollection = ApplyContextAttribute.ApplyContext(testCollection, context);
            var clonedTestCases = subTestCases
                .Select(testCase => ApplyContextAttribute.ApplyContext(testCase, context))
                .ToArray();

            return await base.RunTestCollectionAsync(messageBus, clonedTestCollection, clonedTestCases, cancellationTokenSource);
        }
        else
        {
            var summary = new RunSummary();
            foreach (var (_, subTestCases) in testIsolationConfigGroup)
                summary.Aggregate(await RunTestCollectionAsync(messageBus, testCollection, subTestCases, cancellationTokenSource));

            return summary;
        }
    }
}