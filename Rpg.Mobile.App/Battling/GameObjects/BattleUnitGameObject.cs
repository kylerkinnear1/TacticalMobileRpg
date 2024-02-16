using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using Font = Microsoft.Maui.Graphics.Font;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class BattleUnitState
{
    public int PlayerId { get; set; }
    public bool IsVisible { get; set; } = true;
    public Coordinate Position { get; set; } = new(0, 0);
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

public class BattleUnitGameObject : IGameObject
{
    private readonly BattleSceneState _scene;
    private readonly BattleUnitState _state;

    public BattleUnitGameObject(BattleSceneState scene, BattleUnitState state)
    {
        _scene = scene;
        _state = state;
    }


    public void Update(TimeSpan delta)
    {
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (!_state.IsVisible)
            return;

        var x = _state.Position.X * _scene.Grid.Size + _scene.Grid.Position.X;
        var y = _state.Position.Y * _scene.Grid.Size + _scene.Grid.Position.Y;
        canvas.Draw(_state.Sprite, new(x, y - 3f), _state.Scale);
        canvas.Font = _state.StatusFont;
        canvas.FontSize = 22f;
        canvas.FontColor = _state.PlayerId == 0 ? Colors.Aqua : Colors.Orange;
        canvas.FillColor = Colors.SlateGrey.WithAlpha(.65f);
        canvas.FillRoundedRectangle(x - 12f, y + 20f, 30f, 25f, 2f);
        canvas.DrawString($"{_state.RemainingHealth}", x - 10f, y + 40f, HorizontalAlignment.Left);
    }
}