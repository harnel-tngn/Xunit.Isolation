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
    /// If true, always create new context for each test class. Created context will be unloaded at the end of test class run.
    /// </summary>
    public bool AlwaysCreateNewContext { get; }

    /// <summary>
    /// Required ID of isolation context. If null, any context is acceptable.
    /// </summary>
    public object? IsolationId { get; }

    /// <summary>
    /// Required category ID of isolation context. If null, any category is acceptable.
    /// </summary>
    public object? IsolationGroupId { get; }

    /// <summary>
    /// Constructor of <see cref="IsolationContextConfigAttribute"/>
    /// </summary>
    /// <param name="createNewContext">If true, always create new context for each test class. Created context will be unloaded at the end of test class run.</param>
    /// <param name="isolationId">Required ID of isolation context. If null, any context is acceptable.</param>
    /// <param name="isolationGroupId">Required group ID of isolation context. If null, any group is acceptable.</param>
    public IsolationContextConfigAttribute(bool createNewContext = true, object? isolationId = null, object? isolationGroupId = null)
    {
        AlwaysCreateNewContext = createNewContext;

        if (createNewContext)
        {
            if (isolationId != null)
                throw new ArgumentException("Isolation ID must be null when createNewContext is true", nameof(isolationId));

            if (isolationGroupId != null)
                throw new ArgumentException("Isolation Group ID must be null when createNewContext is true", nameof(isolationGroupId));
        }

        IsolationId = isolationId;
        IsolationGroupId = isolationGroupId;
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
        && AlwaysCreateNewContext == other.AlwaysCreateNewContext
        && Equals(IsolationId, other.IsolationId)
        && Equals(IsolationGroupId, other.IsolationGroupId);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as IsolationContextConfigAttribute);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(AlwaysCreateNewContext, IsolationId, IsolationGroupId);
}
