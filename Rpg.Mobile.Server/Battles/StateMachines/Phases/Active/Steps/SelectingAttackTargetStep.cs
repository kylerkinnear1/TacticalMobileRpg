using System.Drawing;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingAttackTargetStep(Context _context) : ActivePhase.IStep
{
    public record AttackTargetSelectedEvent(BattleUnitData Target) : IEvent;

    public void Enter()
    {
        var gridToUnit = _context.Data.UnitCoordinates.ToLookup<KeyValuePair<BattleUnitData, Point>, Point, BattleUnitData>(x => x.Value, x => x.Key);

        var legalTargets = _context.Path
            .CreateFanOutArea(
                _context.Data.UnitCoordinates[_context.Data.CurrentUnit()],
                _context.Data.Map.Corner(),
                _context.Data.CurrentUnit().Stats.AttackMinRange,
                _context.Data.CurrentUnit().Stats.AttackMaxRange)
            .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != _context.Data.CurrentUnit().PlayerId))
            .ToList();

        _context.Data.Active.AttackTargetTiles.Set(legalTargets);
    }
    
    public void Leave()
    {
        _context.Data.Active.AttackTargetTiles.Clear();
    }
    
    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        var enemy = _context.Data.UnitsAt(evnt.Tile).Single(x => x.PlayerId != _context.Data.CurrentUnit().PlayerId);
        Bus.Global.Publish(new AttackTargetSelectedEvent(enemy));
    }
}