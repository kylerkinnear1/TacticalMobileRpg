using Rpg.Mobile.App.Game.Battling.Systems.Data;

namespace Rpg.Mobile.App.Game.Battling.Systems;

public static class SpellPresets
{
    public static readonly SpellState Fire1 = new(SpellType.Fire1, "Fire", 2, 1, 2, false, true);
    public static readonly SpellState Fire2 = new(SpellType.Fire2, "Fire 2", 6, 1, 2, false, true, 2);
    public static readonly SpellState Cure1 = new(SpellType.Cure1, "Cure", 3, 0, 1, true, false);
}