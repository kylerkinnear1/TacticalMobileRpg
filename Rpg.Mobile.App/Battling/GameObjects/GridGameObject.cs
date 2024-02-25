using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public record GridState(PointF Position, int RowCount, int ColumnCount, float Size)
{
    public RectF Bounds { get; init; } = new(Position.X, Position.Y, ColumnCount * Size, RowCount * Size);
}

public class GridGameObject : ComponentBase
{
    private readonly GridState _state;

    public GridGameObject(GridState state) : base(CalculateBounds(state)) => _state = state;

    public override void Update(TimeSpan delta) => Bounds = CalculateBounds(_state);

    private static RectF CalculateBounds(GridState state) => 
        new(state.Position.X, state.Position.Y, state.Size * state.ColumnCount, state.Size * state.RowCount);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite.WithAlpha(.3f);

        for (int row = 0; row < _state.ColumnCount + 1; row++)
        {
            var y = row * _state.Size + _state.Position.X;
            canvas.DrawLine(_state.Position.X, y, Bounds.Right, y);
        }

        for (int col = 0; col < _state.RowCount + 1; col++)
        {
            var x = col * _state.Size + _state.Position.Y;
            canvas.DrawLine(x, _state.Position.Y, x, Bounds.Bottom);
        }
    }
}
