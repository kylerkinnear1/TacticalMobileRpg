using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Battling.Systems.Handlers;

public class ChangeBattleStateHandler
{
    private readonly BattleData _state;
    private readonly IPathCalculator _path;

    public ChangeBattleStateHandler(BattleData state, IPathCalculator path)
    {
        _state = state;
        _path = path;
    }

    public void Handle(BattleStep step)
    {
        _state.AttackTargetTiles.Clear();
        _state.WalkableTiles.Clear();
        _state.SpellTargetTiles.Clear();
        _state.CurrentSpell = step == BattleStep.SelectingMagicTarget ? _state.CurrentSpell : null;
        _state.Step = step switch
        {
            BattleStep.Moving => SetupMovingStep(),
            BattleStep.SelectingAttackTarget => SetupAttackTarget(),
            BattleStep.SelectingSpell => SetupSelectSpell(),
            BattleStep.SelectingMagicTarget => SetupMagicTarget(),
            _ => throw new ArgumentException()
        };

        Bus.Global.Publish(new BattleStepChangedEvent(step));
    }

    private BattleStep SetupMovingStep()
    {
        var walkableTiles = _path
            .CreateFanOutArea(_state.ActiveUnitStartPosition, _state.Map.Corner, _state.CurrentUnit.Stats.Movement)
            .Where(x => x == _state.ActiveUnitStartPosition ||
                        !_state.UnitCoordinates.ContainsValue(x) && 
                        _state.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();

        _state.WalkableTiles = walkableTiles;
        return BattleStep.Moving;
    }

    private BattleStep SetupAttackTarget()
    {
        var gridToUnit = _state.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);

        var legalTargets = _path
            .CreateFanOutArea(
                _state.UnitCoordinates[_state.CurrentUnit],
                _state.Map.Corner,
                _state.CurrentUnit.Stats.AttackMinRange,
                _state.CurrentUnit.Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != _state.CurrentUnit.PlayerId))
            .ToList();

        _state.AttackTargetTiles.Set(legalTargets);
        return BattleStep.SelectingAttackTarget;
    }

    private BattleStep SetupSelectSpell()
    {
        return BattleStep.SelectingSpell;
    }

    private BattleStep SetupMagicTarget()
    {
        var gridToUnit = _state.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var allTargets = _path
            .CreateFanOutArea(
                _state.UnitCoordinates[_state.CurrentUnit],
                _state.Map.Corner,
                _state.CurrentSpell.MinRange,
                _state.CurrentSpell.MaxRange)
            .Where(x =>
                !gridToUnit.Contains(x) ||
                (_state.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != _state.CurrentUnit.PlayerId) ||
                 (_state.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == _state.CurrentUnit.PlayerId))))
            .ToList();

        _state.SpellTargetTiles.Set(allTargets);
        return BattleStep.SelectingMagicTarget;
    }
}