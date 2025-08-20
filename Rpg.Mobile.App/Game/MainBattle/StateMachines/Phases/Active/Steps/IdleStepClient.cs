namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class IdleStepClient
{
    public void Enter()
    {
        _context.Menu.SetButtons(
            new("Attack", _ => Bus.Global.Publish(new ActivePhaseServer.AttackClickedEvent())),
            new("Magic", _ => Bus.Global.Publish(new ActivePhaseServer.MagicClickedEvent())),
            new("Wait", _ => Bus.Global.Publish(new CompletedEvent(_context.Data.CurrentUnit()))));
    }
    
    public void Execute(float deltaTime)
    {
        var walkShadows = _context.Data.Active.WalkableTiles.Select(x => new RectF(x.X * TileWidth, x.Y * TileWidth, TileWidth, TileWidth));
        _context.Main.MoveShadow.Shadows.Set(walkShadows);
    }
    
    public void Leave()
    {
        _context.Main.MoveShadow.Shadows.Clear();
    }
    
    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_context.Data.Active.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }
        
        _context.Data.UnitCoordinates[_context.Data.CurrentUnit()] = evnt.Tile;
        var finalTarget = _context.Main.GetPositionForTile(evnt.Tile, _context.Main.CurrentUnit.Unit.Bounds.Size);
        _context.Main.Units[_context.Data.CurrentUnit()].MoveTo(finalTarget, speed: 500f);
    }
}