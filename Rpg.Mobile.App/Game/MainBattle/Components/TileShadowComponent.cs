using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class TileShadowComponent : ComponentBase
{
    public List<RectF> Shadows { get; } = new();
    public Color BackColor { get; set; } = Colors.SlateGrey.WithAlpha(.3f);
    public Color BorderColor { get; set; } = Colors.Transparent;
    public float BorderSize { get; set; } = 0f;

    public TileShadowComponent(RectF bounds) : base(bounds)
    {
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = BorderColor;
        canvas.StrokeSize = BorderSize;

        foreach (var shadow in Shadows)
        {
            canvas.FillColor = BackColor;
            canvas.FillRectangle(shadow);
            canvas.FillColor = Colors.Transparent;
            canvas.DrawRectangle(shadow);
        }
    }
}
