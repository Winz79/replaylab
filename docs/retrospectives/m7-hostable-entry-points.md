# M7 Retrospective - Hostable Entry Points

## What Shipped

M7 delivered hostable CLI and Web entry points, plus a synthetic sample that proves external-style composition.

Shipped items:
- `ReplayLab.Cli.Hosting` reusable CLI runner
- `ReplayLab.Web.Hosting` ASP.NET Core hosting hooks
- `samples/ReplayLab.HostSample` for private-style composition
- Documentation for the consumption model and scope boundaries

## What Changed From Plan

- CLI hostability consumed external parser and sender services through DI as planned.
- Web hostability was delivered as `AddReplayLabWeb()` and `MapReplayLabWeb()`, but external parser/sender DI consumption was deferred.
- The repo kept the runnable `ReplayLab.Cli` and `ReplayLab.Web` shells intact.

## Decisions Made

- Keep hostable surfaces in companion libraries rather than moving startup into the app shells.
- Keep private composition root ownership outside the public repo.
- Document the current Web limitation explicitly instead of widening scope.

## Risks That Remain

- Web still uses its internal CSV parser and mock sender path.
- External Web parser/sender composition remains deferred.
- Packaging and release automation remain separate from hostability work.

## Follow-Ups

- Web parser/sender DI consumption.
- Package publishing and release automation.
- Future hostability or desktop-shell work.

## Process Notes

- M7 closed with the expected issue and PR trail.
- The closeout workflow stayed documentation-first and release-focused.
