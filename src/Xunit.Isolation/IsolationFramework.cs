using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation;

/// <summary>
/// Framework for isolated tests
/// </summary>
public class IsolationFramework : XunitTestFramework
{
    /// <summary>
    /// Constructor for IsolationFramework
    /// </summary>
    /// <param name="messageSink"></param>
    public IsolationFramework(IMessageSink messageSink) : base(messageSink)
    {
        messageSink.OnMessage(new DiagnosticMessage($"Testing w/ {typeof(IsolationFramework)}"));
    }

    /// <summary>
    /// Create executor for isolated tests
    /// </summary>
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
    {
        return new IsolationExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}
