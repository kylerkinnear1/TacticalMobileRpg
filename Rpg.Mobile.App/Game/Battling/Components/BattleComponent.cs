using Rpg.Mobile.App.Game.Battling.Systems;
using Rpg.Mobile.App.Game.Battling.Systems.Calculators;
using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleComponent : ComponentBase
{
    public static int TileSize => MapComponent.TileSize;

    public readonly MapComponent Map;
    public readonly TileShadowComponent MoveShadow;
    public readonly TileShadowComponent AttackShadow;
    public readonly TileShadowComponent CurrentUnitShadow;
    public readonly MultiDamageIndicatorComponent DamageIndicator;
    public readonly TextIndicatorComponent Message;
    public readonly TargetIndicatorComponent AttackTargetHighlight;
    public readonly TargetIndicatorComponent CurrentTileHighlight;
    public readonly Sprite PlaceUnitSprite;
    public readonly Dictionary<BattleUnitData, BattleUnitComponent> Units = new();
    public BattleUnitComponent? CurrentUnit => _state.CurrentUnit is not null
        ? Units[_state.CurrentUnit] : null;

    public ITween<PointF>? CurrentUnitTween;

    private readonly BattleData _state;
    private readonly BattleStateService _battleService;

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);

    public BattleComponent(PointF location, BattleData battle, BattleStateService battleService) 
        : base(CalcBounds(location, battle.Map.Width, battle.Map.Height, TileSize))
    {
        _battleService = battleService;
        _state = battle;

        var path = new PathCalculator();
        AddChild(Map = new(battle.Map));
        AddChild(MoveShadow = new(Map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(AttackShadow = new(Map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(CurrentUnitShadow = new(Map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });
        AddChild(AttackTargetHighlight = new(battle.Map, MapComponent.TileSize, Map.Bounds, path)
        {
            StrokeColor = Colors.Crimson.WithAlpha(.8f),
            StrokeWidth = 10f,
            Visible = false
        });
        AddChild(CurrentTileHighlight = new(battle.Map, MapComponent.TileSize, Map.Bounds, path)
        {
            StrokeColor = Colors.White.WithAlpha(.7f),
            Visible = false
        });
        AddChild(PlaceUnitSprite = new(Images.WarriorIdle01) { Visible = false });
        PlaceUnitSprite.UpdateScale(1.5f);
        DamageIndicator = new(Map.Bounds);
        AddChild(Message = new() 
        { 
            Bounds = new(Map.Bounds.Left, Map.Bounds.Height - 10f, Map.Bounds.Width, 200f)
        });
        Message.Position = new(Map.Bounds.Left, Map.Bounds.Top - 10f);
        
        Bus.Global.Subscribe<BattleStartedEvent>(_ => AddChild(DamageIndicator));
        Bus.Global.Subscribe<ActiveUnitChangedEvent>(ActiveUnitChanged);
        Bus.Global.Subscribe<NotEnoughMpEvent>(_ => ShowMessage("Not enough MP."));
    }

    public override void Update(float deltaTime)
    {
        var currentUnitPosition = _state.UnitCoordinates[CurrentUnit.State];
        CurrentUnitShadow.Shadows.SetSingle(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));
    }

    public override void Render(ICanvas canvas, RectF dirtyRect) { }
    
    public PointF GetPositionForTile(Point point, SizeF componentSize) => GetPositionForTile(point.X, point.Y, componentSize);

    public PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (TileSize - componentSize.Width) / 2;
        var marginY = (TileSize - componentSize.Height) / 2;
        return new((x * TileSize) + marginX, (y * TileSize) + marginY);
    }

    private void ShowMessage(string message)
    {
        Message.Position = new(Map.Bounds.Left, Map.Bounds.Top - 10f);
        Message.Play(message);
    }

    private void ActiveUnitChanged(ActiveUnitChangedEvent evnt)
    {
        CurrentUnitTween = null;
        if (evnt.PreviousUnit is null)
            return;

        var previousComponent = Units[evnt.PreviousUnit];
        var coordinate = _state.UnitCoordinates[evnt.PreviousUnit];
        previousComponent.Position = GetPositionForTile(coordinate, previousComponent.Bounds.Size);

        AttackTargetHighlight.Range = 1;
    }
}

public record BattleTileHoveredEvent(BattleUnitData? Unit) : IEvent;
