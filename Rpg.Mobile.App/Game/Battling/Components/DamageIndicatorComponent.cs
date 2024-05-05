using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class DamageIndicatorComponent : TextboxComponent
{
    public TimeSpan DisplayTime = TimeSpan.FromSeconds(1);
    public float FontSize = 16f;

    private ITween<PointF>? _movement;

    public DamageIndicatorComponent() : base(RectF.Zero, "") { }

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
        TextColor = damage >= 0 ? Colors.DarkRed : Colors.Lime;
        Label = $"{damage}";

        _movement = Position.TweenTo(new(Position.X, Position.Y - 50f), 50f);
    }
}
