using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.Systems.Data;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.MainBattleStateMachine;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.MainBattle.States;

public class SetupState : IBattleState
{
    private readonly Context _context;

    private Point? _lastHoveredTile;
    private static int TileSize => MainBattleComponent.TileSize;

    public SetupState(Context context) => _context = context;

    public void Enter()
    {
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
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

        _context.Main.PlaceUnitSprite.Visible = _lastHoveredTile.HasValue && currentOrigins.Contains(_lastHoveredTile.Value);
    }

    public void Leave()
    {
        Bus.Global.Unsubscribe<TileHoveredEvent>(TileHovered);
        _context.Main.AddChild(_context.Main.DamageIndicator);
        _context.Data.TurnOrder = _context.Data.PlaceOrder.OrderBy(_ => Guid.NewGuid()).ToList();
        _context.Data.ActiveUnitIndex = 0;
    }

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
            Bus.Global.Publish(new UnitPlacementCompletedEvent());
    }

    private void PlaceUnit(Point tile)
    {
        var unit = _context.Data.PlaceOrder[_context.Data.CurrentPlaceOrder];
        _context.Data.UnitCoordinates[unit] = tile;
        _context.Data.CurrentPlaceOrder++;

        var component = CreateBattleUnitComponent(unit);
        _context.Main.Units[unit] = component;
        _context.Main.AddChild(component);

        var point = _context.Data.UnitCoordinates[component.State];
        component.Position = _context.Main.GetPositionForTile(point, component.Bounds.Size);

        Bus.Global.Publish(new UnitPlacedEvent(tile, unit));
    }

    private static BattleUnitComponent CreateBattleUnitComponent(BattleUnitData state) =>
        (state.Stats.UnitType, state.PlayerId) switch
        {
            (BattleUnitType.Archer, 0) => new BattleUnitComponent(Images.ArcherIdle01, state),
            (BattleUnitType.Healer, 0) => new BattleUnitComponent(Images.HealerIdle01, state),
            (BattleUnitType.Mage, 0) => new BattleUnitComponent(Images.MageIdle01, state),
            (BattleUnitType.Warrior, 0) => new BattleUnitComponent(Images.WarriorIdle01, state),
            (BattleUnitType.Ninja, 0) => new BattleUnitComponent(Images.NinjaIdle01, state),
            (BattleUnitType.Archer, 1) => new BattleUnitComponent(Images.ArcherIdle02, state),
            (BattleUnitType.Healer, 1) => new BattleUnitComponent(Images.HealerIdle02, state),
            (BattleUnitType.Mage, 1) => new BattleUnitComponent(Images.MageIdle02, state),
            (BattleUnitType.Warrior, 1) => new BattleUnitComponent(Images.WarriorIdle02, state),
            (BattleUnitType.Ninja, 1) => new BattleUnitComponent(Images.NinjaIdle02, state),
            _ => throw new ArgumentException()
        };
}

public record UnitPlacedEvent(Point Tile, BattleUnitData Unit) : IEvent;
public record UnitPlacementCompletedEvent : IEvent;
