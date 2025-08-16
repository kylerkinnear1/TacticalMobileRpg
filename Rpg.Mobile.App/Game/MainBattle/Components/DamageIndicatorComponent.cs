using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class MultiDamageIndicatorComponent : ComponentBase
{
    public bool IsPlaying => _labels.Any(x => x.IsPlaying);
    
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

public class DamageIndicatorComponent : ComponentBase
{
    private readonly TextIndicatorComponent _text;
    
    public bool IsPlaying => _text.IsPlaying;

    public DamageIndicatorComponent(TextIndicatorComponent text) : base(text.Bounds) => AddChild(_text = text);
    public DamageIndicatorComponent() : this(new()) { }

    public void ShowDamage(int damage) =>
        _text.Play(
            damage >= 0 ? $"-{damage}" : $"+{-damage}",
            damage >= 0 ? Colors.Red : Colors.Lime);

    public override void Update(float deltaTime) { }
    public override void Render(ICanvas canvas, RectF dirtyRect) { }
}
