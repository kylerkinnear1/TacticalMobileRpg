using Rpg.Mobile.Api.Battles.Data;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class IdleStep(Context _context) : ActivePhase.IStep
{
    public void Enter()
    {
        var walkableTiles = _context.Path
            .CreateFanOutArea(_context.Data.Active.ActiveUnitStartPosition, _context.Data.Map.Corner(), _context.Data.CurrentUnit().Stats.Movement)
            .Where(x => 
                x == _context.Data.Active.ActiveUnitStartPosition ||
                !_context.Data.UnitCoordinates.ContainsValue(x) &&
                _context.Data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();
        _context.Data.Active.WalkableTiles = walkableTiles;
    }

    public void Leave()
    {
        _context.Data.Active.WalkableTiles.Clear();
    }

    public void TileClicked(TileClickedEvent evnt)
    {
        if (!_context.Data.Active.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _context.Data.UnitCoordinates[_context.Data.CurrentUnit()] = evnt.Tile;
    }
}