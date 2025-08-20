using Rpg.Mobile.Api;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active;

public class ActivePhaseClient
{
    public void Execute(float deltaTime)
    {
        var currentUnitPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit()];
        _context.Main.CurrentUnitShadow.Shadows.SetSingle(
            new(currentUnitPosition.X * TileWidth, currentUnitPosition.Y * TileWidth, TileWidth, TileWidth));
        _step.Execute(deltaTime);
    }

    private void BackClicked(ActivePhaseBackClickedEvent evnt)
    {
        var position = _context.Main.GetPositionForTile(
            _context.Data.Active.ActiveUnitStartPosition, 
            _context.Main.CurrentUnit.Unit.Bounds.Size);
        _context.Main.CurrentUnit.MoveTo(
            position, 
            () => _step.Change(new IdleStepServer(_context)));
    }
}