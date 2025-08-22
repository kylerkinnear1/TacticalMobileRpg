using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public class BattlePhaseMachine
{
    private readonly StateMachine<IBattlePhase> _phase = new();
}