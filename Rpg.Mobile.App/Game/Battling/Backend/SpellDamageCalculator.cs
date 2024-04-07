using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.App.Game.Battling.Backend;

public interface ISpellCalculator
{
    int CalcDamage(SpellState spell);
}

public class SpellDamageCalculator : ISpellCalculator
{
    private readonly IRng _rng;

    public SpellDamageCalculator(IRng rng) => _rng = rng;

    public int CalcDamage(SpellState spell) =>
        spell.Type switch
        {
            SpellType.Fire1 => _rng.Int(6, 8),
            SpellType.Cure1 => -6,
            _ => throw new ArgumentException()
        };
}