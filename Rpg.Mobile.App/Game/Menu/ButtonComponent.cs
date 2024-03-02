using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Game.Menu;

public class ButtonComponent : ComponentBase
{
    private Action<IEnumerable<PointF>>[] _handlers;

    public string Label { get; set; } = "";
    public Color BackColor { get; set; } = Colors.LightBlue;
    public Color TextColor { get; set; } = Colors.Black;
    public Font Font { get; set; } = DefaultFont.ExtraBold;
    public float CornerRadius { get; set; } = 2f;
    public bool Visible { get; set; } = true;

    public ButtonComponent(RectF bounds, string label, params Action<IEnumerable<PointF>>[] handlers) : base(bounds)
    {
        Label = label;;
        _handlers = handlers;
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

    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        foreach (var handler in _handlers)
            handler(touches);
    }
}
