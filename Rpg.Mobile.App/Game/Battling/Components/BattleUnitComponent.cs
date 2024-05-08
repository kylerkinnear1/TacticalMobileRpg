using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleUnitComponent : SpriteComponentBase
{
    public BattleUnitState State { get; }
    public BattleUnitHealthBarComponent HealthBar { get; }

    public BattleUnitComponent(IImage sprite, BattleUnitState state) : base(sprite)
    {
        UpdateScale(1.5f);
        State = state;

        HealthBar = AddChild(new BattleUnitHealthBarComponent(State));
        HealthBar.FontColor = State.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        HealthBar.Position = new(-10f, Sprite.Height - HealthBar.Bounds.Height + 10f);
    }
}

public class BattleUnitHealthBarComponent : ComponentBase
{
    public BattleUnitState State { get; }
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);
    public Color FontColor { get; set; } = Colors.Orange;
    public bool HasGone { get; set; } = false;

    public BattleUnitHealthBarComponent(BattleUnitState state) : base(new(0f, 0f, 30f, 25f))
    {
        State = state;
    }

    public override void Update(float deltaTime)
    {

    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (State.RemainingHealth <= 0)
            return;

        canvas.Font = Font;
        canvas.FontSize = HasGone ? 19f : 22f;
        canvas.FontColor = FontColor.WithAlpha(HasGone ? 0.8f : 1.0f);
        canvas.FillColor = HasGone ? Colors.Black.WithAlpha(0.5f) : Colors.SlateGray.WithAlpha(.7f);
        canvas.FillRoundedRectangle(0f, 0f, Bounds.Width, Bounds.Height, 2f);
        canvas.DrawString($"{State.RemainingHealth}", 0f, 20f, HorizontalAlignment.Left);
    }
}
