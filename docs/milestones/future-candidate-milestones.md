# Future Candidate Milestones

This file collects post-M13 candidate milestone dossiers. These are planning
artifacts, not final commitments. Promote a candidate only after its value,
dependencies, risks, and required artifacts are accepted.

## ~~Candidate M14: SDK Adoption Instrumentation & Polish~~ → COMPLETE

**Delivered.** Issues #134–#137. Closeout via #148.

---

## Candidate M15: Web Deployment & Observability Guide

### Goal

Make the ReplayLab Web UI deployable and observable. Dockerize, auto-deploy on
version tags, and document Cloudflare Tunnel and Seq setup.

### User Value

- Developers can see a running instance of the Web UI without cloning the repo.
- Docker Compose gives a one-command local setup (Web + Seq).
- Cloudflare Tunnel guide provides zero-cost HTTPS for self-hosted deployments.
- Seq guide turns M14's structured logging into visible runtime behavior.

### Dependency On Previous Work

- Builds on M14 (structured logging) for the Seq guide.
- Web app already exists — no new code needed, just packaging and deployment.

### Required Artifacts

- Plan doc: `docs/milestones/m15-deployment-observability.md` (created).
- Issues #141–#144 created under milestone #16.

### Candidate Slices

- Dockerize `ReplayLab.Web` with multi-stage build + docker-compose (Web + Seq).
- GitHub Actions deploy workflow triggering on version tags.
- Cloudflare Tunnel setup guide (`docs/cloudflare-tunnel.md`).
- Seq observability guide (`docs/observability-seq.md`).

### Explicit Non-Goals

- Kubernetes, managed cloud, Cloudflare Pages/Workers.
- Desktop or CLI deployment.
- Seq code dependency.

### Readiness

In progress. #141, #142, #143 delivered. #144 (Seq observability guide) remains.

---

## Candidate M12: Local Sessions / Persistence

### Goal

Persist local replay workspace state later if the product direction requires it.

### Status

**Deferred.** Reopen only when >=1 external consumer exists and the SDK/toolkit
story is stable.

### Possible Future Scope

- Save and reload local replay workspace state.
- Preserve imported messages, edits, selected rows, and replay results.
- Export/import replay plans.
- Keep persistence local-first and business-agnostic.

### Non-Goals

- Database-backed storage, cloud sync, authentication, business-specific persistence.
