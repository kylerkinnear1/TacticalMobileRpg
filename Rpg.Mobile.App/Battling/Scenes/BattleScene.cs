using Rpg.Mobile.App.Battling.GameObjects;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Battling.Scenes;

public record BattleSceneState(
    GridState Grid,
    List<ButtonState> Buttons,
    List<BattleUnitState> BattleUnits,
    List<BattleUnitState> TurnOrder,
    ShadowOverlayState MovementShadows,
    ShadowOverlayState AttackShadows)
{
    public int? ActiveUnitIndex { get; set; }
    public BattleUnitState? ActiveUnit => ActiveUnitIndex.HasValue ? TurnOrder[ActiveUnitIndex.Value] : null;
}

public enum BattleMenuOptions
{
    SelectingMove,
    SelectingAction,
    SelectingTarget,
}

public class BattleScene
{
    private readonly BattleSceneState _state;
    private readonly Rng _rng = new(new());
    private readonly IDamageCalculator _damageCalculator;

    private readonly IGameLoop _game;

    private readonly PathCalculator _pathCalc = new();
    private Coordinate _lastPosition;
    private BattleMenuOptions _menuState = BattleMenuOptions.SelectingMove;
    private const float ButtonSpacing = 100f;

    public BattleScene(IGameLoop game)
    {
        _game = game;
        _damageCalculator = new DamageCalculator(_rng);

        var gridState = new GridState(new(50f, 50f), 20, 20, 32f);
        var buttonLeft = gridState.ColumnCount * gridState.Size + gridState.Position.X + ButtonSpacing;
        var buttonTop = 50f;
        const float buttonWidth = 150f;
        const float buttonHeight = 50f;
        var attackButtonState = new ButtonState("Attack", new(buttonLeft, buttonTop, buttonWidth, buttonHeight));
        var waitButtonState = new ButtonState("Wait", new(buttonLeft,  buttonTop += ButtonSpacing, buttonWidth, buttonHeight));
        var backButtonState = new ButtonState("Back", new(buttonLeft, buttonTop += ButtonSpacing, buttonWidth, buttonHeight));
        var shadowState = new ShadowOverlayState();
        var attackShadows = new ShadowOverlayState { Color = Colors.DarkRed.WithAlpha(.7f) };

        var spriteLoader = new EmbeddedResourceImageLoader(new(GetType().Assembly));
        var archer1 = spriteLoader.Load("ArcherIdle01.png");
        var archer2 = spriteLoader.Load("ArcherIdle02.png");
        var healer1 = spriteLoader.Load("HealerIdle01.png");
        var healer2 = spriteLoader.Load("HealerIdle02.png");
        var mage1 = spriteLoader.Load("MageIdle01.png");
        var mage2 = spriteLoader.Load("MageIdle02.png");
        var ninja1 = spriteLoader.Load("NinjaIdle01.png");
        var ninja2 = spriteLoader.Load("NinjaIdle02.png");
        var warrior1 = spriteLoader.Load("WarriorIdle01.png");
        var warrior2 = spriteLoader.Load("WarriorIdle02.png");

        var battleUnits = new List<BattleUnitState>
        {
            new(0, archer1) { Position = new(3, 3), Attack = 8, Defense = 4, AttackRange = 3, Movement = 5},
            new(0, healer1) { Position = new(3, 6), Attack = 7, Defense = 4, Movement = 5 },
            new(0, mage1) { Position = new(3, 9), Attack = 8, Defense = 5, AttackRange = 1, Movement = 5},
            new(0, ninja1) { Position = new(3, 12), Attack = 10, Defense = 6, Movement = 7 },
            new(0, warrior1) { Position = new(3, 15), Attack = 10, Defense = 7, Movement = 4 },
            new(1, archer2) { Position = new(17, 3), Attack = 8, Defense = 4, AttackRange = 3, Movement = 5},
            new(1, healer2) { Position = new(17, 6), Attack = 7, Defense = 4, Movement = 5 },
            new(1, mage2) { Position = new(17, 9), Attack = 8, Defense = 5, AttackRange = 1, Movement = 5},
            new(1, ninja2) { Position = new(17, 12), Attack = 10, Defense = 6, Movement = 7 },
            new(1, warrior2) { Position = new(17, 15), Attack = 10, Defense = 7, Movement = 4 },
        };
        var turnOrder = battleUnits.OrderBy(_ => Guid.NewGuid()).ToList(); // pseudo random for now.
        _lastPosition = turnOrder.First().Position;

        var buttons = new List<ButtonState> { attackButtonState, waitButtonState, backButtonState };
        _state = new(
            gridState,
            buttons,
            battleUnits,
            turnOrder,
            shadowState,
            attackShadows)
        {
            ActiveUnitIndex = 0
        };

        var gridGameObject = new GridGameObject(_state.Grid);
        var mapGameObject = new MapGameObject();
        var buttonGameObject1 = new ButtonGameObject(attackButtonState);
        var buttonGameObject2 = new ButtonGameObject(waitButtonState);
        var buttonGameObject3 = new ButtonGameObject(backButtonState);
        var shadowGameObject = new ShadowOverlayGameObject(shadowState, _state);
        var attackShadowGameObject = new ShadowOverlayGameObject(attackShadows, _state);

        _game.AddGameObject(mapGameObject);
        _game.AddGameObject(gridGameObject);
        _game.AddGameObject(shadowGameObject);
        _game.AddGameObject(attackShadowGameObject);

        foreach (var unit in battleUnits)
            _game.AddGameObject(new BattleUnitGameObject(_state, unit));

        _game.AddGameObject(buttonGameObject1);
        _game.AddGameObject(buttonGameObject2);
        _game.AddGameObject(buttonGameObject3);
        
        _game.AddTouchUpHandler(AttackButtonClicked, () => attackButtonState.Bounds);
        _game.AddTouchUpHandler(WaitButtonClicked, () => waitButtonState.Bounds);
        _game.AddTouchUpHandler(BackButtonClicked, () => backButtonState.Bounds);
        _game.AddTouchUpHandler(HandleGridClick, () => gridState.Bounds);

        AdvanceToNextUnit();
    }

