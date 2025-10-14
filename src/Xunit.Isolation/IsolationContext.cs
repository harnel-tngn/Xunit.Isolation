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
    private static readonly ConcurrentDictionary<string, int> _pooledContextIdCounter = new();

    private static readonly ConcurrentDictionary<string, IsolationContext> _isolationContextCache = new();
    private static readonly ConcurrentDictionary<string, ConcurrentBag<IsolationContext>> _isolationContextPoolCache = new();

    internal static IsolationContext GetOrCreate(IsolationContextConfigBaseAttribute? isolationConfig)
    {
        if (isolationConfig == null)
            return new IsolationContext(unloadAtEnd: true);

        if (isolationConfig is IsolationContextIdAttribute isolationContextIdAttr)
        {
            return _isolationContextCache.GetOrAdd(
                isolationContextIdAttr.IsolationId,
                static isolationContextId => new IsolationContext(isolationContextId: isolationContextId));
        }

        if (isolationConfig is IsolationContextPoolIdAttribute isolationContextPoolIdAttr)
        {
            var bag = _isolationContextPoolCache.GetOrAdd(
                isolationContextPoolIdAttr.IsolationPoolId,
                static _ => new ConcurrentBag<IsolationContext>());

            if (bag.TryTake(out var context))
                return context;

            return new IsolationContext(isolationContextPoolId: isolationContextPoolIdAttr.IsolationPoolId);
        }

        throw new NotImplementedException($"Unknown context attribute type {isolationConfig.GetType()}");
    }

    /// <summary>
    /// ID of isolation context
    /// </summary>
    public string? IsolationContextId { get; }

    /// <summary>
    /// ID of isolation context pool
    /// </summary>
    public string? IsolationContextPoolId { get; }

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
    private IsolationContext(bool unloadAtEnd = false, string? isolationContextId = null, string? isolationContextPoolId = null)
    {
        UnloadAtEnd = unloadAtEnd;
        IsolationContextId = isolationContextId;
        IsolationContextPoolId = isolationContextPoolId;

        AssemblyLoadContext = new AssemblyLoadContext(GetAssemblyLoadContextName(), true);
    }

    private string GetAssemblyLoadContextName()
    {
        if (IsolationContextId != null)
            return $"Isolation.Named.{IsolationContextId}";

        if (IsolationContextPoolId != null)
        {
            var counter = _pooledContextIdCounter.AddOrUpdate(IsolationContextPoolId, 1, (_, v) => v + 1);
            return $"Isolation.Pooled.{IsolationContextPoolId}#{counter}";
        }

        return $"Isolation.Anonymous.{Interlocked.Increment(ref _anonymousContextIdCounter)}";
    }

    /// <summary>
    /// Finalizer to ensure unloading
    /// </summary>
    ~IsolationContext()
    {
        Dispose(forceUnload: true);
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    private void Dispose(bool forceUnload)
    {
        var needUnload = forceUnload;

        if (needUnload)
        {
            AssemblyLoadContext.Unload();
            GC.SuppressFinalize(this);
            return;
        }

        if (IsolationContextPoolId != null)
        {
            var bag = _isolationContextPoolCache.GetOrAdd(
                IsolationContextPoolId,
                static _ => new ConcurrentBag<IsolationContext>());

            bag.Add(this);
        }
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    public void Dispose()
    {
        Dispose(forceUnload: UnloadAtEnd);
    }

    /// <inheritdoc/>
    public override string ToString() => AssemblyLoadContext.Name ?? String.Empty;
}