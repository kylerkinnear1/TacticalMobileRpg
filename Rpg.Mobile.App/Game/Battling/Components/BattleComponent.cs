﻿using Rpg.Mobile.App.Game.Battling.Systems;
using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Battling.Systems.Handlers;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
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
    private readonly TextIndicatorComponent _message;
    private readonly TargetIndicatorComponent _attackTargetHighlight;
    private readonly TargetIndicatorComponent _currentHighlightTarget;
    private readonly Sprite _placeUnitSprite;

    private readonly BattleData _state;
    private readonly BattleStateService _battleService;

    private Dictionary<BattleUnitData, BattleUnitComponent> _unitComponents = new();
    private ITween<PointF>? _unitTween;

    private BattleUnitComponent? CurrentUnit => _state.CurrentUnit is not null ? _unitComponents[_state.CurrentUnit] : null;

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);

    public BattleComponent(PointF location, BattleData battle, BattleStateService battleService) 
        : base(CalcBounds(location, battle.Map.Width, battle.Map.Height, TileSize))
    {
        _battleService = battleService;
        _state = battle;

        var path = new PathCalculator();
        AddChild(_map = new(battle.Map));
        AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });
        AddChild(_attackTargetHighlight = new(battle.Map, MapComponent.TileSize, _map.Bounds, path)
        {
            StrokeColor = Colors.Crimson.WithAlpha(.8f),
            StrokeWidth = 10f,
            Visible = false
        });
        AddChild(_currentHighlightTarget = new(battle.Map, MapComponent.TileSize, _map.Bounds, path)
        {
            StrokeColor = Colors.White.WithAlpha(.7f),
            Visible = false
        });
        AddChild(_placeUnitSprite = new(Images.WarriorIdle01) { Visible = false });
        _placeUnitSprite.UpdateScale(1.5f);
        _damageIndicator = new(_map.Bounds);
        AddChild(_message = new() 
        { 
            Bounds = new(_map.Bounds.Left, _map.Bounds.Height - 10f, _map.Bounds.Width, 200f)
        });
        _message.Position = new(_map.Bounds.Left, _map.Bounds.Top - 10f);

        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<BattleStartedEvent>(_ => AddChild(_damageIndicator));
        Bus.Global.Subscribe<UnitsDefeatedEvent>(UnitsDefeated);
        Bus.Global.Subscribe<UnitDamagedEvent>(UnitDamaged);
        Bus.Global.Subscribe<ActiveUnitChangedEvent>(ActiveUnitChanged);
        Bus.Global.Subscribe<UnitMovedEvent>(UnitMoved);
        Bus.Global.Subscribe<UnitPlacedEvent>(UnitPlaced);
        Bus.Global.Subscribe<NotEnoughMpEvent>(_ => ShowMessage("Not enough MP."));
    }

    public override void Update(float deltaTime)
    {
        if (_state.Step == BattleStep.Setup)
        {
            var currentOrigins = _state.CurrentPlaceOrder % 2 == 0
                ? _state.Map.Player1Origins
                : _state.Map.Player2Origins;

            var originTiles = currentOrigins
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
            _moveShadow.Shadows.Set(originTiles);
            return;
        }

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

    private PointF GetPositionForTile(Point point, SizeF componentSize) => GetPositionForTile(point.X, point.Y, componentSize);

    private PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (TileSize - componentSize.Width) / 2;
        var marginY = (TileSize - componentSize.Height) / 2;
        return new((x * TileSize) + marginX, (y * TileSize) + marginY);
    }

    private void ShowMessage(string message)
    {
        _message.Position = new(_map.Bounds.Left, _map.Bounds.Top - 10f);
        _message.Play(message);
    }

    private void UnitPlaced(UnitPlacedEvent evnt)
    {
        var component = CreateBattleUnitComponent(evnt.Unit);
        _unitComponents[evnt.Unit] = component;
        AddChild(component);

        var point = _state.UnitCoordinates[component.State];
        component.Position = GetPositionForTile(point, component.Bounds.Size);
    }

    private void TileClicked(TileClickedEvent evnt)
    {
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
            _attackTargetHighlight.Range = 1;
            _attackTargetHighlight.Visible = true;
        }
        else if (_battleService.IsValidMagicTargetTile(evnt.Tile))
        {
            _attackTargetHighlight.Center = evnt.Tile;
            _attackTargetHighlight.Range = _state.CurrentSpell.Aoe;
            _attackTargetHighlight.Visible = true;
        }

        _placeUnitSprite.Visible = _state.Step == BattleStep.Setup;

        var origins = _state.CurrentPlaceOrder % 2 == 0
            ? _state.Map.Player1Origins
            : _state.Map.Player2Origins;

        _placeUnitSprite.Visible = _state.Step == BattleStep.Setup && origins.Contains(evnt.Tile);
        if (_placeUnitSprite.Visible)
        {
            _placeUnitSprite.Position = GetPositionForTile(evnt.Tile, _placeUnitSprite.Bounds.Size);
            _placeUnitSprite.Sprite = CreateBattleUnitComponent(_state.PlaceOrder[_state.CurrentPlaceOrder]).Sprite;
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

        _attackTargetHighlight.Range = 1;
    }

    private void UnitMoved(UnitMovedEvent evnt)
    {
        var finalTarget = GetPositionForTile(evnt.Tile, CurrentUnit.Bounds.Size);
        _unitTween = CurrentUnit.Position.SpeedTween(500f, finalTarget);
    }
}

public record BattleTileHoveredEvent(BattleUnitData? Unit) : IEvent;
