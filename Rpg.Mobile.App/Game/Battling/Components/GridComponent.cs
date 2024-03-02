using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class GridComponent : ComponentBase
{
    public int ColCount { get; set; }
    public int RowCount { get; set; }
    public float Size { get; set; }

    public GridComponent(int colCount, int rowCount, float size = 30f) : base(CalcBounds(PointF.Zero, colCount, rowCount, size))
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

    private static RectF CalcBounds(PointF position, int colCount, int rowCount, float size) => 
        new(position.X, position.Y, colCount * size, rowCount * size);
}
