using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.System.Handlers;

public class AdvanceToNextUnitHandler
{
    private readonly BattleState _state;
    private readonly ChangeBattleStateHandler _changeStateHandler;

    public AdvanceToNextUnitHandler(BattleState state, ChangeBattleStateHandler changeStateHandler)
    {
        _state = state;
        _changeStateHandler = changeStateHandler;
    }

    public void Handle()
    {
        var previousUnit = _state.ActiveUnitIndex >= 0 ? _state.CurrentUnit : null;
        var isLastUnit = _state.ActiveUnitIndex + 1 >= _state.TurnOrder.Count;
        _state.ActiveUnitIndex = !isLastUnit ? _state.ActiveUnitIndex + 1 : 0;
        if (isLastUnit)
            _state.TurnOrder.Set(_state.TurnOrder.Shuffle(Rng.Instance).ToList());

        _state.ActiveUnitStartPosition = _state.UnitCoordinates[_state.CurrentUnit];
        _state.PlayerRerolls.Clear();

        Bus.Global.Publish(new ActiveUnitChangedEvent(previousUnit, _state.CurrentUnit));

        _changeStateHandler.Handle(BattleStep.Moving);
    }
}