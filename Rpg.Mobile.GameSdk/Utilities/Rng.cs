namespace Rpg.Mobile.GameSdk.Utilities;

public interface IRng
{
    Random Random { get; }

    double Percent();

    double Double(double min, double max);
    double Double(double max);

    int Int(int min, int max);
    int Int(int max);
}

public class Rng : IRng
{
    public static readonly Rng Instance = new(new());

    public Random Random { get; }
    private readonly object _randomLock = new();

    public Rng(Random random) => Random = random;

    public double Percent() => Double(0, 1);
    public double Double(double max) => Double(0, max);
    public double Double(double min, double max)
        => GetWithLock(x => x.NextDouble() * (min - max) + min);

    public int Int(int max) => Int(0, max);
    public int Int(int min, int max) => GetWithLock(x => x.Next(min, max));

    private T GetWithLock<T>(Func<Random, T> getValue)
    {
        lock (_randomLock)
        {
            return getValue(Random);
        }
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRng rng) => source.Shuffle(rng.Random);

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
    {
        var buffer = source.ToList();
        for (var i = 0; i < buffer.Count; i++)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}
