namespace Rpg.Mobile.App.Game.MainBattle.Data;

public enum SpellType
{
    Fire1,
    Fire2,
    Cure1
}

public record SpellData(
    SpellType Type,
    string Name,
    int MpCost,
    int MinRange,
    int MaxRange,
    bool TargetsFriendlies,
    bool TargetsEnemies,
    int Aoe = 1);