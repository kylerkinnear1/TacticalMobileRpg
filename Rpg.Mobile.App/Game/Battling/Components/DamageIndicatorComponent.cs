using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class MultiDamageIndicatorComponent : ComponentBase
{
    private readonly List<DamageIndicatorComponent> _labels =
        // TODO: Just do 5 since that's the max for now.
        Enumerable.Range(0, 5).Select(_ => new DamageIndicatorComponent()).ToList();

    public MultiDamageIndicatorComponent(RectF bounds) : base(bounds)
    {
        foreach (var label in _labels) AddChild(label);
    }
    public override void Update(float deltaTime) {}
    public override void Render(ICanvas canvas, RectF dirtyRect) {}

    public void SetDamage(List<(PointF Position, int Damage)> damage)
    {
        foreach (var label in _labels.Zip(damage))
        {
            label.First.Position = label.Second.Position;
            label.First.ShowDamage(label.Second.Damage);
        }
    }
}

public class DamageIndicatorComponent : TextboxComponent
{
    private ITween<PointF>? _movement;

    public DamageIndicatorComponent() : base(new(0f, 0f, 100f, 50f))
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

    public void ShowDamage(int damage)
    {
        TextColor = damage >= 0 ? Colors.Red : Colors.Lime;
        Label = damage >= 0 ? $"-{damage}" : $"+{-damage}";

        _movement = Position.TweenTo(new(Position.X, Position.Y - 40f), 50f);
    }
}
