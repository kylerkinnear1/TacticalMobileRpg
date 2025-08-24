using System.Drawing;

namespace Rpg.Mobile.Api.Battles.Data;

public class BattleData
{
    public MapData Map { get; set; } = new();
    public List<BattleUnitData> Units { get; set; } = [];
    public Dictionary<int, Point> UnitCoordinates { get; set; } = new();
    public BattleSetupPhaseData Setup { get; set; } = new();
    public BattleActivePhaseData Active { get; set; } = new();
    public List<BattleUnitType> Team0 { get; set; } = new();
    public List<BattleUnitType> Team1 { get; set; } = new();

    // TODO: Remove
    public List<BattleUnitData> UnitsAt(Point point) =>
        UnitCoordinates
            .Where(x => x.Value == point)
            .Select(x => Units.Single(y => y.UnitId == x.Key))
            .ToList();

    // TODO: Remove
    public BattleUnitData CurrentUnit() =>
        Units.Single(x => x.UnitId == Active.TurnOrderIds[Active.ActiveUnitIndex]);
}

public class BattleSetupPhaseData
{
    public int CurrentPlaceOrderIndex { get; set; } = 0;
    public List<int> PlaceOrderIds { get; set; } = new();
}

public class BattleActivePhaseData
{
    public List<int> TurnOrderIds { get; set; } = new();
    public int ActiveUnitIndex { get; set; } = -1;
    public Point ActiveUnitStartPosition { get; set; } = Point.Empty;
    public List<Point> WalkableTiles { get; set; } = new();
    public List<Point> AttackTargetTiles { get; set; } = new();
    public List<Point> SpellTargetTiles { get; set; } = new();

    // TODO: Don't track current spell this way.
    public SpellData? CurrentSpell { get; set; }
}