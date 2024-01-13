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
        var unitLocations = board.Units.ToDictionary(x => x.Value.Position);
        var targetArea = _path.FanOutArea(activeUnit.Position, board.Bounds, source.Range);
        return targetArea
            .Where(x =>
                unitLocations.TryGetValue(x, out var unit) &&
                ((source.TargetsEnemies && unit.Value.PlayerId != source.ActiveUnit.PlayerId) ||
                 (source.TargetsFriendlies && unit.Value.PlayerId == source.ActiveUnit.PlayerId)))
            .ToList();
    }
}
