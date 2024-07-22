using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active.Steps;

public class ActiveStepStateMachine : StateMachine<IActivePhaseStepEvent>
{
}

public interface IActivePhaseStepEvent : IState { }