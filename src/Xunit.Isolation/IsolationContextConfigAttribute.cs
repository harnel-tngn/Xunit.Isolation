using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Isolation;

/// <summary>
/// Attribute class to config isolation context of test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class IsolationContextConfigAttribute : Attribute, IEquatable<IsolationContextConfigAttribute>
{
    /// <summary>
    /// Required ID of isolation context. If null, any context is acceptable.
    /// </summary>
    public string? IsolationId { get; }

    /// <summary>
    /// Constructor of <see cref="IsolationContextConfigAttribute"/>
    /// </summary>
    /// <param name="isolationId">Required ID of isolation context. If null, temporary isolation context will be created.</param>
    public IsolationContextConfigAttribute(string? isolationId = null)
    {
        IsolationId = isolationId;
    }

    private static IsolationContextConfigAttribute Default { get; } = new IsolationContextConfigAttribute();

    /// <summary>
    /// Get config of given type. If not defined, return default config.
    /// <see cref="IsolationContextConfigAttribute"/> of child class has higher priority than parent class.
    /// </summary>
    internal static IsolationContextConfigAttribute GetConfig(Type? type)
    {
        while (type != null)
        {
            if (type.GetCustomAttribute<IsolationContextConfigAttribute>() is IsolationContextConfigAttribute attr)
                return attr;

            type = type.BaseType;
        }

        return Default;
    }

    /// <inheritdoc/>
    public static bool operator ==(IsolationContextConfigAttribute? l, IsolationContextConfigAttribute? r)
    {
        if (Object.ReferenceEquals(l, null))
            return Object.ReferenceEquals(r, null);

        return l.Equals(r);
    }

    /// <inheritdoc/>
    public static bool operator !=(IsolationContextConfigAttribute? l, IsolationContextConfigAttribute? r)
        => !(l == r);

    /// <inheritdoc/>
    public bool Equals(IsolationContextConfigAttribute? other) =>
        other != null
        && Equals(IsolationId, other.IsolationId);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as IsolationContextConfigAttribute);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsolationId);
}
