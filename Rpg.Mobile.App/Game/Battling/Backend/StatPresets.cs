namespace Rpg.Mobile.App.Game.Battling.Backend;

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
            new(BattleUnitType.Archer,
                maxHealth: 10,
                movement: 5,
                attackMinRange: 2,
                attackMaxRange: 3,
                attack: 10,
                defense: 4));

    public static BattleUnitState Warrior =>
        new(playerId: 0,
            new(BattleUnitType.Warrior,
                maxHealth: 16,
                movement: 3,
                attackMinRange: 1,
                attackMaxRange: 1,
                attack: 11,
                defense: 6));

    public static BattleUnitState Mage =>
        new(playerId: 0,
            new(BattleUnitType.Mage,
                maxHealth: 9,
                movement: 5,
                attackMinRange: 1,
                attackMaxRange: 1,
                attack: 9,
                defense: 5)
            {
                MaxMp = 10
            })
        {
            Spells = new() { SpellState.Fire1 }
        };

    public static BattleUnitState Ninja =>
        new(playerId: 0,
            new(BattleUnitType.Ninja,
                maxHealth: 11,
                movement: 7,
                attackMinRange: 1,
                attackMaxRange: 1,
                attack: 11,
                defense: 6));

    public static BattleUnitState Healer =>
        new(playerId: 0,
            new(BattleUnitType.Healer,
                maxHealth: 8,
                movement: 6,
                attackMinRange: 1,
                attackMaxRange: 2,
                attack: 9,
                defense: 6)
            {
                MaxMp = 12
            })
        {
            Spells = new() { SpellState.Cure1 }
        };
}