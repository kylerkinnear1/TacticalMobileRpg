using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public record TouchEvent(PointF[] Touches);

public interface IHandleTouchUp
{
    RectF? Bounds { get; }
    void OnTouchUp(TouchEvent touch);
}
