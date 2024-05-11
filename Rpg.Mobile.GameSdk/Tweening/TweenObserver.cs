namespace Rpg.Mobile.GameSdk.Tweening;

public class TweenObserver<T> : ITween<T>
{
    private bool WasComplete = false;
    public bool IsComplete => _subject.IsComplete;

    private readonly ITween<T> _subject;

    private readonly List<Action<float, T>> _onCompleteHandlers = new();

    public TweenObserver(ITween<T> subject) => _subject = subject;

    public T Advance(float deltaTime)
    {
        var value = _subject.Advance(deltaTime);
        if (IsComplete && !WasComplete)
        {
            WasComplete = true;
            _onCompleteHandlers.ForEach(x => x(deltaTime, value));
        }

        return value;
    }

    public void AddOnComplete(Action<float, T> onComplete) => _onCompleteHandlers.Add(onComplete);
}