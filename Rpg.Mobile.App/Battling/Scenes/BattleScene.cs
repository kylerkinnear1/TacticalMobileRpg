using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.Scenes;

public record BattleSceneState(
    GenericUnitState GenericUnit,
    GridState Grid,
    ButtonState TestButton,
    BattleUnitState BattleUnit);

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
        var buttonState = new ButtonState("Test Button", new(100f, 100f, 200f, 75f));
        var battleUnitState = new BattleUnitState(warriorSprite, 3, 10);

        _state = new(unitState, gridState, buttonState, battleUnitState);

        var genericGameObject = new GenericUnitGameObject(_state.GenericUnit, _state);
        var gridGameObject = new GridGameObject(_state.Grid);
        var mapGameObject = new MapGameObject();
        var buttonGameObject = new ButtonGameObject(buttonState);
        var battleUnitGameObject = new BattleUnitGameObject(_state, battleUnitState);

        AddGameObject(mapGameObject);
        AddGameObject(gridGameObject);
        AddGameObject(genericGameObject);
        AddGameObject(battleUnitGameObject);
        AddGameObject(buttonGameObject);

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

    public void OnClickDown(TouchEventArgs touchEventArgs)
    {
    }

    public void OnClickUp(TouchEventArgs touchEventArgs)
    {
        var point = touchEventArgs.Touches.First();
        var relativeX = point.X - _state.Grid.Position.X;
        var relativeY = point.Y - _state.Grid.Position.Y;

        var col = (int)(relativeX / _state.Grid.Size);
        var row = (int)(relativeY / _state.Grid.Size);

        _state.BattleUnit.X = col;
        _state.BattleUnit.Y = row;
    }
}
