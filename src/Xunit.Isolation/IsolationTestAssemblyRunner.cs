using System;
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
    /// </summary>
    protected override async Task<RunSummary> RunTestCollectionAsync(
        IMessageBus messageBus,
        ITestCollection testCollection,
        IEnumerable<IXunitTestCase> testCases,
        CancellationTokenSource cancellationTokenSource)
    {
        var testIsolationContextDict = testCases
            .GroupBy(testCase => IsolationContextConfigAttribute.GetConfig(testCase.TestMethod.TestClass.Class.ToRuntimeType()))
            .Select(g => KeyValuePair.Create(
                IsolationContextConfigAttribute.GetConfig(g.First().TestMethod.TestClass.Class.ToRuntimeType()),
                g.ToArray()))
            .ToArray();

        if (testIsolationContextDict.Length == 1)
        {
            var config = testIsolationContextDict.First().Key;
            using var context = IsolationContext.GetOrCreate(config);

            var clonedTestCollection = ApplyContextAttribute.ApplyContext(testCollection, context);
            var clonedTestCases = testCases
                .Select(testCase => ApplyContextAttribute.ApplyContext(testCase, context))
                .ToArray();

            return await base.RunTestCollectionAsync(messageBus, testCollection, clonedTestCases, cancellationTokenSource);
        }
        else
        {
            var summary = new RunSummary();
            foreach (var (_, subTestCases) in testIsolationContextDict)
                summary.Aggregate(await RunTestCollectionAsync(messageBus, testCollection, subTestCases, cancellationTokenSource));

            return summary;
        }
    }
}
