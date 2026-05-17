# ADR 0001: Use .NET

## Status

Accepted

## Context

ReplayLab needs a simple, maintainable foundation for a replay/testing toolkit with libraries, tests, and a future CLI. The local machine has current .NET SDKs installed.

## Decision

Use .NET with a standard solution layout:

- Core class library.
- Adapter class libraries.
- Console application for future CLI work.
- xUnit test projects.

The initial scaffold targets `net10.0` because the current default stable SDK available on this machine is .NET 10.

After review, ReplayLab will keep `net10.0` for the next vertical slice.

## Consequences

- The repository can use standard `dotnet build` and `dotnet test` workflows.
- The public API can be packaged later without changing the basic layout.
- If broader consumer compatibility is required, the target framework may need to be revisited before the first package release.

## Assumptions

- .NET 10 is acceptable for the scaffold and the next concrete implementation slice.
- Compatibility decisions can still be revisited before publishing packages.

## Open Questions

- Should the first public package release multi-target a lower LTS framework for broader adoption?
