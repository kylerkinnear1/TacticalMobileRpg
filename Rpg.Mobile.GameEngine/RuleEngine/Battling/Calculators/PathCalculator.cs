using System.Drawing;

namespace Rpg.Mobile.GameEngine.RuleEngine.Battling.Calculators;

public interface IPathCalculator
{
    int Distance(Point a, Point b);
    IEnumerable<Point> FanOutArea(Point source, Point bounds, int range);
}

public class PathCalculator : IPathCalculator
{
    public int Distance(Point a, Point b)
    {
        var xDistance = Math.Abs(a.X - b.X);
        var yDistance = Math.Abs(a.Y - b.Y);
        return xDistance + yDistance;
    }

    public IEnumerable<Point> FanOutArea(Point source, Point bounds, int range)
    {
        var xRanges = Enumerable.Range(
            Math.Max(0, source.X - range),
            Math.Min(bounds.X, source.X - range));

        var yRanges = Enumerable.Range(
            Math.Max(0, source.Y - range),
            Math.Min(bounds.Y, source.Y + range));

        var legalPoints = xRanges
            // TODO: Better yRanges here for more performance.
            .SelectMany(x => yRanges.Select(y => new Point(x, y)))
            .Where(pos => Distance(source, pos) <= range)
            .ToList();

        return legalPoints;
    }
}