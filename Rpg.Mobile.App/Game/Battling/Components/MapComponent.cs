using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; }

    public MapComponent(Action<int, int> onGridClick, RectF bounds) : base(bounds)
    {
        Grid = AddChild(new GridComponent(onGridClick, 10, 15));
    }

    public override void Update(float deltaTime)
    {
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.Fill(Bounds.Size);
    }
}