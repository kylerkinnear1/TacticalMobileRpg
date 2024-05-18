using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Battling.Systems.Handlers;

public class PlaceUnitHandler
{
    private readonly BattleData _state;
    private readonly StartBattleHandler _startBattle;

    public PlaceUnitHandler(BattleData state, StartBattleHandler startBattle)
    {
        _state = state;
        _startBattle = startBattle;
    }

    public void Handle(Point tile)
    {
        if (_state.UnitCoordinates.ContainsValue(tile))
            return;

        var currentOrigins = _state.CurrentPlaceOrder % 2 == 0
            ? _state.Map.Player1Origins
            : _state.Map.Player2Origins;

        if (!currentOrigins.Contains(tile))
            return;

        var unit = _state.PlaceOrder[_state.CurrentPlaceOrder];
        _state.UnitCoordinates[unit] = tile;

        _state.CurrentPlaceOrder++;
        if (_state.CurrentPlaceOrder >= _state.PlaceOrder.Count)
            _startBattle.Handle();

        Bus.Global.Publish(new UnitPlacedEvent(tile, unit));
    }
}

public record UnitPlacedEvent(Point Tile, BattleUnitData Unit) : IEvent;
