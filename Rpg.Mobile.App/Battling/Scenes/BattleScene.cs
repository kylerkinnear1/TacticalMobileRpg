using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Battling.Scenes;

public record BattleSceneState(
    GridState Grid,
    ButtonState TestButton,
    List<BattleUnitState> BattleUnits,
    ShadowOverlayState ShadowUnit)
{
    public BattleUnitState? ActiveUnit { get; set; }
}

public class BattleScene : IScene, IDrawable
{
    private readonly BattleSceneState _state;
    private readonly IGraphicsView _view;

    private readonly List<IUpdateGameObject> _updates = new();
    private readonly List<IRenderGameObject> _renderers = new();

    private readonly PathCalculator _pathCalc = new();
    private readonly Coordinate _lastPosition = new(10, 15);

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png"); 

        var gridState = new GridState(new(50f, 50f), 60, 30, 20);
        var buttonState = new ButtonState("Test Button", new(1300f, 50f, 100f, 50f));
        var battleState1 = new BattleUnitState(warriorSprite) { X = _lastPosition.X, Y = _lastPosition.Y };
        var battleState2 = new BattleUnitState(warriorSprite) { X = 20, Y = 4 };
        var shadowState = new ShadowOverlayState();

        _state = new(gridState, buttonState, new() { battleState1, battleState2 }, shadowState)
        {
            ActiveUnit = battleState1
        };

        var gridGameObject = new GridGameObject(_state.Grid);
        var mapGameObject = new MapGameObject();
        var buttonGameObject = new ButtonGameObject(buttonState);
        var battleObject1 = new BattleUnitGameObject(_state, battleState1);
        var battleObject2 = new BattleUnitGameObject(_state, battleState2);
        var shadowGameObject = new ShadowOverlayGameObject(shadowState, _state);

        AddGameObject(mapGameObject);
        AddGameObject(gridGameObject);
        AddGameObject(shadowGameObject);
        AddGameObject(battleObject1);
        AddGameObject(battleObject2);
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
        _state.ShadowUnit.ShadowPoints.Clear();
        if (_state.ActiveUnit is null)
            return;

        var walkablePath = _pathCalc.CreateFanOutArea(
            new(_state.ActiveUnit.X, _state.ActiveUnit.Y),
            new(_state.Grid.RowCount, _state.Grid.ColumnCount),
            _state.ActiveUnit.Movement);

        _state.ShadowUnit.ShadowPoints.AddRange(walkablePath);

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
        if (_state.ActiveUnit is null)
            return;
        
        var point = touchEventArgs.Touches.First();
        var relativeX = point.X - _state.Grid.Position.X;
        var relativeY = point.Y - _state.Grid.Position.Y;
        
        var col = (int)(relativeX / _state.Grid.Size);
        var row = (int)(relativeY / _state.Grid.Size);

        if (!_state.ShadowUnit.ShadowPoints.Contains(new(col, row)))
        {
            return;
        }

        if (_state.ActiveUnit.X.IsBetweenInclusive(0, _state.Grid.ColumnCount))
            _state.ActiveUnit.X = col;

        if (_state.ActiveUnit.Y.IsBetweenInclusive(0, _state.Grid.RowCount))
            _state.ActiveUnit.Y = row;
    }
}
