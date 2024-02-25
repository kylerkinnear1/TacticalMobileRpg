using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class ButtonState
{
    public string Text { get; set; }
    public float FontSize { get; set; } = 16f;
    public RectF Bounds { get; set; }
    public IFont Font { get; set; }
    public Color FontColor { get; set; } = Colors.Orange;
    public Color BackgroundColor { get; set; } = Colors.SlateGray;
    public bool IsVisible { get; set; } = true;

    public ButtonState(string text, RectF bounds, IFont? font = null)
    {
        Text = text;
        Bounds = bounds;
        Font = font ?? new Font("Arial");
    }
}

public class ButtonGameObject : ComponentBase
{
    private readonly ButtonState _state;

    public ButtonGameObject(ButtonState state) : base(state.Bounds) => _state = state;

    public override void Update(TimeSpan delta) => Bounds = _state.Bounds;

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!_state.IsVisible)
            return;

        canvas.Font = _state.Font;
        canvas.FontSize = _state.FontSize;
        canvas.FillColor = _state.BackgroundColor;
        canvas.FontColor = _state.FontColor;
        canvas.FillRectangle(_state.Bounds);
        canvas.DrawString(_state.Text, _state.Bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
