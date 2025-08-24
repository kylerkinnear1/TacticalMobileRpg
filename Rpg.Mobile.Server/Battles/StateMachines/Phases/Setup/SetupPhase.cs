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
    public record UnitPlacedEvent(Point Tile, int UnitId, int CurrentPlaceOrderIndex) : IEvent;
    public record StartedEvent(List<BattleUnitData> Units, BattleSetupPhaseData SetupData) : IEvent;
    
    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        var team1 = _data.Team0
            .Select(x => _data.Map.BaseStats.Single(y => x == y.UnitType))
            .Select((x, i) => new BattleUnitData { UnitId = i, PlayerId = 0, Stats = x});
        var team2 = _data.Team1
            .Select(x => _data.Map.BaseStats.Single(y => x == y.UnitType))
            .Select((x, i) => new BattleUnitData { UnitId = i + _data.Team0.Count, PlayerId = 1, Stats = x});

        _data.Units = team1.Concat(team2).ToList();
        _data.Setup.PlaceOrderIds = team1
            .Zip(team2)
            .SelectMany(x => new[] { x.First.UnitId, x.Second.UnitId })
            .ToList();
        
        _subscriptions = 
        [
            _bus.Subscribe<TileClickedEvent>(TileClicked)
        ];
        
        _bus.Publish(new StartedEvent(_data.Units, _data.Setup));
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

        var unit = _data.Units.Single(x => x.UnitId == _data.Setup.PlaceOrderIds[_data.Setup.CurrentPlaceOrderIndex]);
        var currentOrigins = unit.PlayerId == 0
            ? _data.Map.Player1Origins
            : _data.Map.Player2Origins;

        if (!currentOrigins.Contains(evnt.Tile))
            return;

        PlaceUnit(evnt.Tile);
        if (_data.Setup.CurrentPlaceOrderIndex >= _data.Setup.PlaceOrderIds.Count)
        {
            _bus.Publish(new CompletedEvent());
        }
    }

    private void PlaceUnit(Point tile)
    {
        var unit = _data.Setup.PlaceOrderIds[_data.Setup.CurrentPlaceOrderIndex];
        _data.UnitCoordinates[unit] = tile;
        _data.Setup.CurrentPlaceOrderIndex++;
        var point = _data.UnitCoordinates[unit];
        _bus.Publish(new UnitPlacedEvent(point, unit, _data.Setup.CurrentPlaceOrderIndex));
    }
}