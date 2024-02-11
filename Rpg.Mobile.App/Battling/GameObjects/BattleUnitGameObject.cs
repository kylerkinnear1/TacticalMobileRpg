using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class BattleUnitState
{
    public bool IsVisible { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public IImage Sprite { get; set; }

    public BattleUnitState(
        IImage sprite, 
        int x = 0,
        int y = 0,
        bool isVisible = true)
    {
        IsVisible = isVisible;
        X = x;
        Y = y;
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

        canvas.Draw(_state.Sprite, new(
            _state.X * _scene.Grid.Size + _scene.Grid.Position.X,
            _state.Y * _scene.Grid.Size + _scene.Grid.Position.Y), 
            .5f);
    }
}