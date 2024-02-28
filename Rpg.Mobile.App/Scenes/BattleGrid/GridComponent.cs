using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid;

public class GridState
{
    public PointF Position { get; set; } = PointF.Zero;
    public int ColCount { get; set; } = 1;
    public int RowCount { get; set; } = 1;
    public float Size { get; set; } = 30f;

    public RectF Bounds => new(Position, new(ColCount * Size, RowCount * Size));
}

public class GridComponent : ComponentBase
{
    public GridState State { get; }

    public GridComponent(GridState state) : base(state.Bounds) => State = state;

    public override void Update(TimeSpan delta) => Bounds = State.Bounds;

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite.WithAlpha(.3f);

        for (var row = 0; row < State.RowCount + 1; row++)
        {
            var y = row * State.Size;
            canvas.DrawLine(0, y, Bounds.Width, y);
        }

        for (var col = 0; col < State.ColCount + 1; col++)
        {
            var x = col * State.Size;
            canvas.DrawLine(x, 0, x, Bounds.Height);
        }
    }
}
