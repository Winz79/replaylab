# ADR 0009: M7 Hostable Entry Point Architecture

## Status

Accepted

## Context

M6 completed the private adapter extension model in ADR 0008. Private projects
can now implement `IReplaySender` and `IMessageParser`, register their own
services through DI, and compose against `ReplayLab.Core` without modifying the
public repo.

That M6 boundary is intentionally incomplete for CLI and Web reuse. The current
`ReplayLab.Cli` and `ReplayLab.Web` entry points are still repo-owned apps, not
cleanly reusable host surfaces. A private project can build its own composition
root today, but it cannot yet reuse ReplayLab's CLI or Web entry behavior
without cloning or reshaping repo-owned startup code.

## Problem

ReplayLab has a usable private extension model, but private projects cannot yet
consume the public CLI and Web entry points cleanly. M7 needs a hostable
architecture that preserves the M6 public/private boundary while making generic
CLI and Web workflow behavior reusable.

## Decision Direction

ReplayLab will expose hostable CLI and Web entry points in M7.

Direction:

- expose hostable CLI and Web entry points
- keep private projects responsible for composition root ownership
- require private projects to register their adapters and parsers through DI
- keep ReplayLab responsible for generic CLI and Web workflow behavior
- keep business-specific composition out of the public repo

This ADR accepts the architecture direction for M7, not the exact implementation
signatures. Exact CLI API shape, exact Web API shape, package boundaries, and
packaging timing are resolved below for the next M7 slices.

## Options Considered

### 1. Keep CLI and Web as repo-owned apps only

Rejected as the M7 direction.

This preserves the current app boundary, but it leaves private projects unable
to reuse ReplayLab CLI/Web behavior cleanly.

### 2. Expose hostable entry APIs

Accepted direction.

This keeps ReplayLab responsible for generic workflow behavior while allowing
private hosts to own service registration and composition.

### 3. Create separate hostable packages

Possible packaging direction, but not the architectural decision itself.

This becomes a later packaging concern. M7 resolves the host boundary and API
surface first.

### 4. Implement Desktop AppHost now

Rejected for M7.

Desktop AppHost depends on hostable Web entry points and remains future work.
It should not be used to define M7 architecture prematurely.

## Consequences

- ReplayLab gets a clearer public/private boundary for entry-point reuse.
- Private projects gain reusable CLI and Web entry points without moving
  business-specific composition into the public repo.
- M7 will need a small, versioned API surface for hostable entry points.
- Composition-root ownership remains with the private project, consistent with
  the M6 extension model.
- Desktop AppHost remains future work after hostable Web entry points exist.

## Explicit Non-Goals

- Editable Web grid values before replay (`#68`).
- RFC-compliant CSV parser strategy (`#69`).
- Desktop AppHost with WebView2 (`#70`).
- Business-specific adapters, composition, or mappings.
- Product UX expansion beyond current generic CLI/Web workflows.

## Resulting Guidance

- CLI hostability should be implemented as a reusable async runner in a
  companion host library.
- Web hostability should be implemented as ASP.NET Core composition hooks in a
  companion host library.
- Private hosts own the composition root and DI graph.
- Current app projects remain runnable shells.
- Packaging happens after the API shape is proven in M7.
