using System.Drawing;
using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.Events;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Setup;

public class SetupPhaseServer(Context _context) : IBattlePhase
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

public class SetupPhaseClient
{
    public void Execute(float deltaTime)
    {
        var currentOrigins = _context.Data.Setup.CurrentPlaceOrder % 2 == 0
            ? _context.Data.Map.Player1Origins
            : _context.Data.Map.Player2Origins;

        var originTiles = currentOrigins
            .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
            .ToList();

        _context.Main.MoveShadow.Shadows.Set(originTiles);
        _context.Main.PlaceUnitSpriteComponent.Visible = IsPlaceableTile(_lastHoveredTile, currentOrigins);
        if (_context.Main.PlaceUnitSpriteComponent.Visible)
        {
            _context.Main.PlaceUnitSpriteComponent.Position = _context.Main
                .GetPositionForTile(_lastHoveredTile!.Value, _context.Main.PlaceUnitSpriteComponent.Bounds.Size);
        }
    }

    public void Leave()
    {
        _context.Main.PlaceUnitSpriteComponent.Visible = false;
    }
    
    private void PlaceUnit(SetupPhaseServer.UnitPlacedEvent evnt)
    {
        var unit = evnt.Unit;
        var component = CreateBattleUnitComponent(unit);
        var state = new BattleUnitComponentStateMachine(component);
        _context.Main.Units[unit] = state;
        _context.Main.AddChild(component);
        
        component.Position = _context.Main.GetPositionForTile(point, component.Bounds.Size);

        if (_context.Data.Setup.CurrentPlaceOrder < _context.Data.Setup.PlaceOrder.Count)
        {
            var nextUnit = _context.Data.Setup.PlaceOrder[_context.Data.Setup.CurrentPlaceOrder];
            _context.Main.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(nextUnit.Stats.UnitType, nextUnit.PlayerId);
        }
    }
    
    private bool IsPlaceableTile(Point? hover, ICollection<Point> currentOrigins) => 
        hover.HasValue && currentOrigins.Contains(hover.Value);
    
    private BattleUnitComponent CreateBattleUnitComponent(BattleUnitData state) =>
        new(new(state, _context.Data), GetUnitSprite(state.Stats.UnitType, state.PlayerId));
    
    private static IImage GetUnitSprite(BattleUnitType type, int playerId) =>
        (type, playerId) switch
        {
            (BattleUnitType.Archer, 0) => Images.ArcherIdle01,
            (BattleUnitType.Healer, 0) => Images.HealerIdle01,
            (BattleUnitType.Mage, 0) =>  Images.MageIdle01,
            (BattleUnitType.Warrior, 0) =>  Images.WarriorIdle01,
            (BattleUnitType.Ninja, 0) =>  Images.NinjaIdle01,
            (BattleUnitType.Archer, 1) =>  Images.ArcherIdle02,
            (BattleUnitType.Healer, 1) =>  Images.HealerIdle02,
            (BattleUnitType.Mage, 1) =>  Images.MageIdle02,
            (BattleUnitType.Warrior, 1) =>  Images.WarriorIdle02,
            (BattleUnitType.Ninja, 1) =>  Images.NinjaIdle02,
            _ => throw new ArgumentException()
        };
}