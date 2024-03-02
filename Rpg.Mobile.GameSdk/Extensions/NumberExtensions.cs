namespace Rpg.Mobile.GameSdk.Extensions;

// For float fuzzy math
public static class FloatExtensions
{
    public static float Mod(this float value, float mod, float tolerance = .1f)
    {
        throw new NotImplementedException();
    }

    public static float Equal(this float a, float b, float tolerance = .1f)
    {
        throw new NotImplementedException();
    }
}

public static class IntExtensions
{
    public static bool IsBetweenInclusive(this int value, int a, int b) => value >= a && value <= b;
    public static int Abs(this int value) => Math.Abs(value);
}