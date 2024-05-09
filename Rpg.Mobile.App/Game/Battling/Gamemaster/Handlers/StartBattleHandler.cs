﻿using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster.Handlers;

public class StartBattleHandler
{
    private readonly BattleState _state;
    private readonly AdvanceToNextUnitHandler _nextUnitHandler;

    public StartBattleHandler(BattleState state, AdvanceToNextUnitHandler nextUnitHandler)
    {
        _state = state;
        _nextUnitHandler = nextUnitHandler;
    }

    public void Handle()
    {
        if (_state.ActiveUnitIndex >= 0)
            throw new NotSupportedException("Battle already started.");

        _state.TurnOrder = _state.PlaceOrder.Shuffle(Rng.Instance).ToList();
        Bus.Global.Publish(new BattleStartedEvent());

        _nextUnitHandler.Handle();
    }
}