    private void AttackButtonClicked() => UpdateMenuState(BattleMenuOptions.SelectingTarget);

    private void WaitButtonClicked() => AdvanceToNextUnit();

    private void AdvanceToNextUnit()
    {
        _state.ActiveUnitIndex = _state.ActiveUnitIndex + 1 < _state.TurnOrder.Count ? _state.ActiveUnitIndex + 1 : 0;
        _lastPosition = _state.ActiveUnit!.Position;
        UpdateMenuState(BattleMenuOptions.SelectingMove);
    }

    private void BackButtonClicked()
    {
        if (_state.ActiveUnit is null)
            return;

        _state.ActiveUnit.Position = _lastPosition;
        UpdateMenuState(BattleMenuOptions.SelectingMove);
    }

    private void HandleGridClick(TouchEvent touches)
    {
        if (_state.ActiveUnit is null)
            return;

        var point = touches.Touches.First();
        var relativeX = point.X - _state.Grid.Position.X;
        var relativeY = point.Y - _state.Grid.Position.Y;

        var col = (int)(relativeX / _state.Grid.Size);
        var row = (int)(relativeY / _state.Grid.Size);

        var position = new Coordinate(col, row);
        if (_state.MovementShadows.ShadowPoints.Contains(position))
        {
            _state.ActiveUnit.Position = position;
            UpdateMenuState(BattleMenuOptions.SelectingAction);
            return;
        }

        var defender = _state.TurnOrder.FirstOrDefault(x => x.Position == position && _state.ActiveUnit.PlayerId != x.PlayerId);
        if (defender is null || !_state.AttackShadows.ShadowPoints.Contains(position)) 
            return;

        HandleAttack(_state.ActiveUnit, defender);
    }

    private void HandleAttack(BattleUnitState attacker, BattleUnitState defender)
    {
        var damage = _damageCalculator.CalcDamage(attacker.Attack, defender.Defense);
        defender.RemainingHealth = Math.Max(0, defender.RemainingHealth - damage);
        if (defender.RemainingHealth <= 0)
        {
            _state.TurnOrder.Remove(defender);
            defender.IsVisible = false;
        }

        AdvanceToNextUnit();
    }

    private void UpdateMenuState(BattleMenuOptions options)
    {
        _menuState = options;
        foreach (var x in _state.Buttons)
        {
            x.IsVisible = options != BattleMenuOptions.SelectingMove;
        }

        _state.MovementShadows.ShadowPoints.Clear();
        _state.AttackShadows.ShadowPoints.Clear();

        if (_state.ActiveUnit is null)
            return;

        if (_menuState == BattleMenuOptions.SelectingMove)
        {
            var walkablePath = _pathCalc.CreateFanOutArea(
                _state.ActiveUnit.Position,
                new(_state.Grid.RowCount, _state.Grid.ColumnCount),
                _state.ActiveUnit.Movement);

            walkablePath = walkablePath
                .Where(shadow =>
                    !_state.TurnOrder.Exists(unit => unit.IsVisible && shadow == unit.Position && unit != _state.ActiveUnit))
                .ToList();

            _state.MovementShadows.ShadowPoints.AddRange(walkablePath);
        }

        if (_menuState == BattleMenuOptions.SelectingTarget)
        {
            var attackPath = _pathCalc.CreateFanOutArea(
                _state.ActiveUnit.Position,
                new(_state.Grid.RowCount, _state.Grid.ColumnCount),
                _state.ActiveUnit.AttackRange);

            attackPath = attackPath
                .Where(a => a != _state.ActiveUnit.Position &&
                            a.X >= 0 && a.Y >= 0 && a.X < _state.Grid.ColumnCount && a.Y < _state.Grid.RowCount)
                .ToList();

            _state.AttackShadows.ShadowPoints.AddRange(attackPath);
        }
    }
}
