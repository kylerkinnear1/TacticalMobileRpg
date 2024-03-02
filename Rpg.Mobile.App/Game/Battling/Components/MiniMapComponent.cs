using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MiniMapComponent : ComponentBase
{
    private readonly Camera _camera;
    private SpeedTween? _cameraMove;

    public MiniMapComponent(Camera camera, RectF bounds) : base(bounds) => _camera = camera;

    public override void Update(float deltaTime)
    {
        if (_cameraMove is null)
            return;

        _camera.Offset = _cameraMove.Advance(deltaTime);
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(0, 0, Bounds.Width, Bounds.Height);
    }
    
    public override void OnTouchUp(IEnumerable<PointF> touches)
    {
        var touchPoint = touches.First();
        var xPercent = touchPoint.X / Bounds.Width;
        var yPercent = touchPoint.Y / Bounds.Height;
        var target = new PointF(_camera.Size.Width * xPercent, _camera.Size.Height * yPercent);
        _cameraMove = _camera.Offset.TweenTo(target, 100f);
    }
}
