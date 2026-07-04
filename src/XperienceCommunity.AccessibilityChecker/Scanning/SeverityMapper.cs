using XperienceCommunity.AccessibilityChecker.Models;
using XperienceCommunity.AccessibilityChecker.Scanning.AxeCore;

namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    /// <summary>
    /// Groups raw axe-core violations into the four severity buckets, collapsing each
    /// violated rule into a single issue entry (one per rule, not one per affected element).
    /// </summary>
    internal static class SeverityMapper
    {
        public static IssuesBySeverityDto GroupBySeverity(IEnumerable<AxeRawViolation> violations)
        {
            var critical = new List<AccessibilityIssueDto>();
            var serious = new List<AccessibilityIssueDto>();
            var moderate = new List<AccessibilityIssueDto>();
            var minor = new List<AccessibilityIssueDto>();

            foreach (var violation in violations)
            {
                var issue = new AccessibilityIssueDto
                {
                    Rule = violation.Id,
                    Description = violation.Description,
                    AffectedElementCount = violation.Nodes.Count,
                    Selectors = violation.Nodes
                        .Select(node => node.Target.Count > 0 ? string.Join(" ", node.Target) : "")
                        .Where(selector => selector.Length > 0)
                        .ToList()
                };

                // Unrecognized/missing impact values are bucketed as minor, the least
                // score-impactful bucket, rather than dropped or treated as critical.
                var bucket = violation.Impact switch
                {
                    "critical" => critical,
                    "serious" => serious,
                    "moderate" => moderate,
                    _ => minor
                };
                bucket.Add(issue);
            }

            return new IssuesBySeverityDto
            {
                Critical = critical,
                Serious = serious,
                Moderate = moderate,
                Minor = minor
            };
        }
    }
}
