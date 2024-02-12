using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;

namespace Rpg.Mobile.App.Battling.Scenes;

public record BattleSceneState(
    GridState Grid,
    List<ButtonState> Buttons,
    List<BattleUnitState> BattleUnits,
    ShadowOverlayState ShadowUnit)
{
    public int? ActiveUnitIndex { get; set; }
    public BattleUnitState? ActiveUnit => ActiveUnitIndex.HasValue ? BattleUnits[ActiveUnitIndex.Value] : null;
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
        var buttonState1 = new ButtonState("Attack", new(1275f, 50f,  150f, 50f));
        var buttonState2 = new ButtonState("...", new(1275f, 150f, 150f, 50f));
        var buttonState3 = new ButtonState("...", new(1275f, 250f, 150f, 50f));
        var buttonState4 = new ButtonState("Wait", new(1275f, 350f, 150f, 50f));
        var buttonState5 = new ButtonState("Back", new(1275f, 450f, 150f, 50f));
        var battleState1 = new BattleUnitState(warriorSprite) { X = _lastPosition.X, Y = _lastPosition.Y };
        var battleState2 = new BattleUnitState(warriorSprite) { X = 20, Y = 4 };
        var shadowState = new ShadowOverlayState();

        _state = new(
            gridState,
            new()
            {
                buttonState1,
                buttonState2,
                buttonState3,
                buttonState4,
                buttonState5
            },
            new()
            {
                battleState1,
                battleState2
            },
            shadowState)
        {
            ActiveUnitIndex = 0
        };

        var gridGameObject = new GridGameObject(_state.Grid);
        var mapGameObject = new MapGameObject();
        var buttonGameObject1 = new ButtonGameObject(buttonState1);
        var buttonGameObject2 = new ButtonGameObject(buttonState2);
        var buttonGameObject3 = new ButtonGameObject(buttonState3);
        var buttonGameObject4 = new ButtonGameObject(buttonState4);
        var buttonGameObject5 = new ButtonGameObject(buttonState5);
        var battleObject1 = new BattleUnitGameObject(_state, battleState1);
        var battleObject2 = new BattleUnitGameObject(_state, battleState2);
        var shadowGameObject = new ShadowOverlayGameObject(shadowState, _state);

        AddGameObject(mapGameObject);
        AddGameObject(gridGameObject);
        AddGameObject(shadowGameObject);
        AddGameObject(battleObject1);
        AddGameObject(battleObject2);
        AddGameObject(buttonGameObject1);
        AddGameObject(buttonGameObject2);
        AddGameObject(buttonGameObject3);
        AddGameObject(buttonGameObject4);
        AddGameObject(buttonGameObject5);

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

        walkablePath = walkablePath
            .Where(shadow => !_state.BattleUnits.Exists(unit => unit.IsVisible && shadow.X == unit.X && shadow.Y == unit.Y))
            .ToList();

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

        _state.ActiveUnitIndex = _state.ActiveUnitIndex + 1 < _state.BattleUnits.Count ? _state.ActiveUnitIndex + 1 : 0;
    }
}
