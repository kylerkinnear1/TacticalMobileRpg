using Rpg.Mobile.Api;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Setup;

public class SetupPhase(Context _context) : IBattlePhase
{
    public record CompletedEvent : IEvent;

    private Point? _lastHoveredTile;

    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = 
        [
            Bus.Global.Subscribe<TileClickedEvent>(TileClicked)
        ];

        var firstUnit = _context.Data.Setup.PlaceOrder[0];
        _context.Main.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(firstUnit.Stats.UnitType, firstUnit.PlayerId);
    }

    public void Leave()
    {
        _subscriptions?.DisposeAll();
    }
    
    private void TileClicked(TileClickedEvent evnt)
    {
        if (_context.Data.UnitCoordinates.ContainsValue(evnt.Tile))
            return;

        var currentOrigins = _context.Data.Setup.CurrentPlaceOrder % 2 == 0
            ? _context.Data.Map.Player1Origins
            : _context.Data.Map.Player2Origins;

        if (!currentOrigins.Contains(evnt.Tile))
            return;

        PlaceUnit(evnt.Tile);
        if (_context.Data.Setup.CurrentPlaceOrder >= _context.Data.Setup.PlaceOrder.Count)
        {
            Bus.Global.Publish(new CompletedEvent());
        }
    }

    private void PlaceUnit(Point tile)
    {
        var unit = _context.Data.Setup.PlaceOrder[_context.Data.Setup.CurrentPlaceOrder];
        _context.Data.UnitCoordinates[unit] = tile;
        _context.Data.Setup.CurrentPlaceOrder++;
        var point = _context.Data.UnitCoordinates[unit];
        Bus.Global.Publish(new UnitPlacedEvent(point, unit));
    }
}