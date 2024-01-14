namespace Rpg.Mobile.GameSdk;

public interface IRng
{
    double Percent();

    double Double(double min, double max);
    double Double(double max);

    int Int(int min, int max);
    int Int(int max);
}

public class Rng : IRng
{
    private readonly Random _random;
    private readonly object _randomLock = new();

    public Rng(Random random) => _random = random;

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
            return getValue(_random);
        }
    }
}
