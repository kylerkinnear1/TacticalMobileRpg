using Rpg.Mobile.GameSdk;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleUnitState
{
    public int PlayerId { get; set; }
    public int RemainingHealth { get; set; } = 12;
    public int Movement { get; set; } = 4;

    public BattleUnitState(int playerId) => PlayerId = playerId;
}

public class BattleUnitComponent : SpriteComponentBase
{
    public BattleUnitState State { get; }
    public BattleUnitHealthBarComponent HealthBar { get; }

    public BattleUnitComponent(IImage sprite, BattleUnitState state) : base(sprite)
    {
        State = state;

        HealthBar = AddChild(new BattleUnitHealthBarComponent(State));
        HealthBar.Position = new(-10f, Sprite.Height - HealthBar.Bounds.Height + 10f);
    }
}

public class BattleUnitHealthBarComponent : ComponentBase
{
    public BattleUnitState State { get; }
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);

    public BattleUnitHealthBarComponent(BattleUnitState state) : base(new(0f, 0f, 30f, 25f))
    {
        State = state;
    }

    public override void Update(float deltaTime) { }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Font = Font;
        canvas.FontSize = 22f;
        canvas.FontColor = State.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.FillColor = Colors.SlateGrey.WithAlpha(.65f);
        canvas.FillRoundedRectangle(0f, 0f, Bounds.Width, Bounds.Height, 2f);
        canvas.DrawString($"{State.RemainingHealth}", 0f, 20f, HorizontalAlignment.Left);
    }
}
