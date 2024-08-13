using Rpg.Mobile.App.Game.MainBattle.Data;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.SelectUnit;

public class SelectUnitPhase : IBattlePhase
{
    public void Enter() { }

    public void Execute(float deltaTime) { }

    public void Leave() { }

    private void AdvanceToUnit(BattleUnitData unit) => throw new NotImplementedException();
}
