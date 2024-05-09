using Rpg.Mobile.GameSdk;

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

        var player1Units = StatPresets.All.Append(StatPresets.Warrior).Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Append(StatPresets.Warrior).Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(_state.Map.Player1Origins))
            _state.UnitCoordinates[unit] = point;

        foreach (var (unit, point) in player2Units.Zip(_state.Map.Player2Origins))
            _state.UnitCoordinates[unit] = point;

        _state.TurnOrder = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        Bus.Global.Publish(new BattleStartedEvent());

        _nextUnitHandler.Handle();
    }
}
