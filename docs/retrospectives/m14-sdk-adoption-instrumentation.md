# M14 Retrospective: SDK Adoption Instrumentation & Polish

## What shipped

| # | Slice | PR | Outcome |
|---|-------|-----|---------|
| #134 | `ILogger<T>` structured logging | #139 | Engine, parser, sender instrumented with Info/Debug/Warning/Error levels. `Microsoft.Extensions.Logging.Abstractions` 9.0.0 added to Core only. 11 spy-logger tests. |
| #135 | XML doc comments on Core API | #145 | Every public type and member documented. `GenerateDocumentationFile` enabled, CS1591 suppressed for now. |
| #136 | `docs/getting-started.md` | #146 | Self-contained guide: NuGet source, PackageReference, custom parser/sender, Web UI hosting, run instructions. |
| #137 | GitHub Packages badge | #140 | Shield.io badge on README, linking to public packages page. |

All 4 slices delivered. Zero bugs, zero regressions. Full test suite passes.

## What changed from the plan

- **Plan followed exactly.** No scope changes, no deferred items, no additions.
- **SpyLogger duplication noted:** Each test project has its own `SpyLogger<T>` helper. This is acceptable for now — extracting a shared test utility would be premature abstraction at this scale.

## Key decisions made

| Decision | Rationale |
|----------|-----------|
| `ILogger<T>?` nullable pattern | Non-breaking. Direct construction without DI passes `null`. Standard .NET pattern. |
| Semantic templates, not string interpolation | Consistent with structured logging best practices. |
| CS1591 suppressed, not enforced | Plan to tighten incrementally once all members are documented. |
| No event IDs | No convention exists yet. Add later if needed. |
| No logging in MockReplaySender | Explicitly scoped out — it's a test adapter. |
| `ReplayMessage.Headers` vs `Properties` in docs | Code uses `Metadata` (not `Properties`). Docs matched actual API. |

## Risks that remain

| Risk | Status |
|------|--------|
| `Microsoft.Extensions.Logging.Abstractions` 9.0.0 on net10.0 | ✅ Validated. Builds and all tests pass. |
| Getting-started guide stale over time | Low. Links to sample, roadmap, and architecture docs for discoverability. |
| SpyLogger duplication across test projects | Low. Acceptable for now. |

## Follow-up

- **M12 persistence** — still deferred. Revisit only when >=1 external consumer exists.
- **NuGet.org publishing** — deferred. GitHub Packages is sufficient.
- **CS1591 tightening** — when Core API stabilizes, switch from `<NoWarn>` to `<WarningsAsErrors>CS1591</WarningsAsErrors>`.
- **SpyLogger extraction** — if a fourth test project needs it, extract to a shared test utility.

## Process improvements

- **Parallel slices worked well:** #135 (docs) and #137 (badge) ran simultaneously — independent, no conflicts.
- **Docs-only PRs can be fast-tracked:** The reviewer approved docs PRs quickly with minimal checks.
- **Conductor merge flow is correct:** Report → ask → merge → cleanup. Guardrails prevent unauthorized merges.
- **Planner → implementer → reviewer → conductor loop is stable:** M14 proved the full flow works end-to-end without process friction.
