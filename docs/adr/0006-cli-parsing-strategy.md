# ADR 0006: CLI Parsing Strategy

## Status

Accepted

## Context

ReplayLab's CLI started with a deliberately small manual parsing surface in M3:

- `replaylab <file>`
- `replaylab --format csv <file>`

That scope was small enough to keep parsing logic readable without introducing
another dependency before the CLI shape was better understood.

M4 adds the next CLI pressure point. The planned HTTP Sender Preview introduces
sender selection and endpoint URL input, and it is a likely entry point for
future preview options such as:

- `--sender`
- endpoint URL
- `--dry-run`
- `--inspect-only`
- `--max-count`
- config file input
- subcommands

Continuing to grow manual parsing past the M3 surface would make argument rules
harder to extend, test, and document consistently.

## Decision

ReplayLab will adopt `System.CommandLine` before implementing M4 sender
selection such as `--sender http`.

Manual parsing was acceptable for M3 because the CLI only needed to support one
compatibility command and one explicit format option. That threshold has now
been reached. M4 sender selection and related preview inputs are sufficient
reason to stop growing the manual parser.

This ADR records the strategy decision only. It does not migrate the CLI in the
same slice unless a separate implementation issue explicitly scopes that work.

## Rationale

`System.CommandLine` is a better fit once the CLI moves beyond a tiny linear
argument shape.

The M4 command surface needs more durable parsing behavior because it will need
to:

- validate multiple related options coherently
- keep usage/help output consistent
- support clearer error handling as the CLI grows
- reduce ad hoc branching in `Program.cs` and `CliApplication`
- give future issues a stable parsing model before adding more preview options

Keeping the M3 parser in place for one more milestone would save little and
would push the migration into the middle of feature work that already depends on
more complex argument handling.

## Consequences

- M4 sender-selection work should begin with CLI parser migration, not by
  extending the current manual parsing logic.
- Future CLI option work can assume a structured parser rather than adding more
  manual argument branching.
- The current M3 commands remain valid evidence that manual parsing was an
  acceptable interim step, not a long-term direction.
- This decision does not force immediate adoption of subcommands, config files,
  or additional preview features; it only sets the parser strategy before they
  are added.

## Triggers Confirmed By This Decision

The following inputs now count as confirmed triggers for using
`System.CommandLine` rather than extending manual parsing:

- `--sender`
- endpoint URL input
- `--dry-run`
- `--inspect-only`
- `--max-count`
- config file input
- subcommands

## Alternatives Considered

### Keep Manual Parsing Through M4

ReplayLab could keep extending the current manual parser for sender selection
and endpoint URL input, then revisit parser migration later. This was rejected
because M4 is exactly the point where the CLI stops being a trivial two-shape
surface.

### Migrate Only After Several More Options Exist

ReplayLab could wait until multiple new preview flags exist before adopting a
real parser. This was rejected because it would make the migration happen after
manual parsing had already spread into the next feature slice.

### Adopt `System.CommandLine` Before M4 Sender Work

ReplayLab can migrate parsing at the point where M4 introduces sender
selection and endpoint URL input. This keeps the parsing decision aligned with
the first real CLI growth threshold and avoids making HTTP preview work depend
on a parser shape the project already expects to replace.

## Guidance for Future Agents

- Treat M3 manual parsing as an intentionally temporary solution.
- Do not add `--sender http` on top of the current manual parser.
- Do not use this ADR to justify broader CLI redesign beyond parser adoption.
- Keep parser migration separate from HTTP sender implementation unless a future
  issue explicitly batches them together.
