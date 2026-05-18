# M4: HTTP Sender Preview

## Goal

Add the first generic non-mock sender by letting developers replay synthetic CSV
messages to a local HTTP endpoint from the CLI.

M4 should prove one small local preview workflow: CSV input is parsed into
generic `ReplayMessage` values, the CLI selects an HTTP sender, and replay
results show request-level success or failure without introducing private
adapter concepts.

## Planning Inputs

- Roadmap: `docs/roadmap.md` marks M4 as the current planning milestone and
  keeps M5 and M6 directional.
- Prior milestone retrospective:
  `docs/retrospectives/m3-configurable-replay-inputs.md` recommends a small
  HTTP Sender Preview and moving detailed M4 planning into
  `docs/milestones/`.
- Relevant PRDs:
  - `docs/prd/0002-file-parsing.md`
  - `docs/prd/0003-replay-engine.md`
  - `docs/prd/0004-sender-adapters.md`
  - `docs/prd/0005-cli-experience.md`
- Relevant ADRs:
  - `docs/adr/0002-separate-core-from-adapters.md`
  - `docs/adr/0004-architecture-style.md`
  - `docs/adr/0005-distribution-strategy.md`
  - `docs/adr/0006-cli-parsing-strategy.md`
- Recent completed work:
  - M3 added `replaylab --format csv <file>` while keeping
    `replaylab <file>`.
  - Recent closed M3 issues covered explicit input configuration behavior,
    unsupported format validation, M3 usage docs, and synthetic examples.
  - Recent merged PRs include M3 command-shape coverage, M3 sample usage, and
    documentation cleanup before M4 planning.
- Resolved dependency:
  - `docs/adr/0006-cli-parsing-strategy.md` records that ReplayLab should adopt
    `System.CommandLine` before implementing M4 sender selection such as
    `--sender http`.

## Revised M4 Scope

### In Scope

- A minimal generic HTTP sender adapter outside `ReplayLab.Core`.
- POST only.
- Endpoint URL as the only required HTTP sender configuration.
- `ReplayMessage.Payload` as the request body.
- Default `Content-Type` of `application/json`.
- Per-message `ReplayResult` values for HTTP success, non-success status codes,
  request exceptions, and cancellation.
- CLI sender selection after `docs/adr/0006-cli-parsing-strategy.md` is in
  place.
- A local synthetic preview workflow using public sample data.

### Out Of Scope

- HTTP method selection, unless a specific implementation need appears.
- Generic header mapping, unless a specific implementation test need appears.
- Body mapping.
- Response body capture.
- Retry policy.
- Authentication.
- Certificate handling.
- Config DSL or config files.
- Long-running daemon, hosted service, scheduler, or background worker.
- Web UI.
- Persistence.
- WCF, private adapters, proprietary formats, customer endpoints, or
  business-specific mappings.

### Constraints

- `ReplayLab.Core` must remain independent from adapters, CLI, UI, persistence,
  Docker, WCF, and business-specific concerns.
- The HTTP sender should live in a separate adapter project, likely
  `ReplayLab.Adapters.Http`.
- M4 should reuse the M3 command/configuration direction and should not invent a
  second configuration model.
- Any CLI parsing change needed for sender selection should follow
  `docs/adr/0006-cli-parsing-strategy.md`.
- Tests should not require external network services. HTTP behavior should be
  covered with an in-process `HttpMessageHandler` test double or local fixture.
- HTTP result semantics should stay inside the minimal adapter issue unless a
  core model change is required.

### Success Criteria

- `dotnet test ReplayLab.sln` passes with HTTP adapter and CLI coverage.
- The existing mock sender CLI workflow still works.
- The CLI can run a documented HTTP preview workflow against a synthetic local
  endpoint or test receiver.
- HTTP preview sends POST requests with `ReplayMessage.Payload` as the body and
  `Content-Type: application/json` by default.
- HTTP failures produce clear non-zero command behavior and per-message failure
  details.
- Documentation states the preview limits and keeps private adapter boundaries
  explicit.

## ADR Candidates

M4 should create ADRs only if the decision changes architecture direction or
creates a reusable precedent.

Candidate decisions:

- Core result model change, only if the minimal HTTP sender cannot report useful
  status information through existing `ReplayResult` fields.

Non-candidates for M4:

- HTTP method selection.
- Header mapping model.
- Body mapping model.
- Response body capture model.
- Retry policy design.
- Authentication or certificate strategy.
- Web UI architecture.
- Private adapter extension model.

## Revised Vertical Slices

### Slice 1: CLI Parsing Decision

This slice is resolved by `docs/adr/0006-cli-parsing-strategy.md`.

M4 should adopt `System.CommandLine` before adding sender selection rather than
extend the current manual parser.

### Slice 2: Minimal HTTP POST Sender

Add the smallest generic HTTP sender adapter project and tests. It should POST
`ReplayMessage.Payload` to a configured endpoint URL with default
`Content-Type: application/json`, then return a `ReplayResult`.

