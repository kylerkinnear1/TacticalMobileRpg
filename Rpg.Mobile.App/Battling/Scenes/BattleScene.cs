using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.Scenes;

public record BattleSceneState(
    GenericUnitState GenericUnit,
    GridState Grid);

public class BattleScene : IScene, IDrawable
{
    private readonly BattleSceneState _state;
    private readonly IGraphicsView _view;

    private readonly List<IUpdateGameObject> _updates = new();
    private readonly List<IRenderGameObject> _renderers = new();

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png"); 

        var unitState = new GenericUnitState(new(100f, 100f), warriorSprite, 0.5f);
        var gridState = new GridState(new(50f, 50f), 60, 30, 20);
        _state = new(unitState, gridState);

        var genericGameObject = new GenericUnitGameObject(_state.GenericUnit, _state);
        var gridGameObject = new GridGameObject(_state.Grid);
        var mapGameObject = new MapGameObject();

        AddGameObject(mapGameObject);
        AddGameObject(gridGameObject);
        AddGameObject(genericGameObject);

        _view = view;

        void AddGameObject(IGameObject obj)
        {
            _updates.Add(obj);
            _renderers.Add(obj);
        }
    }

    public void Update(TimeSpan delta)
    {
        foreach (var update in _updates) 
            update.Update(delta);
    }

    public void Render() => _view.Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var renderer in _renderers)
            renderer.Render(canvas, dirtyRect);
    }
}
