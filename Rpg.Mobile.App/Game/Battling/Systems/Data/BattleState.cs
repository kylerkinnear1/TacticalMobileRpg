using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Battling.Systems.Data;

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

        var team1 = map.Team1
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitState(0, x));
        var team2 = map.Team2
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitState(1, x));

        PlaceOrder = team1.Zip(team2).SelectMany(x => new[] { x.First, x.Second }).ToList();

        // TODO: workaround
        foreach (var unit in PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Mage))
            unit.Spells = new() { SpellPresets.Fire1, SpellPresets.Fire2 };

        foreach (var unit in PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Healer))
            unit.Spells = new() { SpellPresets.Cure1 };
    }
}

public record BattleStartedEvent : IEvent;
public record ActiveUnitChangedEvent(BattleUnitState? PreviousUnit, BattleUnitState NextUnit) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;
public record UnitsDefeatedEvent(IEnumerable<BattleUnitState> Defeated) : IEvent;
public record NotEnoughMpEvent(SpellState Spell) : IEvent;
public record UnitDamagedEvent(List<(BattleUnitState Unit, int Damage)> Hits) : IEvent;
public record UnitMovedEvent(BattleUnitState Unit, Point Tile) : IEvent;
