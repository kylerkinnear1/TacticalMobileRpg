using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Setup;

public class SetupPhase(
    BattleData _data,
    IEventBus _bus) : IBattlePhase
{
    public record CompletedEvent : IEvent;
    public record UnitPlacedEvent(Point Tile, BattleUnitData Unit) : IEvent;
    
    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = 
        [
            _bus.Subscribe<TileClickedEvent>(TileClicked)
        ];
    }

    public void Execute(float deltaTime) { }

    public void Leave()
    {
        _subscriptions.DisposeAll();
    }
    
    private void TileClicked(TileClickedEvent evnt)
    {
        if (_data.UnitCoordinates.ContainsValue(evnt.Tile))
            return;

        var currentOrigins = _data.Setup.CurrentPlaceOrder % 2 == 0
            ? _data.Map.Player1Origins
            : _data.Map.Player2Origins;

        if (!currentOrigins.Contains(evnt.Tile))
            return;

        PlaceUnit(evnt.Tile);
        if (_data.Setup.CurrentPlaceOrder >= _data.Setup.PlaceOrder.Count)
        {
            _bus.Publish(new CompletedEvent());
        }
    }

    private void PlaceUnit(Point tile)
    {
        var unit = _data.Setup.PlaceOrder[_data.Setup.CurrentPlaceOrder];
        _data.UnitCoordinates[unit] = tile;
        _data.Setup.CurrentPlaceOrder++;
        var point = _data.UnitCoordinates[unit];
        _bus.Publish(new UnitPlacedEvent(point, unit));
    }
}