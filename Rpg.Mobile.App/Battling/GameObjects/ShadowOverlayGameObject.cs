using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class ShadowOverlayState
{
    public List<Coordinate> ShadowPoints { get; } = new();
}

public class ShadowOverlayGameObject : IGameObject
{
    private readonly ShadowOverlayState _state;
    private readonly BattleSceneState _scene;

    public ShadowOverlayGameObject(ShadowOverlayState state, BattleSceneState scene)
    {
        _state = state;
        _scene = scene;
    }

    public void Update(TimeSpan delta)
    {
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (_state.ShadowPoints.Count <= 0)
        {
            return;
        }

        canvas.FillColor = Colors.DarkSlateGrey.WithAlpha(.5f);

        foreach (var point in _state.ShadowPoints)
        {
            var left = _scene.Grid.Size * point.X + _scene.Grid.Position.X;
            var top = _scene.Grid.Size * point.Y + _scene.Grid.Position.Y;
            canvas.FillRectangle(left, top, _scene.Grid.Size, _scene.Grid.Size);
        }
    }
}