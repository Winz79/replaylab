# ADR 0005: Distribution Strategy

## Status

Accepted

## Context

M1 proved the first local CLI replay preview: a synthetic CSV file can be parsed, replayed through the mock sender, and summarized in the console.

M2 needs to make that workflow easier to run as a local executable without opening the solution in an IDE. The project is still early, targets `net10.0`, and has not stabilized public package or release semantics. Distribution should therefore validate the executable path without adding release infrastructure or package formats prematurely.

## Decision

Use `dotnet publish` as the first local executable distribution mechanism for `ReplayLab.Cli`.

The first M2 implementation should prefer a framework-dependent publish unless a specific verification need requires a self-contained publish. The published output must be able to run the existing CLI preview against the synthetic sample CSV.

This is a pragmatic first decision, not a permanent packaging decision. Later milestones may revisit single-file output, self-contained publishing, NuGet distribution, GitHub release artifacts, Docker, or other delivery channels after the CLI and public contract expectations are clearer.

## Alternatives Considered

### Run From Source Only

Running with `dotnet run --project src/ReplayLab.Cli/ReplayLab.Cli.csproj -- samples/basic.csv` already works for M1, but it does not satisfy M2 because users still need source-oriented commands and solution context.

### Single-File Executable

Single-file publishing can improve portability and artifact handling, but it adds packaging choices before the basic publish path is proven. It remains a future option.

### Framework-Dependent Publish

Framework-dependent publish keeps output simple and small, matches the current .NET developer audience, and avoids bundling the runtime before distribution needs are clearer. This is the preferred first M2 path.

### Self-Contained Publish

Self-contained publishing helps users without the required runtime installed, but it increases artifact size and requires runtime identifier choices. It should wait until there is a clear M2 need or a later release goal.

### NuGet Global Tool

A .NET global tool may become useful later, but it implies package naming, versioning, publishing, and consumer compatibility decisions that are too heavy for M2.

### Docker Image

Docker is explicitly out of scope for M2 and would add infrastructure that is not needed for a local executable preview.

## Consequences

- M2 can focus on a documented, verifiable local executable output.
- The current CLI behavior remains the product surface being verified.
- Release automation and package publishing stay out of scope until the project needs them.
- The project should document future distribution options without partially implementing them in M2.

## Assumptions

- M2 users are developers with access to the required .NET runtime or SDK.
- The pinned SDK line in `global.json` is acceptable for producing the first local executable output.
- The existing sample CSV is sufficient for publish verification.

## Open Questions

- When should ReplayLab choose a stable versioning scheme for distributed artifacts?
- Should a later milestone provide self-contained artifacts for specific operating systems?
- Should the CLI eventually ship as a .NET global tool, a GitHub release artifact, or both?
