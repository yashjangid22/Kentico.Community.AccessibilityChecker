using XperienceCommunity.AccessibilityChecker.Scanning;
using XperienceCommunity.AccessibilityChecker.Scanning.AxeCore;

namespace XperienceCommunity.AccessibilityChecker.Tests;

public class SeverityMapperTests
{
    [Test]
    public void GroupBySeverity_EmptyList_ReturnsAllEmptyBuckets()
    {
        var result = SeverityMapper.GroupBySeverity([]);

        Assert.That(result.Critical, Is.Empty);
        Assert.That(result.Serious, Is.Empty);
        Assert.That(result.Moderate, Is.Empty);
        Assert.That(result.Minor, Is.Empty);
    }

    [Test]
    public void GroupBySeverity_RuleWithMultipleNodes_FlattensToOneIssuePerRule()
    {
        var violation = new AxeRawViolation
        {
            Id = "image-alt",
            Impact = "critical",
            Description = "Images must have alternate text",
            Nodes =
            [
                new AxeRawNode { Target = ["img.logo"] },
                new AxeRawNode { Target = ["img.hero"] },
            ]
        };

        var result = SeverityMapper.GroupBySeverity([violation]);

        Assert.That(result.Critical, Has.Count.EqualTo(1));
        var issue = result.Critical[0];
        Assert.That(issue.Rule, Is.EqualTo("image-alt"));
        Assert.That(issue.AffectedElementCount, Is.EqualTo(2));
        Assert.That(issue.Selectors, Is.EqualTo(new[] { "img.logo", "img.hero" }));
    }

    [TestCase("critical")]
    [TestCase("serious")]
    [TestCase("moderate")]
    [TestCase("minor")]
    public void GroupBySeverity_KnownImpact_GoesToMatchingBucket(string impact)
    {
        var violation = new AxeRawViolation { Id = "rule", Impact = impact, Description = "d", Nodes = [new AxeRawNode { Target = ["x"] }] };

        var result = SeverityMapper.GroupBySeverity([violation]);

        var bucket = impact switch
        {
            "critical" => result.Critical,
            "serious" => result.Serious,
            "moderate" => result.Moderate,
            _ => result.Minor
        };
        Assert.That(bucket, Has.Count.EqualTo(1));
    }

    [Test]
    public void GroupBySeverity_UnrecognizedImpact_FallsBackToMinor()
    {
        var violation = new AxeRawViolation { Id = "rule", Impact = "unexpected-value", Description = "d", Nodes = [] };

        var result = SeverityMapper.GroupBySeverity([violation]);

        Assert.That(result.Minor, Has.Count.EqualTo(1));
        Assert.That(result.Critical, Is.Empty);
        Assert.That(result.Serious, Is.Empty);
        Assert.That(result.Moderate, Is.Empty);
    }
}
