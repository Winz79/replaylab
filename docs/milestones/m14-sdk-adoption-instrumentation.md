# M14: SDK Adoption Instrumentation & Polish

## Status

Planning. Not yet started.

## Intent

Make the ReplayLab SDK observable, documented, and discoverable for external
developers consuming ReplayLab packages from GitHub Packages.

This is a polish-and-instrument milestone, not a feature milestone. Every slice
makes the SDK easier to adopt without changing the existing public API contract.

## Context

### Where we are

M10A/B proved the packageable SDK works and external-style consumption is viable.
M11 hardened the DI composition conventions (`TryAdd*`, parser/sender override
patterns). M13 proved the release path — GitHub Actions publishes SDK packages
to GitHub Packages on version tags, and `v0.13.1-preview.1` is the current
release.

### What is missing

An external developer who picks up a ReplayLab package today will find:

- **No structured logs.** The engine, parser, and sender run silently. When
  something goes wrong, the only signal is in `ReplayResult.ErrorMessage`.
  There is no way to see the engine starting a batch, a parser reading rows,
  or a sender dispatching HTTP requests without attaching a debugger.

- **No IntelliSense docs.** The public API surface in `ReplayLab.Core` has
  almost no XML doc comments. `IMessageParser`, `IReplaySender`, `ReplayBatch`,
  `ReplayResult`, and `ReplayResultStatus` all lack `<summary>` documentation.
  When a developer hovers over these types in an IDE, they get nothing.

- **No getting-started guide.** The `README.md` has a "Build your own replay
  tool" section and the `samples/CustomReplayTool` directory, but there is no
  single document that walks a new consumer from zero to their first replay:
  setting up the NuGet source, adding `PackageReference`, implementing a custom
  parser, implementing a custom sender, and hosting the Web UI.

- **No download badge.** The `README.md` has CI, release, .NET version, license,
  and roadmap badges, but nothing that signals "packages available on GitHub
  Packages." A badge lowers the discoverability friction for potential consumers.

### What M14 does not do

- Persistence/session storage (M12, deferred).
- NuGet.org publishing (deferred).
- Docker, installers, or dynamic plugins (deferred).
- New parser formats (JSON, XML) or new adapter types (gRPC, MQTT).
- Breaking API changes, new contracts, or new public types.
- Telemetry, metrics, OpenTelemetry, or distributed tracing.

## Requirements

### R1: Structured logging in engine, parser, and sender

Add `ILogger`-based structured logging to:

| Component | Project | Key log points |
| --- | --- | --- |
| `SequentialReplayEngine` | `ReplayLab.Core` | Batch start/completion, per-message send, per-message failure, cancellation, elapsed timing |
| `CsvReplayMessageParser` | `ReplayLab.Parsers.Csv` | Parse start, header row detection, per-record parsing, field-count mismatch warnings, parse completion with record count |
| `HttpReplaySender` | `ReplayLab.Adapters.Http` | HTTP request dispatch, non-success status codes, request exceptions, cancellation |

Logs must use structured logging with semantic message templates (e.g.
`"Parsed {RecordCount} messages from CSV input"`) so consumers can filter and
route log output by category and level.

Log levels:
- `Information` — batch start/completion, parse completion, record counts.
- `Debug` — per-message send, per-record parse details.
- `Warning` — non-success HTTP status codes, field-count mismatches.
- `Error` — parse failures, sender exceptions, unexpected engine errors.

Dependency: `Microsoft.Extensions.Logging.Abstractions` (version 9.0.0 to match
the existing `Microsoft.Extensions.DependencyInjection.Abstractions` version).

### R2: XML doc comments on all public ReplayLab.Core API surfaces

Every public type and public member in `ReplayLab.Core` must have a `<summary>`
XML doc comment. This covers:

| Type | Public members needing docs |
| --- | --- |
| `ReplayMessage` | Record, all positional parameters (`Id`, `Payload`, `Headers`, `Metadata`) |
| `ReplayBatch` | Record, `Messages` parameter |
| `ReplayResult` | Class and all properties (`Success`, `MessageId`, `ErrorMessage`, `Status`, `Elapsed`, `ExceptionType`, `ExceptionMessage`, `ExceptionDetails`) |
| `ReplayResultStatus` | Enum and both values (`Succeeded`, `Failed`) |
| `IMessageParser` | Interface and `ParseAsync` method |
| `IReplaySender` | Interface and `SendAsync` method |
| `SequentialReplayEngine` | Class, constructor, and `ReplayAsync` method (the method already has doc comments; review and ensure completeness) |

