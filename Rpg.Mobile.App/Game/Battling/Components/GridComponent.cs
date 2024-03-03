using Rpg.Mobile.GameSdk;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class GridComponent : ComponentBase
{
    private readonly Action<int, int> _gridClickedHandler;

    public int ColCount { get; set; }
    public int RowCount { get; set; }
    public float Size { get; set; }

    public GridComponent(Action<int, int> gridClickedHandler, int colCount, int rowCount, float size = 30f) : base(CalcBounds(PointF.Zero, colCount, rowCount, size))
    {
        _gridClickedHandler = gridClickedHandler;
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

    private static RectF CalcBounds(PointF position, int colCount, int rowCount, float size) => 
        new(position.X, position.Y, colCount * size, rowCount * size);

    public Point GetTileForPosition(PointF point) => new((int)(point.X / Size), (int)(point.Y / Size));
    public PointF GetPositionForTile(int x, int y) => new(x * Size, y * Size);

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        var touch = touches.First();
        var x = (int)(touch.X / Size);
        var y = (int)(touch.Y / Size);
        _gridClickedHandler(x, y);
    }
}
