using Rpg.Mobile.App.Game.Battling.Systems.Data;

namespace Rpg.Mobile.App.Game.Battling.Systems.Calculators;

public class ValidTargetCalculator
{
    private readonly BattleData _state;

    public ValidTargetCalculator(BattleData state) => _state = state;

    public bool IsValidMagicTargetTile(Point tile)
    {
        if (_state.CurrentSpell is null || !_state.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = _state.UnitsAt(tile).FirstOrDefault();
        return _state.Step == BattleStep.SelectingMagicTarget &&
               hoveredUnit != null &&
               ((_state.CurrentSpell.TargetsEnemies && hoveredUnit.PlayerId != _state.CurrentUnit.PlayerId) ||
                (_state.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == _state.CurrentUnit.PlayerId));
    }

    public bool IsValidAttackTargetTile(Point tile)
    {
        if (!_state.AttackTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = _state.UnitsAt(tile).FirstOrDefault();
        return _state.Step == BattleStep.SelectingAttackTarget &&
               hoveredUnit != null &&
               hoveredUnit.PlayerId != _state.CurrentUnit.PlayerId;
    }
}
