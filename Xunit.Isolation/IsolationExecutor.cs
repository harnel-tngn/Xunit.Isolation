using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation;

/// <summary>
/// Executor for isolated tests
/// </summary>
public class IsolationExecutor : XunitTestFrameworkExecutor
{
    /// <summary>
    /// Constructor for IsolationExecutor
    /// </summary>
    public IsolationExecutor(
        AssemblyName assemblyName, 
        ISourceInformationProvider sourceInformationProvider,
        IMessageSink diagnosticMessageSink)
        : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
    {
    }

    /// <summary>
    /// Run test cases in isolated environment
    /// </summary>
    protected override async void RunTestCases(
        IEnumerable<IXunitTestCase> testCases, 
        IMessageSink executionMessageSink, 
        ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new IsolationTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}
