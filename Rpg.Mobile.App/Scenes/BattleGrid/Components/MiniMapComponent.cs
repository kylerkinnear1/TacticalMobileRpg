using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class MiniMapComponent : ComponentBase
{
    private readonly Camera _camera;

    public MiniMapComponent(Camera camera, RectF bounds) : base(bounds)
    {
        _camera = camera;
    }

    public override void Update(TimeSpan delta) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.ForestGreen;
        canvas.FillRectangle(0, 0, Bounds.Width, Bounds.Height);
    }
    
    public override void OnTouchUp(TouchEventArgs touch)
    {
        _camera.Offset = touch.Touches.First();
    }
}
