using Rpg.Mobile.App.Game.MainBattle.Data;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.SelectUnit;

public class SelectUnitPhase : IBattlePhase
{
    private readonly BattleData _data;

    public SelectUnitPhase(BattleData data) => _data = data;

    public void Enter() { }

    public void Execute(float deltaTime) { }

    public void Leave() { }

    private void AdvanceToUnit(BattleUnitData unit) => throw new NotImplementedException();
}
