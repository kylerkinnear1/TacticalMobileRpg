using System.Drawing;
using Rpg.Mobile.Api.Battles.Data;

namespace Rpg.Mobile.Api.Battles;

public class UnitsDamagedData
{
    public List<DamageAssignmentData> DamagedUnits { get; set; } = [];
    public List<BattleUnitData> DefeatedUnits { get; set; } = [];
    public List<int> ActiveTurnOrderIds { get; set; } = [];
    public Dictionary<int, Point> UnitCoordinates { get; set; } = new();
    public int ActiveActiveUnitIndex { get; set; }
    public int RemainingMp { get; set; }
}

public class DamageAssignmentData
{
    public int UnitId { get; set; }
    public int Damage { get; set; }
    public int RemainingHealth { get; set; }
}