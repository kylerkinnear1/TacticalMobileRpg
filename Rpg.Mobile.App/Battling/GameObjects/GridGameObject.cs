using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public record GridState(PointF Position, int RowCount, int ColumnCount, float Size)
{
    public RectF Bounds { get; init; } = new(Position.X, Position.Y, ColumnCount * Size, RowCount * Size);
}

public class GridGameObject : IGameObject, IHandleTouchUp
{
    private readonly GridState _state;

    public GridGameObject(GridState state) => _state = state;

    public void Update(TimeSpan delta) { }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        var right = _state.Position.X + (_state.Size * _state.RowCount);
        var bottom = _state.Position.Y + (_state.Size * _state.ColumnCount);

        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite.WithAlpha(.3f);

        for (int row = 0; row < _state.ColumnCount + 1; row++)
        {
            var y = row * _state.Size + _state.Position.X;
            canvas.DrawLine(_state.Position.X, y, right, y);
        }

        for (int col = 0; col < _state.RowCount + 1; col++)
        {
            var x = col * _state.Size + _state.Position.Y;
            canvas.DrawLine(x, _state.Position.Y, x, bottom);
        }

        // TODO: dirty rect
    }

    public void OnTouchUp(TouchEvent touches)
    {
        throw new NotImplementedException();
    }
}
