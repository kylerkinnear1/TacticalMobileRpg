using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Tweening;

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
        CurrentTween.Start = Last ?? default!;
        return CurrentTween.Advance(deltaTime);
    }

    public PointF Advance(float deltaTime)
    {
        Last = GetNext(deltaTime);
        return Last ?? PointF.Zero;
    }
}

public class MultiTweenF : ITween<float>
{
    private int _index = 0;
    public ITween<float>[] Steps { get; }
    public ITween<float> CurrentTween => IsComplete ? Steps.Last() : Steps[_index];
    public float Last { get; private set; } = default;

    public MultiTweenF(params ITween<float>[] steps) => Steps = steps;

    public bool IsComplete => Steps.Last().IsComplete;

    public float Advance(float deltaTime)
    {
        if (IsComplete)
            return Last;

        if (CurrentTween.IsComplete)
            _index++;

        Last = CurrentTween.Advance(deltaTime);
        return Last;
    }
}