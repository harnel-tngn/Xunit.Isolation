using System.Runtime.CompilerServices;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation.SkippableFact;

/// <summary>
/// ApplyContext methods for Xunit.SkippableFact
/// </summary>
public class SkippableFactApplyContextMethods
{
    [ApplyContext]
    private static SkippableFactTestCase ApplyContextImpl(SkippableFactTestCase testCase, IsolationContext context)
    {
        return new SkippableFactTestCase(
            GetSkippingExceptionNames(testCase),
            GetDiagnosticMessageSink(testCase),
            GetDefaultMethodDisplay(testCase),
            GetDefaultMethodDisplayOptions(testCase),
            ApplyContextAttribute.ApplyContext(testCase.TestMethod, context)!,
            testCase.TestMethodArguments);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_SkippingExceptionNames")]
        static extern string[] GetSkippingExceptionNames(SkippableFactTestCase testCase);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DiagnosticMessageSink")]
        static extern IMessageSink GetDiagnosticMessageSink(XunitTestCase testCase);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DefaultMethodDisplay")]
        static extern TestMethodDisplay GetDefaultMethodDisplay(TestMethodTestCase testCase);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DefaultMethodDisplayOptions")]
        static extern TestMethodDisplayOptions GetDefaultMethodDisplayOptions(TestMethodTestCase testCase);
    }

}
