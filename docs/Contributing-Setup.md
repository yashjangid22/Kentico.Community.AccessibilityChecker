# Contributing Setup

---This documents the steps a maintainer or developer would follow to work on the library in their development environment---

---Update the details for this project, replacing "repotemplate" and anything else that needs changed---

## Required Software

The requirements to setup, develop, and build this project are listed below.

### .NET Runtime

.NET SDK 10.0 or newer

- <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>
- See `global.json` file for specific SDK requirements

### Node.js Runtime

- [Node.js](https://nodejs.org/en/download) v24 or newer
- [NVM for Windows](https://github.com/coreybutler/nvm-windows) to manage multiple installed versions of Node.js
- See `engines` in the solution `package.json` for specific version requirements

### C# Editor

- VS Code/VS
- Cursor
- Rider

### Database

SQL Server 2019 or newer compatible database

- [SQL Server Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup?view=sql-server-ver15)

### SQL Editor

- VS Code with official [MSSQL extension](https://marketplace.visualstudio.com/items?itemName=ms-mssql.mssql)
- MS SQL Server Management Studio

### Client npm dependencies

Before running `dotnet build`/`dotnet restore` on `XperienceCommunity.AccessibilityChecker` for the
first time (or after pulling changes to `Client/package.json`), run `npm install` inside
`src/XperienceCommunity.AccessibilityChecker/Client`:

```powershell
cd src/XperienceCommunity.AccessibilityChecker/Client
npm install
```

This is required *before* the .NET build, not just before the client bundle is built: the project
embeds axe-core's `axe.min.js` (used for server-side scanning) straight out of
`Client/node_modules/axe-core`, and that `EmbeddedResource` item is evaluated at MSBuild
project-evaluation time - before any build target runs. If `Client/node_modules` doesn't exist yet,
the build will fail to find the file rather than silently skipping it.

### Headless browser (Playwright)

The accessibility scan feature renders pages with a headless Chromium browser via
[Playwright for .NET](https://playwright.dev/dotnet/). The `Microsoft.Playwright` NuGet package
does not bundle browser binaries - install Chromium once per machine (dev box and CI) after building
the **consuming application** (the Sample Project below, or your own site) - not this library
project itself. A class library's own build output does not copy its dependency assemblies, so
`playwright.ps1` only works from an app's build output, where `Microsoft.Playwright.dll` is
actually present alongside it:

```powershell
pwsh <path-to-consuming-app>/bin/Debug/<tfm>/playwright.ps1 install chromium
# e.g. pwsh examples/DancingGoat/bin/Debug/net8.0/playwright.ps1 install chromium
```

This is a manual, one-time-per-machine step, deliberately not wired into `dotnet build` - running a
browser download on every build would slow down normal development for no benefit after the first
install.

## Sample Project

### Database Setup

Running the sample project requires creating a new Xperience by Kentico database using the included template.

Change directory in your console to `./examples/DancingGoat` and follow the instructions in the Xperience
documentation on [creating a new database](https://docs.kentico.com/documentation/developers-and-admins/installation#create-the-project-database).

### Admin Customization

To run the Sample app Admin customization in development mode, add the following to your [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows#secret-manager) for the application.

```json
"CMSAdminClientModuleSettings": {
  "kentico-xperience-integrations-repotemplate": {
    "Mode": "Proxy",
    "Port": 3009
  }
}
```

## Development Workflow

1. Create a new branch with one of the following prefixes
   - `feat/` - for new functionality
   - `refactor/` - for restructuring of existing features
   - `fix/` - for bugfixes

1. Run `dotnet format` against the `XperienceCommunity.AccessibilityChecker` solution

   > use `dotnet: format` VS Code task.

1. Commit changes, with a commit message preferably following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

1. Once ready, create a PR on GitHub. The PR will need to have all comments resolved and all tests passing before it will be merged.
   - The PR should have a helpful description of the scope of changes being contributed.
   - Include screenshots or video to reflect UX or UI updates
   - Indicate if new settings need to be applied when the changes are merged - locally or in other environments

1. This repository is stored with `lf` line endings. If you are developing on Windows you can set your Git config to automatically checkout as `crlf` and commit as `lf`.

   ```powershell
   # git config --global core.autocrlf true
   ```
