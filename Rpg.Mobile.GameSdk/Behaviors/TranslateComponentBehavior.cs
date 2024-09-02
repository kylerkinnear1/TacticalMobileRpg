using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Tweening;

namespace Rpg.Mobile.GameSdk.Behaviors;

public class TranslateComponentBehavior(
    ITween<PointF> _tween, 
    ComponentBase _component) : IBehavior
{
    public void Update(float deltaTime)
    {
        _component.Position = _tween.Advance(deltaTime);
        if (_tween.IsComplete)
            _component.RemovePreBehavior(this);
    }
}

public static class TranslateComponentExtensions
{
    public static void TranslateBySpeed(this ComponentBase component, PointF target, float speed)
    {
        var tween = component.Position.SpeedTween(target, speed);
        var behavior = new TranslateComponentBehavior(tween, component);
        component.AddPreBehavior(behavior);
    }
}

