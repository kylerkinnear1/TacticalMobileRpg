using System.Drawing;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.Events;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

// TODO: look at duplication with attack target state. Combine into 'SelectingTarget' state.
public class SelectingMagicTargetStepServer(Context _context) : ActivePhaseServer.IStep
{
    public record MagicTargetSelectedEvent(Point Target) : IEvent;
    
    private BattleData Data => _context.Data;

    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = 
        [
            Bus.Global.Subscribe<TileHoveredEvent>(TileHovered),
            Bus.Global.Subscribe<TileClickedEvent>(TileClicked)
        ];

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
        _subscriptions.DisposeAll();
        _context.Data.Active.SpellTargetTiles.Clear();
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile)) 
            return;
        
        Bus.Global.Publish(new MagicTargetSelectedEvent(evnt.Tile));
    }
}

public class SelectingMagicTargetStepClient
{
    public void Enter()
    {
        _context.Main.AttackTargetHighlight.Range = Data.Active.CurrentSpell.Aoe;
        
        var attackTiles = legalTargets
            .Select(x => 
                new RectF(_context.Main.GetPositionForTile(x, TileSize), TileSize));
        
        _context.Main.AttackShadow.Shadows.Set(attackTiles);
        _context.Menu.SetButton(new("Back", _ => Bus.Global.Publish(new ActivePhaseServer.BackClickedEvent())));
    }
    
    public void Leave()
    {
        _context.Main.AttackTargetHighlight.Visible = false;
        _context.Main.AttackShadow.Shadows.Clear();
    }
    
    private void TileHovered(TileHoveredEvent evnt)
    {
        if (!IsValidMagicTargetTile(evnt.Tile))
        {
            _context.Main.AttackTargetHighlight.Visible = false;
            return;
        }

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Visible = true;
    }
}

public class SelectingMagicTargetStepShared
{
    private bool IsValidMagicTargetTile(Point tile)
    {
        if (!Data.Active.SpellTargetTiles.Contains(tile))
            return false;

        var hoveredUnit = Data.UnitsAt(tile).FirstOrDefault();
        return hoveredUnit != null &&
               (Data.Active.CurrentSpell!.TargetsEnemies && hoveredUnit.PlayerId != Data.CurrentUnit().PlayerId ||
                Data.Active.CurrentSpell.TargetsFriendlies && hoveredUnit.PlayerId == Data.CurrentUnit().PlayerId);
    }
}
