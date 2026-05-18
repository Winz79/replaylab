# M3 Retrospective - Configurable Replay Inputs

## What Shipped

M3 stabilized the first explicit replay-input command shape while keeping the
existing local CLI path usable.

The milestone outcome was intentionally small:

- `replaylab <file>` remains the compatibility command
- `replaylab --format csv <file>` is the explicit M3 command shape
- unsupported formats and malformed `--format` usage fail clearly
- source and published-executable usage docs now show the stable M3 path
- the mock sender remains the default sender

## What Worked

- The milestone stayed inside the public-repo boundary and did not pull in HTTP,
  Web UI, WCF, private mappings, or configuration-dsl work.
- Keeping CSV as the only supported format made the command shape easier to
  stabilize without over-designing future parser selection.
- CLI-level tests and smoke tests were enough to lock down the user-facing
  behavior.
- The published executable story stayed aligned with the source-run workflow,
  which reduced documentation drift.

## What Was Too Heavy

- M3 created more planning discussion than implementation complexity required.
- Some planning references pointed to documents that were never formalized at
  the final repository paths, which made later cleanup harder.
- The roadmap temporarily carried too much milestone-specific planning detail
  instead of staying directional.

## Workflow Issues

- The repository benefited from milestone and issue structure, but the process
  still needs stricter boundaries about where planning artifacts belong.
- Roadmap, milestone planning, and retrospective content can drift into each
  other if the folder structure is not kept explicit.
- Documentation-only cleanup work became necessary before M4 planning could
  start cleanly.

## Decisions Made

- Keep the M3 command surface small and CLI-first.
- Keep the mock sender as the default sender.
- Defer HTTP sender design to M4.
- Keep roadmap content high-level and move milestone-specific planning into
  `docs/milestones/`.

## Follow-Ups

- Clean up the docs structure before detailed M4 planning starts.
- Create M4 planning artifacts under `docs/milestones/` rather than expanding
  `docs/roadmap.md`.
- Add new ADRs or PRDs for M4 only if M4 planning proves they are needed.

## Next Milestone Recommendation

M4 should plan the first generic non-mock sender as a small HTTP Sender Preview.
The planning should stay focused on one local preview workflow and avoid turning
sender configuration into a broad integration framework too early.
