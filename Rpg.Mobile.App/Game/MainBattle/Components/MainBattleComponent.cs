using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Calculators;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Tweening;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class MainBattleComponent : ComponentBase
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

    public BattleUnitComponent CurrentUnit => Units[_data.CurrentUnit];

    public ITween<PointF>? CurrentUnitTween;

    private readonly BattleData _data;

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);

    public MainBattleComponent(PointF location, BattleData data)
        : base(CalcBounds(location, data.Map.Width, data.Map.Height, TileSize))
    {
        _data = data;

        var path = new PathCalculator();
        AddChild(Map = new(data.Map));
        AddChild(MoveShadow = new(Map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(AttackShadow = new(Map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(CurrentUnitShadow = new(Map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });
        AddChild(AttackTargetHighlight = new(data.Map, MapComponent.TileSize, Map.Bounds, path)
        {
            StrokeColor = Colors.Crimson.WithAlpha(.8f),
            StrokeWidth = 10f,
            Visible = false
        });
        AddChild(CurrentTileHighlight = new(data.Map, MapComponent.TileSize, Map.Bounds, path)
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
    }

    public override void Update(float deltaTime)
    {
        if (_data.ActiveUnitIndex <= 0)
            return;

        var currentUnitPosition = _data.UnitCoordinates[CurrentUnit.State];
        CurrentUnitShadow.Shadows.SetSingle(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));
    }

    public override void Render(ICanvas canvas, RectF dirtyRect) { }

    public PointF GetPositionForTile(Point point, SizeF componentSize) =>
        GetPositionForTile(point.X, point.Y, componentSize);

    public PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (TileSize - componentSize.Width) / 2;
        var marginY = (TileSize - componentSize.Height) / 2;
        return new(x * TileSize + marginX, y * TileSize + marginY);
    }
}
