using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Game.Menu;

public class TextboxComponent : ComponentBase
{
    public string Label { get; set; } = "";
    public Color BackColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.White;
    public Font Font { get; set; } = DefaultFont.ExtraBold;
    public float CornerRadius { get; set; } = 2f;

    public TextboxComponent(RectF bounds, string label) : base(bounds)
    {
        Label = label;
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!Visible)
            return;

        canvas.FillColor = BackColor;
        canvas.FontColor = TextColor;
        canvas.Font = Font;

        canvas.FillRoundedRectangle(0f, 0f, Bounds.Width, Bounds.Height, CornerRadius);
        canvas.DrawString(Label, 0f, 0f, Bounds.Width, Bounds.Height, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
