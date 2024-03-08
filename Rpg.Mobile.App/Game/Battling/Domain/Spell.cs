namespace Rpg.Mobile.App.Game.Battling.Domain;

public enum SpellType
{
    Fire1, Cure1
}

public record SpellState(SpellType Type, string Name, int MpCost, int MinRange, int MaxRange, bool TargetsFriendlies, bool TargetsEnemies)
{
    public static readonly SpellState Fire1 = new(SpellType.Fire1, "Fire", 2, 1, 2, false, true);
    public static readonly SpellState Cure1 = new(SpellType.Cure1, "Cure", 3, 1, 1, true, false);

    public static SpellState FromType(SpellType type) =>
        type switch
        {
            SpellType.Fire1 => Fire1,
            SpellType.Cure1 => Cure1,
            _ => throw new ArgumentException()
        };
};
