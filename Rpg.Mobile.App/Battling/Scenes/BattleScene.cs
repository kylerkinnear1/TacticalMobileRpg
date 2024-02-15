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
    private Coordinate _lastPosition = new(10, 15);
    private bool _isSelectingAction = false;
    private const float ButtonSpacing = 100f;

    public BattleScene(IGraphicsView view)
    {
        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var warriorSprite = spriteLoader.Load("Warrior.png"); 

        var gridState = new GridState(new(50f, 50f), 60, 30, 20);
        var attackButtonState = new ButtonState("Attack", new(1275f, 50f,  150f, 50f), AttackButtonClicked);
        var waitButtonState = new ButtonState("Wait", new(1275f, 50f + ButtonSpacing, 150f, 50f), WaitButtonClicked);
        var backButtonState = new ButtonState("Back", new(1275f, 50f + (2 * ButtonSpacing), 150f, 50f), BackButtonClicked);
        var battleState1 = new BattleUnitState(warriorSprite) { Position = _lastPosition };
        var battleState2 = new BattleUnitState(warriorSprite) { Position = new(4, 20) };
        var shadowState = new ShadowOverlayState();

        _state = new(
            gridState,
            new()
            {
                attackButtonState,
                waitButtonState,
                backButtonState
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
        var buttonGameObject1 = new ButtonGameObject(attackButtonState);
        var buttonGameObject2 = new ButtonGameObject(waitButtonState);
        var buttonGameObject3 = new ButtonGameObject(backButtonState);
        var buttonGameObject4 = new ButtonGameObject(waitButtonState);
        var buttonGameObject5 = new ButtonGameObject(backButtonState);
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

        UpdateButtons();

        void AddGameObject(IGameObject obj)
        {
            _updates.Add(obj);
            _renderers.Add(obj);
        }
    }

    private void AttackButtonClicked()
    {
        _isSelectingAction = false;
        _state.ActiveUnitIndex = _state.ActiveUnitIndex + 1 < _state.BattleUnits.Count ? _state.ActiveUnitIndex + 1 : 0;
        UpdateButtons();
    }

    private void WaitButtonClicked()
    {
        _isSelectingAction = false;
        _state.ActiveUnitIndex = _state.ActiveUnitIndex + 1 < _state.BattleUnits.Count ? _state.ActiveUnitIndex + 1 : 0;
        _lastPosition = _state.ActiveUnit.Position;
        UpdateButtons();
    }

    private void BackButtonClicked()
    {
        if (_state.ActiveUnit is null)
            return;

        _state.ActiveUnit.Position = _lastPosition;
        _isSelectingAction = false;

        UpdateButtons();
    }

    public void Update(TimeSpan delta)
    {
        _state.ShadowUnit.ShadowPoints.Clear();
        if (_state.ActiveUnit is not null && !_isSelectingAction)
        {
            var walkablePath = _pathCalc.CreateFanOutArea(
                _state.ActiveUnit.Position,
                new(_state.Grid.RowCount, _state.Grid.ColumnCount),
                _state.ActiveUnit.Movement);

            walkablePath = walkablePath
                .Where(shadow =>
                    !_state.BattleUnits.Exists(unit => unit.IsVisible && shadow == unit.Position))
                .ToList();

            _state.ShadowUnit.ShadowPoints.AddRange(walkablePath);
        }

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
        HandleGridClick(touchEventArgs);
        HandleButtonClick(touchEventArgs);
    }

    private void HandleGridClick(TouchEventArgs touchEventArgs)
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

        if (_state.ActiveUnit.Position.X.IsBetweenInclusive(0, _state.Grid.ColumnCount) &&
            _state.ActiveUnit.Position.Y.IsBetweenInclusive(0, _state.Grid.RowCount))
        {
            _state.ActiveUnit.Position = new(col, row);
        }

        _isSelectingAction = true;
        UpdateButtons();
    }

    private void HandleButtonClick(TouchEventArgs touchEventArgs)
    {
        var point = touchEventArgs.Touches.First();
        var clickedButton = _state.Buttons.FirstOrDefault(x => x.IsVisible && x.Bounds.Contains(point));
        clickedButton?.Handler();
    }

    private void UpdateButtons()
    {
        foreach (var x in _state.Buttons)
        {
            x.IsVisible = _isSelectingAction;
        }
    }
}
