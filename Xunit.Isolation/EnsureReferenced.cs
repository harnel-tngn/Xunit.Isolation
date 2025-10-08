using System;

namespace Xunit.Isolation;

/// <summary>
/// Does nothing but ensures that assembly of referenced type is loaded,
/// So make them available for reflection based discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class EnsureReferencedAttribute : Attribute
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public EnsureReferencedAttribute(Type type)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
    }
}