HTTP result semantics belong in this slice. Cover 2xx, non-2xx, request
exception, and cancellation behavior through adapter tests. Split out a separate
design issue only if the implementation proves `ReplayResult` needs a core
model change.

### Slice 3: CLI Sender Selection

Extend the CLI so users can choose the HTTP sender and provide an endpoint URL.
Keep the existing mock-sender commands working and keep mock as the default.

### Slice 4: Local HTTP Preview Documentation

Document and verify a synthetic local HTTP preview workflow. Prefer a simple
local receiver or request-inspector flow that does not require private
infrastructure, secrets, certificates, Docker, or external network services.

### Slice 5: M4 Closeout Documentation

After the workflow is proven, update public docs and add an M4 retrospective.
Keep M5 and M6 directional only.

## Revised Issue Drafts

### Draft 1: Decision: settle CLI parsing strategy for M4 sender options

**Goal:** Resolve issue #31 before M4 adds sender selection.

**Scope:**

- Review GitHub issue #31.
- Decide whether manual parsing remains acceptable for M4's limited sender
  option and endpoint URL.
- Decide whether `System.CommandLine` or another parser package is needed
  before adding sender selection.
- Record concrete triggers for adopting a parser package if M4 does not adopt
  one immediately.

**Acceptance Criteria:**

- The repository has a documented CLI parsing decision.
- The decision explicitly covers M4 sender selection and endpoint URL input.
- Future M4 CLI work has a clear parsing approach to follow.

**Linked Docs or ADRs:**

- `docs/prd/0005-cli-experience.md`
- `docs/retrospectives/m3-configurable-replay-inputs.md`
- `docs/adr/0006-cli-parsing-strategy.md`

**Implementation Notes:**

- This is a documentation or ADR slice only.
- Do not add HTTP sender behavior in this issue.
- Do not add sender selection in this issue.
- Do not add a parser package dependency unless the decision itself requires
  the package migration to happen first.

**Test Expectations:**

- Documentation review only.
- No product behavior tests required unless CLI parsing code changes are
  explicitly included.

**Risks:**

- Deferring this decision could make M4 CLI behavior brittle.
- Adopting a package too early could add dependency churn before the CLI shape
  is stable.

**Out Of Scope:**

- HTTP sender implementation.
- New CLI sender options.
- Config files.
- Subcommands.

### Draft 2: Adapter: add minimal HTTP POST sender

**Goal:** Add a generic HTTP sender adapter that POSTs one replay message to one
configured endpoint URL.

**Scope:**

- Add `src/ReplayLab.Adapters.Http`.
- Add `tests/ReplayLab.Adapters.Http.Tests`.
- Implement a minimal sender that sends `ReplayMessage.Payload` as the POST
  request body.
- Require only endpoint URL for sender configuration.
- Default request `Content-Type` to `application/json`.
- Return successful `ReplayResult` values for 2xx responses.
- Return failed `ReplayResult` values for non-2xx responses and request
  exceptions.
- Test cancellation behavior and keep it consistent with existing replay engine
  semantics.
- Keep `ReplayLab.Core` unchanged unless existing `ReplayResult` fields cannot
  carry enough generic failure information.

**Acceptance Criteria:**

- Tests prove the adapter sends POST requests.
- Tests prove the configured endpoint URL is used.
- Tests prove the request body equals `ReplayMessage.Payload`.
- Tests prove `Content-Type` defaults to `application/json`.
- Tests cover 2xx success, non-2xx failure, request exception failure, and
  cancellation behavior.
- No authentication, retry, certificate, response body capture, method
  selection, header mapping, body mapping, config DSL, or private mapping code
  is introduced.

**Linked Docs or ADRs:**

- `docs/prd/0004-sender-adapters.md`
- `docs/prd/0003-replay-engine.md`
- `docs/adr/0002-separate-core-from-adapters.md`
- `docs/adr/0004-architecture-style.md`

**Implementation Notes:**

- Prefer constructor-injected `HttpClient` for testability.
- Use an in-process `HttpMessageHandler` test double.
- Keep HTTP configuration strongly typed inside the HTTP adapter project.
- Treat result semantics as part of this issue unless a core model change is
  required.

**Test Expectations:**

- Focused adapter unit tests for request shape and result behavior.
- No tests should depend on an external HTTP service.
- Run focused adapter tests, then `dotnet test ReplayLab.sln`.

**Risks:**

- HTTP status reporting may reveal a real gap in `ReplayResult`.
- Exception behavior could conflict with current replay failure semantics.
- Even simple endpoint configuration could become the start of a broader config
  model if the issue is not kept narrow.

**Out Of Scope:**

- CLI sender selection.
- Local sample receiver.
- HTTP method selection.
- Header mapping.
- Body mapping.
- Response body capture.
- Authentication.
- Certificates.
- Retries.
- Config DSL or config files.
- WCF or private adapters.

### Draft 3: CLI: select HTTP sender with endpoint URL

**Goal:** Let CLI users choose the HTTP sender and provide an endpoint URL while
preserving the existing mock sender workflow.

**Scope:**

