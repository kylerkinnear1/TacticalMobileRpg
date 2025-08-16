using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.Calculators;

public interface IAttackDamageCalculator
{
    int CalcDamage(int attack, int defense);
}

public class AttackDamageCalculator : IAttackDamageCalculator
{
    public int CalcDamage(int attack, int defense)
    {
        var deterministicDamage = Math.Max(1, attack - defense);
        var damageRangeModifier = Rng.Instance.Double(0.25) * deterministicDamage;

        var damage = deterministicDamage + (int)Math.Round(damageRangeModifier);
        return Math.Max(1, damage);
    }
}
