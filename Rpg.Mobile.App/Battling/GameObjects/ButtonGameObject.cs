using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class ButtonState
{
    public string Text { get; set; }
    public float FontSize { get; set; } = 16f;
    public RectF Bounds { get; set; }
    public IFont Font { get; set; }
    public Color BackgroundColor { get; set; } = Colors.SlateGray;
    public bool IsVisible { get; set; } = true;
    public Action Handler { get; set; }
    
    public ButtonState(string text, RectF bounds, Action handler, IFont? font = null)
    {
        Text = text;
        Bounds = bounds;
        Font = font ?? new Font("Arial");
        Handler = handler;
    }
}

public class ButtonGameObject : IGameObject
{
    private readonly ButtonState _state;

    public ButtonGameObject(ButtonState state) => _state = state;

    public void Update(TimeSpan delta)
    {
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!_state.IsVisible)
            return;

        canvas.Font = _state.Font;
        canvas.FontSize = _state.FontSize;
        canvas.FillColor = _state.BackgroundColor;
        canvas.FillRectangle(_state.Bounds);
        canvas.DrawString(_state.Text, _state.Bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
