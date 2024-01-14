namespace Rpg.Mobile.GameEngine.RuleEngine;

public interface IRng
{
    double Percent();

    double Double(double min, double max);
    double Double(double max);
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

    public double Double(Range<double> range) => Double(range.Start, range.End);

    private T GetWithLock<T>(Func<Random, T> getValue)
    {
        lock (_randomLock)
        {
            return getValue(_random);
        }
    }
}

public record Range<T>(T Start, T End);
