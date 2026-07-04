namespace XperienceCommunity.AccessibilityChecker.Models
{
    public sealed class IssuesBySeverityDto
    {
        public IReadOnlyList<AccessibilityIssueDto> Critical { get; set; } = [];
        public IReadOnlyList<AccessibilityIssueDto> Serious { get; set; } = [];
        public IReadOnlyList<AccessibilityIssueDto> Moderate { get; set; } = [];
        public IReadOnlyList<AccessibilityIssueDto> Minor { get; set; } = [];
    }
}
