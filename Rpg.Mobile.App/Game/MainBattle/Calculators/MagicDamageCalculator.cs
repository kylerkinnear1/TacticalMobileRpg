using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.Calculators;

public interface IMagicDamageCalculator
{
    int CalcDamage(SpellData spell);
}

public class MagicDamageCalculator : IMagicDamageCalculator
{
    public int CalcDamage(SpellData spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => Rng.Instance.Int(6, 8),
            SpellType.Fire2 => Rng.Instance.Int(7, 9),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };
}