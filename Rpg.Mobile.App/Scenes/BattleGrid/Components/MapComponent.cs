using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; } = new(10, 15);

    public MapComponent(RectF bounds) : base(bounds)
    {
        AddChild(Grid);
    }

    public override void Update(TimeSpan delta) =>
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(Bounds);
    }
}