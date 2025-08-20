using Rpg.Mobile.Api;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingAttackTargetStepClient
{
    public void Enter()
    {
        _context.Main.AttackTargetHighlight.Range = 1;

        var attackTiles = legalTargets
            .Select(x => 
                new RectF(_context.Main.GetPositionForTile(x, TileSize), TileSize));
        
        _context.Main.AttackShadow.Shadows.Set(attackTiles);
        _context.Menu.SetButtons(new ButtonData("Back", _ => Bus.Global.Publish(new ActivePhaseServer.BackClickedEvent())));
    }
    
    public void Leave()
    {
        _context.Main.AttackTargetHighlight.Visible = false;
        _context.Main.AttackShadow.Shadows.Clear();
    }
    
    private void TileHovered(TileHoveredEvent evnt)
    {
        if (!IsValidAttackTargetTile(evnt.Tile)) return;

        _context.Main.AttackTargetHighlight.Center = evnt.Tile;
        _context.Main.AttackTargetHighlight.Range = 1;
        _context.Main.AttackTargetHighlight.Visible = true;
    }
}