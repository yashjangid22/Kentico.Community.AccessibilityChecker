namespace XperienceCommunity.AccessibilityChecker.Models
{
    public sealed class ScanResultDto
    {
        public required string Url { get; set; }
        public required int Score { get; set; }
        public required DateTime Timestamp { get; set; }
        public required IssuesBySeverityDto IssuesBySeverity { get; set; }
    }
}
