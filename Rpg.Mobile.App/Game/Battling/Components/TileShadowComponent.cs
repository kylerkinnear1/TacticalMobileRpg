using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class TileShadowComponent : ComponentBase
{
    public List<RectF> Shadows { get; } = new();
    public Color BackColor { get; set; } = Colors.SlateGrey.WithAlpha(.3f);

    public TileShadowComponent(RectF bounds) : base(bounds)
    {
    }

    public override void Update(float deltaTime)
    {
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = BackColor;
        foreach (var shadow in Shadows)
            canvas.FillRectangle(shadow);
    }
}
