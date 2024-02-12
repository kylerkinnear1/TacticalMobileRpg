using Rpg.Mobile.GameSdk;

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
