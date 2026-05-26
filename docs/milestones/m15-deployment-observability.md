# M15: Web Deployment & Observability Guide

## Status

**Complete.** All four slices delivered: #141 (Dockerize), #142 (Deploy workflow),
#143 (Cloudflare Tunnel guide), #144 (Seq observability guide).

## Intent

Make the ReplayLab Web UI deployable and observable so external developers
can see it running and understand its runtime behavior.

M14 added structured logging. M15 makes that logging visible (Seq) and makes
the app reachable (Docker + Cloudflare Tunnel + GitHub Actions deploy).

## Context

### Where we are

M13 proved the release path (NuGet packages to GitHub Packages). M14 added
`ILogger<T>` structured logging, XML docs, a getting-started guide, and a
GitHub Packages badge. The SDK is now well-documented and observable.

What is missing: the Web UI — the most visible artifact of ReplayLab — has
no deployment path. A developer who discovers ReplayLab today can install
packages and build their own tool, but they cannot see a running instance
of the reference Web UI without cloning the repo and running `dotnet run`.

### Why deployment matters now

- **Credibility**: if the Web UI is the showcase, it should be reachable
  without cloning the repo.
- **Dogfooding**: deploying the reference Web UI exercises the SDK
  composition path and finds gaps before external consumers do.
- **Observability closes the loop**: M14 added logging, but logging is
  invisible without a collector. Seq gives devs a concrete, 5-minute path
  from "I have logs" to "I can see them."

## Scope

### Slice 1: Dockerize the Web app — #141

- Multi-stage Dockerfile for `ReplayLab.Web`:
  - Stage 1: `mcr.microsoft.com/dotnet/sdk:10.0` — restore, build, publish
  - Stage 2: `mcr.microsoft.com/dotnet/aspnet:10.0` — runtime image
- `docker-compose.yml` that runs both the Web app and Seq:
  - Web app on port 5213 (`ASPNETCORE_URLS=http://+:5213`)
  - Seq on port 5341 (ingest) / port 80 (UI)
- `.dockerignore` to exclude unnecessary files
- `docker build` + `docker compose up` works locally

### Slice 2: GitHub Actions deploy workflow — #142

- New workflow `deploy-web.yml` triggered by:
  - Version tags (`v*.*.*`) — deploy alongside NuGet release
  - Manual `workflow_dispatch` for on-demand deploys
- Steps:
  1. Checkout
  2. Setup .NET SDK
  3. Restore, build, test (reuse CI pattern)
  4. Docker build + tag
  5. Push image to GitHub Container Registry (ghcr.io)
  6. SSH into host, pull image, restart container
- Secrets: `DEPLOY_HOST`, `DEPLOY_SSH_KEY`, `DEPLOY_USER`

### Slice 3: Cloudflare Tunnel setup guide — #143

- Document (not automate) how to:
  - Install `cloudflared` on the Docker host
  - Create a tunnel to `http://localhost:5213`
  - Point a domain/subdomain at the tunnel
- Document how to add GitHub OAuth or email-based access control via
  Cloudflare Access (optional polish)
- This is a `docs/` file, not infrastructure code

### Slice 4: Seq observability guide — #144

- Document how to:
  - Run Seq via Docker (or the `docker-compose.yml` from Slice 1)
  - Add `Seq.Extensions.Logging` package to the Web project
  - Configure `appsettings.json` with the Seq endpoint
  - See structured logs from `SequentialReplayEngine`, `CsvParser`, and
    `HttpSender` appear in the Seq UI
- Target: a developer follows the guide and sees their first log message in
  Seq within 5 minutes
- Keep the guide in `docs/` — no code changes to Core or adapters
- Mention alternatives (Loki, ELK, Datadog) in a one-liner for context

### Linked Issues

- #141 — Dockerize ReplayLab.Web with multi-stage build and docker-compose (Web + Seq)
- #142 — GitHub Actions deploy-web workflow triggered by version tags
- #143 — Write Cloudflare Tunnel setup guide for self-hosted Web UI
- #144 — Write Seq observability guide for structured logging

## Out of scope

- Kubernetes, Docker Swarm, Nomad, or any orchestrator
- Managed cloud deployment (Azure App Service, AWS ECS, GCP Cloud Run)
- Cloudflare Pages or Workers (not compatible with server-side Razor Pages)
- Deployment of Desktop or CLI apps
- Seq NuGet dependency in any ReplayLab project — the guide references Seq,
  it does not couple the codebase to Seq
- CI/CD for multiple environments (staging, prod) — single instance
- Health checks, metrics, OpenTelemetry (future candidates)
- Automatic TLS certificate management (Cloudflare Tunnel handles this)

## ADR candidates

None. Dockerizing a Web app and writing a guide do not change architecture
boundaries, composition conventions, or the public API contract.

## Verification expectations

| Slice | Verification |
|-------|-------------|
| #141 Dockerize | `docker compose up` — Web UI loads at `localhost:5213`, Seq UI loads at `localhost:80` |
| #142 Deploy workflow | Manual `workflow_dispatch` triggers build → push → SSH → container restart; Web UI accessible on target host |
| #143 Cloudflare guide | Follow the guide end-to-end on a clean machine; tunnel connects and HTTPS works |
| #144 Seq guide | Follow the guide; engine startup, parse, and send log messages appear in Seq UI |

## Risks

| Risk | Mitigation |
|------|-----------|
| `net10.0` Docker images may not be stable at time of implementation | Pin to a specific tag; fall back to `net9.0` images if necessary |
| Docker Hub rate limiting on CI | Use GitHub Container Registry (ghcr.io) — no rate limits for GitHub Actions |
| Cloudflare Tunnel `cloudflared` breaks between versions | Document a pinned version; the guide is advisory, not automated |
| Seq.Extensions.Logging version incompatibility with `net10.0` | Test before writing the guide; fall back to generic `ILogger` → Seq via OpenTelemetry if needed |
| SSH-based deploy is fragile | Acceptable for a single-instance reference deploy; document as "simple, not production-grade" |