The existing XML docs on `SequentialReplayEngine.ReplayAsync` should be reviewed
for accuracy and consistency. Parameter and return value docs (`<param>`,
`<returns>`, `<exception>`) should be added where appropriate.

No `<include>` files or external doc generation tooling is needed. Comments must
be inline and discoverable by standard IDE IntelliSense.

### R3: `docs/getting-started.md`

A single, self-contained document that walks a developer through:

1. **Prerequisites** — .NET SDK version (refer to `global.json`), GitHub account
   with a Personal Access Token (PAT) scoped to `read:packages`.
2. **NuGet source setup** — the `dotnet nuget add source` command for GitHub
   Packages, referencing `sebastienwitz/replaylab`.
3. **Adding PackageReference** — the list of available packages and a minimal
   `.csproj` snippet showing `ReplayLab.Core`, `ReplayLab.Web.Hosting`, and
   optional parser/adapter packages.
4. **Implementing a custom parser** (`IMessageParser`) — a working, minimal
   example with explanation of the contract.
5. **Implementing a custom sender** (`IReplaySender`) — a working, minimal
   example with explanation of the contract.
6. **Hosting the Web UI** — a minimal `Program.cs` that wires up custom
   parser/sender registrations and calls `AddReplayLabWeb()` /
   `MapReplayLabWeb()`.
7. **Running the tool** — `dotnet run` instructions.
8. **Next steps** — links to `docs/architecture.md`, `samples/CustomReplayTool/`,
   and `docs/roadmap.md`.

The guide should use fictional but realistic names (e.g. `MyCustomParser`,
`MyCustomSender`) consistent with the existing sample patterns.

### R4: GitHub Packages download badge on README.md

Add a shield.io badge near the existing badge row that links to the GitHub
Packages page for the repository. The badge should show the latest published
version or a generic "packages" label.

Candidate badge (static, no version auto-detection needed for M14):

```markdown
[![GitHub Packages](https://img.shields.io/badge/packages-github-0A7EA4?logo=nuget)](https://github.com/sebastienwitz/replaylab/pkgs/nuget/ReplayLab.Core)
```

The badge must link to the public GitHub Packages page for the ReplayLab
repository.

## Impact

### Dependency changes

| Project | New dependency |
| --- | --- |
| `ReplayLab.Core` | `Microsoft.Extensions.Logging.Abstractions` 9.0.0 |
| `ReplayLab.Parsers.Csv` | Transitive via Core (no explicit change needed; already depends on Core) |
| `ReplayLab.Adapters.Http` | Transitive via Core (no explicit change needed; already depends on Core) |

`ReplayLab.Parsers.Csv` and `ReplayLab.Adapters.Http` already have
`Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0 as a direct
dependency. Adding `Microsoft.Extensions.Logging.Abstractions` 9.0.0 to Core
keeps the Microsoft.Extensions version aligned and does not introduce a new
major dependency family.

### Test impact

- **`ReplayLab.Core.Tests`** — `SequentialReplayEngineTests` must be extended to
  verify that the engine logs at expected levels for batch start, per-message
  send, failures, and cancellation. A test `ILogger` fake (or `Microsoft.Extensions.Logging.Testing`
  / `NullLogger`) can be used.
- **`ReplayLab.Parsers.Csv.Tests`** — `CsvReplayMessageParserTests` must be
  extended to verify parse start/completion logs and field-count mismatch
  warnings.
- **`ReplayLab.Adapters.Http.Tests`** — `HttpReplaySenderTests` must be extended
  to verify HTTP dispatch and error logs.
- **No new test projects** are introduced.
- **No existing test behavior should break** — logging is additive.

### Architecture boundary check

- `ReplayLab.Core` gains a dependency on `Microsoft.Extensions.Logging.Abstractions`.
  This is a generic, widely-adopted abstraction package. It does not pull in
  business-specific, UI, persistence, or adapter concerns. It is consistent
  with the existing `Microsoft.Extensions.DependencyInjection.Abstractions`
  dependency in the parser and adapter projects.
- XML doc comments are purely additive documentation. They do not change
  behavior, contracts, or assembly structure.
- The getting-started guide and README badge are documentation-only artifacts.

**No ADR is needed for M14.** The architectural decisions (Core independence,
package-oriented layout, vertical-slice delivery) are already covered by ADRs
0002 and 0004. M14 does not change those boundaries.

### Consumer compatibility

- `SequentialReplayEngine` constructor adds an optional `ILogger<SequentialReplayEngine>?`
  parameter to avoid breaking existing consumers. When `null`, logging is a
  no-op. When provided, structured logs are emitted.
- `CsvReplayMessageParser` follows the same pattern: optional `ILogger<CsvReplayMessageParser>?`
  constructor parameter.
- `HttpReplaySender` follows the same pattern: optional `ILogger<HttpReplaySender>?`
  constructor parameter.

Consumers who construct these types directly without DI can pass `null` and see
no behavioral change. Consumers using DI (the recommended path) get logging
automatically when `ILogger<T>` is registered.

## Design

### Logging strategy

All three components use the standard `ILogger<T>` pattern from
`Microsoft.Extensions.Logging.Abstractions`:

```csharp
public sealed class SequentialReplayEngine
{
    private readonly IReplaySender _sender;
    private readonly ILogger<SequentialReplayEngine>? _logger;

