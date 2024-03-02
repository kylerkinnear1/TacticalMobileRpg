using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; }
    public List<BattleUnitComponent> BattleUnits { get; }

    public MapComponent(RectF bounds) : base(bounds)
    {
        Grid = AddChild(new GridComponent(10, 15));

        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1Sprite = spriteLoader.Load("ArcherIdle01.png");

        BattleUnits = new()
        {
            AddChild(new BattleUnitComponent(archer1Sprite, new(0)))
        };

        MoveToTile(BattleUnits[0], 6, 8);
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
        
        MoveToTile(BattleUnits.First(), x, y);
    }

    private void MoveToTile(BattleUnitComponent unit, int x, int y)
    {
        unit.MoveTo(x * Grid.Size, y * Grid.Size - 5f);
    }
}