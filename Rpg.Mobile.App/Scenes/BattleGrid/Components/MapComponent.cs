using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MapState
{
    public GridState Grid { get; set; } = new();
}

public class MapComponent : ComponentBase
{
    public MapState State { get; }

    public MapComponent(RectF bounds, MapState state) : base(bounds)
    {
        State = state;
        AddChild(new GridComponent(state.Grid));
    }

    public override void Update(TimeSpan delta) {}

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(Bounds);
    }
}