using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;

public class SetupPhase(Context _context) : IBattlePhase
{
    public record UnitPlacedEvent(BattleUnitData Unit) : IEvent;
    public record UnitPlacementCompletedEvent : IEvent;
    public record CompletedEvent : IEvent;

    private Point? _lastHoveredTile;
    private static int TileSize => MainBattleComponent.TileSize;

    private ISubscription[] _subscriptions = [];

    public void Enter()
    {
        _subscriptions = 
        [
            Bus.Global.Subscribe<TileHoveredEvent>(TileHovered),
            Bus.Global.Subscribe<TileClickedEvent>(TileClicked)
        ];

        var firstUnit = _context.Data.PlaceOrder[0];
        _context.Main.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(firstUnit.Stats.UnitType, firstUnit.PlayerId);
    }

    public void Execute(float deltaTime)
    {
        var currentOrigins = _context.Data.CurrentPlaceOrder % 2 == 0
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

    public void Leave() => _subscriptions?.DisposeAll();

    private void TileHovered(TileHoveredEvent evnt) => _lastHoveredTile = evnt.Tile;

    private void TileClicked(TileClickedEvent evnt)
    {
        if (_context.Data.UnitCoordinates.ContainsValue(evnt.Tile))
            return;

        var currentOrigins = _context.Data.CurrentPlaceOrder % 2 == 0
            ? _context.Data.Map.Player1Origins
            : _context.Data.Map.Player2Origins;

        if (!currentOrigins.Contains(evnt.Tile))
            return;

        PlaceUnit(evnt.Tile);
        if (_context.Data.CurrentPlaceOrder >= _context.Data.PlaceOrder.Count)
        {
            Bus.Global.Publish(new UnitPlacementCompletedEvent());
        }
    }

    private void PlaceUnit(Point tile)
    {
        var unit = _context.Data.PlaceOrder[_context.Data.CurrentPlaceOrder];
        _context.Data.UnitCoordinates[unit] = tile;
        _context.Data.CurrentPlaceOrder++;

        var component = CreateBattleUnitComponent(unit);
        var state = new BattleUnitComponentStateMachine(component);
        _context.Main.Units[unit] = state;
        _context.Main.AddChild(component);

        var point = _context.Data.UnitCoordinates[unit];
        component.Position = _context.Main.GetPositionForTile(point, component.Bounds.Size);
        component.HealthBar.RemainingHealth = unit.Stats.MaxHealth;
        component.HealthBar.PlayerId = unit.PlayerId;

        Bus.Global.Publish(new UnitPlacedEvent(unit));

        if (_context.Data.CurrentPlaceOrder < _context.Data.PlaceOrder.Count)
        {
            var nextUnit = _context.Data.PlaceOrder[_context.Data.CurrentPlaceOrder];
            _context.Main.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(nextUnit.Stats.UnitType, nextUnit.PlayerId);
        }
    }

    private bool IsPlaceableTile(Point? hover, ICollection<Point> currentOrigins) => 
        hover.HasValue && currentOrigins.Contains(hover.Value);

    private static BattleUnitComponent CreateBattleUnitComponent(BattleUnitData state) =>
        new(GetUnitSprite(state.Stats.UnitType, state.PlayerId));

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