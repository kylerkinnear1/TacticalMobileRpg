namespace Rpg.Mobile.GameSdk.Utilities;

public static class StringExtensions
{
    public static bool IsBlank(this string? value) => string.IsNullOrWhiteSpace(value);
    public static bool IsEmpty(this string? value) => string.IsNullOrEmpty(value);

    public static string Join(this IEnumerable<string> values, string separator) =>
        string.Join(separator, values);

    public static string JoinLines(this IEnumerable<string> values, bool removeEmpty = false) =>
        string.Join(
            Environment.NewLine,
            removeEmpty
                ? values.Where(x => !string.IsNullOrEmpty(x))
                : values);
}
