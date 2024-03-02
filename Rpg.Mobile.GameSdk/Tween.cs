using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.GameSdk;

public interface ITween<T>
{
    T Advance();
}

public class SpeedTween : ITween<PointF>
{
    public PointF Start { get; set; }
    public PointF End { get; set; }
    public float Speed { get; set; }
    public PointF? Last { get; private set; }
    public bool Completed { get; private set; }

    public SpeedTween(PointF end, float speed) : this(PointF.Zero, end, speed) { }

    public SpeedTween(PointF start, PointF end, float speed)
    {
        Start = start;
        End = end;
        Speed = speed;
    }

    public PointF Advance()
    {
        if (Completed)
            return End;

        Last ??= Start;
        var normal = Last.Value.NormalTo(End);
        var scaled = normal.Scale(Speed);

        var x = End.X >= 0 ? Math.Min(scaled.X - Last.Value.X, End.X) : Math.Max(scaled.X - Last.Value.X, End.X);
        var y = End.Y >= 0 ? Math.Min(scaled.Y - Last.Value.Y, End.Y) : Math.Max(scaled.Y - Last.Value.Y, End.Y);

        Completed = x >= End.X & y >= End.Y;
        Last = new(x, y);
        return Last.Value;
    }
}

public static class SpeedTweenExtensions
{
    public static SpeedTween TweenTo(this PointF start, PointF end, float speed) => new(start, end, speed);
}
