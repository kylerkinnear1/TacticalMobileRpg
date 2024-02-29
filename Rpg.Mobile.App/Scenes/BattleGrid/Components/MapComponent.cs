using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MapComponent : ComponentBase
{
    public GridComponent Grid { get; } = new(10, 15);
    public BattleUnitComponent BattleUnit { get; }

    public MapComponent(RectF bounds) : base(bounds)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1 = spriteLoader.Load("ArcherIdle01.png");
        BattleUnit = new(archer1);

        AddChild(Grid);
        AddChild(BattleUnit);
    }

    public override void Update(TimeSpan delta) =>
        Bounds = new(Bounds.X, Bounds.Y, Grid.ColCount * Grid.Size, Grid.RowCount * Grid.Size);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(Bounds);
    }
}

public class BattleUnitComponent : ComponentBase
{
    public int PlayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public Point Position { get; set; } = new(0, 0);
    public int Movement { get; set; } = 4;
    public int RemainingHealth { get; set; } = 12;
    public int MaxHealth { get; set; } = 12;
    public IImage Sprite { get; set; }
    public int Attack { get; set; } = 8;
    public int AttackRange { get; set; } = 1;
    public int Defense { get; set; } = 5;
    public float Scale { get; set; } = 1f;
    public Font StatusFont { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);

    public BattleUnitComponent(IImage sprite, float scale = 1.0f) : base(new(0, 0, sprite.Width * scale, sprite.Height * scale))
    {
        Sprite = sprite;
        Scale = scale;
    }

    public override void Update(TimeSpan delta) => Bounds = new(Bounds.X, Bounds.Y, Sprite.Width * Scale, Sprite.Height * Scale);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!IsVisible)
            return;

        canvas.DrawImage(Sprite, 0, 0, Sprite.Width, Sprite.Height);

        canvas.Font = StatusFont;
        canvas.FontSize = 22f;
        canvas.FontColor = PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.FillColor = Colors.SlateGrey.WithAlpha(.65f);
        canvas.FillRoundedRectangle(-12f, 20f, 30f, 25f, 2f);
        canvas.DrawString($"{RemainingHealth}", 10f, 40f, HorizontalAlignment.Left);
    }
}