- Add the sender selection command shape chosen after Draft 1.
- Keep `replaylab <file>` and `replaylab --format csv <file>` working with the
  mock sender.
- Keep mock as the default sender.
- Add an HTTP preview command that requires only endpoint URL.
- Compose `ReplayLab.Adapters.Http` from `ReplayLab.Cli`.
- Print concise per-message replay results using existing replay summary style.

**Acceptance Criteria:**

- Existing mock sender CLI tests still pass unchanged or with intentional
  compatibility updates.
- HTTP sender selection requires endpoint URL and fails early with usage text
  and exit code `2` when missing.
- Unsupported sender names fail early with usage text and exit code `2`.
- HTTP replay failures return non-zero command results with clear message IDs.
- CLI tests cover mock default, HTTP sender selection, invalid sender, and
  missing endpoint URL.
- No method selection, header mapping, body mapping, response capture, auth,
  certificates, retries, config DSL, or config files are added.

**Linked Docs or ADRs:**

- CLI parsing decision from Draft 1.
- `docs/prd/0005-cli-experience.md`
- `docs/prd/0004-sender-adapters.md`

**Implementation Notes:**

- Keep CLI composition in `ReplayLab.Cli`.
- Keep adapter implementation in `ReplayLab.Adapters.Http`.
- Do not make HTTP the default sender in M4.
- Do not add an inspect-only, dry-run, max-count, or subcommand surface as part
  of this slice.

**Test Expectations:**

- `tests/ReplayLab.Cli.Tests` coverage for command parsing and end-to-end
  behavior.
- Adapter tests should remain separate from CLI parsing tests.
- Run focused CLI tests, then `dotnet test ReplayLab.sln`.

**Risks:**

- The CLI option shape may imply more stability than a preview deserves.
- Manual parsing may become error-prone if Draft 1 keeps it and more options
  are added later.

**Out Of Scope:**

- Config files.
- Subcommands unless the CLI parsing decision explicitly requires them.
- Interactive prompts.
- Web UI.
- New parser formats.

### Draft 4: Samples/Docs: document local HTTP POST preview

**Goal:** Provide a reproducible local HTTP POST preview workflow using
synthetic data and public tooling only.

**Scope:**

- Add or update sample documentation for replaying `samples/basic.csv` to a
  local HTTP endpoint.
- Show that the preview sends POST with `ReplayMessage.Payload` as the body and
  default `Content-Type: application/json`.
- Include expected output shape for successful and failed HTTP preview runs.
- Document any required local test receiver command or request-inspector setup.
- Update README usage only after the command shape is proven.
- Keep M5 and M6 as directional roadmap candidates only.

**Acceptance Criteria:**

- A maintainer can follow the sample from a clean checkout.
- The sample uses synthetic data only.
- The sample makes clear that HTTP sender behavior is a preview.
- The sample does not require secrets, certificates, private services, Docker,
  or external network access.
- Documentation explicitly excludes method selection, header mapping, body
  mapping, response body capture, auth, certificates, retries, and config DSL
  from M4.

**Linked Docs or ADRs:**

- `samples/README.md`
- `README.md`
- `docs/roadmap.md`
- `docs/milestones/m4-http-sender-preview.md`

**Implementation Notes:**

- Prefer a local-only receiver or test fixture over a public internet service.
- Keep setup commands short and platform-aware.
- Do not add Docker just to host the receiver.

**Test Expectations:**

- If a local receiver helper is added, cover it with focused tests.
- Verify documented commands manually or through existing smoke-test style.
- Run `dotnet test ReplayLab.sln` if files beyond docs change.

**Risks:**

- Sample setup could become larger than the sender preview itself.
- Public external services could make tests or docs flaky.
- Docs could make preview behavior sound production-ready.

**Out Of Scope:**

- Private endpoints.
- Docker-based receiver.
- Hosted service deployment.
- Authentication examples.
- M5 Web UI planning.
- M6 private adapter planning.

## Recommended Sequence

1. Follow `docs/adr/0006-cli-parsing-strategy.md` for CLI parser migration
   before sender selection work.
2. Add the minimal HTTP POST adapter with result semantics covered in adapter
   tests.
3. Add CLI sender selection using the chosen parsing approach.
4. Document and verify the local HTTP POST preview workflow.
5. Add M4 retrospective notes after the milestone closes.

## Risks

- Sender selection is the first CLI option that may force a parser-library
  decision.
- The current `ReplayResult` model is expected to be sufficient, but HTTP status
  reporting may expose a need for a generic result detail change.
- Endpoint URL configuration could expand into config-file or DSL work if the
  slice is not held to the preview boundary.
- Header and method support are tempting but should wait for a specific test or
  user need.
- A local preview sample can become too large if it tries to ship a full test
  receiver.
- M4 should not make commitments about authentication, retries, Web UI, private
  adapters, or release packaging.

## Directional Roadmap Notes

M5 remains a Minimal Web UI candidate only after CLI and HTTP preview behavior
are small and stable.

M6 remains a Private Adapter Extension Model candidate only after generic sender
learning exposes which extension boundaries need documentation.
