# Changelog

## 1.4.1

- VACA's frontmatter strip no longer empties the file it is stripping.
  `conventions.md` and `sync-epic.md` both prescribed
  `sed '1,/^---$/d; 1,/^---$/d'`, which deletes to end of file whenever the body
  carries no second `---` - the normal case for an epic or a task. It appeared to
  work only on documents that happened to contain a horizontal rule. Syncing the
  `identity-and-roles` epic posted its epic issue with an empty body before this
  was caught; every one of that epic's seven files strips to zero lines under the
  old idiom and correctly under the new one. Both files now use an `awk` form
  keyed on the first two delimiters, which also tolerates CRLF.
- Both call sites check the body is non-empty before posting. An empty body is not
  visibly wrong until the issue exists, and a posted issue cannot be un-posted.
- `sync-epic.md`'s GitHub snippets match the tools. `gh issue create` has no
  `--json` flag - it prints the URL, so the number comes from the URL.
  `gh sub-issue add` takes positional arguments rather than `--parent`, and
  `gh sub-issue create` accepts `--body` but not `--body-file`, so a task body of
  any size is created with `gh issue create` and linked afterwards. All three
  failed during the same sync.

## 1.4.0

- The law stops naming an identity provider. `backend.md` §6 and `frontend.md`
  §9 said "Entra ID"; they now state obligations that hold whichever provider and
  permission source an application runs, and the choice is recorded in that
  application's `DECISIONS.md`. Amendment A-03 in `enforcement.md` records why.
- `backend.md` §6 covers a remote permission authority. An unreachable,
  timing-out, or erroring authority denies (`BE-99`). A cached decision has a
  bounded, declared lifetime that is the revocation window, and an outage never
  extends it (`BE-100`). The authority answers for the user, so for an agent its
  answer is still intersected with the manifest (`BE-103`). Its codes stay in
  `Api` (`BE-102`).
- A transport filter, including a vendor package's, is never the only
  enforcement point: the decision is reachable from the authorization behavior,
  so in-process and agent callers get the same answer (`BE-101`).
- `frontend.md` §9: sign-out ends the identity-provider session, including an
  upstream federated one, so the next person at a shared workstation is prompted
  (`FE-29`). A credential nested inside a token is never extracted, stored,
  forwarded, or logged (`FE-30`).
- The runtime rules are required tests, named in `backend.md` §14
  **Authorization** and `frontend.md` §13 **Authentication**.
- The installed plugin is read-only, and a hook now says so. Claude had tried to
  change the law by editing the plugin cache, which changes every project on the
  machine and is discarded by the next update. `scripts/plugin-guard.sh` runs on
  `PreToolUse` and refuses a write under `~/.claude/plugins/cache/` or
  `~/.claude/plugins/marketplaces/`: exactly for the file tools, on a
  best-effort basis for shell commands. The prime directive states the rule and
  where changes go instead.

## 1.3.0

- `backend.md` §2 admits the two shared platform projects. A modular monolith
  needs its dispatch pipeline somewhere, and the per-module table had no row for
  it: `SharedKernel` is BCL-only because `Domain` references it, and a module
  MUST NOT reference `Api`, so `Platform.Application` and
  `Platform.Infrastructure` are what is left. Both have rows now, and the two
  module rows admit the matching half of the pair.
- The reference table is two columns, projects and packages. It was one column
  mixing both, which is how the `Domain` row's "BCL, `SharedKernel`" reads as a
  statement about packages. Written the same way, `Platform.Application` would
  have been banned from the Mediator and FluentValidation it exists to hold.
  `BE-01` also now has an unambiguous thing to parse.
- `SharedKernel` has a row. It never did, so a check asserting that it declares
  no package was enforcing a rule the law did not state.
- `Result`, the solution-wide page shape and the cursor type live in
  `Platform.Application`. `Domain` does not reference that project, so "the
  domain MUST NOT reference `Result`" is carried by the compiler instead of by an
  analyzer telling two types apart behind one shared reference. `Contracts`
  references `Platform.Application` in turn, because a published query message
  names both its `Result` response and the abstraction it is dispatched by — and
  that abstraction is a package every project defining message types must
  reference, so the published surface could not have been kept package-free by
  any arrangement.
- The `Api` row admits `SharedKernel` and `Platform.Application`. It did not, so
  the `Result`-to-`ProblemDetails` translator the law itself assigns to `Api`
  (`SKILL.md` §6) could not name the type it translates. That defect predates
  everything else here.
- Four facts sit above the table and the table is its consequence: a shared
  project never names a module or `Api`; a module reaches another module only
  through its `Contracts`; inside a module dependencies point inward, with
  `Domain` and `Contracts` as peers that do not name each other; `Api` composes
  and nothing references `Api`. Adding a project is deriving a row from four
  sentences rather than amending the law.
- Every project declares its kind as an MSBuild property. Inferring it from the
  assembly name cannot separate `Platform.Application` from
  `<Module>.Application`, and has no answer at all for a project it has not seen,
  which is how a check acquires a silent escape hatch. A kind that is missing,
  misspelled or absent from the table may reference nothing and be referenced by
  nothing.
