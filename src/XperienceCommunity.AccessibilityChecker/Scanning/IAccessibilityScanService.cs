namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    public interface IAccessibilityScanService
    {
        public Task<AccessibilityScanOutcome> ScanAsync(string rawUrl, CancellationToken cancellationToken = default);
    }
}
