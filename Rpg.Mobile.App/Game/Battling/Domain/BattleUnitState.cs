namespace Rpg.Mobile.App.Game.Battling.Domain;

public enum BattleUnitType
{
    Warrior, Healer, Mage, Archer, Ninja
}

public class BattleUnitState
{
    public int PlayerId { get; set; } = 0;
    
    public int RemainingHealth { get; set; } = 12;
    public int RemainingMp { get; set; } = 0;
    public BattleUnitStats Stats { get; set; } = new();
    public List<SpellState> Spells { get; set; } = new();

    public BattleUnitState() { }
    public BattleUnitState(
        int playerId,
        BattleUnitStats stats)
    {
        PlayerId = playerId;
        Stats = stats;
        RemainingHealth = stats.MaxHealth;
        RemainingMp = stats.MaxMp;
    }
}

public class BattleUnitStats
{
    public BattleUnitType UnitType { get; set; } = BattleUnitType.Warrior;
    public int MaxHealth { get; set; } = 12;
    public int MaxMp { get; set; } = 0;
    public int Movement { get; set; } = 4;
    public int AttackMinRange { get; set; } = 1;
    public int AttackMaxRange { get; set; } = 2;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 4;

    public BattleUnitStats() { }
    public BattleUnitStats(
        BattleUnitType unitType,
        int maxHealth,
        int movement,
        int attackMinRange,
        int attackMaxRange,
        int attack,
        int defense)
    {
        UnitType = unitType;
        MaxHealth = maxHealth;
        Movement = movement;
        AttackMinRange = attackMinRange;
        AttackMaxRange = attackMaxRange;
        Attack = attack;
        Defense = defense;
    }
}