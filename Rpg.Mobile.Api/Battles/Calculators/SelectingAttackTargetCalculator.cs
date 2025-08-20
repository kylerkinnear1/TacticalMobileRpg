using System.Drawing;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public interface ISelectingAttackTargetCalculator
{
    bool IsValidAttackTargetTile(Point tile, BattleData data);
}

public class SelectingAttackTargetCalculator : ISelectingAttackTargetCalculator
{
    public bool IsValidAttackTargetTile(Point tile, BattleData data)
    {
        if (!data.Active.AttackTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               hoveredUnit.PlayerId != data.CurrentUnit().PlayerId;
    }
}