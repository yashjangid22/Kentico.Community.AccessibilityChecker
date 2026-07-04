namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    public interface IAccessibilityScanService
    {
        Task<AccessibilityScanOutcome> ScanAsync(string rawUrl, CancellationToken cancellationToken = default);
    }
}
