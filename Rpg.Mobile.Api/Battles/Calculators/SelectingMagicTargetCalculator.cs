using System.Drawing;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public interface ISelectingMagicTargetCalculator
{
    bool IsValidMagicTargetTile(Point tile, BattleData data);
}

public class SelectingMagicTargetCalculator : ISelectingMagicTargetCalculator
{
    public bool IsValidMagicTargetTile(Point tile, BattleData data)
    {
        if (!data.Active.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               (data.Active.CurrentSpell!.TargetsEnemies && hoveredUnit.PlayerId != data.CurrentUnit().PlayerId ||
                data.Active.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == data.CurrentUnit().PlayerId);
    }
}