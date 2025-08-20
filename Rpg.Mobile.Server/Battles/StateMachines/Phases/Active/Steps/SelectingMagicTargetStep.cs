using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetStep(Context _context) : ActivePhase.IStep
{
    public record MagicTargetSelectedEvent(Point Target) : IEvent;
    
    private BattleData Data => _context.Data;
    
    public void Enter()
    {
        var gridToUnit = _context.Data.UnitCoordinates.ToLookup(x => x.Value, x => x.Key);
        var legalTargets = _context.Path
            .CreateFanOutArea(
                Data.UnitCoordinates[_context.Data.CurrentUnit()],
                Data.Map.Corner(),
                Data.Active.CurrentSpell!.MinRange,
                Data.Active.CurrentSpell.MaxRange)
            .Where(x =>
                !gridToUnit.Contains(x) ||
                Data.Active.CurrentSpell.TargetsEnemies && gridToUnit[x].Any(y => y.PlayerId != Data.CurrentUnit().PlayerId) ||
                 Data.Active.CurrentSpell.TargetsFriendlies && gridToUnit[x].Any(y => y.PlayerId == Data.CurrentUnit().PlayerId))
            .ToList();

        _context.Data.Active.SpellTargetTiles.Set(legalTargets);
    }
    public void Leave()
    {
        _context.Data.Active.SpellTargetTiles.Clear();
    }

    public void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) 
            return;
        
        Bus.Global.Publish(new MagicTargetSelectedEvent(evnt.Tile));
    }
}