# ReplayLab agent guide

ReplayLab is a .NET 10 replay/testing toolkit for loading structured replay messages and sending them through configurable adapters.

## Repo facts
- Solution: `ReplayLab.sln`
- Target framework: `net10.0`
- Pinned SDK: `10.0.203` from `global.json`
- Main apps: `src/ReplayLab.Cli`, `src/ReplayLab.Web`
- Tests live under `tests/`
- Public architecture boundary: Core -> Parsers/Adapters -> Applications
- Keep business-specific and private adapters out of this repository

## Working rules
- Prefer minimal, focused changes.
- Preserve public/private adapter boundaries described in `README.md` and the ADRs.
- Do not introduce business-specific integration code into this public repo.
- This repo's workflow is heavily GitHub-issue-driven; when planning or executing work, check whether there is a relevant issue, issue scope, or issue discussion to align with.
- When changing behavior, add or update tests.
- Before finishing, run the nearest relevant `dotnet test` scope first, then broader validation if the change crosses boundaries.
- Respect `global.json` and use the pinned .NET SDK for build/test commands.

## Validation
- Build whole solution: `dotnet build ReplayLab.sln`
- Test whole solution: `dotnet test ReplayLab.sln`
- For targeted work, prefer the closest affected test project before broader runs.
- If changing hostable entry points or shared abstractions, consider CLI, Web, adapters, and sample host impact.

## Architecture guardrails
- `ReplayLab.Core` must stay generic.
- Parsers and adapters may depend on Core.
- Applications may depend on Core and extension packages.
- Core must not depend on UI, sender implementations, private business contracts, or private systems.
- WCF, proprietary, certificate-specific, and customer-specific adapters stay outside this repo.

## Review checklist
- Is the change still aligned with the public/private adapter boundary?
- If `ReplayLab.Core` changed, were downstream impacts checked?
- Were tests updated at the right scope?
- Were docs/ADRs updated if the architectural contract changed?
