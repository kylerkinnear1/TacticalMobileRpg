using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.GameSdk.Inputs;

public record TouchEvent(PointF[] Touches);

public interface IHandleTouchUp : IHaveBounds
{
    void OnTouchUp(TouchEvent touch);
}
