namespace Rpg.Mobile.Api.Battles.Data;

public static class SpellPresets
{
    public static readonly SpellData Fire1 = new()
    {
        Type = SpellType.Fire1,
        Name = "Fire",
        MpCost = 2,
        MinRange = 1,
        MaxRange = 2,
        TargetsFriendlies = false,
        TargetsEnemies = true,
        Aoe = 1
    };

    public static readonly SpellData Fire2 = new()
    {
        Type = SpellType.Fire2,
        Name = "Fire 2",
        MpCost = 6,
        MinRange = 1,
        MaxRange = 2,
        TargetsFriendlies = false,
        TargetsEnemies = true,
        Aoe = 2
    };

    public static readonly SpellData Cure1 = new()
    {
        Type = SpellType.Cure1,
        Name = "Cure",
        MpCost = 3,
        MinRange = 0,
        MaxRange = 1,
        TargetsFriendlies = true,
        TargetsEnemies = false,
        Aoe = 1
    };
}