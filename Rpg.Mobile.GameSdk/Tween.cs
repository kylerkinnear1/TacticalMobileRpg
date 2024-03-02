using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.GameSdk;

// TODO: change to iterator!
public interface ITween<T>
{
    bool IsComplete { get; }

    T Advance(float deltaTime);
}

public interface IMoveStartTween<T> : ITween<T>
{
    T Start { get; set; }
}

public class MultiTween : ITween<PointF>
{
    private int _index = 0;
    public List<IMoveStartTween<PointF>> Steps { get; } = new();
    public IMoveStartTween<PointF> CurrentTween => IsComplete ? Steps.Last() : Steps[_index];
    public PointF? Last { get; private set; }

    public MultiTween(IMoveStartTween<PointF> initial) => Steps.Add(initial);
    public MultiTween(IEnumerable<IMoveStartTween<PointF>> steps) => Steps.AddRange(steps);

    public bool IsComplete => _index >= Steps.Count;

    public PointF GetNext(float deltaTime)
    {
        if (IsComplete)
            return Last ?? PointF.Zero;

        var lastTween = CurrentTween;
        if (!lastTween.IsComplete)
        {
            return lastTween.Advance(deltaTime);
        }

        _index++;
        CurrentTween.Start = Last ?? PointF.Zero;
        return CurrentTween.Advance(deltaTime);

    }

    public PointF Advance(float deltaTime)
    {
        Last = GetNext(deltaTime);
        return Last.Value;
    }
}

public class SpeedTween : ITween<PointF>, IMoveStartTween<PointF>
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

public static class SpeedTweenExtensions
{
    public static MultiTween TweenTo(this PointF start, float speed, params PointF[] path)
    {
        var startTween = start.TweenTo(path[0], speed);
        var multi = new MultiTween(startTween);
        for (int i = 1; i < path.Length; i++)
        {
            multi = multi.TweenTo(path[i], speed);
        }

        return multi;
    }

    public static SpeedTween TweenTo(this PointF end, float speed) => new(PointF.Zero, end, speed);

    public static SpeedTween TweenTo(this PointF start, PointF end, float speed) => new(start, end, speed);

    public static MultiTween TweenTo(this SpeedTween speedTween, PointF end, float speed)
    {
        var multi = new MultiTween(speedTween);
        multi.TweenTo(end, speed);
        return multi;
    }

    public static MultiTween TweenTo(this MultiTween multi, PointF end, float speed)
    {
        multi.Steps.Add(new SpeedTween(end, speed));
        return multi;
    }
}
