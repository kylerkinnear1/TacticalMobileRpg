using Rpg.Mobile.Api;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class IdleStepServer(Context _context) : ActivePhaseServer.IStep
{
    public record CompletedEvent(BattleUnitData CurrentUnit) : IEvent;
    
    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = [Bus.Global.Subscribe<TileClickedEvent>(TileClicked)];

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
        _subscriptions.DisposeAll();
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_context.Data.Active.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _context.Data.UnitCoordinates[_context.Data.CurrentUnit()] = evnt.Tile;
    }
}

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
