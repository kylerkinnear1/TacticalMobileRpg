using Rpg.Mobile.App.Game.Battling.Systems.Data;

namespace Rpg.Mobile.App.Game.Battling.Systems.Handlers;

public class TargetSpellHandler
{
    private readonly BattleState _state;
    private readonly ChangeBattleStateHandler _changeState;

    public TargetSpellHandler(BattleState state, ChangeBattleStateHandler changeState)
    {
        _state = state;
        _changeState = changeState;
    }

    public void Handle(SpellState spell)
    {
        _state.CurrentSpell = spell;
        _changeState.Handle(BattleStep.SelectingMagicTarget);
    }
}
