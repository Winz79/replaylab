# Web Hostable Surface Design

## Goal

Implement issue `#74` by extracting the current repo-owned Web startup path into a reusable hostable Web surface that matches ADR `0009`, while keeping `ReplayLab.Web` as the runnable shell and preserving existing behavior.

## Scope

- Introduce a companion hostable Web library, `ReplayLab.Web.Hosting`, or the closest implementation-compatible equivalent.
- Expose the ASP.NET Core composition hooks:
  - `AddReplayLabWeb(this IServiceCollection services)`
  - `MapReplayLabWeb(this IEndpointRouteBuilder endpoints)`
- Keep `ReplayLab.Web` as a thin runnable shell that uses the hostable library.
- Preserve the current Web UI behavior and existing Web tests.
- Add focused tests proving a minimal host can reference `ReplayLab.Web.Hosting` directly.

## Out Of Scope

- Desktop AppHost or WebView2 work.
- CSV parser strategy changes.
- Editable Web grid behavior.
- Business-specific adapters, contracts, or mappings.
- Packaging or publishing work.
- Follow-up M7 issues `#75` and `#76`.

## Architecture

### Recommended Approach

Create `ReplayLab.Web.Hosting` as the reusable ASP.NET Core surface and extract the generic ReplayLab Web workflow into that companion library using the smallest strategy that is compatible with ASP.NET Core Razor Pages discovery.

`ReplayLab.Web` remains a repo-owned shell that creates the `WebApplicationBuilder`, applies shell-owned middleware and environment policy, calls the hostable extension methods, and runs the app.

This matches ADR `0009` more directly than keeping the reusable surface inside `ReplayLab.Web`, and it gives private hosts a clean composition boundary without moving business-specific composition into the public repo.

Before moving any pages or static assets, inspect the current `ReplayLab.Web` structure and choose the smallest extraction strategy that still produces a real reusable host surface. Do not move Razor Pages or assets blindly.

### Project Boundaries

- `ReplayLab.Web.Hosting`
  - owns the reusable Web composition boundary
  - owns the generic Razor Pages workflow and page model behavior using the smallest compatible extraction strategy
  - owns the public extension methods `AddReplayLabWeb()` and `MapReplayLabWeb()`
- `ReplayLab.Web`
  - remains the runnable shell in this repo
  - owns `Program.cs`, appsettings, and app-level environment policy
  - delegates service registration and endpoint mapping to `ReplayLab.Web.Hosting`
- `ReplayLab.Core`, parser projects, and adapter projects
  - keep their current responsibilities unchanged

## Composition Boundary

### Public API

The public hostable API should stay small and aligned with ADR `0009`:

```csharp
IServiceCollection AddReplayLabWeb(this IServiceCollection services)
IEndpointRouteBuilder MapReplayLabWeb(this IEndpointRouteBuilder endpoints)
```

### Ownership Rules

The host owns:

- `WebApplicationBuilder`
- service registration order outside ReplayLab's own additions
- configuration and logging
- environment-specific middleware and app lifetime

ReplayLab owns:

- registration of the generic Web workflow surface
- mapping of the ReplayLab Web pages/endpoints
- the generic request handling behavior already present in the current Web app

The hostable library must not create its own app builder, hide a second composition root, or introduce business-specific wiring.

`MapReplayLabWeb()` must only map ReplayLab-owned pages or endpoints. It must not configure global middleware, static files, exception handling, environment policy, or app lifetime.

## Request Flow

1. A host creates a `WebApplicationBuilder`.
2. The host calls `services.AddReplayLabWeb()`.
3. ReplayLab registers Razor Pages and any supporting services needed for the generic page workflow.
4. The host builds the app and applies host-owned middleware.
5. The host calls `app.MapReplayLabWeb()`.
6. Requests to the ReplayLab Web surface are handled through the reusable library.
7. The existing CSV upload, preview, selection, and mock replay workflow continue to behave as they do today.

## UI And Behavior Preservation

The extracted hostable surface should preserve the current UI and interaction model:

- the home page remains the existing ReplayLab Web page
- CSV upload continues to parse via the current generic parser path
- preview/grid state continues to be rendered the same way
- replay continues to use the current mock sender workflow
- warnings, summaries, and validation messages stay behaviorally equivalent

The extraction is architectural, not a product UX redesign.

## Error Handling

### Host-Owned Policy

`ReplayLab.Web` should keep the current shell-owned exception handling policy:

- non-development exception handling remains configured in the runnable shell
- static files and routing middleware remain host-managed

`MapReplayLabWeb()` should not force global exception handling, static file registration, or environment policy.

### Workflow-Owned Validation

The reusable page workflow should continue to own its current in-page validation behavior:

- missing upload errors
- CSV parse failures
- replay selection validation
- resend confirmation warnings

Those behaviors should remain page-level responses rather than being converted into a different API or endpoint model.

## Testing Strategy

### Existing Tests

Keep the current `ReplayLab.Web.Tests` flow tests passing against the runnable `ReplayLab.Web` shell.

### New Hostability Tests

Add focused tests that prove hostability through a minimal test host that references `ReplayLab.Web.Hosting` directly and does not depend on `ReplayLab.Web.Program`.

The new coverage should demonstrate that:

- `AddReplayLabWeb()` and `MapReplayLabWeb()` are sufficient to light up the ReplayLab Web UI
- the existing upload and replay workflow functions through the extracted composition boundary
- the shell is no longer required to prove the generic Web behavior itself
- any required Razor Class Library, application-part, or static-asset configuration is explicit, documented in code/comments or docs only if needed, and proven by the minimal-host test

### TDD Execution Rule

Implementation should follow TDD:

1. add the failing minimal-host tests first
2. verify they fail for the missing hostable Web surface
3. extract the hostable implementation with the smallest coherent change
4. run the new hostability tests and the existing Web tests until both are green

## Implementation Notes

- The extraction should prefer the smallest coherent move that yields a real companion host library.
- Keep the public API minimal and avoid speculative abstractions.
- Favor implementation compatibility with the existing Web UI over premature restructuring.
- Update docs only if the resulting implementation needs a small usage note to reflect the actual hostable surface.

## Risks And Mitigations

### Razor Pages Asset/Discovery Risk

Razor Pages discovery and related static asset behavior are the main technical risks in this extraction. Moving reusable UI elements into a separate library can introduce page discovery, application-part, Razor Class Library, or asset resolution issues.

Mitigation:

- keep the extraction narrow
- inspect the current Web project structure before selecting the extraction strategy
- prove page discovery through a minimal host test
- preserve the current shell tests to catch regressions in the runnable app

### Shell Drift Risk

If `ReplayLab.Web` keeps too much setup logic, the hostable library may not be sufficient for real reuse.

Mitigation:

- keep the shell thin
- ensure the minimal test host only uses the public extension methods to exercise the reusable workflow

### Over-Extraction Risk

Refactoring too aggressively could create new abstractions that ADR `0009` does not require.

Mitigation:

- keep the API to the two composition hooks
- preserve the current workflow behavior with minimal structural change beyond the library boundary

## Acceptance Alignment

This design satisfies issue `#74` if the implementation results in:

- a stable public hostable Web surface in a companion library
- a thin `ReplayLab.Web` runnable shell using that surface
- preserved current Web behavior
- preserved existing Web tests
- new focused tests proving a minimal host can consume `ReplayLab.Web.Hosting`
- no scope creep into `#75` or `#76`
- no business-specific types or composition added to the public repo
