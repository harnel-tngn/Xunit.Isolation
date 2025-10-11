namespace Xunit.Isolation.SkippableFact.Tests;

/// <summary>
/// Struct used as paramters in theory tests.
/// </summary>
public record struct ParameterStruct(int Value)
{
    private static int _staticValue;
    public int StaticValue
    {
        get => _staticValue;
        set => _staticValue = value;
    }
}
