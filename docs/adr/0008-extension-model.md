# ADR 0008: M6 Private Adapter Extension Model

## Status

Accepted - M6 extension model implementation is complete across issues #56,
#57, #58, and #59. NuGet packaging for `ReplayLab.Core` is packageable and
pack verified. Publication is intentionally out of scope.

## Context

M1-M5 established the core replay model, CSV parser, sequential replay engine,
mock sender, HTTP sender, CLI, and Web UI. All of these live in the public repo
and are composed internally.

No mechanism existed for a private project to extend ReplayLab without forking
or cloning the public repo. The public contracts in `ReplayLab.Core` had minor
inconsistencies (casing, nullable ambiguity, constructor vs init confusion) that
made the extension surface unclear. DI registration helpers and an example
adapter were also missing. For packaging, `ReplayLab.Core` needed explicit
NuGet metadata and pack verification.

M6 addresses this by hardening the public contracts, adding DI registration
helpers per adapter/parser project, adding a compilable example adapter, and
preparing `ReplayLab.Core` for NuGet packaging.

## Decisions

### 1. Public Extension Points

The following types in `ReplayLab.Core` are the stable public extension surface:

- `IReplaySender` - implement this to create a custom sender adapter.
- `IMessageParser` - implement this to create a custom parser.
- `ReplayMessage` - consume this as the generic message model.
- `ReplayResult` - produce and consume this as the replay result model.
- `ReplayBatch` - consume this as the parsed message collection.
- `SequentialReplayEngine` - use this to drive replay through any `IReplaySender`.

Private projects implement `IReplaySender` and/or `IMessageParser`. They
consume the rest. They do not reimplement `SequentialReplayEngine` unless they
need a different execution model.

### 2. Contract Hardening (Breaking Changes for M6)

M6 implemented breaking changes to `ReplayLab.Core` to make the extension
surface cleaner. These changes are recorded below as implemented in issue #56.

Implemented breaking changes:

- `ReplayResult` constructor was removed and replaced with object initializer syntax.
  - All usages updated to use `{ Success = ..., MessageId = ... }` pattern.
  - Properties remain `init`-only, preserving immutability.
  - `Status` is now a computed get-only property derived from `Success`, ensuring consistency.
  - This removes the constructor vs init ambiguity and prevents inconsistent state.

- `ReplayMessage.Headers` and `ReplayMessage.Metadata` changed from nullable optional
  parameters to required non-nullable parameters.
  - Changed from `IReadOnlyDictionary<string, string>?` to `IReadOnlyDictionary<string, string>`.
  - This clarifies intent: parsers should provide empty dictionaries when no headers/metadata
    are applicable, rather than passing null.

All call sites throughout the codebase and tests have been updated to match
these new signatures.

### 3. DI Registration Pattern

Each adapter and parser project provides its own `IServiceCollection` extension
methods. `ReplayLab.Core` does not gain a dependency on
`Microsoft.Extensions.DependencyInjection` or its abstractions.

- `ReplayLab.Adapters.Mock` - `AddMockReplaySender(this IServiceCollection)`
- `ReplayLab.Adapters.Http` - `AddHttpReplaySender(this IServiceCollection, ...)`
- `ReplayLab.Parsers.Csv` - `AddCsvMessageParser(this IServiceCollection)`
- `ReplayLab.Adapters.Example` - `AddExampleReplaySender(this IServiceCollection)`
- Wherever `SequentialReplayEngine` lives - `AddSequentialReplayEngine(this IServiceCollection)`

Each extension method references only
`Microsoft.Extensions.DependencyInjection.Abstractions`.

### 4. Example Adapter

