using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Isolation;

/// <summary>
/// Attribute class to config isolation context of test class. <br/>
/// Multiple <see cref="IsolationContext"/> may be shared between test classes if they have the same <see cref="IsolationId"/>.
/// </summary>
public class IsolationContextIdAttribute : IsolationContextConfigBaseAttribute, IEquatable<IsolationContextIdAttribute>
{
    /// <summary>
    /// Required ID of isolation context.
    /// </summary>
    public string IsolationId { get; }

    /// <summary>
    /// Constructor of <see cref="IsolationContextIdAttribute"/>.
    /// Multiple <see cref="IsolationContext"/> may be shared between test classes if they have the same <paramref name="isolationId"/>.
    /// </summary>
    /// <param name="isolationId">Required ID of isolation context.</param>
    public IsolationContextIdAttribute(string isolationId)
    {
        IsolationId = isolationId;
    }

    /// <inheritdoc/>
    public static bool operator ==(IsolationContextIdAttribute? l, IsolationContextIdAttribute? r)
    {
        if (Object.ReferenceEquals(l, null))
            return Object.ReferenceEquals(r, null);

        return l.Equals(r);
    }

    /// <inheritdoc/>
    public static bool operator !=(IsolationContextIdAttribute? l, IsolationContextIdAttribute? r)
        => !(l == r);

    /// <inheritdoc/>
    public bool Equals(IsolationContextIdAttribute? other) =>
        other != null
        && Equals(IsolationId, other.IsolationId);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as IsolationContextIdAttribute);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsolationId);
}
