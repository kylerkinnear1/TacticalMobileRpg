using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Tweening;

public static class TweenExtensions
{
    public static MultiTween SpeedTween(this PointF start, float speed, params PointF[] path)
    {
        var startTween = start.SpeedTween(path[0], speed);
        var multi = new MultiTween(startTween);
        for (int i = 1; i < path.Length; i++)
        {
            multi = multi.SpeedTween(path[i], speed);
        }

        return multi;
    }
    
    public static SpeedTween SpeedTween(this PointF start, PointF end, float speed) => new(start, end, speed);

    public static MultiTween SpeedTween(this SpeedTween speedTween, PointF end, float speed)
    {
        var multi = new MultiTween(speedTween);
        multi.SpeedTween(end, speed);
        return multi;
    }

    public static MultiTween SpeedTween(this MultiTween multi, PointF end, float speed)
    {
        multi.Steps.Add(new SpeedTween(end, speed));
        return multi;
    }
}