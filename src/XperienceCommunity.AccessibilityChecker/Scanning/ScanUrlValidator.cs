using System.Net;
using System.Net.Sockets;

namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    internal sealed class UrlValidationResult
    {
        public bool IsValid { get; private init; }
        public Uri? Uri { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static UrlValidationResult Valid(Uri uri) => new() { IsValid = true, Uri = uri };
        public static UrlValidationResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
    }

    /// <summary>
    /// Validates a user-supplied scan URL: must be an absolute http(s) URL, and must not
    /// resolve to a loopback/private/link-local address (basic SSRF safeguard). Does not
    /// protect against a page later redirecting to an internal address - only the initial
    /// host is checked.
    /// </summary>
    internal static class ScanUrlValidator
    {
        /// <param name="rawUrl">The URL to validate, as entered by the user.</param>
        /// <param name="allowPrivateNetworkTargets">
        /// Skips the loopback/private/link-local rejection when true. Intended only for when the
        /// admin application itself is running in a trusted local development context (see
        /// IWebHostEnvironment.IsDevelopment() at the call site) - a developer scanning their own
        /// localhost isn't crossing a real trust boundary, but a deployed admin instance allowing
        /// scans of its own internal network would be a real SSRF hole.
        /// </param>
        /// <param name="cancellationToken">Cancellation token for the DNS resolution step.</param>
        public static async Task<UrlValidationResult> ValidateAsync(string? rawUrl, bool allowPrivateNetworkTargets = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return UrlValidationResult.Invalid("URL is required.");
            }

            if (!Uri.TryCreate(rawUrl.Trim(), UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return UrlValidationResult.Invalid("Enter a valid absolute http:// or https:// URL.");
            }

            IPAddress[] addresses;
            try
            {
                addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
            }
            catch (Exception ex) when (ex is SocketException or ArgumentException)
            {
                return UrlValidationResult.Invalid($"Could not resolve host '{uri.Host}'.");
            }

            if (addresses.Length == 0)
            {
                return UrlValidationResult.Invalid($"Could not resolve host '{uri.Host}'.");
            }

            if (!allowPrivateNetworkTargets && Array.Exists(addresses, IsPrivateOrLoopback))
            {
                return UrlValidationResult.Invalid("Scanning internal/private network addresses is not allowed.");
            }

            return UrlValidationResult.Valid(uri);
        }

        internal static bool IsPrivateOrLoopback(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            var bytes = address.GetAddressBytes();

            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                if (bytes[0] == 10) return true; // 10.0.0.0/8
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true; // 172.16.0.0/12
                if (bytes[0] == 192 && bytes[1] == 168) return true; // 192.168.0.0/16
                if (bytes[0] == 169 && bytes[1] == 254) return true; // 169.254.0.0/16 (link-local)
                return false;
            }

            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (address.IsIPv6LinkLocal) return true;
                if ((bytes[0] & 0xFE) == 0xFC) return true; // fc00::/7 (unique local)
                return false;
            }

            return false;
        }
    }
}
