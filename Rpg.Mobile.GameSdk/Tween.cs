using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.GameSdk;

public interface ITween<T>
{
    bool IsComplete { get; }
    T Advance();
}

public class SpeedTween : ITween<PointF>
{
    public PointF Start { get; set; }
    public PointF End { get; set; }
    public float Speed { get; set; }
    public PointF? Last { get; private set; }
    public bool IsComplete => Last.HasValue && End.CloseTo(Last.Value);

    public SpeedTween(PointF end, float speed) : this(PointF.Zero, end, speed) { }

    public SpeedTween(PointF start, PointF end, float speed)
    {
        Start = start;
        End = end;
        Speed = speed;
    }

    public PointF Advance()
    {
        if (IsComplete)
            return End;

        Last ??= Start;
        var normal = Last.Value.NormalTo(End);
        var scaled = normal
            .Scale(Speed)
            .Add(Last.Value);

        var movingLeft = normal.X < 0;
        var movingUp = normal.Y < 0;
        var x = movingLeft ? Math.Max(scaled.X, End.X) : Math.Min(scaled.X, End.X);
        var y = movingUp ? Math.Max(scaled.Y, End.Y) : Math.Min(scaled.Y, End.Y);
        Last = new(x, y);
        return Last.Value;
    }
}

public static class SpeedTweenExtensions
{
    public static SpeedTween TweenTo(this PointF start, PointF end, float speed) => new(start, end, speed);
}
