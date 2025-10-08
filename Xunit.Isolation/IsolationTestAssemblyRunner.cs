using AssemblyLoadContextHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    private static Dictionary<Type, MethodInfo> _methodInfoDict;

    static IsolationTestAssemblyRunner()
    {
        _methodInfoDict = typeof(IsolationTestAssemblyRunner)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Where(mi => mi.IsStatic
                && mi.Name == "ApplyContextImpl"
                && mi.GetParameters().Length == 2
                && mi.GetParameters()[1].ParameterType == typeof(IsolationContext))
            .ToDictionary(
                mi => mi.GetParameters()[0].ParameterType,
                mi => mi);
    }

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

        var clonedTestCollection = ApplyContext(testCollection, context);
        var clonedTestCases = testCases
            .Select(testCase => ApplyContext(testCase, context))
            .ToArray();

        return await base.RunTestCollectionAsync(messageBus, clonedTestCollection, clonedTestCases, cancellationTokenSource);
    }

    private static T? ApplyContext<T>(T? original, IsolationContext context)
        where T : class
    {
        if (original == null)
            return null;

        var type = original.GetType();
        if (!_methodInfoDict.TryGetValue(type, out var func))
            throw new NotImplementedException();

        return (T)func.Invoke(null, [original, context])!;
    }

    private static TestCollection ApplyContextImpl(TestCollection testCollection, IsolationContext context)
    {
        return new TestCollection(
            ApplyContext(testCollection.TestAssembly, context),
            ApplyContext(testCollection.CollectionDefinition, context),
            testCollection.DisplayName,
            testCollection.UniqueID);
    }

    private static XunitTestCase ApplyContextImpl(XunitTestCase testCase, IsolationContext context)
    {
        return new XunitTestCase(
            GetDiagnosticMessageSink(testCase),
            GetDefaultMethodDisplay(testCase),
            GetDefaultMethodDisplayOptions(testCase),
            ApplyContext(testCase.TestMethod, context),
            testCase.TestMethodArguments);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DiagnosticMessageSink")]
        static extern IMessageSink GetDiagnosticMessageSink(XunitTestCase testCase);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DefaultMethodDisplay")]
        static extern TestMethodDisplay GetDefaultMethodDisplay(TestMethodTestCase testCase);

        [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_DefaultMethodDisplayOptions")]
        static extern TestMethodDisplayOptions GetDefaultMethodDisplayOptions(TestMethodTestCase testCase);
    }

    private static TestAssembly ApplyContextImpl(TestAssembly testAssembly, IsolationContext context)
    {
        return new TestAssembly(
            ApplyContext(testAssembly.Assembly, context),
            testAssembly.ConfigFileName,
            testAssembly.Version);
    }

    private static TestClass ApplyContextImpl(TestClass testClass, IsolationContext context)
    {
        return new TestClass(
            ApplyContext(testClass.TestCollection, context),
            ApplyContext(testClass.Class, context));
    }

    private static TestMethod ApplyContextImpl(TestMethod testMethod, IsolationContext context)
    {
        return new TestMethod(
            ApplyContext(testMethod.TestClass, context),
            ApplyContext(testMethod.Method, context));
    }

    private static ReflectionAssemblyInfo ApplyContextImpl(ReflectionAssemblyInfo assemblyInfo, IsolationContext context)
    {
        return new ReflectionAssemblyInfo(context.AssemblyLoadContext.GetMatchingAssembly(assemblyInfo.Assembly, loadIfNotLoaded: true));
    }

    private static ReflectionTypeInfo ApplyContextImpl(ReflectionTypeInfo typeInfo, IsolationContext context)
    {
        return new ReflectionTypeInfo(context.AssemblyLoadContext.GetMatchingType(typeInfo.Type, loadIfNotLoaded: true));
    }

    private static ReflectionMethodInfo ApplyContextImpl(ReflectionMethodInfo methodInfo, IsolationContext context)
    {
        return new ReflectionMethodInfo(context.AssemblyLoadContext.GetMatchingMethod(methodInfo.MethodInfo, loadIfNotLoaded: true));
    }
}
