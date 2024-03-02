using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MiniMapComponent : ComponentBase
{
    private readonly Action<PointF> _onTouchUp;

    public MiniMapComponent(Action<PointF> onTouchUp, RectF bounds) : base(bounds)
    {
        _onTouchUp = onTouchUp;
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(0, 0, Bounds.Width, Bounds.Height);
    }
    
    public override void OnTouchUp(IEnumerable<PointF> touches) => _onTouchUp(touches.First());
}
