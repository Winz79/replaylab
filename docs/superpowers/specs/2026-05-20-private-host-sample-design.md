# Private Host Composition Sample Design

## Goal

Implement issue `#75` by adding a generic external/private-style host sample that proves an external project can own the composition root, register parser/sender services through DI, and invoke ReplayLab hostable entry points without modifying ReplayLab product code.

## Scope

- Add a generic sample project under `samples/`.
- Keep the sample synthetic, business-agnostic, and understandable.
- Demonstrate explicit external composition-root ownership.
- Register parser and sender services from the sample through DI.
- Demonstrate both hostable entry points from one sample host if that remains clean:
  - `ReplayLab.Cli.Hosting`
  - `ReplayLab.Web.Hosting`
- Add focused verification proving the sample composition works.
- Update docs only if needed to explain how to run the sample.

## Out Of Scope

- Issue `#76`, except for minimal notes only if unavoidable.
- Desktop AppHost or WebView2 work.
- CSV parser strategy changes.
- Editable Web grid behavior.
- Business-specific adapters, contracts, mappings, or naming.
- Package publishing or packaging workflow changes.
- Hidden or secondary composition roots inside ReplayLab.
- Issues `#68`, `#69`, and `#70`.

## Recommended Approach

Add one runnable sample host project, likely `samples/ReplayLab.HostSample`, that owns a single composition root and can exercise both ReplayLab hostable surfaces through two entry modes only: CLI mode and Web mode.

This keeps the sample small enough to understand, avoids duplicating synthetic parser/sender wiring across multiple sample projects, and most directly proves the M7 composition-root ownership model from ADR `0009`.

The sample must remain a composition proof, not a new product shell.

## Architecture

### Host Ownership

The sample owns:

- service registration
- synthetic parser and sender implementations
- host startup choice between CLI mode and Web mode only
- host policy for Web static assets, middleware, and app lifetime

ReplayLab owns:

- generic CLI workflow behavior through `CliApplication.RunAsync(...)`
- generic Web workflow behavior through `AddReplayLabWeb()` and `MapReplayLabWeb()`

The sample must not introduce a hidden second composition root and ReplayLab must not absorb sample-specific wiring.

### Project Shape

The sample should be one project with a small set of focused files:

- `samples/ReplayLab.HostSample/ReplayLab.HostSample.csproj`
- `samples/ReplayLab.HostSample/Program.cs`
- `samples/ReplayLab.HostSample/SampleServiceCollectionExtensions.cs`
- `samples/ReplayLab.HostSample/SyntheticMessageParser.cs`
- `samples/ReplayLab.HostSample/SyntheticReplaySender.cs`
- `samples/ReplayLab.HostSample/SyntheticReplaySenderFactory.cs`

If an additional helper file is needed for clarity, keep it narrowly scoped to sample composition.

## Sample Behavior

### CLI Mode

The sample should support a CLI entry mode that:

1. builds the sample-owned DI container
2. registers the synthetic parser and sender factory
3. passes the container to `ReplayLab.Cli.Hosting.CliApplication.RunAsync(...)`
4. returns the ReplayLab CLI exit code directly

This proves ReplayLab CLI hostability consumes externally registered services instead of assuming the repo-owned CLI shell owns composition.

### Web Mode

The sample should support a Web entry mode that:

1. creates a `WebApplicationBuilder`
2. registers the same synthetic sample services
3. calls `AddReplayLabWeb()`
4. builds the app
5. applies host-owned static asset and middleware configuration in the sample
6. calls `MapReplayLabWeb()`

This proves ReplayLab Web hostability can be mounted from an external-style host while leaving app startup ownership with the sample.

### Mode Selection

The sample should select CLI mode or Web mode from arguments, such as:

- `cli ...`
- `web ...`

Argument handling should remain lightweight and explicit. The sample is not a new product surface; it is a composition proof. Mode selection should stay limited to `cli` and `web`.

## Synthetic Services

The sample-owned parser and sender should be deliberately synthetic:

- generic names only
- deterministic, easy-to-read behavior
- no business semantics
- no private or proprietary concepts

Their purpose is to prove ownership of composition and DI registration, not to model a realistic private integration.

The sample parser should parse a simple, predictable input format or reuse the existing CSV path in a way that makes the sample’s DI ownership visible. The sample sender should produce deterministic replay behavior suitable for focused verification.

The synthetic parser and sender should also be observable in tests so the sample can prove that ReplayLab consumed the sample-owned DI registrations rather than falling back to repo-owned defaults.

## Web Host Boundary

The sample’s Web mode should make host ownership explicit:

- static asset setup belongs to the sample host
- environment policy belongs to the sample host
- app lifetime belongs to the sample host
- `MapReplayLabWeb()` remains limited to ReplayLab-owned page mapping

The sample should follow the same hostability direction already proven in `#74` and should not pull static asset or middleware ownership back into ReplayLab.

## Testing Strategy

### Focused Coverage

Add focused sample-oriented verification rather than re-testing all existing product behavior.

Preferred coverage:

- a CLI-focused test proving the sample-owned composition root can register services and invoke `CliApplication.RunAsync(...)`
- a Web-focused test proving the sample-owned host can register `AddReplayLabWeb()`, expose required static assets/pages, and serve the ReplayLab UI

These tests should validate the sample or the sample composition helpers directly. They should not duplicate the full existing CLI/Web behavior coverage already present elsewhere in the repo.

For CLI mode specifically, verification should assert that `ReplayLab.Cli.Hosting.CliApplication.RunAsync(...)` uses the sample-owned parser and sender factory resolved from DI.

For Web mode, verification should prove the sample can host `ReplayLab.Web.Hosting` through `AddReplayLabWeb()` and `MapReplayLabWeb()`.

If the current Web workflow does not yet consume parser or sender behavior from DI, the implementation should document that limitation rather than expanding `#75` into a Web-internal refactor. Do not over-refactor Web internals in this issue.

### Existing Coverage Preservation

Existing repo tests should continue to pass unchanged except where solution or project wiring needs to include the sample.

## Documentation

Add documentation only if needed to explain how to run the sample.

If documentation is added, keep it short and practical:

- where the sample lives
- how to run CLI mode
- how to run Web mode
- what the sample is proving about composition ownership

## Risks And Mitigations

### Scope Expansion Risk

Demonstrating both CLI and Web in one sample can drift into building a new product shell.

Mitigation:

- keep mode selection simple
- keep synthetic services minimal
- avoid adding extra UX, configuration, or business behavior

### Composition Blurring Risk

The sample could accidentally hide composition inside helper code that resembles a second container or product shell.

Mitigation:

- keep service registration explicit in sample-owned code
- keep ReplayLab integration limited to the public hostable APIs
- avoid new abstractions unless they make ownership clearer

### Test Duplication Risk

Sample tests could re-run product-level behavior instead of composition-specific proof.

Mitigation:

- target sample composition boundaries only
- verify that sample-owned registrations are the services consumed by the hostable entry points

## Acceptance Alignment

This design satisfies issue `#75` if the implementation results in:

- a generic sample under `samples/`
- explicit external composition-root ownership in sample code
- sample-owned DI registration for parser and sender services
- consumption of at least one hostable entry point, preferably both CLI and Web from the same sample host
- observable proof in tests that the CLI hostable path consumed sample-owned DI services
- Web proof focused on external hosting of `ReplayLab.Web.Hosting`, without expanding into `#76` or broader Web workflow refactoring
- focused verification proving the sample works
- no business-specific types, no hidden composition root, and no scope creep into `#76`