    public SequentialReplayEngine(IReplaySender sender, ILogger<SequentialReplayEngine>? logger = null)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReplayResult>> ReplayAsync(
        ReplayBatch batch,
        CancellationToken cancellationToken = default)
    {
        // ...
        _logger?.LogInformation("Starting replay of {MessageCount} messages", batch.Messages.Count);
        // ...
    }
}
```

The `?` (nullable) pattern means:
- Direct construction without a logger continues to work.
- DI will always inject a non-null `ILogger<T>` (via `NullLogger<T>` if no
  provider is registered).
- No breaking API change.

### Log message templates

**SequentialReplayEngine:**

| Event | Level | Template |
| --- | --- | --- |
| Batch start | Information | `"Starting sequential replay of {MessageCount} messages"` |
| Per-message send | Debug | `"Sending message {MessageId} ({MessageIndex}/{TotalMessages})"` |
| Per-message success | Debug | `"Message {MessageId} sent successfully in {ElapsedMs}ms"` |
| Per-message failure (sender exception) | Error | `"Message {MessageId} failed: {ErrorMessage}"` |
| Per-message cancellation (non-replay-token) | Warning | `"Message {MessageId} was canceled"` |
| Batch complete | Information | `"Replay complete: {SuccessCount} succeeded, {FailureCount} failed out of {TotalMessages} in {TotalElapsedMs}ms"` |

**CsvReplayMessageParser:**

| Event | Level | Template |
| --- | --- | --- |
| Parse start | Information | `"Starting CSV parse"` |
| Header detected | Debug | `"Detected CSV header with {HeaderCount} columns: {Headers}"` |
| Per-record parse | Debug | `"Parsed CSV row {SourceRowNumber} as record {DataRecordNumber}"` |
| Field-count mismatch | Warning | `"CSV row {RawRow} has {ActualFieldCount} fields but header has {HeaderFieldCount} fields"` |
| Parse complete | Information | `"CSV parse complete: {RecordCount} messages from {TotalRows} rows"` |

**HttpReplaySender:**

| Event | Level | Template |
| --- | --- | --- |
| Request dispatch | Debug | `"Sending HTTP POST {MessageId} to {EndpointUrl}"` |
| Non-success status | Warning | `"HTTP POST {MessageId} returned {StatusCode} ({ReasonPhrase})"` |
| Request exception | Error | `"HTTP POST {MessageId} failed: {ErrorMessage}"` |
| Cancellation (replay token) | Debug | `"HTTP POST {MessageId} was canceled"` |

### XML doc comment conventions

Example for `IMessageParser`:

```csharp
namespace ReplayLab.Core;

