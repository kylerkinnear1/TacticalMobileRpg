using Rpg.Mobile.App.Game.Battling.Gamemaster.Handlers;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Gamemaster;

public enum BattleStep
{
    Setup,
    Moving,
    SelectingSpell,
    SelectingAttackTarget,
    SelectingMagicTarget
}

public class BattleState
{
    public MapState Map { get; set; }

    public int CurrentPlaceOrder { get; set; } = 0;
    public List<BattleUnitState> PlaceOrder { get; set; } = new();
    public List<BattleUnitState> TurnOrder { get; set; } = new();
    public Dictionary<BattleUnitState, Point> UnitCoordinates { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;
    public Point ActiveUnitStartPosition { get; set; } = Point.Empty;

    public BattleStep Step { get; set; } = BattleStep.Setup;
    public SpellState? CurrentSpell { get; set; }

    public BattleUnitState? CurrentUnit => ActiveUnitIndex >= 0 ? TurnOrder[ActiveUnitIndex] : null;
    public List<Point> WalkableTiles { get; set; } = new();
    public List<Point> AttackTargetTiles { get; set; } = new();
    public List<Point> SpellTargetTiles { get; set; } = new();
    public HashSet<int> PlayerRerolls { get; set; } = new();

    public IEnumerable<BattleUnitState> UnitsAt(Point tile) =>
        UnitCoordinates.Where(x => x.Value == tile).Select(x => x.Key);

    public BattleState(MapState map)
    {
        Map = map;

        // TODO: Move out of here
        var team1 = map.Team1.Select(StatPresets.GetStatsByType);
        var team2 = map.Team2.Select(StatPresets.GetStatsByType).ToList();
        foreach (var unit in team2) unit.PlayerId = 1;

        PlaceOrder = team1.Zip(team2).SelectMany(x => new[] { x.First, x.Second }).ToList();
    }
}

public class BattleStateService
{
    private readonly BattleState _state;
    private readonly IPathCalculator _path;
    private readonly StartBattleHandler _startBattle;
    private readonly AdvanceToNextUnitHandler _advanceUnit;
    private readonly ChangeBattleStateHandler _changeStep;
    private readonly SelectTileHandler _selectTile;
    private readonly TargetSpellHandler _targetSpell;
    private readonly ValidTargetCalculator _validTargetCalculator;
    private readonly PlaceUnitHandler _placeUnit;

    private BattleUnitState CurrentUnit => _state.TurnOrder[_state.ActiveUnitIndex];

    public BattleStateService(BattleState state, IPathCalculator path)
    {
        _state = state;
        _path = path;

        _changeStep = new(_state, _path);
        _advanceUnit = new(_state, _changeStep);
        _startBattle = new(_state, _advanceUnit);
        _placeUnit = new(_state, _startBattle);
        _selectTile = new(_state, _path, _placeUnit, _advanceUnit);
        _targetSpell = new(_state, _changeStep);
        
        _validTargetCalculator = new ValidTargetCalculator(_state);
    }

    public void PlaceUnit(Point tile) => _placeUnit.Handle(tile);

    public void AdvanceToNextUnit() => _advanceUnit.Handle();

    public void ChangeBattleState(BattleStep step) => _changeStep.Handle(step);

    public void SelectTile(Point tile) => _selectTile.Handle(tile);

    public void TargetSpell(SpellState spell) => _targetSpell.Handle(spell);

    public bool IsValidMagicTargetTile(Point tile) => _validTargetCalculator.IsValidMagicTargetTile(tile);
    public bool IsValidAttackTargetTile(Point tile) => _validTargetCalculator.IsValidAttackTargetTile(tile);

    public void RerollUnit()
    {
        if (_state.PlayerRerolls.Contains(_state.CurrentUnit.PlayerId))
            return;

        var alreadyGoneUnits = _state.TurnOrder.Where((_, i) => i < _state.ActiveUnitIndex);
        var previousUnit = CurrentUnit;
        var remainingUnits = _state.TurnOrder
            .Where((_, i) => i >= _state.ActiveUnitIndex)
            .Shuffle(Rng.Instance);

        var selection = remainingUnits.First(x => x.PlayerId == _state.CurrentUnit.PlayerId);
        var newTurnOrder = alreadyGoneUnits
            .Append(selection)
            .Concat(remainingUnits
                .Skip(1));
        _state.TurnOrder.Set(newTurnOrder);
        _state.PlayerRerolls.Add(_state.CurrentUnit.PlayerId);

        Bus.Global.Publish(new ActiveUnitChangedEvent(previousUnit, _state.CurrentUnit));
        Bus.Global.Publish(new BattleStepChangedEvent(BattleStep.Moving));
    }
}

public record BattleStartedEvent : IEvent;
public record ActiveUnitChangedEvent(BattleUnitState? PreviousUnit, BattleUnitState NextUnit) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;
public record UnitsDefeatedEvent(IEnumerable<BattleUnitState> Defeated) : IEvent;
public record NotEnoughMpEvent(SpellState Spell) : IEvent;
public record UnitDamagedEvent(List<(BattleUnitState Unit, int Damage)> Hits) : IEvent;
public record UnitMovedEvent(BattleUnitState Unit, Point Tile) : IEvent;
