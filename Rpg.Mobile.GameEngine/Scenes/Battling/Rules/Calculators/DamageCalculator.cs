using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;

public record Attack(
    Unit Attacker,
    Unit Defender);

public record Damage(int Stamina);

public interface IDamageCalculator
{
    Damage Calc(Attack attack);
}

public class DamageCalculator : IDamageCalculator
{
    private readonly IRng _rng;

    public DamageCalculator(IRng rng) => _rng = rng;

    public Damage Calc(Attack attack)
    {
        // Shining Force Algorithm
        var (attacker, defender) = attack;
        var deterministicDamage = Math.Max(1, attacker.Stats.Current.Attack - defender.Stats.Current.Defense);
        var damageRangeModifier = _rng.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage - (int)Math.Round(damageRangeModifier);
        return new(Math.Max(1, damage));
    }
}