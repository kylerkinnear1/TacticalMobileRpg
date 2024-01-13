using System.Drawing;

namespace Rpg.Mobile.GameEngine.RuleEngine.Battling.Calculators;

public interface IPathCalculator
{
    int ShortestDistance(Point a, Point b);
}

public class PathCalculator : IPathCalculator
{
    public int ShortestDistance(Point a, Point b) => ShortestDistance(a, b, Enumerable.Empty<Point>());

    public int ShortestDistance(Point a, Point b, IEnumerable<Point> blockedPoints) => throw new NotImplementedException();
}