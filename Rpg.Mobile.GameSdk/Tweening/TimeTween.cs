namespace Rpg.Mobile.GameSdk.Tweening;

public class TimeTween : IMoveStartTween<float>
{
    public bool IsComplete => LastTick.HasValue && StartTick.HasValue && LastTick - StartTick >= TargetDuration;
    public DateTime? StartTick { get; private set; }
    public DateTime? LastTick { get; private set; }
    
    public TimeSpan TargetDuration { get; }
    public float Start { get; set; }
    public float End { get; }

    public float Advance(float deltaTime)
    {
        StartTick ??= DateTime.Now;
        LastTick ??= DateTime.Now;
        if (LastTick - StartTick >= TargetDuration)
            return End;

        LastTick = DateTime.Now;

        var percent = (float)(LastTick.Value - StartTick.Value).TotalSeconds;
        return (End >= Start ? (End - Start) * percent : (Start - End)) * (1.0f - percent);
    }

    public TimeTween(float start, float end, TimeSpan duration)
    {
        Start = start;
        End = end;
        TargetDuration = duration;
    }
}