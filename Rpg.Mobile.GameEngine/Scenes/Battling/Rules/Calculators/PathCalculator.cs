using Rpg.Mobile.GameSdk.Extensions;
using System.Drawing;

namespace Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;

public interface IPathCalculator
{
    int Distance(Point a, Point b);
    IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int range);
}

public class PathCalculator : IPathCalculator
{
    public int Distance(Point a, Point b)
    {
        var xDistance = Math.Abs(a.X - b.X);
        var yDistance = Math.Abs(a.Y - b.Y);
        return xDistance + yDistance;
    }

    public IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int range)
    {
        var legalPoints = new List<Point>(100);
        var left = Math.Max(0, source.X - range);
        var right = Math.Min(source.X + range + 1, boundingCorner.X);
        for (var x = left; x < right && x.IsBetweenInclusive(0, boundingCorner.X); x++)
        {
            var leftDistance = (source.X - x).Abs();
            var yRemaining = range - leftDistance + 1;

            if (source.Y.IsBetweenInclusive(0, boundingCorner.Y - 1))
            legalPoints.Add(new(x, source.Y));

            for (var i = 1; i < yRemaining; i++)
            {
                if ((source.Y + i).IsBetweenInclusive(0, boundingCorner.Y - 1))
                    legalPoints.Add(new(x, source.Y + i));

                if ((source.Y - i).IsBetweenInclusive(0, boundingCorner.Y - 1))
                    legalPoints.Add(new(x, source.Y - i));
            }
        }

        return legalPoints;
    }
}