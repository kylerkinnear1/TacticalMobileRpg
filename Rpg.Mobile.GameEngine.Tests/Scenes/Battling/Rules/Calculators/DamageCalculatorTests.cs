using Moq;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Calculators;
using Rpg.Mobile.GameSdk;
using Xunit;

namespace Rpg.Mobile.GameEngine.Tests.Scenes.Battling.Rules.Calculators;

public class DamageCalculatorTests
{
    private readonly Mock<IRng> _rng = new();
    private readonly DamageCalculator _calculator;

    public DamageCalculatorTests() => _calculator = new(_rng.Object);

    [Fact]
    public void DamageCalculator_LessAttackThanDefense_DoesOneDamage()
    {
        // TODO: Autofixture, overrides for records.
    }
}
