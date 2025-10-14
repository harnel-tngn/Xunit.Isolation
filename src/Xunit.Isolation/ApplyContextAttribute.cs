using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using AssemblyLoadContextHelper;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Isolation;

/// <summary>
/// Indicates which method is used for applying context to test related objects. <br/>
/// Methods marked with this attribute executed on default <see cref="AssemblyLoadContext"/>. <br/>
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

    private static readonly Dictionary<Type, (int priority, MethodInfo methodInfo)> _methodInfoDict = new();

    static ApplyContextAttribute()
    {
        var assemblyLoadContext = AssemblyLoadContext.Default;
        var assemblyNames = new HashSet<string>(
        [
            ..IsolationExecutionConfig.KnownIsolationAssemblies,
            ..IsolationExecutionConfig.Instance.IsolationAssemblies,
        ], StringComparer.OrdinalIgnoreCase);

        foreach (var assemblyName in assemblyNames)
        {
            try
            {
                var assembly = assemblyLoadContext.LoadFromAssemblyName(new AssemblyName(assemblyName));
                LoadFromAssembly(assembly);
            }
            catch (FileNotFoundException ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }

    private static void LoadFromAssembly(Assembly assembly)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    try
                    {
                        var attr = method.GetCustomAttribute<ApplyContextAttribute>();
                        if (attr == null)
                            continue;

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
                    catch (TypeLoadException ex) when (ex.TypeName == "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")
                    {
                        // There is a known issue about bad reference in Microsoft.TestPlatform.CoreUtilities,
                        // which can cause TypeInitializationException
                        // Since there are no any references to ApplyContextAttribute in this assembly, simply skip it.
                        // https://github.com/microsoft/vstest/issues/4624
                        // https://github.com/microsoft/vstest/issues/4638
                        Debug.WriteLine(ex);
                    }
                }
        }
        catch (TypeInitializationException ex) when (ex.TypeName == "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute")
        {
            // There is a known issue about bad reference in Microsoft.TestPlatform.CoreUtilities,
            // which can cause TypeInitializationException
            // Since there are no any references to ApplyContextAttribute in this assembly, simply skip it.
            // https://github.com/microsoft/vstest/issues/4624
            // https://github.com/microsoft/vstest/issues/4638
            Debug.WriteLine(ex);
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
            throw new NotImplementedException($"Failed to ApplyContext to type {type.FullName}");

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
    private static XunitTheoryTestCase ApplyContextImpl(XunitTheoryTestCase testCase, IsolationContext context)
    {
        return new XunitTheoryTestCase(
            GetDiagnosticMessageSink(testCase),
            GetDefaultMethodDisplay(testCase),
            GetDefaultMethodDisplayOptions(testCase),
            ApplyContext(testCase.TestMethod, context));

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