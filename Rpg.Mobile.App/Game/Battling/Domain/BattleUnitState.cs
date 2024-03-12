namespace Rpg.Mobile.App.Game.Battling.Domain;

public enum BattleUnitType
{
    Warrior, Healer, Mage, Archer, Ninja
}

public class BattleUnitState
{
    public int PlayerId { get; set; } = 0;
    public BattleUnitType UnitType { get; set; } = BattleUnitType.Warrior;
    public int RemainingHealth { get; set; } = 12;
    public int MaxHealth { get; set; } = 12;
    public int RemainingMp { get; set; } = 0;
    public int MaxMp { get; set; } = 0;
    public int Movement { get; set; } = 4;
    public int AttackMinRange { get; set; } = 1;
    public int AttackMaxRange { get; set; } = 2;
    public int Attack { get; set; } = 8;
    public int Defense { get; set; } = 4;
    public List<SpellState> Spells { get; set; } = new();

    public BattleUnitState() { }
    public BattleUnitState(
        int playerId,
        BattleUnitType unitType,
        int remainingHealth,
        int maxHealth,
        int movement,
        int attackMinRange,
        int attackMaxRange,
        int attack,
        int defense)
    {
        PlayerId = playerId;
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

public class StatPresets
{
    public static IEnumerable<BattleUnitState> All
    {
        get
        {
            yield return Archer;
            yield return Warrior;
            yield return Mage;
            yield return Ninja;
            yield return Healer;
        }
    }

    public static BattleUnitState Archer =>
        new(playerId: 0, 
            unitType: BattleUnitType.Archer,
            remainingHealth: 10,
            maxHealth: 10,
            movement: 5,
            attackMinRange: 2,
            attackMaxRange: 3,
            attack: 10,
            defense: 4);
    public static BattleUnitState Warrior =>
        new(playerId: 0,
            unitType: BattleUnitType.Warrior,
            remainingHealth: 16,
            maxHealth: 16,
            movement: 3,
            attackMinRange: 1,
            attackMaxRange: 1,
            attack: 11,
            defense: 6);
    public static BattleUnitState Mage =>
        new(playerId: 0, 
            unitType: BattleUnitType.Mage,
            remainingHealth: 9,
            maxHealth: 9,
            movement: 5,
            attackMinRange: 1,
            attackMaxRange: 1,
            attack: 9,
            defense: 5)
        {
            Spells = new() { SpellState.Fire1 },
            RemainingMp = 10,
            MaxMp = 10
        };
    public static BattleUnitState Ninja =>
        new(playerId: 0, 
            unitType: BattleUnitType.Ninja,
            remainingHealth: 11,
            maxHealth: 11,
            movement: 7,
            attackMinRange: 1,
            attackMaxRange: 1,
            attack: 11,
            defense: 6);
    public static BattleUnitState Healer =>
        new(playerId: 0, 
            unitType: BattleUnitType.Healer,
            remainingHealth: 8,
            maxHealth: 8,
            movement: 6,
            attackMinRange: 1,
            attackMaxRange: 2,
            attack: 9,
            defense: 6) 
        { 
            Spells = new() { SpellState.Cure1 },
            RemainingMp = 12,
            MaxMp = 12
        };
}
