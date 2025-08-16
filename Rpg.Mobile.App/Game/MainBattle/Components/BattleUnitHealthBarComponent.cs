using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class BattleUnitHealthBarComponent() : ComponentBase(new(0f, 0f, 30f, 25f))
{
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);
    public bool HasGone { get; set; } = false;
    public int PlayerId { get; set; } = 0;
    public int RemainingHealth { get; set; } = 0;

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Font = Font;
        canvas.FontSize = HasGone ? 19f : 22f;

        canvas.FillColor = HasGone ? Colors.SlateGray.WithAlpha(.8f) : Colors.Black.WithAlpha(.4f);
        canvas.FontColor = PlayerId switch
        {
            0 when !HasGone => Colors.Aqua,
            1 when !HasGone => Colors.Orange,
            0 when HasGone => Colors.DarkBlue,
            1 when HasGone => Colors.Brown,
            _ => throw new ArgumentException()
        };

        canvas.StrokeColor = PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.StrokeSize = 1f;

        var bounds = new RectF(PointF.Zero, Bounds.Size);
        canvas.FillRoundedRectangle(bounds, 2f);
        canvas.DrawString($"{RemainingHealth}", bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}