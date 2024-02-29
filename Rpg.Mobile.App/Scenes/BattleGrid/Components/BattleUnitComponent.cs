using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Scenes.BattleGrid.Components;

public class BattleUnitState
{
    public int PlayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public Point Position { get; set; } = new(0, 0);
    public int Movement { get; set; } = 4;
    public int RemainingHealth { get; set; } = 12;
    public int MaxHealth { get; set; } = 12;
    public IImage Sprite { get; set; }
    public int Attack { get; set; } = 8;
    public int AttackRange { get; set; } = 1;
    public int Defense { get; set; } = 5;
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

    public override void Update(TimeSpan delta) => Bounds = State.Sprite.GetBounds(State.Scale);

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

    public override void Update(TimeSpan delta) => Bounds = new(-10f, 20f, 30f, 25f);

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
