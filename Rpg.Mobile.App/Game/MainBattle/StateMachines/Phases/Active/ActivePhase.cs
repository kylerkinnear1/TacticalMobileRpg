using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;

public class ActivePhase : IBattlePhase
{
    public interface IStep : IState
    {
    }

    public record BackClickedEvent : IEvent;
    public record AttackClickedEvent : IEvent;
    public record MagicClickedEvent : IEvent;
    public record WaitClickedEvent : IEvent;
    
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly MenuComponent _menu;
    private readonly IEventBus _bus;

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();

    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<BackClickedEvent>(BackClicked)
        ];
    }

    public void Execute(float deltaTime)
    {
        var currentUnitPosition = _data.UnitCoordinates[_data.CurrentUnit()];
        _mainBattle.CurrentUnitShadow.Shadows.SetSingle(
            new(
                currentUnitPosition.X * MainBattleComponent.TileWidth, 
                currentUnitPosition.Y * MainBattleComponent.TileWidth, 
                MainBattleComponent.TileWidth, 
                MainBattleComponent.TileWidth));
    }

    public void Leave()
    {
        _subscriptions.DisposeAll();
    }

    private void BackClicked(BackClickedEvent evnt)
    {
        var position = _mainBattle.GetPositionForTile(
            _data.Active.ActiveUnitStartPosition, 
            _mainBattle.CurrentUnit.Unit.Bounds.Size);
        _mainBattle.CurrentUnit.MoveTo(
            position, 
            () => _step.Change(new IdleStepClient(
                _menu,
                _bus,
                _data,
                _mainBattle)));
    }
}