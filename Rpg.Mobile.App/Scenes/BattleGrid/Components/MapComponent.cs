using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; }
    public List<BattleUnitComponent> BattleUnits { get; } = new();

    public MapComponent(RectF bounds) : base(bounds)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1Sprite = spriteLoader.Load("ArcherIdle01.png");
        BattleUnits.Add(new(new(0, archer1Sprite)));

        Grid = AddChild(new GridComponent(10, 15));
        BattleUnits.ForEach(x => AddChild(x));
    }

    public override void Update(TimeSpan delta) =>
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.Fill(Bounds.Size);
    }

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        var touch = touches.First();
        var x = (int)(touch.X / Grid.Size);
        var y = (int)(touch.Y / Grid.Size);

        if (x < 0 || x > Grid.ColCount || y < 0 || y > Grid.RowCount)
            return;
        
        BattleUnits.First().MoveTo(x * Grid.Size, y * Grid.Size);
    }
}