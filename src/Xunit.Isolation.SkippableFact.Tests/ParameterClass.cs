namespace Xunit.Isolation.SkippableFact.Tests;

/// <summary>
/// Class used as paramters in theory tests.
/// </summary>
public record class ParameterClass(int Value)
{
    private static int _staticValue;
    public int StaticValue
    {
        get => _staticValue;
        set => _staticValue = value;
    }
}
