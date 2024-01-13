using System.Drawing;

namespace Rpg.Mobile.GameEngine.RuleEngine.Battling.Calculators;

public record TargetSource(Unit ActiveUnit, int Range, bool TargetsFriendlies, bool TargetsEnemies);

public interface IRangeCalculator
{
    IEnumerable<Point> GetLegalTargets(Board board, Unit activeUnit, TargetSource source);
}

public class RangeCalculator : IRangeCalculator
{
    private readonly IPathCalculator _path;

    public RangeCalculator(IPathCalculator path) => _path = path;

    public IEnumerable<Point> GetLegalTargets(Board board, Unit activeUnit, TargetSource source)
    {
        var xRanges = Enumerable.Range(
            Math.Max(0, activeUnit.Position.X - source.Range),
            Math.Min(board.Bounds.X, activeUnit.Position.X + source.Range));

        var yRanges = Enumerable.Range(
            Math.Max(0, activeUnit.Position.Y - source.Range),
            Math.Min(board.Bounds.Y, activeUnit.Position.Y + source.Range));

        var enemyLocations = board.Units
            .Where(x => activeUnit.PlayerId != x.Value.PlayerId)
            .ToDictionary(x => x.Value.Position);

        var friendlyLocations = board.Units
            .Where(x => activeUnit.PlayerId == x.Value.PlayerId)
            .ToDictionary(x => x.Value.Position);

        var pointsInRange = xRanges
            .SelectMany(x => yRanges
                .Select(y => new Point(x, y)))
            .Where(x =>
                (source.TargetsEnemies || !enemyLocations.ContainsKey(x)) &&
                (source.TargetsFriendlies || !friendlyLocations.ContainsKey(x)) &&
                _path.ShortestDistance(activeUnit.Position, x) <= source.Range)
            .ToList();

        return pointsInRange;
    }
}
