using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.Common;

public class TextboxComponent : ComponentBase
{
    public string Label { get; set; } = "";
    public Color BackColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.White;
    public Font Font { get; set; } = DefaultFont.ExtraBold;
    public float CornerRadius { get; set; } = 2f;
    public float FontSize = 16f;

    public TextboxComponent(RectF bounds, string label = "") : base(bounds)
    {
        Label = label;
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = BackColor;
        canvas.FontColor = TextColor;
        canvas.Font = Font;
        canvas.FontSize = FontSize;

        canvas.FillRoundedRectangle(0f, 0f, Bounds.Width, Bounds.Height, CornerRadius);
        canvas.DrawString(Label, 0f, 0f, Bounds.Width, Bounds.Height, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
