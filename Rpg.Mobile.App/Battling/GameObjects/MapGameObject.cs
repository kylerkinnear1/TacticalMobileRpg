using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class MapGameObject : ComponentBase
{
    private readonly ICamera _camera;

    public MapGameObject(ICamera camera) : base(CalculateBounds(camera)) => _camera = camera;

    public override void Update(TimeSpan delta)
    {
        Bounds = CalculateBounds(_camera);
    }

    private static RectF CalculateBounds(ICamera camera) => new(camera.FocalPoint, new(camera.Width, camera.Height));

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(dirtyRect);
    }
}
