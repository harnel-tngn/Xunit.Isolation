using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Isolation;

/// <summary>
/// Attribute class to config isolation context of test class. Assigns pool ID. <br/>
/// Multiple <see cref="IsolationContext"/> may be created but not shared between test classes if they have the same <see cref="IsolationPoolId"/>.
/// </summary>
public class IsolationContextPoolIdAttribute : IsolationContextConfigBaseAttribute, IEquatable<IsolationContextPoolIdAttribute>
{
    /// <summary>
    /// Required ID of isolation context.
    /// </summary>
    public string IsolationPoolId { get; }

    /// <summary>
    /// Constructor of <see cref="IsolationContextPoolIdAttribute"/>.
    /// </summary>
    /// <param name="isolationPoolId">Required pool ID of isolation context.</param>
    public IsolationContextPoolIdAttribute(string isolationPoolId)
    {
        IsolationPoolId = isolationPoolId;
    }

    /// <inheritdoc/>
    public bool Equals(IsolationContextPoolIdAttribute? other) =>
        other != null
        && Equals(IsolationPoolId, other.IsolationPoolId);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as IsolationContextPoolIdAttribute);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsolationPoolId);
}