/// <summary>
/// Converts an input stream into a <see cref="ReplayBatch"/> of replay messages.
/// </summary>
/// <remarks>
/// Implementations should handle stream lifecycle externally. The stream is
/// left open after parsing so the caller can manage disposal.
/// </remarks>
public interface IMessageParser
{
    /// <summary>
    /// Parses the input stream and returns a batch of replay messages.
    /// </summary>
    /// <param name="input">The stream containing structured replay input data.</param>
    /// <param name="cancellationToken">A token that cancels the parse operation.</param>
    /// <returns>A <see cref="ReplayBatch"/> containing zero or more parsed messages.</returns>
    Task<ReplayBatch> ParseAsync(Stream input, CancellationToken cancellationToken = default);
}
```

All doc comments must follow standard C# XML doc conventions with `<summary>`
at minimum. `<param>`, `<returns>`, `<exception>`, and `<remarks>` should be
added where they add clarity.

### Getting-started guide structure

The guide lives at `docs/getting-started.md` and follows the structure defined
in R3. It should reference real package names and versions, and the code
examples must be copy-pasteable with only fictional parser/sender logic needing
replacement.

### README badge

Add a new badge in the badge row, between the existing badges. The badge links
to `https://github.com/sebastienwitz/replaylab/pkgs/nuget/ReplayLab.Core`.

## Delivery Plan

M14 has **four vertical slices**, each delivering observable value independently.

### Slice 1: Add structured logging to engine, parser, and sender

**Goal:** Wire `ILogger<T>` into `SequentialReplayEngine`, `CsvReplayMessageParser`,
and `HttpReplaySender` with structured log templates.

**Scope:**
- Add `Microsoft.Extensions.Logging.Abstractions` 9.0.0 to `ReplayLab.Core`.
- Add optional `ILogger<T>?` constructor parameters to all three types.
- Wire log calls at the log points defined in the Design section.
- Keep the nullable-parameter pattern so direct construction without DI still works.

**Acceptance criteria:**
- `dotnet build ReplayLab.sln` passes.
- Existing tests pass without modification (no behavioral regression).
- New or extended tests verify key log events for each component (at least one
  test per component proving logger receives expected calls).

**Test expectations:**
- Use a simple `ILogger` fake/spy (or `NullLogger<T>`) in unit tests to verify
  log method invocations with correct levels and templates.
- Do not introduce a logging-specific test framework dependency unless it
  simplifies the tests significantly.

**Risks:**
- Low. The nullable-parameter pattern is non-breaking. The `Microsoft.Extensions.Logging.Abstractions`
  dependency is lightweight and widely used.

**Out of scope:**
- Logging in `MockReplaySender` or other adapters not listed in the scope.
- Configurable log levels at the component level.
- Log scopes or activity IDs.

### Slice 2: Add XML doc comments to all public ReplayLab.Core API surfaces

**Goal:** Every public type and member in `ReplayLab.Core` has IntelliSense-visible
XML documentation.

**Scope:**
- Add `<summary>` to `ReplayMessage`, `ReplayBatch`, `ReplayResult`, `ReplayResultStatus`,
  `IMessageParser`, `IReplaySender`.
- Review and extend the existing docs on `SequentialReplayEngine.ReplayAsync`.
- Add `<param>`, `<returns>`, `<exception>` where appropriate.

