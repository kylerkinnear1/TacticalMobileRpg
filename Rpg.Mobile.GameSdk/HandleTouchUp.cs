using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public record TouchEvent(PointF[] Touches);

public interface IHandleTouchUp : IHaveBounds
{
    void OnTouchUp(TouchEvent touch);
}
