using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public record GridState(PointF Position, int RowCount, int ColumnCount, float Size);

public class GridGameObject : IGameObject<GridState>
{
    public void Update(GridState state) { }

    public void Render(GridState state, ICanvas canvas, RectF dirtyRect)
    {
        var right = state.Position.X + (state.Size * state.RowCount);
        var bottom = state.Position.Y + (state.Size * state.ColumnCount);

        canvas.StrokeSize = 2;
        canvas.StrokeColor = Colors.GhostWhite;

        for (int row = 0; row < state.RowCount; row++)
        {
            var y = row * state.Size + state.Position.X;
            canvas.DrawLine(0f, y, right, y);
        }

        for (int col = 0; col < state.ColumnCount; col++)
        {
            var x = col * state.Size + state.Position.Y;
            canvas.DrawLine(x, 0f, x, bottom);
        }

        // TODO: dirty rect
    }
}
