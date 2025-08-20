using System.Drawing;

namespace Rpg.Mobile.Api.Battles.Data;

public class BattleData
{
    public MapData Map { get; set; } = new();
    public Dictionary<BattleUnitData, Point> UnitCoordinates { get; set; } = new();
    public BattleSetupPhaseData Setup { get; set; } = new();
    public BattleActivePhaseData Active { get; set; } = new();

    // TODO: Remove
    public List<BattleUnitData> UnitsAt(Point point) =>
        UnitCoordinates.Where(x => x.Value == point).Select(x => x.Key).ToList();

    // TODO: Remove
    public BattleUnitData CurrentUnit() =>
        Active.TurnOrder[Active.ActiveUnitIndex];
    
    // TODO: Remove
    public static BattleData FromMap(MapData map)
    {
        var data = new BattleData();
        data.Map = map;

        var team1 = map.Team1
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitData { PlayerId = 0, Stats = x});
        var team2 = map.Team2
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitData { PlayerId = 1, Stats = x});

        data.Setup.PlaceOrder = team1.Zip(team2).SelectMany(x => new[] { x.First, x.Second }).ToList();

        // TODO: workaround
        foreach (var unit in data.Setup.PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Mage))
            unit.Spells = new() { SpellPresets.Fire1, SpellPresets.Fire2 };

        foreach (var unit in data.Setup.PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Healer))
            unit.Spells = new() { SpellPresets.Cure1 };

        return data;
    }
}

public class BattleSetupPhaseData
{
    public int CurrentPlaceOrder { get; set; } = 0;
    public List<BattleUnitData> PlaceOrder { get; set; } = new();
}

public class BattleActivePhaseData
{
    public List<BattleUnitData> TurnOrder { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;
    public Point ActiveUnitStartPosition { get; set; } = Point.Empty;
    public List<Point> WalkableTiles { get; set; } = new();
    public List<Point> AttackTargetTiles { get; set; } = new();
    public List<Point> SpellTargetTiles { get; set; } = new();
    
    // TODO: Don't track current spell this way.
    public SpellData? CurrentSpell { get; set; }
}