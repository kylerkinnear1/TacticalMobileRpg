namespace Rpg.Mobile.GameEngine.RuleEngine.Battling.Calculators;

public record TargetSource(Unit ActiveUnit, int Range);

public interface ITargetRangeCalculator
{
    IEnumerable<Unit> GetTargetsInRange(Board board, TargetSource source, IEnumerable<Unit> availableTargets);
}

public class TargetCalculator : ITargetRangeCalculator
{
    private readonly IPathCalculator _path;

    public TargetCalculator(IPathCalculator path) => _path = path;

    public IEnumerable<Unit> GetTargetsInRange(Board board, TargetSource source, IEnumerable<Unit> availableTargets)
    {
        var unitLocations = availableTargets.ToDictionary(x => x.Position);
        var targetArea = _path.CreateFanOutArea(source.ActiveUnit.Position, board.Bounds, source.Range);
        return targetArea
            .Where(unitLocations.ContainsKey)
            .Select(x => unitLocations[x])
            .ToList();
    }
}
