using System.Net;

using XperienceCommunity.AccessibilityChecker.Scanning;

namespace XperienceCommunity.AccessibilityChecker.Tests;

public class ScanUrlValidatorTests
{
    [Test]
    public async Task ValidateAsync_PublicIpLiteral_IsValid()
    {
        // A public IP literal is used (rather than a hostname) so this test needs no real
        // DNS resolution and stays deterministic in CI.
        var result = await ScanUrlValidator.ValidateAsync("http://8.8.8.8/page");

        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not-a-url")]
    [TestCase("/relative/path")]
    [TestCase("ftp://example.com")]
    [TestCase("javascript:alert(1)")]
    public async Task ValidateAsync_MalformedOrNonHttpInput_IsInvalid(string rawUrl)
    {
        var result = await ScanUrlValidator.ValidateAsync(rawUrl);

        Assert.That(result.IsValid, Is.False);
    }

    [TestCase("http://127.0.0.1/")]
    [TestCase("http://localhost/")]
    [TestCase("http://10.0.0.5/")]
    [TestCase("http://172.16.0.1/")]
    [TestCase("http://192.168.1.1/")]
    [TestCase("http://169.254.1.1/")]
    public async Task ValidateAsync_PrivateOrLoopbackHost_IsRejected(string rawUrl)
    {
        var result = await ScanUrlValidator.ValidateAsync(rawUrl);

        Assert.That(result.IsValid, Is.False);
    }

    [TestCase("http://127.0.0.1/")]
    [TestCase("http://localhost/")]
    [TestCase("http://10.0.0.5/")]
    [TestCase("http://192.168.1.1/")]
    public async Task ValidateAsync_PrivateOrLoopbackHost_WithAllowPrivateNetworkTargets_IsAllowed(string rawUrl)
    {
        var result = await ScanUrlValidator.ValidateAsync(rawUrl, allowPrivateNetworkTargets: true);

        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("127.0.0.1", true)]
    [TestCase("10.1.2.3", true)]
    [TestCase("172.16.0.1", true)]
    [TestCase("172.31.255.255", true)]
    [TestCase("172.32.0.1", false)]
    [TestCase("192.168.0.1", true)]
    [TestCase("169.254.0.1", true)]
    [TestCase("8.8.8.8", false)]
    [TestCase("1.1.1.1", false)]
    public void IsPrivateOrLoopback_CoversKnownRanges(string ip, bool expected)
    {
        var address = IPAddress.Parse(ip);

        Assert.That(ScanUrlValidator.IsPrivateOrLoopback(address), Is.EqualTo(expected));
    }
}
