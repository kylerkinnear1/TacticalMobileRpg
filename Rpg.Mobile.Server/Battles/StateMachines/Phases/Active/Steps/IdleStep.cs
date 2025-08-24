using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class IdleStep(
    BattleData _data,
    IEventBus _bus,
    IPathCalculator _path) : ActivePhase.IStep
{
    public record StartedEvent(List<Point> WalkableTiles) : IEvent;
    public record CompletedEvent(int UnitId) : IEvent;

    private ISubscription[] _subscriptions = [];
    
    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<TileClickedEvent>(TileClicked)
        ];
        
        var walkableTiles = _path
            .CreateFanOutArea(_data.Active.ActiveUnitStartPosition, _data.Map.Corner(), _data.CurrentUnit().Stats.Movement)
            .Where(x => 
                x == _data.Active.ActiveUnitStartPosition ||
                !_data.UnitCoordinates.ContainsValue(x) &&
                _data.Map.Tiles[x.X, x.Y].Type != TerrainType.Rock)
            .ToList();
        _data.Active.WalkableTiles = walkableTiles;
        
        _bus.Publish(new StartedEvent(_data.Active.WalkableTiles));
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        _data.Active.WalkableTiles.Clear();
        _subscriptions.DisposeAll();
        _bus.Publish(new CompletedEvent(_data.CurrentUnit().UnitId));
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        if (!_data.Active.WalkableTiles.Contains(evnt.Tile))
        {
            return;
        }

        _data.UnitCoordinates[_data.CurrentUnit().UnitId] = evnt.Tile;
        _bus.Publish(new ActivePhase.UnitMovedEvent(evnt.Tile));
    }
}