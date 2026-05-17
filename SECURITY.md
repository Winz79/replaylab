# Security Policy

## Supported Versions

ReplayLab is currently in early development and has not published a stable release yet.

| Version | Supported |
| ------- | --------- |
| main    | Best effort |

## Reporting a Vulnerability

If you find a security issue, please do not open a public issue with exploit details.

Instead, report it privately using GitHub Security Advisories if available, or contact the maintainer directly.

Please include:

- a clear description of the issue
- affected component
- reproduction steps if possible
- potential impact
- suggested mitigation if known

## Scope

ReplayLab is a generic replay/testing toolkit.

Security reports are especially relevant for:

- unsafe file parsing
- command execution risks
- path traversal
- insecure handling of secrets
- unsafe logging of sensitive data
- dependency vulnerabilities
- future network sender adapters

Out of scope:

- vulnerabilities in private adapters not hosted in this repository
- customer-specific integrations
- proprietary WCF contracts
- reports based on synthetic or non-exploitable scenarios only

## Response Expectations

This project is maintained on a best-effort basis.

Valid reports will be reviewed and addressed depending on severity and project maturity.
