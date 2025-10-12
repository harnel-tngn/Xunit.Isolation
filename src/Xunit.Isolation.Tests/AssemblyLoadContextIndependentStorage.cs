using System;

namespace Xunit.Isolation.Tests;

public class AssemblyLoadContextIndependentStorage<TValue>
{
    private readonly string _identifier;

    public AssemblyLoadContextIndependentStorage(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        _identifier = identifier;
        AssemblyLoadContextIndependentStorageProxy.EnsureInitialized(identifier, default(TValue));
    }

    public AssemblyLoadContextIndependentStorage(string identifier, TValue initialValue)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        _identifier = identifier;
        AssemblyLoadContextIndependentStorageProxy.EnsureInitialized(identifier, initialValue);
    }

    public TValue Value
    {
        get => AssemblyLoadContextIndependentStorageProxy.Get<TValue>(_identifier);
        set => AssemblyLoadContextIndependentStorageProxy.Set<TValue>(_identifier, value);
    }
}
