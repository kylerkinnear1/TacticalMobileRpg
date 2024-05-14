using Rpg.Mobile.App.Game.Battling.Systems.Data;
using Rpg.Mobile.GameSdk.Core;


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
        HealthBar.Position = new(-10f, Sprite.Height - HealthBar.Bounds.Height + 10f);
    }
}

public class BattleUnitHealthBarComponent : ComponentBase
{
    public BattleUnitState State { get; }
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);
    public bool HasGone { get; set; } = false;

    public BattleUnitHealthBarComponent(BattleUnitState state) : base(new(0f, 0f, 30f, 25f))
    {
        State = state;
    }

    public override void Update(float deltaTime)
    {
        Visible = State.RemainingHealth > 0;
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Font = Font;
        canvas.FontSize = HasGone ? 19f : 22f;

        canvas.FillColor = HasGone ? Colors.SlateGray.WithAlpha(.8f) : Colors.Black.WithAlpha(.4f);
        canvas.FontColor = State.PlayerId switch
        {
            0 when !HasGone => Colors.Aqua,
            1 when !HasGone => Colors.Orange,
            0 when HasGone => Colors.DarkBlue,
            1 when HasGone => Colors.Brown,
            _ => throw new ArgumentException()
        };

        canvas.StrokeColor = State.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.StrokeSize = 1f;

        var bounds = new RectF(PointF.Zero, Bounds.Size);
        canvas.FillRoundedRectangle(bounds, 2f);
        canvas.DrawString($"{State.RemainingHealth}", bounds, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
