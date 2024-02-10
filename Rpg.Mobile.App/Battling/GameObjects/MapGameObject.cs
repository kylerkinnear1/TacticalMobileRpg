using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class MapGameObject : IGameObject
{
    public void Update(TimeSpan delta)
    {
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(dirtyRect);
    }
}

public class ButtonState
{
    public string Text { get; set; }
    public float FontSize { get; set; }
    public RectF Bounds { get; set; }
    public IFont Font { get; set; }

    public ButtonState(string text, RectF bounds, IFont? font = null, float fontSize = 16f)
    {
        Text = text;
        Bounds = bounds;
        Font = font ?? new Font("Arial");
        FontSize = fontSize;
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
        canvas.Font = _state.Font;
        canvas.FontSize = _state.FontSize;
        canvas.FillColor = Colors.SlateGrey;
        canvas.FillRectangle(_state.Bounds);
        canvas.DrawString(_state.Text, _state.Bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
