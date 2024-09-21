using Rpg.Mobile.GameSdk.Tweening;

namespace Rpg.Mobile.App.Game.UserInterface;

public class TextIndicatorComponent : TextboxComponent
{
    public bool IsPlaying => _movement?.IsComplete ?? true;
    
    private Color _baseColor = Colors.Red;
    private PointF? _start;
    private ITween<PointF>? _movement;

    public TextIndicatorComponent() : base(new(0f, 0f, 100f, 50f))
    {
        FontSize = 35f;
        TextColor = _baseColor.WithAlpha(0f);
    }

    public override void Update(float deltaTime)
    {
        if (_movement == null || _movement.IsComplete)
        {
            Visible = false;
            Position = _start ?? Position;
            return;
        }

        Visible = true;
        Position = _movement.Advance(deltaTime);

        TextColor = _baseColor;
    }

    public void Play(string label, Color? color = null)
    {
        _baseColor = color ?? _baseColor;
        Label = label;

        _start = Position;
        _movement = Position.SpeedTween(new(Position.X, Position.Y - 40f), 30f);
    }
}
