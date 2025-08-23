namespace Rpg.Mobile.Api.Battles.Data;

public enum SpellType
{
    Fire1,
    Fire2,
    Cure1
}

public class SpellData
{
    public SpellType Type { get; set; }
    public string Name { get; set; } = "";
    public int MpCost { get; set; }
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
    public bool TargetsFriendlies { get; set; }
    public bool TargetsEnemies { get; set; }
    public int Aoe { get; set; } = 1;
}