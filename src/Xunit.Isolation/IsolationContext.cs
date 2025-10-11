using System;
using System.Collections.Concurrent;
using System.Runtime.Loader;
using System.Threading;

namespace Xunit.Isolation;

/// <summary>
/// Context of isolation
/// </summary>
public class IsolationContext : IDisposable
{
    private static int _anonymousContextIdCounter = 0;

    private static ConcurrentDictionary<(object contextId, object contextGroupId), ConcurrentBag<IsolationContext>> _namedPool = new();
    private static ConcurrentDictionary<object, ConcurrentBag<IsolationContext>> _nullContextIdPool = new();
    private static ConcurrentDictionary<object, ConcurrentBag<IsolationContext>> _nullContextGroupIdPool = new();

    internal static IsolationContext GetOrCreate(bool unloadAtEnd = true, object? isolationId = null, object? isolationGroupId = null)
    {
        var bag = GetContextPool(isolationId, isolationGroupId);

        if (bag != null && bag.TryTake(out var context))
            return context;

        var newContext = new IsolationContext(unloadAtEnd, isolationId, isolationGroupId);
        return newContext;
    }

    private static ConcurrentBag<IsolationContext>? GetContextPool(object? contextId, object? contextGroupId)
    {
        ConcurrentBag<IsolationContext> bag;

        if (contextId != null && contextGroupId != null)
            bag = _namedPool.GetOrAdd((contextId, contextGroupId), static _ => new ConcurrentBag<IsolationContext>());
        else if (contextId != null)
            bag = _nullContextGroupIdPool.GetOrAdd(contextId, static _ => new ConcurrentBag<IsolationContext>());
        else if (contextGroupId != null)
            bag = _nullContextIdPool.GetOrAdd(contextGroupId, static _ => new ConcurrentBag<IsolationContext>());
        else
            return null;

        return bag;
    }

    /// <summary>
    /// ID of context
    /// </summary>
    public object? ContextId { get; }

    /// <summary>
    /// Group ID of context.
    /// </summary>
    public object? ContextGroupId { get; }

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
    private IsolationContext(bool unloadAtEnd, object? contextId, object? contextGroupId)
    {
        UnloadAtEnd = unloadAtEnd;
        ContextId = contextId;
        ContextGroupId = contextGroupId;

        var assemblyLoadContext = contextId != null
            ? $"Isolation.Named.{contextId}"
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
        if (returnToPool)
        {
            var bag = GetContextPool(ContextId, ContextGroupId);
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