- Test projects have a row, and no other kind may reference one.
- A module's projects exist **at most** once per module rather than once. A
  module with no model of its own omits `Domain`; one that publishes nothing to
  its siblings omits `Contracts`. The previous wording made such a module a
  defect by existing.
- `enforcement.md` `BE-41` stops riding on `BE-01`. It was recorded as covered
  because `Application` cannot reference EF; with the unit-of-work port in
  `Platform.Application` a command handler can reach a commit path without ever
  naming EF, so it needs its own assertion.
- Data classification defines each class by a test rather than by a list of
  field names, and the examples are stated to illustrate rather than enumerate.
  A list reads as exhaustive, which is the wrong reading under "unclassified is
  `Restricted`". The law also no longer assumes an HR domain: two assertions
  about one and the field examples drawn from one are gone, and nothing the law
  requires changed with them.

No rule gained or lost a ⚙, so `enforcement.md`'s inventory counts are unchanged.

## 1.2.0

- One issue per session, enforced rather than encouraged. Four hooks:
  `PreToolUse` counts each `vaca-stream` launch, `SubagentStop` counts each
  finish and arms a lock when the counts balance, `UserPromptSubmit` refuses
  every prompt while the lock is armed, and `SessionStart` releases it.
  `UserPromptSubmit` is the only event that can stop a session continuing —
  blocking `Stop` makes Claude carry on instead — and no hook can run `/clear`,
  so the gate refuses work until the user does. Arming only once the counts
  balance is what keeps parallel streams working: the first of three to finish
  must not lock the session the other two are still running in. Escape hatches:
  any prompt beginning with `/`, and `VACA OVERRIDE`.
- `vaca-stream` agent. Streams launched as `general-purpose` were
  indistinguishable from task-creation and sync batches, so the end of an
  issue's work could not be identified from the harness at all. The agent also
  holds the standing rules that `execute.md` was duplicating into every launch
  prompt — law first, scope discipline, commit format, never `--force` — which
  is where the agents actually read them.
- Work stream files are checkpointed. An agent writes its checkpoint list before
  its first line of code, then commits and records one checkpoint at a time.
  `last_commit` names the newest commit the file accounts for, so its claims can
  be checked against `git log` rather than believed. Schema in `conventions.md`.
- `SessionStart` hands every session the project's state — in progress, next,
  blocked — before the user types. Clearing is only cheap if the next session
  starts oriented, and VACA already keeps everything in files.
- Resuming is defined. "continue the `<feature>` epic" reconciles in-progress
  streams before acting: no agent survives a `/clear`, so an in-progress stream
  in a fresh session either finished without recording or stopped partway. Those
  two were previously indistinguishable, which made a stranded stream something
  no later session would ever pick up.
- "Start the epic" takes the next ready issue instead of launching every ready
  issue at once. Cross-issue parallelism is now more sessions rather than one
  longer one.
- `conventions.md` states that code is committed in the worktree while all
  `.claude/` state is written at the project root. State written into the
  worktree would have been invisible to every report and every later session
  until the epic merged.

## 1.1.1

- `sync.md` split by operation. It was 308 lines against ~105 for the other
  phases, because it carried five distinct operations. It is now a router
  holding the shared repository safety check, pointing at `sync-epic.md`,
  `sync-issue.md`, `sync-close.md`, `sync-merge.md` and `sync-bug.md`. Reading
  the router plus one operation costs 339–428 lines where the whole file cost
  567.
- Every phase reference now names `conventions.md`. Previously only `SKILL.md`
  did, so a phase read on its own had no pointer to the frontmatter schemas,
  path standards or datetime rule. `track.md` states the exception: its scripts
  parse frontmatter themselves.

## 1.1.0

- `/rancho:help` command. `help.sh` is a reference card for a person, but the
  only way to reach it was to ask the model to run it. The command gives it a
  front door that survives version bumps, unlike the versioned install path.
- Removed `init.sh`. It created three directories VACA never uses and missed
  one it does, carried a dead copy block, wrote a boilerplate `CLAUDE.md` that
  competed with the `law` skill, and exited before doing anything on Windows.
  The phase references already create directories on demand.
- Epic sync now creates the labels it applies. `gh issue create --label` fails
  outright on a label that does not exist, and nothing created `feature`, `bug`
  or `epic:<name>` — so sync was already broken on a fresh repository.
- ASD-STE100 output style scoped to `on-skill-invoke:vaca` rather than
  applying to every response.

## 1.0.0

- Engineering law shipped as the `law` skill: root law in `SKILL.md`, backend,
  frontend, and enforcement inventory as on-demand references.
- `SessionStart` hook injects the prime directive and points at the skill.
- VACA shipped as the `vaca` skill: five phases, 13 tracking scripts, and four
  awk readers that parse frontmatter in exactly one place per entity type.
- Scaffolded the remaining plugin component directories.
