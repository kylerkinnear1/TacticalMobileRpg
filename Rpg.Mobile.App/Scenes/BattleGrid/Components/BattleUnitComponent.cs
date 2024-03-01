using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class BattleUnitState
{
    public int PlayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public int RemainingHealth { get; set; } = 12;
    public IImage Sprite { get; set; }
    public float Scale { get; set; } = 1f;
    
    public BattleUnitState(int playerId, IImage sprite)
    {
        PlayerId = playerId;
        Sprite = sprite;
    }
}

public class BattleUnitComponent : ComponentBase
{
    public BattleUnitState State { get; }
    public BattleUnitHealthBar HealthBar { get; }

    public BattleUnitComponent(BattleUnitState state) 
        : base(new(0, 0, state.Sprite.Width * state.Scale, state.Sprite.Height * state.Scale))
    {
        State = state;

        HealthBar = AddChild(new BattleUnitHealthBar(State));
    }

    public override void Update(TimeSpan delta) => Bounds = State.Sprite.GetBounds(Bounds.Location, State.Scale);

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!State.IsVisible)
            return;

        canvas.Draw(State.Sprite);
    }
}

public class BattleUnitHealthBar : ComponentBase
{
    public BattleUnitState State { get; }
    public Font Font { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);

    public BattleUnitHealthBar(BattleUnitState state) : base(new(-20f, 40f, 30f, 25f))
    {
        State = state;
    }

    public override void Update(TimeSpan delta) { }

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
