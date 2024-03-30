namespace Rpg.Mobile.GameSdk.Extensions;

// For float fuzzy math
public static class FloatExtensions
{
    public static bool CloseTo(this float a, float b, float tolerance = .001f)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (a == b)
            return true;

        var absA = Math.Abs(a);
        var absB = Math.Abs(b);
        var diff = Math.Abs(a - b);
        if (a == 0 || b == 0 || absA + absB < float.MinValue)
            return diff < (tolerance * float.MinValue);

        return diff / (absA + absB) < tolerance;
    }
}

public static class IntExtensions
{
    public static bool IsBetweenInclusive(this int value, int a, int b) => value >= a && value <= b;
    public static int Abs(this int value) => Math.Abs(value);
    public static int RoundToInt(this double value) => (int)double.Round(value);
}