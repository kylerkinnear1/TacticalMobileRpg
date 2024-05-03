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

    public SpellState? CurrentSpell { get; set; }

    public BattleState(MapState map)
    {
        Map = map;
    }
}

public class BattleStateService
{
    private readonly BattleState _state;
    private BattleUnitState CurrentUnit => _state.TurnOrder[_state.ActiveUnitIndex];

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

        _state.TurnOrder = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        Bus.Global.Publish(new BattleStartedEvent());
        AdvanceToNextUnit();
    }

    public void AdvanceToNextUnit()
    {
        var isLastUnit = _state.ActiveUnitIndex + 1 >= _state.TurnOrder.Count;
        _state.ActiveUnitIndex = isLastUnit ? _state.ActiveUnitIndex + 1 : 0;
        _state.TurnOrder = _state.TurnOrder.Shuffle(Rng.Instance).ToList();

        Bus.Global.Publish(new ActiveUnitChangedEvent(CurrentUnit));

        ChangeBattleState(BattleStep.Moving);
    }

    public void ChangeBattleState(BattleStep step)
    {
        _state.Step = step;
        _state.CurrentSpell = null;

        Bus.Global.Publish(new BattleStepChangedEvent(step));
    }
}

public record BattleStartedEvent : IEvent;
public record ActiveUnitChangedEvent(BattleUnitState State) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;