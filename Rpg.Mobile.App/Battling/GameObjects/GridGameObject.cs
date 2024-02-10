using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public record GridState(PointF Position, int RowCount, int ColumnCount, float Size);

public class GridGameObject : IGameObject
{
    private readonly GridState _state;

    public GridGameObject(GridState state) => _state = state;

    public void Update(TimeSpan delta) { }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        var right = _state.Position.X + (_state.Size * _state.RowCount);
        var bottom = _state.Position.Y + (_state.Size * _state.ColumnCount);

        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite;

        for (int row = 0; row < _state.RowCount; row++)
        {
            var y = row * _state.Size + _state.Position.X;
            canvas.DrawLine(0f, y, right, y);
        }

        for (int col = 0; col < _state.ColumnCount; col++)
        {
            var x = col * _state.Size + _state.Position.Y;
            canvas.DrawLine(x, 0f, x, bottom);
        }

        // TODO: dirty rect
    }
}
