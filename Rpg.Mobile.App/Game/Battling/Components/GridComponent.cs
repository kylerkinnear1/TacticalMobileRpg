using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class GridComponent : ComponentBase
{
    public int Width { get; set; }
    public int Height { get; set; }
    public float Size { get; set; }

    private Point? _lastHoverGrid;

    public GridComponent(int width, int height, float size) : base(CalcBounds(PointF.Zero, width, height, size))
    {
        Width = width;
        Height = height;
        Size = size;
    }

    public override void Update(float deltaTime) => Bounds = CalcBounds(Bounds.Location, Width, Height, Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite.WithAlpha(.3f);

        for (var col = 0; col < Width + 1; col++)
        {
            var x = col * Size;
            canvas.DrawLine(x, 0, x, Bounds.Height);
        }

        for (var row = 0; row < Height + 1; row++)
        {
            var y = row * Size;
            canvas.DrawLine(0, y, Bounds.Width, y);
        }
    }

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        var touch = touches.First();
        var x = (int)(touch.X / Size);
        var y = (int)(touch.Y / Size);

        Bus.Global.Publish(new TileClickedEvent(new(x, y)));
    }

    public override void OnHover(PointF hover)
    {
        var tile = GetTileForPosition(hover);
        if (tile == _lastHoverGrid)
            return;

        _lastHoverGrid = tile;
        Bus.Global.Publish(new TileHoveredEvent(tile));
    }

    public Point GetTileForPosition(PointF point) => new((int)(point.X / Size), (int)(point.Y / Size));

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);
}

public record TileHoveredEvent(Point Tile) : IEvent;
public record TileClickedEvent(Point Tile) : IEvent;
