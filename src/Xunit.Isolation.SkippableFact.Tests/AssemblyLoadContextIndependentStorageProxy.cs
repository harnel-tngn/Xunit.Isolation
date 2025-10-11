using AssemblyLoadContextHelper;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;

namespace Xunit.Isolation.SkippableFact.Tests;

/// <summary>
/// Class used for <see cref="AssemblyLoadContext"/> independent storage.
/// </summary>
internal static class AssemblyLoadContextIndependentStorageProxy
{
    private static ConcurrentDictionary<string, object?> _dict;
    private static AssemblyLoadContext _currentAssemblyLoadContext;

    private static MethodInfo _staticEnsureInitializedMethodInfo;
    private static MethodInfo _staticGetMethodInfo;
    private static MethodInfo _staticSetMethodInfo;

    static AssemblyLoadContextIndependentStorageProxy()
    {
        _dict = new();
        _currentAssemblyLoadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!;

        _staticEnsureInitializedMethodInfo = AssemblyLoadContext.Default.GetMatchingMethod(
            typeof(AssemblyLoadContextIndependentStorageProxy).GetMethod(nameof(EnsureInitialized))!,
            loadIfNotLoaded: true);

        _staticGetMethodInfo = AssemblyLoadContext.Default.GetMatchingMethod(
            typeof(AssemblyLoadContextIndependentStorageProxy).GetMethod(nameof(Get))!,
            loadIfNotLoaded: true);

        _staticSetMethodInfo = AssemblyLoadContext.Default.GetMatchingMethod(
            typeof(AssemblyLoadContextIndependentStorageProxy).GetMethod(nameof(Set))!,
            loadIfNotLoaded: true);
    }

    public static void EnsureInitialized<TValue>(string identifier, TValue value)
    {
        if (typeof(TValue).Assembly.GetName().Name != "System.Private.CoreLib")
            throw new InvalidOperationException($"Type '{typeof(TValue).FullName}' is not supported because it is not in 'System.Private.CoreLib' assembly.");

        if (_currentAssemblyLoadContext != AssemblyLoadContext.Default)
        {
            _staticEnsureInitializedMethodInfo.MakeGenericMethod(typeof(TValue)).Invoke(null, [identifier, value]);
        }
        else
        {
            _dict.TryAdd(identifier, value);
        }
    }

    public static TValue Get<TValue>(string identifier)
    {
        if (typeof(TValue).Assembly.GetName().Name != "System.Private.CoreLib")
            throw new InvalidOperationException($"Type '{typeof(TValue).FullName}' is not supported because it is not in 'System.Private.CoreLib' assembly.");

        if (_currentAssemblyLoadContext != AssemblyLoadContext.Default)
        {
            var val = _staticGetMethodInfo.MakeGenericMethod(typeof(TValue)).Invoke(null, [identifier]);
            return (TValue)val!;
        }
        else
        {
            return (TValue)_dict[identifier]!;
        }
    }

    public static void Set<TValue>(string identifier, TValue value)
    {
        if (typeof(TValue).Assembly.GetName().Name != "System.Private.CoreLib")
            throw new InvalidOperationException($"Type '{typeof(TValue).FullName}' is not supported because it is not in 'System.Private.CoreLib' assembly.");

        if (_currentAssemblyLoadContext != AssemblyLoadContext.Default)
        {
            _staticSetMethodInfo.MakeGenericMethod(typeof(TValue)).Invoke(null, [identifier, value]);
        }
        else
        {
            _dict[identifier] = value;
        }
    }
}
