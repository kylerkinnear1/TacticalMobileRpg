using Rpg.Mobile.Api;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class TargetIndicatorComponent : ComponentBase
{
    public int Range { get; set; } = 1;
    public Point Center { get; set; } = Point.Empty;
    public int TileSize { get; set; }
    public float StrokeWidth { get; set; } = 2f;
    public Color StrokeColor { get; set; } = Colors.White;

    private readonly List<RectF> _tiles = new();
    private readonly MapData _map;

    public TargetIndicatorComponent(MapData map, int tileSize, RectF bounds) : base(bounds)
    {
        _map = map;
        TileSize = tileSize;
    }

    public override void Update(float deltaTime)
    {
        _tiles.Set(CalculateTiles());
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = StrokeColor;
        canvas.StrokeSize = StrokeWidth;
        foreach (var tile in _tiles)
        {
            canvas.DrawRectangle(tile);
        }
    }

    private IEnumerable<RectF> CalculateTiles() =>
        _path
            .CreateFanOutArea(Center, _map.Corner(), Range - 1)
            .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
}
