namespace Rpg.Mobile.GameSdk.Tweening;

public interface ITween<T>
{
    bool IsComplete { get; }

    T Advance(float deltaTime);
}

public interface IMoveStartTween<T> : ITween<T>
{
    T Start { get; set; }
}