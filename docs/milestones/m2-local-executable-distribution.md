# M2: Local Executable Distribution

## Goal

Make ReplayLab runnable from a local published executable output folder, using the existing CLI preview and synthetic sample CSV.

## Definition Of Done

- The first distribution strategy is documented and accepted.
- A maintainer can produce a local published output for `ReplayLab.Cli`.
- The published executable can run the existing CLI preview against `samples/basic.csv`.
- Documentation explains how to publish, run, and verify the local executable.
- Any release/package follow-up is prepared as future work, not overbuilt in M2.

## Candidate Vertical Slices

### Slice 1: Distribution Decision

Capture the M2 distribution strategy and keep alternatives visible without expanding scope.

### Slice 2: Local Publish Path

Add the smallest useful publish command or helper needed to create local executable output for `ReplayLab.Cli`.

### Slice 3: Published CLI Verification

Verify that the published executable runs the current CSV-to-mock-sender preview with `samples/basic.csv`.

### Slice 4: Usage Documentation

Document the publish and run workflow for maintainers and early users.

### Slice 5: Release Preparation

Only if justified, prepare lightweight release artifact guidance. Do not add release automation unless a separate decision makes it necessary.

## Issue Draft List

1. Docs/Decision: define local executable distribution strategy.
2. Build: add publish command or helper for local executable output.
3. Tests: verify published executable with sample CSV.
4. Docs: document local executable usage.
5. Release: prepare GitHub release artifact guidance, if justified after the publish path is proven.
6. Versioning: add basic version output, only if needed for release verification.

## Risks

- Publishing can drift into release automation before the local executable path is proven.
- Self-contained, single-file, or runtime-specific choices can expand M2 beyond the expected outcome.
- Version output may become a product behavior change if added before release needs are clear.
- The existing `net10.0` target may limit who can run a framework-dependent output.

## Explicit Non-Goals

- Docker.
- NuGet publishing or .NET global tool distribution.
- Web UI.
- HTTP sender.
- WCF or private adapters.
- Persistence.
- Configuration DSL.
- New replay behavior.
- New CLI commands unrelated to publish verification.

## Future Considerations

- Self-contained or single-file artifacts for specific operating systems.
- GitHub release artifacts after versioning expectations are clear.
- NuGet package or .NET tool distribution after public package boundaries are reviewed.
- Broader target framework compatibility before a public package release.
