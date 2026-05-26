# Releases

ReplayLab SDK packages are published to GitHub Packages when a version tag is pushed.

## Published releases

- `v0.1.0-preview.1` — M1 Local CLI Replay Preview
- `v0.7.0-preview.1` — M7 Hostable Entry Points

## Known tags and next candidate

- `v0.3.0-preview.1` — historical tag present in Git; no GitHub release listed.
- `v0.13.0-preview.1` — released. GitHub Release and packages published.

See [releases/v0.13.0-preview.1.md](releases/v0.13.0-preview.1.md) for release notes and readiness checklist.

## Trigger

Push a tag matching `v*.*.*`:

```bash
git tag v0.13.0-preview.1
git push origin v0.13.0-preview.1
```

The release workflow:

1. Restores the solution.
2. Builds in Release configuration.
3. Runs all tests.
4. Verifies C# formatting and JavaScript syntax.
5. Packs the SDK package set.
6. Publishes `.nupkg` files to GitHub Packages.
7. Creates a GitHub Release with attached packages.

## Tag convention

Tags follow milestone-aligned preview conventions:

- `v0.1.0-preview.1`
- `v0.3.0-preview.1`
- `v0.7.0-preview.1`
- `v0.13.0-preview.1` (likely M13 candidate)

The leading `v` is stripped to produce the package version.

## Package set

- `ReplayLab.Core`
- `ReplayLab.Parsers.Csv`
- `ReplayLab.Adapters.Mock`
- `ReplayLab.Adapters.Http`
- `ReplayLab.Cli.Hosting`
- `ReplayLab.Web.Hosting`
- `ReplayLab.Desktop.Hosting`

## Consuming packages

Add the GitHub Packages NuGet source:

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/sebastienwitz/index.json" `
  --name github-replaylab `
  --username <github-username> `
  --password <github-token> `
  --store-password-in-clear-text
```

Then reference packages in your project:

```xml
<PackageReference Include="ReplayLab.Core" Version="0.13.0-preview.1" />
```

## Out of scope

- NuGet.org publishing (deferred).
- Docker image publishing — delivered via #141 (Dockerfile, docker-compose) and #142 (ghcr.io push on version tags).
- Persistence/session features (deferred).
- Dynamic plugin system (deferred).
