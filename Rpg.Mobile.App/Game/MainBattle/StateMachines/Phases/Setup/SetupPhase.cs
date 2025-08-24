using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;

public class SetupPhase : IBattlePhase
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly IEventBus _bus;

    private Point? _lastHoveredTile = null;

    private ISubscription[] _subscriptions = [];

    public SetupPhase(BattleData data, MainBattleComponent mainBattle, IEventBus bus)
    {
        _data = data;
        _mainBattle = mainBattle;
        _bus = bus;
    }

    public void Enter()
    {
        var firstUnit = _data.Units.Single(x => x.UnitId == _data.Setup.PlaceOrderIds[0]);
        _mainBattle.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(firstUnit.Stats.UnitType, firstUnit.PlayerId);

        _subscriptions =
        [
            _bus.Subscribe<BattleNetwork.UnitPlacedEvent>(UnitPlaced),
            _bus.Subscribe<GridComponent.TileHoveredEvent>(TileHovered)
        ];
    }

    public void Execute(float deltaTime)
    {
        var currentOrigins = _data.Setup.CurrentPlaceOrderIndex % 2 == 0
            ? _data.Map.Player1Origins
            : _data.Map.Player2Origins;

        var originTiles = currentOrigins
            .Select(x => new RectF(
                x.X * MainBattleComponent.TileWidth,
                x.Y * MainBattleComponent.TileWidth,
                MainBattleComponent.TileWidth,
                MainBattleComponent.TileWidth))
            .ToList();

        _mainBattle.MoveShadow.Shadows.Set(originTiles);
        _mainBattle.PlaceUnitSpriteComponent.Visible = IsPlaceableTile(_lastHoveredTile, currentOrigins);
        if (_mainBattle.PlaceUnitSpriteComponent.Visible)
        {
            _mainBattle.PlaceUnitSpriteComponent.Position = _mainBattle
                .GetPositionForTile(_lastHoveredTile!.Value, _mainBattle.PlaceUnitSpriteComponent.Bounds.Size);
        }
    }

    public void Leave()
    {
        _mainBattle.PlaceUnitSpriteComponent.Visible = false;
        _subscriptions.DisposeAll();
    }

    private void UnitPlaced(BattleNetwork.UnitPlacedEvent evnt)
    {
        _data.Setup.CurrentPlaceOrderIndex = evnt.CurrentUnitPlaceOrderIndex;
        _data.UnitCoordinates[evnt.UnitId] = evnt.Tile;
        
        var unit = _data.Units.Single(x => x.UnitId == evnt.UnitId);
        var component = CreateBattleUnitComponent(unit);
        var state = new BattleUnitComponentStateMachine(_bus, component);
        _mainBattle.Units[unit] = state;
        _mainBattle.AddChild(component);

        component.Position = _mainBattle.GetPositionForTile(evnt.Tile, component.Bounds.Size);

        if (_data.Setup.CurrentPlaceOrderIndex < _data.Setup.PlaceOrderIds.Count)
        {
            var nextUnit = _data.Units.Single(x => x.UnitId == _data.Setup.PlaceOrderIds[_data.Setup.CurrentPlaceOrderIndex]);
            _mainBattle.PlaceUnitSpriteComponent.Sprite = GetUnitSprite(nextUnit.Stats.UnitType, nextUnit.PlayerId);
        }
    }

    private bool IsPlaceableTile(Point? hover, ICollection<Point> currentOrigins) =>
        hover.HasValue && currentOrigins.Contains(hover.Value);

    private BattleUnitComponent CreateBattleUnitComponent(BattleUnitData state) =>
        new(new(state, _data), GetUnitSprite(state.Stats.UnitType, state.PlayerId));

    private static IImage GetUnitSprite(BattleUnitType type, int playerId) =>
        (type, playerId) switch
        {
            (BattleUnitType.Archer, 0) => Images.ArcherIdle01,
            (BattleUnitType.Healer, 0) => Images.HealerIdle01,
            (BattleUnitType.Mage, 0) => Images.MageIdle01,
            (BattleUnitType.Warrior, 0) => Images.WarriorIdle01,
            (BattleUnitType.Ninja, 0) => Images.NinjaIdle01,
            (BattleUnitType.Archer, 1) => Images.ArcherIdle02,
            (BattleUnitType.Healer, 1) => Images.HealerIdle02,
            (BattleUnitType.Mage, 1) => Images.MageIdle02,
            (BattleUnitType.Warrior, 1) => Images.WarriorIdle02,
            (BattleUnitType.Ninja, 1) => Images.NinjaIdle02,
            _ => throw new ArgumentException()
        };
    
    private void TileHovered(GridComponent.TileHoveredEvent evnt)
    {
        _lastHoveredTile = evnt.Tile;
    }
}