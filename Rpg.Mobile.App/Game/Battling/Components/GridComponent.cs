using Rpg.Mobile.GameSdk;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class GridComponent : ComponentBase
{
    public int ColCount { get; set; }
    public int RowCount { get; set; }
    public float Size { get; set; }

    private Point? _lastHoverGrid;

    public GridComponent(int colCount, int rowCount, float size = 64f) : base(CalcBounds(PointF.Zero, colCount, rowCount, size))
    {
        ColCount = colCount;
        RowCount = rowCount;
        Size = size;
    }

    public override void Update(float deltaTime) => Bounds = CalcBounds(Bounds.Location, ColCount, RowCount, Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite.WithAlpha(.3f);

        for (var row = 0; row < RowCount + 1; row++)
        {
            var y = row * Size;
            canvas.DrawLine(0, y, Bounds.Width, y);
        }

        for (var col = 0; col < ColCount + 1; col++)
        {
            var x = col * Size;
            canvas.DrawLine(x, 0, x, Bounds.Height);
        }
    }

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        var touch = touches.First();
        var x = (int)(touch.X / Size);
        var y = (int)(touch.Y / Size);

        Bus?.Publish(new TileClickedEvent(new(x, y)));
    }

    public override void OnHover(PointF hover)
    {
        var tile = GetTileForPosition(hover);
        if (tile == _lastHoverGrid)
            return;

        _lastHoverGrid = tile;
        Bus?.Publish(new TileHoveredEvent(tile));
    }

    public Point GetTileForPosition(PointF point) => new((int)(point.X / Size), (int)(point.Y / Size));

    public PointF GetPositionForTile(Point point, SizeF componentSize) => GetPositionForTile(point.X, point.Y, componentSize);

    public PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (Size - componentSize.Width) / 2;
        var marginY = (Size - componentSize.Height) / 2;
        return new((x * Size) + marginX, (y * Size) + marginY);
    }

    private static RectF CalcBounds(PointF position, int colCount, int rowCount, float size) =>
        new(position.X, position.Y, colCount * size, rowCount * size);
}

public record TileHoveredEvent(Point Tile) : IEvent;
public record TileClickedEvent(Point Tile) : IEvent;
