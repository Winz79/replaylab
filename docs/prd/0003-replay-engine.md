# PRD 0003: Replay Engine

## Status

Draft

## Purpose

Coordinate sending messages from a `ReplayBatch` through an `IReplaySender` and collect replay outcomes.

## Users

- Developers replaying batches in tests.
- CLI users running local replay commands.
- Future UI users replaying selected messages.

## Requirements

- Accept a batch of replay messages.
- Send each selected message through a configured sender.
- Return per-message `ReplayResult` values.
- Preserve message order unless requirements change.
- Support cancellation.
- Avoid transport-specific assumptions.

## Acceptance Criteria

- A batch can be replayed through `MockReplaySender`.
- Results can be correlated to input message IDs.
- Failed sends do not hide which message failed.
- Engine remains independent from specific sender implementations.

## Out Of Scope

- Retry policy.
- Scheduling.
- Persistence.
- Parallel execution.
- UI progress reporting.

## Assumptions

- Sequential replay is the safest first behavior.
- Retry and parallelism should wait until result semantics are clearer.

## Open Questions

- Should replay stop on first failure or continue by default?
- Should skipped messages have a distinct result state?
- Should replay options live in core or a separate application layer?
