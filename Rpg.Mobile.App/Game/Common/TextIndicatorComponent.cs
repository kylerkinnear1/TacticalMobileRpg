using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Common;

public class TextIndicatorComponent : TextboxComponent
{
    private ITween<PointF>? _movement;

    public TextIndicatorComponent() : base(new(0f, 0f, 100f, 50f))
    {
        FontSize = 35f;
    }

    public override void Update(float deltaTime)
    {
        if (_movement?.IsComplete ?? true)
        {
            Visible = false;
            return;
        }

        Visible = true;
        Position = _movement.Advance(deltaTime);
    }

    public void Play(string label, Color? color = null)
    {
        TextColor = color ?? TextColor;
        Label = label;

        _movement = Position.TweenTo(new(Position.X, Position.Y - 40f), 50f);
    }
}
