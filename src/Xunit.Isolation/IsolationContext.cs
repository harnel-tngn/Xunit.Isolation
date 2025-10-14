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
    private static ConcurrentDictionary<string, int> _pooledContextIdCounter = new();

    private static readonly ConcurrentDictionary<string, IsolationContext> _isolationContextCache = new();

    internal static IsolationContext GetOrCreate(IsolationContextConfigBaseAttribute? isolationConfig)
    {
        if (isolationConfig == null)
            return new IsolationContext(unloadAtEnd: true);

        if (isolationConfig is IsolationContextIdAttribute isolationContextIdAttr)
            return new IsolationContext(unloadAtEnd: false, isolationContextId: isolationContextIdAttr.IsolationId);

        if (isolationConfig is IsolationContextPoolIdAttribute isolationContextPoolIdAttr)
            return new IsolationContext(unloadAtEnd: false, isolationContextPoolId: isolationContextPoolIdAttr.IsolationPoolId);

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
            return $"Isolation.Pooled.{counter}";
        }

        return $"Isolation.Anonymous.{Interlocked.Increment(ref _anonymousContextIdCounter)}";
    }

    /// <summary>
    /// Finalizer to ensure unloading
    /// </summary>
    ~IsolationContext()
    {
        Dispose(unload: true);
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    private void Dispose(bool unload)
    {
        if (!unload || IsolationContextId != null)
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
        Dispose(UnloadAtEnd);
    }

    /// <inheritdoc/>
    public override string ToString() => AssemblyLoadContext.Name ?? String.Empty;
}
