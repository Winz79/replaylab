# Future Candidate Milestones

This file collects post-M13 candidate milestone dossiers. These are planning
artifacts, not final commitments. Promote a candidate only after its value,
dependencies, risks, and required artifacts are accepted.

## ~~Candidate M14: SDK Adoption Instrumentation & Polish~~ → COMPLETE

**Delivered.** Issues #134–#137. Closeout via #148.

---

## ~~Candidate M15: Web Deployment & Observability Guide~~ → COMPLETE

**Delivered.** Issues #141–#144.

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
