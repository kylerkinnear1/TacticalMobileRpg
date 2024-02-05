using Rpg.Mobile.App.Assets;
using Rpg.Mobile.App.States;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

internal class GameSceneView : GraphicsView
{
    public GameSceneView()
    {
        BackgroundColor = Colors.Transparent;
        Drawable = new TestDraw();

        var loop = new GameLoop(Dispatcher, this);
        loop.Start();
    }
}

public class TestDraw : IDrawable
{
    private readonly BattleUnit _unit;
    private float _times = 1f;

    public TestDraw()
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly, Consts.Assets.Paths.Sprites));
        var heroSprite = spriteLoader.Load("Units.Warrior.png");
        _unit = new BattleUnit(new(100f, 100f), heroSprite);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.Red;
        canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);

        canvas.DrawImage(_unit.Sprite, 100f + _times, 100f + _times, 64f, 64f);

        _times = _times <= 300f ? _times + 1f : 1f;
    }
}
