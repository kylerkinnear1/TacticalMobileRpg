using Rpg.Mobile.GameSdk.Tweening;

namespace Rpg.Mobile.App.Game.Common;

public class TextIndicatorComponent : TextboxComponent
{
    private Color _baseColor = Colors.Red;
    private ITween<PointF>? _movement;

    public TimeSpan? FadeIn { get; set; } = TimeSpan.FromSeconds(2f);
    public TimeSpan? DelayFadeOut { get; set; } = TimeSpan.FromSeconds(10f);
    public TimeSpan? FadeOut { get; set; } = TimeSpan.FromSeconds(5);

    private MultiTweenF? _fade;

    public TextIndicatorComponent() : base(new(0f, 0f, 100f, 50f))
    {
        FontSize = 35f;
        TextColor = _baseColor.WithAlpha(0f);
    }

    public override void Update(float deltaTime)
    {
        if (
            _movement == null || _movement.IsComplete || 
            (_fade != null && _fade.IsComplete))
        {
            Visible = false;
            return;
        }

        Visible = true;
        Position = _movement.Advance(deltaTime);
        
        TextColor = _baseColor.WithAlpha(_fade?.Advance(deltaTime) ?? 1.0f);
    }

    public void Play(string label, Color? color = null)
    {
        _baseColor = color ?? _baseColor;
        Label = label;

        _movement = Position.SpeedTween(new(Position.X, Position.Y - 1000f), 30f);
        _fade = FadeIn.HasValue || DelayFadeOut.HasValue || FadeOut.HasValue
            ? new MultiTweenF(
                new TimeTween(0f, 1f, FadeIn ?? TimeSpan.Zero), 
                new TimeTween(1f, 1f, DelayFadeOut ?? TimeSpan.Zero), 
                new TimeTween(1f, 0f, FadeOut ?? TimeSpan.Zero))
            : null;
    }
}
