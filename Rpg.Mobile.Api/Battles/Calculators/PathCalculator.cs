using System.Drawing;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Api.Battles.Calculators;

public interface IPathCalculator
{
    int Distance(Point a, Point b);
    IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int min, int max);
    IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int max);
}

public class PathCalculator : IPathCalculator
{
    public int Distance(Point a, Point b)
    {
        var xDistance = Math.Abs(a.X - b.X);
        var yDistance = Math.Abs(a.Y - b.Y);
        return xDistance + yDistance;
    }

    public IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int min, int max)
    {
        var inner = CreateFanOutList(source, boundingCorner, min - 1);
        var outer = CreateFanOutList(source, boundingCorner, max);

        return outer.Where(x => !inner.Contains(x)).ToList();
    }

    public IEnumerable<Point> CreateFanOutArea(Point source, Point boundingCorner, int max)
    {
        return CreateFanOutList(source, boundingCorner, max);
    }

    private static List<Point> CreateFanOutList(Point source, Point boundingCorner, int max)
    {
        var legalPoints = new List<Point>(100);
        var left = Math.Max(0, source.X - max);
        var right = Math.Min(source.X + max + 1, boundingCorner.X);
        for (var x = left; x < right && x.IsBetweenInclusive(0, boundingCorner.X); x++)
        {
            var leftDistance = (source.X - x).Abs();
            var yRemaining = max - leftDistance + 1;

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