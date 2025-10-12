using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Threading;

namespace Xunit.Isolation;

/// <summary>
/// Context of isolation
/// </summary>
public class IsolationContext : IDisposable
{
    private static int _anonymousContextIdCounter = 0;

    private static ConcurrentDictionary<string, ConcurrentBag<IsolationContext>> _pool = new();

    internal static IsolationContext GetOrCreate(IsolationContextConfigAttribute config)
        => GetOrCreate(config.IsolationId);

    internal static IsolationContext GetOrCreate(string? isolationContextId)
    {
        if (isolationContextId != null)
        {
            var bag = _pool.GetOrAdd(isolationContextId, static _ => new ConcurrentBag<IsolationContext>());
            if (bag != null && bag.TryTake(out var context))
                return context;
        }

        var newContext = new IsolationContext(isolationContextId == null, isolationContextId);
        return newContext;
    }

    /// <summary>
    /// ID of isolation context
    /// </summary>
    public string? IsolationContextId { get; }

    /// <summary>
    /// Unload AssemblyLoadContext if true at the end of use
    /// </summary>
    public bool UnloadAtEnd { get; }

    /// <summary>
    /// AssemblyLoadContext for isolation
    /// </summary>
    public AssemblyLoadContext AssemblyLoadContext { get; }

    /// <summary>
    /// Constructor for isolation context
    /// </summary>
    private IsolationContext(bool unloadAtEnd, string? isolationContextId)
    {
        UnloadAtEnd = unloadAtEnd;
        IsolationContextId = isolationContextId;

        var assemblyLoadContext = isolationContextId != null
            ? $"Isolation.Named.{isolationContextId}"
            : $"Isolation.Anonymous.{Interlocked.Increment(ref _anonymousContextIdCounter)}";

        AssemblyLoadContext = new AssemblyLoadContext(assemblyLoadContext, true);
    }

    /// <summary>
    /// Finalizer to ensure unloading
    /// </summary>
    ~IsolationContext()
    {
        Dispose(returnToPool: true);
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    private void Dispose(bool returnToPool)
    {
        if (returnToPool && IsolationContextId != null)
        {
            var bag = _pool.GetValueOrDefault(IsolationContextId);
            bag?.Add(this);
        }
        else
        {
            AssemblyLoadContext.Unload();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    public void Dispose()
    {
        Dispose(!UnloadAtEnd);
    }
}
