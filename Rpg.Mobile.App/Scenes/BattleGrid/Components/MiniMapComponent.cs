using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MiniMapComponent : ComponentBase
{
    private readonly Camera _camera;

    public MiniMapComponent(Camera camera, RectF bounds) : base(bounds)
    {
        _camera = camera;
        IgnoreCamera = true;
    }

    public override void Update(TimeSpan delta) { }

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
        _camera.Offset = new((_camera.Size.Width * xPercent), (_camera.Size.Height * yPercent));
    }
}
