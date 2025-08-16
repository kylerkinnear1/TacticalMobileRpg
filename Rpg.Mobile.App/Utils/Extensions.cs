namespace Rpg.Mobile.App.Utils;

public static class Extensions
{
    public static void Set<T>(this List<T> source, IEnumerable<T> values)
    {
        source.Clear();
        source.AddRange(values);
    }

    public static void SetSingle<T>(this List<T> source, T value)
    {
        source.Clear();
        source.Add(value);
    }
}
