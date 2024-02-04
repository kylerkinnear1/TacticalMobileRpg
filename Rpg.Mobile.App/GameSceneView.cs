using Rpg.Mobile.App.Stuff;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App;

internal class GameSceneView : GraphicsView
{
    private readonly BattleUnit _hero;

    public GameSceneView()
    {
        BackgroundColor = Colors.Transparent;
        Drawable = new TestDraw(this);
        Invalidate();
    }
}

public class TestDraw : IDrawable
{
    private readonly BattleUnit _unit;
    private readonly GraphicsView _view;
    private float _times = 1f;

    public TestDraw(GraphicsView view)
    {
        var imageLoader = new EmbeddedResourceImageLoader(GetType().Assembly);
        var heroSprite = imageLoader.Load("Rpg.Mobile.App.Assets.Sprites.Units.Warrior.png");
        _unit = new BattleUnit(new(100f, 100f), heroSprite);

        _view = view;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Colors.Red;
        canvas.DrawLine(dirtyRect.Left, dirtyRect.Top, dirtyRect.Right, dirtyRect.Bottom);

        canvas.DrawImage(_unit.Sprite, 100f + _times, 100f + _times, 64f, 64f);

        _times++;
        if (_times > 300f)
        {
            _times = 1f;
        }
        _view.Invalidate();
    }
}
