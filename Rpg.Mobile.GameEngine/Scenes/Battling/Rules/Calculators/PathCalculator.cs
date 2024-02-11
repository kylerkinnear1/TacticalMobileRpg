using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;

namespace Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;

public interface IPathCalculator
{
    int Distance(Coordinate a, Coordinate b);
    IEnumerable<Coordinate> CreateFanOutArea(Coordinate source, Coordinate boundingCorner, int range);
}

public class PathCalculator : IPathCalculator
{
    public int Distance(Coordinate a, Coordinate b)
    {
        var xDistance = Math.Abs(a.X - b.X);
        var yDistance = Math.Abs(a.Y - b.Y);
        return xDistance + yDistance;
    }

    public IEnumerable<Coordinate> CreateFanOutArea(Coordinate source, Coordinate boundingCorner, int range)
    {
        var xRanges = Enumerable.Range(
            Math.Max(0, source.X - range),
            Math.Min(boundingCorner.X, source.X - range));

        var yRanges = Enumerable.Range(
            Math.Max(0, source.Y - range),
            Math.Min(boundingCorner.Y, source.Y + range));

        var legalPoints = xRanges
            // TODO: Better yRanges here for more performance. This checks stuff that will never
            // be in range.
            .SelectMany(x => yRanges.Select(y => new Coordinate(x, y)))
            .Where(pos => Distance(source, pos) <= range)
            .ToList();

        return legalPoints;
    }
}