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
