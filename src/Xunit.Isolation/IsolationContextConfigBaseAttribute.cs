using System;
using System.Reflection;

namespace Xunit.Isolation;

/// <summary>
/// Base class for attributes to config isolation context of test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public abstract class IsolationContextConfigBaseAttribute : Attribute
{
    /// <summary>
    /// Get config of given type. If not defined, return default config.
    /// <see cref="IsolationContextConfigBaseAttribute"/> of child class has higher priority than parent class.
    /// </summary>
    internal static IsolationContextConfigBaseAttribute? GetConfig(Type? type)
    {
        while (type != null)
        {
            if (type.GetCustomAttribute<IsolationContextConfigBaseAttribute>() is { } attr)
                return attr;

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Compare two given IsolationContextConfigBaseAttribute, returns true if equal, and vice versa.
    /// </summary>
    public static bool operator ==(IsolationContextConfigBaseAttribute? l, IsolationContextConfigBaseAttribute? r)
    {
        if (Object.ReferenceEquals(l, null))
            return Object.ReferenceEquals(r, null);

        return l.Equals(r);
    }

    /// <summary>
    /// Compare two given IsolationContextConfigBaseAttribute, returns true if not equal, and vice versa.
    /// </summary>
    public static bool operator !=(IsolationContextConfigBaseAttribute? l, IsolationContextConfigBaseAttribute? r)
        => !(l == r);
}
