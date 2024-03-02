using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; }
    public List<BattleUnitComponent> BattleUnits { get; }

    private SpeedTween? _movingUnit;

    public MapComponent(RectF bounds) : base(bounds)
    {
        Grid = AddChild(new GridComponent(10, 15));

        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1Sprite = spriteLoader.Load("ArcherIdle01.png");

        BattleUnits = new()
        {
            AddChild(new BattleUnitComponent(archer1Sprite, new(0)))
        };

        BattleUnits[0].MoveTo(GetPositionForTile(6, 8));
    }

    public override void Update(TimeSpan delta)
    {
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);
        if (_movingUnit?.Completed ?? true)
            return;

        var next = _movingUnit.Advance();
        BattleUnits[0].MoveTo(next);
    }

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

        
        var target = GetPositionForTile(x, y);
        var unit = BattleUnits[0];
        _movingUnit = unit.Position.TweenTo(target, 5f);
    }

    public PointF GetPositionForTile(int x, int y) => new(x * Grid.Size, y * Grid.Size);
}