using System.Reflection;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using XperienceCommunity.AccessibilityChecker.Models;
using XperienceCommunity.AccessibilityChecker.Scanning.AxeCore;

namespace XperienceCommunity.AccessibilityChecker.Scanning
{
    /// <summary>
    /// Scans a URL by rendering it in a headless Chromium instance and running axe-core
    /// against the resulting DOM. Holds one shared, lazily-launched <see cref="IBrowser"/>
    /// for the lifetime of the service (Chromium startup is the expensive part); each scan
    /// gets its own short-lived <see cref="IBrowserContext"/> for isolation.
    ///
    /// Scans the main frame only - content inside &lt;iframe&gt; elements is not scanned,
    /// matching the previous client-side scan's behavior.
    /// </summary>
    internal sealed class PlaywrightAccessibilityScanService : IAccessibilityScanService, IAsyncDisposable
    {
        private const float NavigationTimeoutMs = 30_000f;
        private const float NetworkIdleGraceMs = 5_000f;
        private const string AxeCoreResourceName = "XperienceCommunity.AccessibilityChecker.Scanning.axe.min.js";

        private readonly SemaphoreSlim initLock = new(1, 1);
        private readonly ILogger<PlaywrightAccessibilityScanService> logger;
        private readonly IWebHostEnvironment environment;
        private readonly string axeCoreScript;
        private IPlaywright? playwright;
        private IBrowser? browser;

        public PlaywrightAccessibilityScanService(ILogger<PlaywrightAccessibilityScanService> logger, IWebHostEnvironment environment)
        {
            this.logger = logger;
            this.environment = environment;
            axeCoreScript = LoadEmbeddedAxeCoreScript();
        }

        public async Task<AccessibilityScanOutcome> ScanAsync(string rawUrl, CancellationToken cancellationToken = default)
        {
            // Only relax the private/loopback network block when the admin app itself is running
            // locally (see ScanUrlValidator's allowPrivateNetworkTargets doc comment for why this
            // is safe): a developer testing their own localhost site isn't crossing a real trust
            // boundary, but a deployed admin instance allowing internal-network scans would be.
            var validation = await ScanUrlValidator.ValidateAsync(rawUrl, environment.IsDevelopment(), cancellationToken);
            if (!validation.IsValid)
            {
                return AccessibilityScanOutcome.Failure(ScanErrorCode.InvalidUrl, validation.ErrorMessage!);
            }

            IBrowser browserInstance;
            try
            {
                browserInstance = await GetBrowserAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start the headless browser for an accessibility scan.");
                return AccessibilityScanOutcome.Failure(ScanErrorCode.ScanFailed, "Unable to start the headless browser.");
            }

            var url = validation.Uri!.ToString();
            await using var context = await browserInstance.NewContextAsync();
            try
            {
                var page = await context.NewPageAsync();
                page.SetDefaultTimeout(NavigationTimeoutMs);

                try
                {
                    await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = NavigationTimeoutMs });

                    // Best-effort settle for JS-rendered content. Allowed to time out without
                    // failing the whole scan - pages with long-polling/analytics beacons may
                    // never go fully idle.
                    try
                    {
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = NetworkIdleGraceMs });
                    }
                    catch (TimeoutException)
                    {
                        // Proceed with whatever has rendered so far.
                    }
                }
                catch (TimeoutException)
                {
                    return AccessibilityScanOutcome.Failure(ScanErrorCode.Timeout, $"Timed out waiting for '{url}' to load.");
                }
                catch (PlaywrightException ex)
                {
                    return AccessibilityScanOutcome.Failure(ScanErrorCode.UnreachablePage, $"Could not reach '{url}': {ex.Message}");
                }

                AxeRawResult axeResult;
                try
                {
                    await page.AddScriptTagAsync(new PageAddScriptTagOptions { Content = axeCoreScript });
                    var axeJson = await page.EvaluateAsync<string>(
                        "async () => JSON.stringify(await axe.run(document, { runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa'] } }))");
                    axeResult = JsonSerializer.Deserialize<AxeRawResult>(axeJson, JsonOptions)
                        ?? throw new InvalidOperationException("axe-core returned an empty result.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "axe-core scan failed for {Url}", url);
                    return AccessibilityScanOutcome.Failure(ScanErrorCode.ScanFailed, "The accessibility scan failed to run on this page.");
                }

                var issuesBySeverity = SeverityMapper.GroupBySeverity(axeResult.Violations);
                var score = AccessibilityScoreCalculator.Calculate(
                    issuesBySeverity.Critical.Count,
                    issuesBySeverity.Serious.Count,
                    issuesBySeverity.Moderate.Count,
                    issuesBySeverity.Minor.Count);

                return AccessibilityScanOutcome.Success(new ScanResultDto
                {
                    Url = url,
                    Score = score,
                    Timestamp = DateTime.UtcNow,
                    IssuesBySeverity = issuesBySeverity
                });
            }
            finally
            {
                await context.CloseAsync();
            }
        }

        private async Task<IBrowser> GetBrowserAsync(CancellationToken cancellationToken)
        {
            if (browser is { IsConnected: true })
            {
                return browser;
            }

            await initLock.WaitAsync(cancellationToken);
            try
            {
                if (browser is { IsConnected: true })
                {
                    return browser;
                }

                playwright ??= await Playwright.CreateAsync();
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                return browser;
            }
            finally
            {
                initLock.Release();
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private static string LoadEmbeddedAxeCoreScript()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(AxeCoreResourceName)
                ?? throw new InvalidOperationException(
                    $"Embedded axe-core script '{AxeCoreResourceName}' not found. " +
                    "Was 'npm install' run in Client/ before the package was built?");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public async ValueTask DisposeAsync()
        {
            if (browser is not null)
            {
                await browser.CloseAsync();
            }
            playwright?.Dispose();
            initLock.Dispose();
        }
    }
}
