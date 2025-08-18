namespace Rpg.Mobile.Api;

public enum BattleUnitType
{
    Warrior, Healer, Mage, Archer, Ninja
}

public class BattleUnitData
{
    public int PlayerId { get; set; } = 0;

    public int RemainingHealth { get; set; } = 12;
    public int RemainingMp { get; set; } = 0;
    public BattleUnitStats Stats { get; set; } = new();
    public List<SpellData> Spells { get; set; } = new();
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
}