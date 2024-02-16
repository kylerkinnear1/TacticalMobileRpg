using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;

public interface IDamageCalculator
{
    int CalcDamage(int attack, int defense);
}

public class DamageCalculator : IDamageCalculator
{
    private readonly IRng _rng;

    public DamageCalculator(IRng rng) => _rng = rng;

    public int CalcDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = _rng.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage - (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }
}