**Acceptance criteria:**
- `dotnet build ReplayLab.sln` passes with no XML doc warnings (enable
  `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the Core
  csproj if not already set, or verify warning suppressions are intentional).
- Hovering over any `ReplayLab.Core` public type in an IDE shows a meaningful
  summary.
- All doc comments are consistent in tone and format.

**Test expectations:**
- No runtime test changes needed — docs are compile-time only.
- Optionally add a build-time check (e.g., `<WarningsAsErrors>CS1591</WarningsAsErrors>`
  scoped to Core) to ensure all public members are documented.

**Risks:**
- Very low. Documentation-only change.

**Out of scope:**
- XML docs for parsers or adapters outside `ReplayLab.Core`.
- Sandcastle, DocFX, or external doc site generation.
- `<include>` files or shared doc fragments.

### Slice 3: Write `docs/getting-started.md`

**Goal:** A single, self-contained guide that takes a developer from zero to
first replay using ReplayLab packages.

**Scope:**
- Prerequisites section (SDK, GitHub PAT).
- NuGet source setup.
- `PackageReference` snippets for Core, Web.Hosting, and optional packages.
- Minimal `IMessageParser` implementation example.
- Minimal `IReplaySender` implementation example.
- Minimal `Program.cs` with `AddReplayLabWeb()` / `MapReplayLabWeb()`.
- `dotnet run` instructions.
- Links to architecture docs, CustomReplayTool sample, and roadmap.

**Acceptance criteria:**
- A developer unfamiliar with ReplayLab can follow the guide end-to-end (noting
  that they must provide their own parser/sender logic).
- The guide is linked from `README.md` documentation map.
- Code snippets are syntactically valid C# for `net10.0`.

**Test expectations:**
- No automated tests for docs. Manual review.

**Risks:**
- Low. The guide may need follow-up updates as the package API evolves.

**Out of scope:**
- Video walkthroughs, screencasts, or interactive tutorials.
- CLI- or Desktop-specific getting-started variants (Web is the primary path).

### Slice 4: Add GitHub Packages badge to README.md

**Goal:** A discoverability badge on the README signaling available packages.

**Scope:**
- Add a shield.io badge linking to the GitHub Packages page.
- Place it in the badge row after the existing badges.

**Acceptance criteria:**
- Badge renders correctly in the GitHub README view.
- Badge link navigates to the GitHub Packages page.

**Test expectations:**
- Manual verification after merge.

**Risks:**
- Negligible.

**Out of scope:**
- Version-specific or auto-updating badges.
- NuGet.org badges.

### Slice ordering rationale

1. **Slice 1 (logging)** first — it touches the most projects and requires a
   new package dependency. Proving the dependency addition and test patterns
   early reduces risk for the remaining slices.
2. **Slice 2 (XML docs)** next — independent of logging, can be developed in
   parallel but benefits from the Core csproj being touched in Slice 1.
3. **Slice 3 (getting-started guide)** — depends on nothing but best written
   after the logging and doc patterns are settled so the guide can reference
   the new conventions.
4. **Slice 4 (badge)** — trivial, can ship at any time.

Slices 1 and 2 could reasonably be developed in parallel by different
contributors. Slices 3 and 4 are independent of each other and of the
implementation slices.

## Build / Review / Learn

### Build and validation

After each slice:

```powershell
dotnet restore ReplayLab.sln
dotnet build ReplayLab.sln --configuration Release --no-restore
dotnet test ReplayLab.sln --configuration Release --no-build
```

For Slice 1 specifically, run targeted tests first:

```powershell
dotnet test tests/ReplayLab.Core.Tests
dotnet test tests/ReplayLab.Parsers.Csv.Tests
dotnet test tests/ReplayLab.Adapters.Http.Tests
```

### Review checklist

- [ ] `ReplayLab.Core` still has no dependency on UI, adapters, hosting, or business logic.
- [ ] The new `Microsoft.Extensions.Logging.Abstractions` dependency is an
  abstraction package only — no concrete logging providers are pulled in.
- [ ] Existing tests pass without modification.
- [ ] New logging tests use fakes or `NullLogger<T>`, not real providers.
- [ ] XML doc comments are complete and internal-facing members are not
  accidentally documented.
- [ ] `docs/getting-started.md` uses the fictional-but-realistic naming
  convention consistent with `samples/CustomReplayTool`.
- [ ] The `README.md` badge renders correctly and links to the right URL.
- [ ] `docs/roadmap.md` is updated to mark M14 complete once all slices are
  merged.

### Success criteria

1. A developer can configure a logger provider and see structured logs from
   the engine, parser, and sender at runtime.
2. All `ReplayLab.Core` public types show useful IntelliSense when consumed
   from another project.
3. A new developer can follow `docs/getting-started.md` and understand the
   end-to-end consumption path (noting that implementing the actual parser/sender
   logic is their responsibility).
4. The README has a visible GitHub Packages badge.

### Learnings to capture

- Were any XML doc conventions ambiguous or inconsistent? Document findings for
  future parser/adapter doc work.
- Did the `ILogger<T>?` nullable pattern create any DI registration surprises?
- Are structured log templates useful enough to justify extending logging to
  other adapters (Mock, Example) in a follow-up?
- Did the getting-started guide reveal any missing convenience methods or
  composition gaps?

## Issue Drafts

### Issue 1: Add ILogger/structured logging to engine, parser, and sender

**Title:** Add `ILogger<T>` structured logging to `SequentialReplayEngine`, `CsvReplayMessageParser`, and `HttpReplaySender`

**Goal:** Instrument the core replay pipeline with structured logging so developers can observe engine, parser, and sender behavior without attaching a debugger.

**Scope:**
- Add `PackageReference` for `Microsoft.Extensions.Logging.Abstractions` 9.0.0 to `ReplayLab.Core`.
- Add optional `ILogger<T>?` constructor parameter to `SequentialReplayEngine`.
- Add optional `ILogger<T>?` constructor parameter to `CsvReplayMessageParser`.
- Add optional `ILogger<T>?` constructor parameter to `HttpReplaySender`.
- Wire log calls at Information, Debug, Warning, and Error levels per the design spec.
- Use semantic message templates with structured parameters.

**Acceptance criteria:**
- `dotnet build ReplayLab.sln` succeeds.
- All existing tests pass without modification.
- New or extended tests verify that each component emits expected log events at correct levels.
- Logging uses semantic templates (e.g. `"Parsed {RecordCount} messages"`), not string interpolation.

**Linked docs:** `docs/milestones/m14-sdk-adoption-instrumentation.md` (Design section).

**Implementation notes:**
- Use `ILogger<T>?` with null-conditional invocation (`_logger?.LogInformation(...)`) so direct construction without DI works.
- Pick clear, consistent log event IDs or omit them (no event ID convention yet).
- Do not add logging to `MockReplaySender` or `ReplayLab.Adapters.Example`.

**Test expectations:**
- Add a spy/fake `ILogger` (or use `NullLogger<T>`) in `SequentialReplayEngineTests`, `CsvReplayMessageParserTests`, and `HttpReplaySenderTests`.
- Assert that expected log methods are called with matching templates and levels.

**Risks:**
- Low. Non-breaking API change. Well-understood pattern.
- DI consumers must register a logging provider to see output; this is standard .NET behavior, not a ReplayLab concern.

**Out of scope:**
- Log scopes, activity IDs, or correlation.
- Configurable log levels.
- Logging in other adapters.
- OpenTelemetry integration.

---

### Issue 2: Add XML doc comments to all public ReplayLab.Core API surfaces

**Title:** Add XML doc comments on all public `ReplayLab.Core` API surfaces

**Goal:** Every public type and member in `ReplayLab.Core` produces useful IntelliSense documentation when consumed from another project.

**Scope:**
- Add `<summary>` XML doc comments to `ReplayMessage`, `ReplayBatch`, `ReplayResult`, `ReplayResultStatus`, `IMessageParser`, `IReplaySender`.
- Review and extend existing doc comments on `SequentialReplayEngine`.
- Add `<param>`, `<returns>`, `<exception>`, and `<remarks>` where they add clarity.
- Enable `GenerateDocumentationFile` or verify warning suppressions are intentional.

**Acceptance criteria:**
- `dotnet build ReplayLab.sln` passes (no XML doc warnings in Core).
- Every public type and public member in `ReplayLab.Core` has a `<summary>`.
- Doc comments are consistent in language and tone.

**Linked docs:** `docs/milestones/m14-sdk-adoption-instrumentation.md` (Design section).

**Implementation notes:**
- Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `ReplayLab.Core.csproj`.
- Consider adding `<NoWarn>$(NoWarn);CS1591</NoWarn>` for now and removing it once all public members are documented, then switch to `<WarningsAsErrors>CS1591</WarningsAsErrors>` to keep them documented going forward.
- Follow standard C# XML doc conventions.

**Test expectations:**
- No runtime test changes. Docs are compile-time only.

**Risks:**
- Negligible. Documentation-only change.

**Out of scope:**
- XML docs on parser/adapter projects outside Core.
- DocFX/Sandcastle integration.

---

### Issue 3: Write `docs/getting-started.md`

**Title:** Write getting-started guide for consuming ReplayLab from GitHub Packages

**Goal:** A single document that takes a developer from zero to their first custom replay tool using ReplayLab packages.

**Scope:**
- Prerequisites (SDK, GitHub PAT with `read:packages`).
- NuGet source configuration (`dotnet nuget add source`).
- `PackageReference` setup with package list.
- Implementing a minimal `IMessageParser`.
- Implementing a minimal `IReplaySender`.
- Hosting the Web UI with `AddReplayLabWeb()` / `MapReplayLabWeb()`.
- Running the tool with `dotnet run`.
- Links to architecture docs, `samples/CustomReplayTool/`, and roadmap.

**Acceptance criteria:**
- The guide is complete, self-contained, and uses realistic fictional names.
- The guide is linked from `README.md` documentation map.
- Code snippets are valid C# for `net10.0`.

**Linked docs:** `docs/milestones/m14-sdk-adoption-instrumentation.md` (Design section).

**Implementation notes:**
- Use the existing `samples/CustomReplayTool` as reference for structure.
- Keep fictional names consistent (e.g. `MyCustomParser`, `MyCustomSender`).
- The guide should target the Web UI path as the primary entry point.

**Test expectations:**
- Manual review. No automated tests for docs.

**Risks:**
- Low. The guide will need updates as the API evolves, but that is normal.

**Out of scope:**
- CLI or Desktop-specific getting-started variants.
- Video tutorials or interactive walkthroughs.

---

### Issue 4: Add GitHub Packages download badge to README

**Title:** Add GitHub Packages badge to README.md

**Goal:** A discoverability badge on the README that signals packages are available on GitHub Packages.

**Scope:**
- Add a shield.io badge in the README badge row.
- Link to `https://github.com/sebastienwitz/replaylab/pkgs/nuget/ReplayLab.Core`.

**Acceptance criteria:**
- Badge renders correctly on the GitHub README.
- Badge link resolves to the public packages page.

**Linked docs:** `docs/milestones/m14-sdk-adoption-instrumentation.md` (Design section).

**Implementation notes:**
- Use a static badge — no version auto-detection needed.
- Place after the existing Release badge, before the .NET badge.

**Test expectations:**
- Manual verification after merge.

**Risks:**
- Negligible.

**Out of scope:**
- Version-specific badges.
- NuGet.org badges.

---

## Assumptions

1. `Microsoft.Extensions.Logging.Abstractions` 9.0.0 is compatible with `net10.0`
   and the existing `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0
   used in the parser and adapter projects.
2. The nullable `ILogger<T>?` constructor pattern is acceptable and does not
   require an ADR (it's a standard .NET pattern for optional logging).
3. Enabling XML doc warnings in `ReplayLab.Core` does not surface pre-existing
   undocumented members in internal or generated code.
4. The GitHub Packages page URL is stable and public.
5. The getting-started guide does not need automated validation (code snippets
   are illustrative, not compiled as part of CI).

## Risks

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| `Microsoft.Extensions.Logging.Abstractions` 9.0.0 has a known incompatibility with net10.0 | Low | Medium | Test build immediately after adding the reference. If 9.0.0 fails, use a 9.x version or a net10.0-compatible preview. |
| XML doc generation surfaces undocumented internal types | Low | Low | Use `<NoWarn>CS1591</NoWarn>` initially and tighten incrementally. |
| The `ILogger<T>?` pattern causes DI resolution issues in edge cases | Low | Low | This is a standard pattern. Test with the `CustomReplayTool` sample and existing test suites. |
| Getting-started guide becomes stale as API changes | Medium | Low | Link to the sample and roadmap in the "Next steps" section so readers naturally discover updates. |
| GitHub Packages page URL changes | Very Low | Low | The badge is a minor discoverability aid; if it breaks, fix it in a trivial follow-up PR. |

## Open Questions

1. **Should `ReplayLab.Core` enable `GenerateDocumentationFile` by default?**
   Enabling it generates an XML doc file alongside the assembly. This is useful
   for consumers who want the doc file but may have packaging implications. The
   decision should be made in Slice 2.

2. **Should logging be extended to `MockReplaySender` in this milestone?**
   The roadmap explicitly calls out `HttpSender` only. `MockReplaySender` is a
   development/test adapter and adding logging there could be done in a follow-up
   if needed. Recommend leaving it out of M14.

3. **What version of `Microsoft.Extensions.Logging.Abstractions` is the right fit?**
   The existing DI abstractions use 9.0.0. The .NET 10 target framework may
   prefer a 10.0 preview. The first action in Slice 1 should be to validate the
   package reference resolves and builds. If 9.0.0 works, stick with it for
   consistency. If not, use the nearest compatible version.

4. **Should there be a CI check for undocumented public APIs?**
   Enabling `<WarningsAsErrors>CS1591</WarningsAsErrors>` in Core would catch
   regressions. This could be done in Slice 2 once all members are documented.
