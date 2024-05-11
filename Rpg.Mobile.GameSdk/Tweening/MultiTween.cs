using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Tweening;

public class MultiTween : MultiTween<PointF>
{
    public MultiTween(IMoveStartTween<PointF> initial) : base(initial)
    {
    }

    public MultiTween(IEnumerable<IMoveStartTween<PointF>> steps) : base(steps)
    {
    }
}

public class MultiTween<T> : ITween<T>
{
    private int _index = 0;
    public List<IMoveStartTween<T>> Steps { get; } = new();
    public IMoveStartTween<T> CurrentTween => IsComplete ? Steps.Last() : Steps[_index];
    public T? Last { get; private set; }

    public MultiTween(IMoveStartTween<T> initial) => Steps.Add(initial);
    public MultiTween(IEnumerable<IMoveStartTween<T>> steps) => Steps.AddRange(steps);

    public bool IsComplete => _index >= Steps.Count;

    public T GetNext(float deltaTime)
    {
        if (IsComplete)
            return Last ?? default!;

        var lastTween = CurrentTween;
        if (!lastTween.IsComplete)
        {
            return lastTween.Advance(deltaTime);
        }

        _index++;
        CurrentTween.Start = Last ?? default!;
        return CurrentTween.Advance(deltaTime);
    }

    public T Advance(float deltaTime)
    {
        Last = GetNext(deltaTime);
        return Last;
    }
}