`ReplayLab.Adapters.Example` was added to the solution (issue #58) as a
thin, fictional sender adapter (`FileReplaySender`) that depends on
`ReplayLab.Core` and `Microsoft.Extensions.DependencyInjection.Abstractions`.
It is explicitly labelled as an extension pattern reference, not a production
adapter. It lives in `src/ReplayLab.Adapters.Example` and is part of
`ReplayLab.sln`.

### 5. NuGet Packaging Scope For M6

`ReplayLab.Core` is prepared for NuGet packaging in M6 (issue #59).
Standard `.csproj` metadata is added and `dotnet pack` is verified to
produce a valid package. Actual publication to NuGet.org is a separate
operational step outside this milestone scope.

Packaging for `ReplayLab.Cli`, `ReplayLab.Web`, parser projects, and adapter
projects is deferred to M7.

### 6. Version Strategy

`ReplayLab.Core` starts at version `0.6.0` for the M6 packageable state.
Pre-1.0 was chosen because the extension surface is hardened but long-term
public API stability commitments are still intentionally conservative at this
stage. Moving to `1.0.0` should happen only when maintainers explicitly declare
stable public API guarantees.

### 7. Consumption Model

A private adapter project should:

1. Use a local project reference during in-repo development:
   `<ProjectReference Include="..\ReplayLab.Core\ReplayLab.Core.csproj" />`.
2. Use a package reference for private adapters outside this repository:
   `<PackageReference Include="ReplayLab.Core" Version="0.6.0" />`.
3. Configure a package source that points to a local package folder or private
   feed.
4. Implement `IReplaySender` (and optionally `IMessageParser`).
5. Use the project's own `IServiceCollection` extension method to register its
   adapter.
6. Use `SequentialReplayEngine` from the DI container to drive replay.

Hostable CLI and Web entry points are M7 scope. In M6, the private project
owns its own composition root.

## Rationale

Keeping DI helpers out of Core preserves Core's dependency-free status. Each
adapter/parser project self-registers, which is the standard .NET library
pattern and keeps the extension model opt-in.

Preparing Core for NuGet packaging in M6 keeps packaging scope small while
proving the consumption model. CLI and Web entry points require a larger refactor
(they are currently entry points, not libraries) and belong in M7.

Accepting breaking changes in M6 is intentional - this is the last milestone
before external adopters are expected to depend on the public contracts.

## Consequences

Once M6 is complete:

- Private projects will be able to reference `ReplayLab.Core` as a NuGet
  package and implement `IReplaySender` without cloning this repo.
- Each adapter and parser project will gain a dependency on
  `Microsoft.Extensions.DependencyInjection.Abstractions`.
- `ReplayLab.Adapters.Example` will be part of the solution and built and tested
  with every `dotnet test ReplayLab.sln` run.
- Any future breaking changes to `ReplayLab.Core` require explicit versioning
  discipline and ADR documentation before release.

## Alternatives Considered

### DI helpers in Core

Rejected. Core would gain a dependency on DI abstractions, which conflicts with
the architecture decision to keep Core independent from infrastructure concerns.
The per-project registration pattern is more composable and keeps Core generic.

### Assembly scanning for adapter discovery

Rejected for M6. Convention-based discovery adds runtime complexity and makes
the extension model harder to reason about. Explicit DI registration is
predictable and standard.

### Publish all packages in M6

Rejected. CLI and Web are currently entry points, not libraries. Making them
hostable requires a non-trivial refactor that belongs in its own milestone (M7).

## Out Of Scope

- WCF, proprietary contracts, or business-specific mapping code.
- Hostable CLI or Web entry points (M7).
- AppHost or desktop entry point (M7 or later).
- NuGet publishing for CLI, Web, parsers, or adapters.
- CI/CD pipeline for automated publishing.
- Multi-targeting.
- Authentication, persistence, Docker, or deployment.

## Guidance For Future Agents

- Do not add business-specific types to `ReplayLab.Core`.
- Do not add DI dependencies to `ReplayLab.Core`.
- Any breaking change to public contracts after M6 requires a major version bump
  and a new ADR entry.
- The example adapter exists to demonstrate the extension pattern - do not grow
  it into a production adapter.
- M7 owns the hostable entry point refactor. Do not start that work in M6.
