namespace Rpg.Mobile.App.Game.Battling.Calculators;

public class StatPresets
{
    public static BattleUnitState Archer => new(10, 10, 5, 3, 3, 8, 4);
    public static BattleUnitState Warrior => new(16, 16, 3, 1, 1, 9, 6);
    public static BattleUnitState Mage => new(9, 9, 5, 1, 2, 13, 3);
    public static BattleUnitState Ninja => new(11, 11, 7, 1, 1, 10, 5);
    public static BattleUnitState Healer => new(8, 8, 6,  1, 2, 6, 7);
}

public class BattleUnitState
{
    public int RemainingHealth { get; set; } = 12;
    public int MaxHealth { get; set; } = 12;
    public int Movement { get; set; } = 4;
    public int AttackMinRange { get; set; } = 1;
    public int AttackMaxRange { get; set; } = 2;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 4;

    public BattleUnitState() { }
    public BattleUnitState(
        int remainingHealth,
        int maxHealth,
        int movement,
        int attackMinRange,
        int attackMaxRange,
        int attack,
        int defense)
    {
        RemainingHealth = remainingHealth;
        MaxHealth = maxHealth;
        Movement = movement;
        AttackMinRange = attackMinRange;
        AttackMaxRange = attackMaxRange;
        Attack = attack;
        Defense = defense;
    }
}
