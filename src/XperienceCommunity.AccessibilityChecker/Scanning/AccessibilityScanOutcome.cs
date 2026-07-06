using XperienceCommunity.AccessibilityChecker.Models;

namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    public enum ScanErrorCode
    {
        InvalidUrl,
        UnreachablePage,
        Timeout,
        AccessRestricted,
        ScanFailed
    }

    /// <summary>
    /// Typed success/failure result for a scan, so callers switch on a known error code
    /// instead of catching exceptions that crossed a service boundary.
    /// </summary>
    public sealed class AccessibilityScanOutcome
    {
        public ScanResultDto? Result { get; private init; }
        public ScanErrorCode? ErrorCode { get; private init; }
        public string? ErrorMessage { get; private init; }

        public bool IsSuccess => Result is not null;

        public static AccessibilityScanOutcome Success(ScanResultDto result) => new() { Result = result };

        public static AccessibilityScanOutcome Failure(ScanErrorCode code, string message) =>
            new() { ErrorCode = code, ErrorMessage = message };
    }
}
