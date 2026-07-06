# Usage Guide

Full installation and usage instructions for the Accessibility Checker. For a quick overview and
screenshots, see the [README](../README.md) first — this guide goes into more detail on setup,
day-to-day usage, and troubleshooting.

## 1. Installation

### 1.1 Add the package

```powershell
dotnet add package XperienceCommunity.AccessibilityChecker
```

### 1.2 Register the services

In `Program.cs`, before `builder.Build()`:

```csharp
builder.Services.AddAccessibilityChecker();
```

This registers the API controller and the scanning/persistence services the admin UI talks to.
Without this call, the admin page will load but every action on it will fail. Nothing else needs
to be registered manually — the admin application tile, its page, and the API routes are
discovered automatically once the package is referenced (Xperience by Kentico scans referenced
assemblies for admin modules on startup).

### 1.3 The headless browser installs itself automatically

Scanning is done by rendering the target page in a real, headless Chromium browser
([Playwright for .NET](https://playwright.dev/dotnet/)), so axe-core can inspect the fully
rendered DOM — including anything added by JavaScript. Playwright's NuGet package does **not**
bundle the browser binary, but you don't need to install it manually: the first time you click
**Scan** on a machine that doesn't have Chromium yet, the package detects that and downloads it
automatically (about 180MB, roughly a minute depending on your connection), then completes that
same scan once it's ready. Every scan after that is fast, and this only happens once per
machine/deployment target — you'll see a log line ("Chromium is not installed yet...") the one
time it happens.

If you'd rather not wait on the first scan (e.g. to warm up a new deployment ahead of time), you
can still trigger the same download manually after your first build:

```powershell
pwsh <your-app>/bin/Debug/<your-target-framework>/playwright.ps1 install chromium
```

This is entirely optional now — it's the same download the app performs automatically, just done
ahead of time instead of on first use.

### 1.4 Verify it's installed

Run your application, sign in to the admin (`/admin`), and look for an **Accessibility checker**
tile under the **Development** section on the dashboard:

![Accessibility checker tile in the admin Development section](./images/admin-development-section.png)

If it's not there, double-check step 1.2 — a missing `AddAccessibilityChecker()` call is the most
common reason the admin page doesn't fully work (the tile itself is driven by assembly discovery
and will still normally appear, but the page will fail once you try to use it).

## 2. Using the Accessibility Checker

Open the **Accessibility checker** tile to get to the scan page. Before you've run anything, you'll
see an empty state:

![Empty state before any scan has been run](./images/empty-state.png)

### 2.1 Running a scan

Paste an absolute URL (e.g. `https://yoursite.com/about-us`, or `http://localhost:5001/`) into the
**URL to scan** field and click **Scan**. The URL must:

- be absolute (include the `http://` or `https://` scheme — a relative path won't work)
- be reachable from the server your Xperience application runs on (not just from your own browser)

A new card appears immediately with a loading spinner, then fills in with results once the scan
completes (this can take a few seconds — the server is loading the whole page in a real browser
before scanning it).

### 2.2 Reading a result card

![A scan result card showing a score, a severity group, and its action buttons](./images/scan-result-card.png)

- **Headline** — the URL that was scanned, and when it was last scanned.
- **Score** — 0–100. Deducted per *rule* the page fails (not per element), weighted by severity:
  critical issues cost the most, minor issues the least. Colored green (≥ 90), amber (60–89), or
  red (< 60).
- **Severity groups** — `Critical`, `Serious`, `Moderate`, `Minor`, each shown as a colored tag
  with a count, only when that group has at least one failing rule.
- **Rule rows** — each failing rule shows its name, a plain-language description, and how many
  elements on the page failed it (e.g. "`image-alt` — Images must have alternate text (12
  elements)").
- **Show affected elements** — expands that rule's row to list the CSS selector of every element
  that failed it, so you can go find and fix them directly. Click again to collapse it.
- **Re-scan** (bottom-left of the card) — re-runs the scan for that exact URL and updates the same
  card in place. The previous score/issues stay visible while the re-scan is running, and only
  replace once the new result arrives — a re-scan that fails doesn't wipe out your last good
  result, it just shows an inline error alongside it.
- **Delete** (the bin icon, bottom-right of the card) — permanently removes that result.

### 2.3 Results persist

Every successful scan is saved automatically — reloading the admin page, or navigating to a
different admin section and back to Accessibility Checker, brings all your previous result cards
back exactly as you left them (newest first). There's no separate "save" step; scanning *is*
saving. Nothing is kept only in your browser tab.

### 2.4 The dev workflow this is built for

1. Scan a page you're building.
2. See an issue (e.g. an image missing alt text).
3. Fix it in your code.
4. Click **Re-scan** on that same card.
5. Watch the score improve, without losing your place or creating a duplicate card.

### 2.5 Scanning your own local site

You can scan `http://localhost:<port>/...` URLs — but only while the *admin application itself* is
running in the `Development` environment (i.e. `ASPNETCORE_ENVIRONMENT=Development`, which is the
default for local development and what you get running from Visual Studio/`dotnet run` without
changing anything). This lets you audit a site before it's ever deployed publicly.

If your admin is deployed to Production or Staging, scanning `localhost` or other private/internal
network addresses is blocked, and you'll see: *"Scanning internal/private network addresses is not
allowed."* This is intentional — a deployed admin instance shouldn't be usable to probe its own
internal network, even by a legitimate admin user. It is **not** a bug; if you need to audit a
staging site, scan its actual public/staging URL instead of `localhost`.

## 3. Troubleshooting

| Symptom | Likely cause / fix |
| --- | --- |
| "Scanning internal/private network addresses is not allowed" | You're scanning `localhost`/a private IP while the app is *not* running in the `Development` environment. Expected in Production/Staging — see [2.5](#25-scanning-your-own-local-site). |
| "Could not resolve host '...'" | The URL's domain doesn't resolve from the server the admin is running on. Check for typos, or that the target site is actually reachable from that machine (not just your own laptop). |
| Scan fails with a timeout | The target page took too long to become interactive (30 second budget). Usually means the page itself is slow or hanging, not a problem with this feature. |
| "Access restricted by this site" | The target site responded with HTTP 401/403/429 on the page itself — it's deliberately blocking automated/script-based access (bot detection, rate limiting, an auth wall). Re-scanning won't help; this is the site's own policy, not something this tool can work around. |
| "Automatic Chromium install failed" / scan fails immediately every time | The one-time automatic browser download (see [1.3](#13-the-headless-browser-installs-itself-automatically)) couldn't complete — usually a network/firewall issue preventing the download. Check outbound internet access from the server, or run the manual install command in 1.3 to see the underlying error directly. |
| First scan on a new machine takes noticeably longer than expected | Normal — Chromium is downloading automatically in the background (see [1.3](#13-the-headless-browser-installs-itself-automatically)). Every scan after that is fast. |
| `dotnet build`/`dotnet run` fails with `MSB3027`/"the process cannot access the file... The file is locked by" | Your application is already running in another window/process and has its own `.exe` locked. Stop that running instance (Task Manager, or Ctrl+C in the terminal that launched it), then build/run again. Not specific to this package — happens with any running .NET app. |
| Accessibility checker tile is missing from the admin | Confirm the package is referenced by the *admin-hosting* project, and that you haven't excluded admin components in a `SeparatedAdmin` build. |
| Page loads, but every action (Scan, Re-scan, Delete) fails | `builder.Services.AddAccessibilityChecker();` is likely missing from `Program.cs` — see [1.2](#12-register-the-services). |

## 4. Under the hood (for the curious)

- Scans run against the **main frame only** — content inside `<iframe>` elements on the target page
  isn't scanned.
- Scan history is stored in a table this package creates automatically on first use, in the same
  database your Xperience application already uses — no separate database or connection string to
  configure.
- One row is kept per scanned URL; re-scanning updates that row rather than creating a new one.
