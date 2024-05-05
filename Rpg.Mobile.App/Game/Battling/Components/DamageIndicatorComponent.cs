using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class DamageIndicatorComponent : TextboxComponent
{
    public TimeSpan DisplayTime = TimeSpan.FromSeconds(1);

    private ITween<PointF>? _movement;

    public DamageIndicatorComponent() : base(new(0f, 0f, 100f, 50f), "") { }

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

    public void ShowDamage(int damage)
    {
        TextColor = damage >= 0 ? Colors.Red : Colors.Lime;
        Label = damage >= 0 ? $"-{damage}" : $"+{-damage}";

        _movement = Position.TweenTo(new(Position.X, Position.Y - 40f), 50f);
    }
}
