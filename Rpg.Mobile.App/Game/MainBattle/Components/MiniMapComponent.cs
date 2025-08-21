using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Images;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class MiniMapComponent : ComponentBase
{
    public record MiniMapClickedEvent(PointF Position) : IEvent;

    private readonly IEventBus _bus;
    
    public MiniMapComponent(IEventBus bus, RectF bounds) : base(bounds)
    {
        _bus = bus;
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(0, 0, Bounds.Width, Bounds.Height);
        canvas.FontSize = 26f;
        canvas.DrawCenteredText("Test the Camera", new(0f, 0f, Bounds.Width, Bounds.Height));
    }
    
    public override void OnTouchUp(IEnumerable<PointF> touches) => 
        _bus.Publish(new MiniMapClickedEvent(touches.First()));
}
