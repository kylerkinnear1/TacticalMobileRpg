using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Battling.GameObjects;

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
    public Font StatusFont { get; set; } = new("Arial", FontWeights.ExtraBold, FontStyleType.Italic);

    public BattleUnitState(int playerId, IImage sprite)
    {
        PlayerId = playerId;
        Sprite = sprite;
    }
}

public class BattleUnitGameObject : ComponentBase
{
    private readonly BattleSceneState _scene;
    private readonly BattleUnitState _state;

    public BattleUnitGameObject(BattleSceneState scene, BattleUnitState state) : base(CalculateBounds(state, scene))
    {
        _scene = scene;
        _state = state;
    }

    public override void Update(TimeSpan delta) => Bounds = CalculateBounds(_state, _scene);

    private static RectF CalculateBounds(BattleUnitState state, BattleSceneState scene)
    {
        var x = state.Position.X * scene.Grid.Size + scene.Grid.Position.X;
        var y = state.Position.Y + scene.Grid.Size + scene.Grid.Position.Y - 3f;
        var width = state.Sprite.Width * state.Scale;
        var height = state.Sprite.Height * state.Scale;
        return new RectF(x, y, width, height);
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!_state.IsVisible)
            return;
        
        canvas.Draw(_state.Sprite, Bounds.Location, _state.Scale);
        
        canvas.Font = _state.StatusFont;
        canvas.FontSize = 22f;
        canvas.FontColor = _state.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.FillColor = Colors.SlateGrey.WithAlpha(.65f);
        canvas.FillRoundedRectangle(Bounds.X - 12f, Bounds.Y + 20f, 30f, 25f, 2f);
        canvas.DrawString($"{_state.RemainingHealth}", Bounds.X - 10f, Bounds.Y + 40f, HorizontalAlignment.Left);
    }
}