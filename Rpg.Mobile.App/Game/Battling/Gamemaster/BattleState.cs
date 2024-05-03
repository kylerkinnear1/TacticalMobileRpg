using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster;

public enum BattleStep
{
    Setup,
    Moving,
    CastingSpell,
    SelectingAttackTarget
}

public class BattleState
{
    public MapState Map { get; set; }

    public List<BattleUnitState> TurnOrder { get; set; } = new();
    public Dictionary<BattleUnitState, Point> UnitCoordinates { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;

    public BattleStep Step { get; set; } = BattleStep.Setup;
    public BattleUnitState? CurrentUnit => ActiveUnitIndex >= 0 ? TurnOrder[ActiveUnitIndex] : null;

    public BattleState(MapState map)
    {
        Map = map;
    }
}

public class BattleStateService
{
    private readonly BattleState _state;

    public BattleStateService(BattleState state) => _state = state;

    public void StartBattle()
    {
        if (_state.ActiveUnitIndex >= 0)
            throw new NotSupportedException("Battle already started.");

        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(_state.Map.Player1Origins))
            _state.UnitCoordinates[unit] = point;

        foreach (var (unit, point) in player2Units.Zip(_state.Map.Player2Origins))
            _state.UnitCoordinates[unit] = point;

        var allUnits = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        _state.TurnOrder.AddRange(allUnits);

        Bus.Global.Publish(new BattleStartedEvent());
    }
}

public record BattleStartedEvent : IEvent
{

}
