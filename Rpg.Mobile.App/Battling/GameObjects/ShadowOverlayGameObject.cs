using Rpg.Mobile.App.Battling.Scenes;
using Rpg.Mobile.GameSdk;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class ShadowOverlayState
{
    public List<Point> ShadowPoints { get; } = new();
    public Color Color { get; set; } = Colors.DarkSlateGray.WithAlpha(.5f);
}

public class ShadowOverlayGameObject : ComponentBase
{
    private readonly ShadowOverlayState _state;
    private readonly BattleSceneState _scene;

    public ShadowOverlayGameObject(ShadowOverlayState state, BattleSceneState scene) : base(CalculateBounds(state, scene))
    {
        _state = state;
        _scene = scene;
    }

    public override void Update(TimeSpan delta)
    {
        CalculateBounds(_state, _scene);
    }

    private static RectF CalculateBounds(ShadowOverlayState state, BattleSceneState scene)
    {
        var xValues = state.ShadowPoints.Select(x => x.X).Append(0).ToList();
        var yValues = state.ShadowPoints.Select(x => x.Y).Append(0).ToList();

        return RectF.FromLTRB(
            scene.Grid.Size * xValues.Min() + scene.Grid.Position.X,
            scene.Grid.Size * yValues.Min() + scene.Grid.Position.Y,
            scene.Grid.Size * xValues.Max() + scene.Grid.Position.X,
            scene.Grid.Size * yValues.Max() + scene.Grid.Position.Y);
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
        if (_state.ShadowPoints.Count <= 0)
        {
            return;
        }

        canvas.FillColor = _state.Color;

        foreach (var point in _state.ShadowPoints)
        {
            var left = _scene.Grid.Size * point.X + _scene.Grid.Position.X;
            var top = _scene.Grid.Size * point.Y + _scene.Grid.Position.Y;
            canvas.FillRectangle(left, top, _scene.Grid.Size, _scene.Grid.Size);
        }
    }
}