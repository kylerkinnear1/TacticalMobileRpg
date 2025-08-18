using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Images;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class MiniMapComponent : ComponentBase
{
    public MiniMapComponent(RectF bounds) : base(bounds)
    {
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(0, 0, Bounds.Width, Bounds.Height);
        canvas.FontSize = 26f;
        canvas.DrawCenteredText("Test the Camera", new(0f, 0f, Bounds.Width, Bounds.Height));
    }
    
    public override void OnTouchUp(IEnumerable<PointF> touches) => Bus.Global.Publish(new MiniMapClickedEvent(touches.First()));
}
