using Rpg.Mobile.App.Game.Battling.Systems.Data;

namespace Rpg.Mobile.App.Game.Battling.Systems.Handlers;

public class TargetSpellHandler
{
    private readonly BattleData _state;
    private readonly ChangeBattleStateHandler _changeState;

    public TargetSpellHandler(BattleData state, ChangeBattleStateHandler changeState)
    {
        _state = state;
        _changeState = changeState;
    }

    public void Handle(SpellData spell)
    {
        _state.CurrentSpell = spell;
        _changeState.Handle(BattleStep.SelectingMagicTarget);
    }
}
