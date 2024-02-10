using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.Scenes;

public class BattleScene : IScene, IDrawable
{
    private readonly GenericUnitState _unitState;
    private readonly GridState _gridState;
    private readonly IGraphicsView _view;

    private readonly List<IUpdateGameObject> _updates = new();
    private readonly List<IRenderGameObject> _renders = new();

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png");
        _unitState = new GenericUnitState(new(100f, 100f), warriorSprite, 5f);
        _gridState = new GridState(new(0f, 0f), 60, 30, 20);

        var genericGameObject = new GenericUnitGameObject(_unitState);
        var gridGameObject = new GridGameObject(_gridState);

        _updates.Add(genericGameObject);
        _updates.Add(gridGameObject);

        _renders.Add(genericGameObject);
        _renders.Add(gridGameObject);

        _view = view;
    }

    public void Update(TimeSpan delta)
    {
        foreach (var update in _updates) 
            update.Update(delta);
    }

    public void Render() => _view.Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var render in _renders)
            render.Render(canvas, dirtyRect);
    }
}
