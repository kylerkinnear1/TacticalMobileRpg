using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Battling.Units;

public class GenericUnitState
{
    public PointF Location { get; set; }
    public IImage Sprite { get; set; }
    public float Scale { get; set; }

    public GenericUnitState(PointF location, IImage sprite, float scale)
    {
        Location = location;
        Sprite = sprite;
        Scale = scale;
    }
}

public class GenericUnitGameObject : IUpdateState<GenericUnitState>, IRenderState<GenericUnitState>
{
    public void Update(GenericUnitState state)
    {
        state.Location = state.Location.X > 300 ? new PointF(100f, 100f) : new PointF(state.Location.X + 1f, state.Location.Y + 1f);
    }

    public void Render(GenericUnitState state, ICanvas canvas, RectF dirtyRect)
    {
        canvas.Draw(state.Sprite, state.Location, state.Scale);
    }
}

public class BattleScene : IScene, IDrawable
{
    private readonly GenericUnitState _unitState;
    private readonly GenericUnitGameObject _unitGameObject;
    private readonly IGraphicsView _view;

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png");
        _unitState = new GenericUnitState(new(100f, 100f), warriorSprite, 5f);
        _unitGameObject = new GenericUnitGameObject();

        _view = view;
    }

    public void Update(TimeSpan delta)
    {
        _unitGameObject.Update(_unitState);
    }

    public void Render() => _view.Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _unitGameObject.Render(_unitState, canvas, dirtyRect);
    }
}
