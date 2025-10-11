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

    private IsolationContext GetOrCreateIsolationContext(bool unloadAtEnd)
    {
        // TODO: use pooling
        return new IsolationContext(unloadAtEnd);
    }

    /// <summary>
    /// Run test collection in isolated environment
    /// </summary>
    protected override async Task<RunSummary> RunTestCollectionAsync(
        IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        using var context = GetOrCreateIsolationContext(false);

        var clonedTestCollection = ApplyContextAttribute.ApplyContext(testCollection, context);
        var clonedTestCases = testCases
            .Select(testCase => ApplyContextAttribute.ApplyContext(testCase, context))
            .ToArray();

        return await base.RunTestCollectionAsync(messageBus, clonedTestCollection, clonedTestCases, cancellationTokenSource);
    }
}
