using System;
using System.Runtime.Loader;
using System.Threading;

namespace Xunit.Isolation;

/// <summary>
/// Context of isolation
/// </summary>
public class IsolationContext : IDisposable
{
    private static int _lastContextId;

    /// <summary>
    /// Id of context
    /// </summary>
    public int ContextId { get; }

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
    public IsolationContext(bool unloadAtEnd)
    {
        ContextId = Interlocked.Increment(ref _lastContextId);
        UnloadAtEnd = unloadAtEnd;
        AssemblyLoadContext = new AssemblyLoadContext($"Isolation.{ContextId}", true);
    }

    /// <summary>
    /// Finalizer to ensure unloading
    /// </summary>
    ~IsolationContext()
    {
        Dispose();
    }

    /// <summary>
    /// Unload the AssemblyLoadContext
    /// </summary>
    public void Dispose()
    {
        AssemblyLoadContext.Unload();
        GC.SuppressFinalize(this);
    }
}
