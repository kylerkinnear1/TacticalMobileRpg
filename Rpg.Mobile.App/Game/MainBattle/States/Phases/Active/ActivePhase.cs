using Rpg.Mobile.App.Game.MainBattle.States.Phases.Active.Steps;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active;

public class ActivePhase(BattlePhaseMachine.Context _context) : IBattlePhase
{
    public interface IStep : IState { }

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();

    public void Enter()
    {
        _subscriptions =
        [
            Bus.Global.Subscribe<MagicClickedEvent>(_ => _step.Change(new SelectingSpellStep(_context))),
            Bus.Global.Subscribe<BackClickedEvent>(_ => throw new NotImplementedException())
        ];
        
        _step.Change(new IdleStep(_context));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave() => _subscriptions.DisposeAll();

}

public record BackClickedEvent : IEvent;
