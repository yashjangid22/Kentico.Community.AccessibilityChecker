using XperienceCommunity.AccessibilityChecker.Scanning;

namespace XperienceCommunity.AccessibilityChecker.Tests;

public class AccessibilityScoreCalculatorTests
{
    [Test]
    public void Calculate_NoIssues_ReturnsFullScore()
    {
        Assert.That(AccessibilityScoreCalculator.Calculate(0, 0, 0, 0), Is.EqualTo(100));
    }

    [Test]
    public void Calculate_MixOfAllSeverities_DeductsWeightedTotal()
    {
        // 1 critical (-10) + 1 serious (-7) + 1 moderate (-3) + 1 minor (-1) = 79
        Assert.That(AccessibilityScoreCalculator.Calculate(1, 1, 1, 1), Is.EqualTo(79));
    }

    [Test]
    public void Calculate_LargeDeduction_ClampsAtZero()
    {
        Assert.That(AccessibilityScoreCalculator.Calculate(criticalRuleCount: 20, seriousRuleCount: 0, moderateRuleCount: 0, minorRuleCount: 0), Is.EqualTo(0));
    }
}
