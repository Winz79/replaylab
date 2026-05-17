# BRD 0002: Public Scope and Boundaries

## Status

Draft

## Public Scope

ReplayLab public scope includes:

- Generic replay engine.
- Generic message model.
- Generic parser contracts and parser implementations for public file formats.
- Mock sender.
- Optional HTTP sender.
- CLI experience.
- Local web UI.
- Docker packaging.
- Documentation and examples using synthetic data only.

## Out Of Scope

ReplayLab public scope excludes:

- Proprietary business formats.
- Internal WCF contracts.
- Company-specific mappings.
- Real certificates.
- Real customer data.
- Internal service endpoints.
- Business-specific adapters.
- Private replay scenarios that cannot be shared publicly.

WCF and business-specific adapters must live outside the public repository.

## Boundary Rule

If a feature requires private contracts, internal endpoints, customer data, real certificates, or domain-specific message interpretation, it does not belong in the public ReplayLab repository.

## Allowed Extension Direction

Private or business-specific integrations may consume public ReplayLab packages through separate repositories or packages, provided they do not force public core concepts to become business-specific.

## Assumptions

- Generic adapters can exist in the public repo when they are broadly reusable.
- Private adapters can depend on public ReplayLab contracts without creating a reverse dependency.
- Examples should use synthetic messages only.

## Open Questions

- Should official generic adapters live in this repository or separate adapter repositories?
- What contribution rules should reject business-specific examples or mappings?
- Should the project include sample private-adapter guidance without implementation?
