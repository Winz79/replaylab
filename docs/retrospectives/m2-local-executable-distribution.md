# M2 Retrospective - Local Executable Distribution

## Main Learning

M2 was intentionally small: prove that ReplayLab can be published and run as a
local executable with `dotnet publish`.

The real workflow learning is that M2 was too small to justify as much process
as it received. For a tiny milestone, the process should be smaller too. In
practice, too many planning and documentation artifacts can turn a simple
`dotnet publish` path into eight Markdown files around one command.

Original note:

> Le vrai apprentissage ici : M2 était trop petit pour mobiliser autant de process. C'est utile à savoir pour ton workflow. Petite milestone = moins d'artefacts, sinon tu te retrouves avec 8 fichiers Markdown pour un dotnet publish.

## What Worked

- The implementation stayed inside the M2 boundary.
- `dotnet publish` was enough for the first local executable distribution path.
- The published executable was verified against `samples/basic.csv`.
- Docker, NuGet publishing, release automation, Web UI, HTTP sender, WCF/private
  adapters, persistence, configuration DSL, and M3 work stayed out of scope.
- The PR kept assumptions, risks, verification, and out-of-scope items visible.

## What Was Too Heavy

- The amount of documentation exceeded the complexity of the implementation.
- The PR accumulated planning, ADR, PRD, milestone, roadmap, retrospective, and
  follow-up material for a change whose core implementation was one publish
  command and one verification helper.
- The process cost became a visible part of the work instead of staying in the
  background.
- Small review fixes also required updating multiple documentation references,
  which is a sign that the artifact graph was too large for the milestone size.

## Workflow Adjustment

For small milestones:

- Prefer updating existing docs over creating new documents.
- Use one compact planning artifact unless a decision truly needs an ADR.
- Keep issue drafts lightweight.
- Capture future work as a short follow-up note instead of expanding the PR.
- Do not add extra process just because the repository has a place for it.
- Treat "boring and useful" as a process constraint, not only an implementation
  constraint.

## Future Rule Of Thumb

If the implementation is mostly a command, script, or narrow documentation
update, the process should fit on one page. More structure is useful only when
it reduces ambiguity or prevents real scope creep.
