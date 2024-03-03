namespace Rpg.Mobile.App.Game.Battling.Calculators;

public class StatPresets
{
    public static BattleUnitState Archer => new(BattleUnitType.Archer, 10, 10, 5, 3, 3, 8, 4);
    public static BattleUnitState Warrior => new(BattleUnitType.Warrior, 16, 16, 3, 1, 1, 9, 6);
    public static BattleUnitState Mage => new(BattleUnitType.Mage, 9, 9, 5, 1, 2, 13, 3);
    public static BattleUnitState Ninja => new(BattleUnitType.Ninja, 11, 11, 7, 1, 1, 10, 5);
    public static BattleUnitState Healer => new(BattleUnitType.Healer, 8, 8, 6,  1, 2, 6, 7);
}

public enum BattleUnitType
{
    Warrior, Healer, Mage, Archer, Ninja
}

public class BattleUnitState
{
    public BattleUnitType UnitType { get; set; } = BattleUnitType.Warrior;
    public int RemainingHealth { get; set; } = 12;
    public int MaxHealth { get; set; } = 12;
    public int Movement { get; set; } = 4;
    public int AttackMinRange { get; set; } = 1;
    public int AttackMaxRange { get; set; } = 2;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 4;

    public BattleUnitState() { }
    public BattleUnitState(
        BattleUnitType unitType,
        int remainingHealth,
        int maxHealth,
        int movement,
        int attackMinRange,
        int attackMaxRange,
        int attack,
        int defense)
    {
        UnitType = unitType;
        RemainingHealth = remainingHealth;
        MaxHealth = maxHealth;
        Movement = movement;
        AttackMinRange = attackMinRange;
        AttackMaxRange = attackMaxRange;
        Attack = attack;
        Defense = defense;
    }
}
