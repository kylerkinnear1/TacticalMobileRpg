using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; } = new(10, 15);
    public List<BattleUnitComponent> BattleUnits { get; } = new();

    public MapComponent(RectF bounds) : base(bounds)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1Sprite = spriteLoader.Load("ArcherIdle01.png");
        BattleUnits.Add(new(new(0, archer1Sprite)));

        AddChild(Grid);
        BattleUnits.ForEach(x => AddChild(x));
    }

    public override void Update(TimeSpan delta) =>
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(Bounds);
    }
}