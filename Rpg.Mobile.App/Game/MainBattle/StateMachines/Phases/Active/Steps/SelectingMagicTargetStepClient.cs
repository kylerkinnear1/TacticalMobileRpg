using Rpg.Mobile.Api;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

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