using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class BattleUnitComponent : SpriteComponentBase
{
    public record Data(
        BattleUnitData BattleUnit,
        BattleData BattleData);
    
    public BattleUnitHealthBarComponent HealthBar { get; }

    public BattleUnitComponent(Data data, IImage sprite) : base(sprite)
    {
        UpdateScale(1.5f);
        HealthBar = AddChild(new BattleUnitHealthBarComponent(data));
        HealthBar.Position = new(-10f, Sprite.Height - HealthBar.Bounds.Height + 10f);
    }
}

public class BattleUnitComponentStateMachine
{
    public record MovementCompleted(BattleUnitComponent Component) : IEvent;

    public BattleUnitComponent Unit { get; }

    public BattleUnitComponentStateMachine(BattleUnitComponent unit) => Unit = unit;

    private readonly StateMachine _state = new(new Idle());

    public void Execute(float deltaTime) => _state.Execute(deltaTime);

    public void MoveTo(PointF target, Action? onComplete = null, float speed = 500f)
    {
        var tween = Unit.Position.SpeedTween(target, speed);
        var moving = new Moving(Unit, tween, () =>
        {
            _state.Change(new Idle());
            Bus.Global.Publish(new MovementCompleted(Unit));
            onComplete?.Invoke();
        });
        _state.Change(moving);
    }
        
    public class Moving(
        BattleUnitComponent _component, 
        ITween<PointF> _tween,
        Action? _onComplete = null) : IState
    {
        public void Enter() { }

        public void Execute(float deltaTime)
        {
            if (!_tween.IsComplete)
                _component.Position = _tween.Advance(deltaTime);

            if (_tween.IsComplete)
                _onComplete?.Invoke();
        }

        public void Leave() { }
    }

    public class Idle : IState
    {
        public void Enter() { }
        public void Execute(float deltaTime) { }
        public void Leave() { }
    }
}


