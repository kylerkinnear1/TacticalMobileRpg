using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IUpdateGameObject
{
    void Update(TimeSpan delta);
}

public interface IRenderGameObject
{
    RectF? Bounds { get; }

    void Render(ICanvas canvas, RectF dirtyRect);
}

public interface IGameObject : IUpdateGameObject, IRenderGameObject { }