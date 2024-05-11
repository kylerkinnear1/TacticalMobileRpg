using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.GameSdk.Tweening;

public class SpeedTween : IMoveStartTween<PointF>
{
    public PointF Start { get; set; }
    public PointF End { get; set; }
    public float Speed { get; set; }
    public PointF? Last { get; private set; }

    public bool IsComplete => Last.HasValue && End.CloseTo(Last.Value);

    public PointF GetNext(float deltaTime)
    {
        if (IsComplete)
            return End;

        Last ??= Start;
        var normal = Last.Value.NormalTo(End);
        var scaled = normal
            .Scale(Speed * deltaTime)
            .Add(Last.Value);

        var movingLeft = normal.X < 0;
        var movingUp = normal.Y < 0;
        var x = movingLeft ? Math.Max(scaled.X, End.X) : Math.Min(scaled.X, End.X);
        var y = movingUp ? Math.Max(scaled.Y, End.Y) : Math.Min(scaled.Y, End.Y);
        return new(x, y);
    }

    public SpeedTween(PointF end, float speed) : this(PointF.Zero, end, speed) { }

    public SpeedTween(PointF start, PointF end, float speed)
    {
        Start = start;
        End = end;
        Speed = speed;
    }

    public PointF Advance(float deltaTime)
    {
        Last = GetNext(deltaTime);
        return Last.Value;
    }
}

// TODO: Think about functions to make some tween stuff more reusable
// to get around type limitations (maybe make a PointF advance, Point advance, etc..)