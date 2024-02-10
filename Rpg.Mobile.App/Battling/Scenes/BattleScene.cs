using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.Scenes;

public class BattleScene : IScene, IDrawable
{
    private readonly GenericUnitState _unitState;
    private readonly GenericUnitGameObject _unitGameObject;

    private readonly GridState _gridState;
    private readonly GridGameObject _gridGameObject;
    private readonly IGraphicsView _view;

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png");
        _unitState = new GenericUnitState(new(100f, 100f), warriorSprite, 5f);
        _unitGameObject = new GenericUnitGameObject(_unitState);

        _gridState = new GridState(new(0f, 0f), 40, 40, 32);
        _gridGameObject = new GridGameObject(_gridState);

        _view = view;
    }

    public void Update(TimeSpan delta)
    {
        _gridGameObject.Update(delta);
        _unitGameObject.Update(delta);
    }

    public void Render() => _view.Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _gridGameObject.Render(canvas, dirtyRect);
        _unitGameObject.Render(canvas, dirtyRect);
    }
}