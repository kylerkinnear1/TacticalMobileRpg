namespace Rpg.Mobile.App.Game.MainBattle.Data;

public class BattleData
{
    public MapData Map { get; set; }

    public int CurrentPlaceOrder { get; set; } = 0;
    public List<BattleUnitData> PlaceOrder { get; set; } = new();
    public List<BattleUnitData> TurnOrder { get; set; } = new();
    public Dictionary<BattleUnitData, Point> UnitCoordinates { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;
    public Point ActiveUnitStartPosition { get; set; } = Point.Empty;

    public SpellData? CurrentSpell { get; set; }

    public BattleUnitData CurrentUnit => ActiveUnitIndex >= 0 
        ? TurnOrder[ActiveUnitIndex] 
        : throw new NotImplementedException("TODO: Split up battle data.");

    public List<Point> WalkableTiles { get; set; } = new();
    public List<Point> AttackTargetTiles { get; set; } = new();
    public List<Point> SpellTargetTiles { get; set; } = new();

    public IEnumerable<BattleUnitData> UnitsAt(Point tile) =>
        UnitCoordinates.Where(x => x.Value == tile).Select(x => x.Key);

    public BattleData(MapData map)
    {
        Map = map;

        var team1 = map.Team1
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitData(0, x));
        var team2 = map.Team2
            .Select(x => map.BaseStats.Single(y => x == y.UnitType))
            .Select(x => new BattleUnitData(1, x));

        PlaceOrder = team1.Zip(team2).SelectMany(x => new[] { x.First, x.Second }).ToList();

        // TODO: workaround
        foreach (var unit in PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Mage))
            unit.Spells = new() { SpellPresets.Fire1, SpellPresets.Fire2 };

        foreach (var unit in PlaceOrder.Where(x => x.Stats.UnitType == BattleUnitType.Healer))
            unit.Spells = new() { SpellPresets.Cure1 };
    }
}

