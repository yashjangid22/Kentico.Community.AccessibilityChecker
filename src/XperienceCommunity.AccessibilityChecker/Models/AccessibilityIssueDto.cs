namespace XperienceCommunity.AccessibilityChecker.Models
{
    /// <summary>
    /// One violated axe-core rule, aggregated across every element it was found on.
    /// </summary>
    public sealed class AccessibilityIssueDto
    {
        public required string Rule { get; set; }
        public required string Description { get; set; }
        public required int AffectedElementCount { get; set; }
        public required IReadOnlyList<string> Selectors { get; set; }
    }
}
