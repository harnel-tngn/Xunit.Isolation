using AssemblyLoadContextHelper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation;

/// <summary>
/// Indicates which method is used for applying context to test related objects. <br/>
/// Target methods should be static method with 2 parameters; 
/// type related object and <see cref="IsolationContext"/>, and return the type related object with context applied. <br/>
/// 
/// e.g. <see cref="TestCollection"/> and <see cref="ApplyContextImpl(TestCollection, IsolationContext)"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ApplyContextAttribute : Attribute
{
    /// <summary>
    /// Priority of this context applier. Only highest priority value will be applied.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Constructor for ApplyContextAttribute
    /// </summary>
    public ApplyContextAttribute(int priority = 0)
    {
        Priority = priority;
    }

    private readonly static Dictionary<Type, (int priority, MethodInfo methodInfo)> _methodInfoDict = new();

    static ApplyContextAttribute()
    {
        foreach (var assembly in AssemblyLoadContext.Default.Assemblies)
            foreach (var type in assembly.GetTypes())
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    foreach (var attr in method.GetCustomAttributes<ApplyContextAttribute>())
                    {
                        if (method.GetParameters().Length != 2)
                            continue;

                        if (method.GetParameters()[1].ParameterType != typeof(IsolationContext))
                            continue;

                        var handleType = method.GetParameters()[0].ParameterType;
                        if (handleType != method.ReturnType)
                            continue;

                        if (_methodInfoDict.TryGetValue(handleType, out var existing))
                        {
                            if (existing.priority < attr.Priority)
                                _methodInfoDict[handleType] = (attr.Priority, method);
                        }
                        else
                        {
                            _methodInfoDict[handleType] = (attr.Priority, method);
                        }
                    }
    }

    /// <summary>
    /// Apply isolation context to test related object
    /// </summary>
    public static T? ApplyContext<T>(T? original, IsolationContext context)
        where T : class
    {
        if (original == null)
            return null;

        var type = original.GetType();
        if (!_methodInfoDict.TryGetValue(type, out var func))
            throw new NotImplementedException();

        return (T)func.methodInfo.Invoke(null, [original, context])!;
    }

    [ApplyContext]
    private static TestCollection ApplyContextImpl(TestCollection testCollection, IsolationContext context)
    {
        return new TestCollection(
            ApplyContext(testCollection.TestAssembly, context),
            ApplyContext(testCollection.CollectionDefinition, context),
            testCollection.DisplayName,
            testCollection.UniqueID);
    }

    [ApplyContext]
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

    [ApplyContext]
    private static TestAssembly ApplyContextImpl(TestAssembly testAssembly, IsolationContext context)
    {
        return new TestAssembly(
            ApplyContext(testAssembly.Assembly, context),
            testAssembly.ConfigFileName,
            testAssembly.Version);
    }

    [ApplyContext]
    private static TestClass ApplyContextImpl(TestClass testClass, IsolationContext context)
    {
        return new TestClass(
            ApplyContext(testClass.TestCollection, context),
            ApplyContext(testClass.Class, context));
    }

    [ApplyContext]
    private static TestMethod ApplyContextImpl(TestMethod testMethod, IsolationContext context)
    {
        return new TestMethod(
            ApplyContext(testMethod.TestClass, context),
            ApplyContext(testMethod.Method, context));
    }

    [ApplyContext]
    private static ReflectionAssemblyInfo ApplyContextImpl(ReflectionAssemblyInfo assemblyInfo, IsolationContext context)
    {
        return new ReflectionAssemblyInfo(context.AssemblyLoadContext.GetMatchingAssembly(assemblyInfo.Assembly, loadIfNotLoaded: true));
    }

    [ApplyContext]
    private static ReflectionTypeInfo ApplyContextImpl(ReflectionTypeInfo typeInfo, IsolationContext context)
    {
        return new ReflectionTypeInfo(context.AssemblyLoadContext.GetMatchingType(typeInfo.Type, loadIfNotLoaded: true));
    }

    [ApplyContext]
    private static ReflectionMethodInfo ApplyContextImpl(ReflectionMethodInfo methodInfo, IsolationContext context)
    {
        return new ReflectionMethodInfo(context.AssemblyLoadContext.GetMatchingMethod(methodInfo.MethodInfo, loadIfNotLoaded: true));
    }
}