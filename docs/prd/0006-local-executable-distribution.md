# PRD 0006: Local Executable Distribution

## Status

Draft

## Purpose

Make ReplayLab runnable as a local executable without opening the solution in an IDE, while keeping the distribution approach small enough for the current CLI preview.

## Users

- Developers who want to run the current CSV-to-mock-sender preview from a published output folder.
- Maintainers verifying that the CLI can be distributed locally.
- Contributors checking the sample workflow without needing to understand the solution layout.

## Requirements

- Document a first local executable distribution path using `dotnet publish`.
- Publish `ReplayLab.Cli` without changing the current CLI preview behavior.
- Ensure the published output can run against `samples/basic.csv`.
- Keep the sample input synthetic and generic.
- Keep the package/release approach prepared but lightweight.
- Record release automation, NuGet publishing, Docker, and richer packaging options as future considerations unless they are required to verify local publish output.

## Acceptance Criteria

- A maintainer can run a documented publish command for `ReplayLab.Cli`.
- The published executable can run the existing CLI preview against `samples/basic.csv`.
- The documented verification uses the existing expected output shape and exit-code behavior.
- The distribution strategy is captured in an ADR.
- M2 issue drafts are traceable to this PRD, the ADR, and the milestone plan.
- No product behavior changes are required unless publish verification exposes a CLI packaging defect.

## Out Of Scope

- Docker images.
- NuGet publishing or .NET global tool distribution.
- Web UI work.
- HTTP sender work.
- WCF, private adapters, proprietary formats, customer data, or business-specific mappings.
- Persistence.
- Configuration DSL.
- New replay features or CLI commands unrelated to local executable verification.

## Assumptions

- The current `net10.0` target and pinned SDK line remain acceptable for M2.
- The first distribution path can be framework-dependent unless implementation proves a self-contained publish is needed for the M2 user workflow.
- `dotnet publish` is enough to validate local executable distribution before release automation is introduced.
- `samples/basic.csv` is the canonical sample for M2 verification.

## Open Questions

- Which runtime identifiers should be documented first, if any, beyond the default local machine publish?
- Should M2 add a small publish helper script, or is a documented command enough?
- Should a future milestone publish a single-file or self-contained executable for non-.NET users?
- Should the CLI expose version information before the first tagged release?
