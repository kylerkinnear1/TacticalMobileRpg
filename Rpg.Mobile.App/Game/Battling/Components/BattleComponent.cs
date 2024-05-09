using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleComponent : ComponentBase
{
    private static int TileSize => MapComponent.TileSize;

    private readonly MapComponent _map;
    private readonly TileShadowComponent _moveShadow;
    private readonly TileShadowComponent _attackShadow;
    private readonly TileShadowComponent _currentUnitShadow;
    private readonly MultiDamageIndicatorComponent _damageIndicator;
    private readonly TargetIndicatorComponent _attackTargetHighlight;
    private readonly TargetIndicatorComponent _currentHighlightTarget;

    private readonly BattleState _state;
    private readonly BattleStateService _battleService;
    private readonly PathCalculator _path = new();

    private Dictionary<BattleUnitState, BattleUnitComponent> _unitComponents = new();
    private ITween<PointF>? _unitTween;

    private BattleUnitComponent? CurrentUnit => _state.CurrentUnit is not null ? _unitComponents[_state.CurrentUnit] : null;

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);

    public BattleComponent(PointF location, BattleState battle, BattleStateService battleService) 
        : base(CalcBounds(location, battle.Map.Width, battle.Map.Height, TileSize))
    {
        _battleService = battleService;
        _state = battle;

        AddChild(_map = new(battle.Map));
        AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });
        AddChild(_attackTargetHighlight = new(battle.Map, MapComponent.TileSize, _map.Bounds, _path)
        {
            StrokeColor = Colors.Crimson.WithAlpha(.8f),
            StrokeWidth = 10f,
            Visible = false
        });
        AddChild(_currentHighlightTarget = new(battle.Map, MapComponent.TileSize, _map.Bounds, _path)
        {
            StrokeColor = Colors.White.WithAlpha(.7f),
            Visible = false
        });
        _damageIndicator = new(_map.Bounds);

        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<BattleStartedEvent>(_ => StartBattle());
        Bus.Global.Subscribe<UnitsDefeatedEvent>(UnitsDefeated);
        Bus.Global.Subscribe<UnitDamagedEvent>(UnitDamaged);
        Bus.Global.Subscribe<ActiveUnitChangedEvent>(ActiveUnitChanged);
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
        {
            CurrentUnit.Position = _unitTween.Advance(deltaTime);
            _unitTween = _unitTween.IsComplete ? null : _unitTween;
        }

        var walkShadows = _state.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _moveShadow.Shadows.Set(walkShadows);

        var currentUnitPosition = _state.UnitCoordinates[CurrentUnit.State];
        _currentUnitShadow.Shadows.SetSingle(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));

        var attackShadows = _state.AttackTargetTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _attackShadow.Shadows.Set(attackShadows);

        var magicShadows = _state.SpellTargetTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _attackShadow.Shadows.AddRange(magicShadows);

        for (var i = 0; i < _state.TurnOrder.Count; i++)
            _unitComponents[_state.TurnOrder[i]].HealthBar.HasGone = i < _state.ActiveUnitIndex;
    }

    public override void Render(ICanvas canvas, RectF dirtyRect) { }

    private void StartBattle()
    {
        _unitComponents = _state.TurnOrder
            .Select(CreateBattleUnitComponent)
            .Select(AddChild)
            .ToDictionary(x => x.State);

        foreach (var component in _unitComponents.Values)
        {
            var point = _state.UnitCoordinates[component.State];
            component.Position = GetPositionForTile(point, component.Bounds.Size);
        }

        AddChild(_damageIndicator);
    }

    private static BattleUnitComponent CreateBattleUnitComponent(BattleUnitState state) =>
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

    private PointF GetPositionForTile(Point point, SizeF componentSize) => GetPositionForTile(point.X, point.Y, componentSize);

    private PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (TileSize - componentSize.Width) / 2;
        var marginY = (TileSize - componentSize.Height) / 2;
        return new((x * TileSize) + marginX, (y * TileSize) + marginY);
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        // TODO: Fix movement step
        if (_state.Step == BattleStep.Moving && _state.WalkableTiles.Contains(evnt.Tile))
        {
            _state.UnitCoordinates[CurrentUnit.State] = evnt.Tile;

            var finalTarget = GetPositionForTile(evnt.Tile, CurrentUnit.Bounds.Size);
            _unitTween = CurrentUnit.Position.TweenTo(500f, finalTarget);
        }

        _battleService.SelectTile(evnt.Tile);
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        var hoveredUnit = _state.UnitCoordinates.ContainsValue(evnt.Tile)
            ? _state.UnitCoordinates.First(x => x.Value == evnt.Tile).Key
            : null;

        _currentHighlightTarget.Visible = true;
        _currentHighlightTarget.Center = evnt.Tile;

        _attackTargetHighlight.Visible = false;
        if (_battleService.IsValidAttackTargetTile(evnt.Tile))
        {
            _attackTargetHighlight.Center = evnt.Tile;
            _attackTargetHighlight.Visible = true;
        }

        if (_battleService.IsValidMagicTargetTile(evnt.Tile))
        {
            _attackTargetHighlight.Center = evnt.Tile;
            _attackTargetHighlight.Range = _state.CurrentSpell.Aoe;
            _attackTargetHighlight.Visible = true;
        }

        Bus.Global.Publish(new BattleTileHoveredEvent(hoveredUnit));
    }

    private void UnitsDefeated(UnitsDefeatedEvent evnt)
    {
        var defeatedComponents = evnt.Defeated.Select(x => _unitComponents[x]).ToList();
        foreach (var unit in defeatedComponents)
        {
            unit.Visible = false;
        }
    }

    private void UnitDamaged(UnitDamagedEvent evnt)
    {
        var positions = evnt.Hits
            .Select(x => (_unitComponents[x.Unit].Position, x.Damage))
            .ToList();

        _damageIndicator.SetDamage(positions);
    }

    private void ActiveUnitChanged(ActiveUnitChangedEvent evnt)
    {
        _unitTween = null;
        if (evnt.PreviousUnit is null)
            return;

        var previousComponent = _unitComponents[evnt.PreviousUnit];
        var coordinate = _state.UnitCoordinates[evnt.PreviousUnit];
        previousComponent.Position = GetPositionForTile(coordinate, previousComponent.Bounds.Size);
    }
}

public record BattleTileHoveredEvent(BattleUnitState? Unit) : IEvent;
