using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IScene
{
    void Update(TimeSpan delta);
    void Render(ICanvas canvas, RectF dirtyRect);
}