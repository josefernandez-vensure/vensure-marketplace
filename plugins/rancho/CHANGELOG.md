# Changelog

## Unreleased

- **Standalone laws.** Core §2 admits a law plugin that says
  `This law stands alone.` before its first section, such as
  `rancho-modular-dotnet`. A tree bound to one reads that law alone: not the
  core law, the contract law, annexes or inventories.
  - The prime directive, `vaca`, `vaca-stream` and VACA's help check for the
    line before anything else.
  - `AUTHORING.md` §2 says what such a law carries: the line and its own
    verification commands, and no inventory.
  - Trees bound to `rancho-dotnet`, `rancho-react` or none read exactly as
    before.
- **Breaking for every project: release as 4.0.0**, together with
  `rancho-dotnet` 3.0.0 and `rancho-react` 2.1.0. The case and the
  outcome are in `RECORDS-4.0-ledger.md` at the repository root, and the
  history of each row is in this plugin's `RECORDS.md`, *4.0*.
- **What changed:** the core law is an index and its principles.
  - §2 lists every law and when to read it: the new contract law, and
    each stack law with its annexes.
  - §3–§5 keep the principles every tree holds: access, data classes and
    egress, the attributable unit of work, and the audit trail.
  - Sections renumber: §3 Access, §4 Data Boundaries, §5 Observability,
    §6 Verification, §7 Changing the Law, and Exceptions.
  - The **contract law**, `skills/law/references/contract.md`, holds what
    a server and a client agree on across HTTP. Its rules keep their
    `ROOT` ids: ROOT-09, -11, -12, -13 and -18.
  - Server architecture moves into `rancho-dotnet`: modules, the outbox,
    the pipeline, error translation, upload handling, and the owner of
    each egress boundary. ROOT-01, -02, -04, -07, -08, -15, -17 and -20
    retire into the BE rows that state them.
  - The core inventory has 7 marks, down from 15.
  - The reading order gains the contract law. `vaca`, `vaca-stream` and
    the prime directive follow it.
- **What did not change:** no obligation is dropped. Each one is in the
  core, the contract law, or the server stack law.
- **Upgrade:**
  - Update core section citations in the project's own records. Old
    §7, §8, §9, §10 and §11 are now §3, §4, §5, §6 and §7. Old §3–§6 are
    now in the contract law or the server stack law (ledger, *Outcome*).
  - In `ENFORCEMENT-STATUS.md`, move each retired ROOT row's check onto
    the BE row it merged into.
- §6 Verification (§10 before 4.0): a test MUST NOT prove only what a successful build already
  proves - that a file, folder, project or type exists, that a reference
  resolves, or that the code compiles. A rule whose inventory row says it
  is covered by another row's check needs no check beyond what its own
  row names. No mark and no inventory row: reached by review.
- `vaca`'s task Definition of Done asks for tests of the behavior a task
  adds, and for the tree's verification commands to pass. A setup or
  scaffolding task adds no behavior; the verification commands prove it.

## 3.0.0

- **Breaking for every project.** Released together with `rancho-dotnet`
  2.0.0 and `rancho-react` 2.0.0; upgrade the three at once. The rationale
  and history of each change are in this plugin's `RECORDS.md`, in its 3.0
  stage sections (stage 0 to stage 8).
- **What changed:**
  - How the law reads (§0, §1). Precedent needs a second occurrence. When
    two rules, or a rule and a decision in `DECISIONS.md`, leave no
    compliant answer, the agent stops and asks. A comment arguing that a
    rule does not apply is a defect, never an exemption. Every `⚙` names
    its row (`⚙ BE-34`) and claims only that the rule can be made to fail
    the build: the plugins ship no checks, and until a project has built
    and reviewed a rule's check, the approving reviewer verifies the rule.
  - §1: each project keeps `ENFORCEMENT-STATUS.md` at its root, listing
    every marked rule its trees bind, its check, and whether that check is
    built and reviewed. A marked rule missing from it counts as unbuilt.
  - §11: one exceptions register per repository, `LAW-EXCEPTIONS.md`, with
    permanent `EXC-<n>` ids. A conflict entry carries the rule id (or the
    section and sentence), the paths, the justification, an upstream
    amendment request and an expiry; a boundary entry carries a review
    date. An agent proposes an entry, a maintainer adds it, and a
    suppression of a check names the entry's id. No other exemption is
    valid.
  - §2: a tree no stack law governs is bound with
    `Stack law: none - <reason>` and names its own verification commands.
    A file's tree is the nearest `CLAUDE.md` holding a `Stack law:` line.
    Agents, caching and real-time rules move to annexes of each stack law,
    which bind a tree once it meets the annex's trigger; the reading order
    names each triggered annex.
  - §10: a check MUST NOT be stricter than its rule; a narrower check is a
    known gap, recorded in the inventory or in `ENFORCEMENT-STATUS.md`.
  - §4 *Wire contract*, new: `x-data-class` on every property and parameter
    schema (ROOT-18); one keyset page shape, whose cursor the server
    encrypts and authenticates and whose class is `internal`; problem
    details with a declared `type` URI, `x-problem-types`, `errors` keyed
    by request paths, and `traceId`; concurrency by the version as a strong
    `ETag`, `etag` in the body, `If-Match` required on an existing resource
    (`428` without it, `412` when stale), `If-None-Match: *` on a create
    naming its id, `Cache-Control: private` for a principal-dependent
    response; `traceparent` and `tracestate`; a CORS policy admitting the
    contract's headers.
  - §4: ROOT-07 admits a Response DTO, no body, or a declared stream;
    framework endpoints are a declared list in the server stack law.
    ROOT-11 paginates only a collection not bounded by construction; a
    bounded one declares `maxItems` and rejects a request beyond it.
    ROOT-13: a missing key is `400`, the same key with a different payload
    `422`, a duplicate in progress a replay or `409`, never `500`, and a
    replay is answered before any precondition. A rate-limit rejection is
    `429`, with `Retry-After` where it can be estimated.
  - §7: an object check on the records an operation names, beside the
    message's policy; a query returns only records the principal may see.
    New *File uploads* subsection. An agent holds a subset of the calling
    user's permissions, never wider, and carries the actor claim; an
    autonomous run, those of a service principal declared for its manifest.
  - §8: an HTTP response carrying `Confidential` or `Restricted` data is
    sent `Cache-Control: no-store`. The telemetry pipeline owns the
    telemetry boundary; an exception's type and stack trace may be
    recorded, its message only where our code wrote it. The authenticated
    boundary is defined and includes the signed-in browser's URLs and
    storage. ROOT-16 binds both roles.
  - §9: the correlation id is the W3C trace id (ROOT-19). A server adopts a
    caller's `traceparent` only through an edge inside its trust boundary,
    and otherwise starts a new trace linked to it; `traceId` reaches the
    caller on problem details. Metrics join traces through exemplars. New
    *Audit trail* subsection. Sampling and log levels may differ per
    environment; whether a signal is emitted may not.
  - Inventory: 20 rows; ROOT-05, ROOT-06 and ROOT-10 have rows, `retired`.
- **What each project does to upgrade:**
  1. Install `rancho` 3.0.0, `rancho-dotnet` 2.0.0 and `rancho-react` 2.0.0
     together, then work through each stack law's upgrade list.
  2. Create `ENFORCEMENT-STATUS.md` at the repository root (§1), listing
     every `⚙` row of each bound stack inventory, its check, and whether
     the check is built and reviewed.
  3. Create `LAW-EXCEPTIONS.md` at the repository root (§11). Record each
     existing exemption - a justifying comment, a PR-declared exception, a
     `Restricted` field an egress boundary admits - as an `EXC-<n>` entry
     with its required fields; point every suppression of a law check at
     its id; delete comments that argue a rule does not apply (§0).
  4. Bind every tree no stack law governs, such as infrastructure-as-code or
     CI scripts, with `Stack law: none - <reason>` and its verification
     commands in its `CLAUDE.md` (§2).
  5. Narrow every check the project has built that is stricter than its
     rule, and record a narrower one's gap in `ENFORCEMENT-STATUS.md` (§10).
  6. Declare the audit trail's retention in `DECISIONS.md` (§9 *Audit
     trail*).
  7. Decide which edge admits callers inside the trust boundary, since only
     a request through it adopts the caller's `traceparent` (§9).
  8. Only where agents run autonomously: declare the service principal for
     each such manifest (§7).

## 2.1.0

- Amendment A-08, from a consuming project's compliance audit of 2026-10-30
  (items 3 and 5), with the owners' review of it. Records in
  `enforcement.md`.
- §6 gains a fourth translation boundary: commit outcome to result value,
  owned by the one pipeline step that commits; the pipeline's own steps
  return result values without one. An adapter MAY catch the exceptions its
  framework or SDK uses to signal an expected outcome - such as a concurrency
  conflict, a unique-key violation, or an unreachable remote service - and
  MUST translate them where they are raised.
- §8: a class is declared on the domain type and on its published contract
  type, and a check proves the two agree. Every other carrier inherits it. A
  property whose type carries no class of its own - a page size, a cursor, a
  field of a platform type, a projected value - declares one on the property.
  The agreement check is marked and inventoried as ROOT-20, server role. The OpenAPI document now carries the class of request properties and
  parameters as well as response properties (ROOT-18), and the client
  enforces it at its own boundaries.
- Released with `rancho-dotnet` 1.1.0 and `rancho-react` 1.1.0, which carry
  the stack-law halves.

## 2.0.0

- **Breaking for every project.** The law is split in two. `rancho` carries
  the core law, which is the same in every project, and VACA. Each technology
  is a stack law in a plugin of its own, which a project enables for itself:
  `rancho-dotnet` 1.0.0, role `server`, was `references/backend.md`, and
  `rancho-react` 1.0.0, role `client`, was `references/frontend.md`. Their
  section numbers are unchanged. Amendment A-07 in `enforcement.md` records
  why.
- **What each project does to upgrade:**
  - Install the stack laws it builds with at project scope, so that
    `enabledPlugins` in its `.claude/settings.json` lists them, and commit
    that file.
  - Replace each tree's `CLAUDE.md` stub with its binding, for example
    `Stack law: /rancho-dotnet:law`.
  - Add the real-time client packages to its lint configuration, for
    `FE-18`.
- §2 Repository Map becomes §2 Stack Laws. Every tree is bound to exactly one
  stack law by its `CLAUDE.md`, and an unbound tree is a defect to stop on,
  never one to guess past. Every stack law declares a role, maps every core
  section to the section that carries it, lists its gaps, and carries Stack,
  Testing and Verification sections and its own enforcement inventory. A
  stack law never depends on another.
- §3, §6, §8 and §9 no longer name a stack. The module boundary is internals
  and published contracts, not four .NET projects; the error contract names a
  result value and RFC 9457 problem details; the egress table names a
  real-time client, not SignalR. No rule is weakened.
- §10 runs the verification of every stack law a change touches, and requires
  each stack law to carry the check of every core rule its role binds. The
  `dotnet` and `pnpm` commands move to the stack laws.
- `enforcement.md` is split three ways. The core inventory keeps the `ROOT`
  rows - now with the role each binds, and without their .NET mechanisms -
  and every record that concerns the core law or more than one stack law.
  Each stack inventory takes its own rows, the core rows it carries with their
  mechanism and check, and the records that concern it alone. Ids are
  unchanged, and every one of the 152 rows survives.
- VACA, the `vaca-stream` agent, the help card and the `SessionStart` prime
  directive read the core law, then the stack law bound to the tree, in place
  of "the backend or frontend reference".

## 1.7.0

- **Breaking for any application still on shadcn/ui or Tailwind.** The law
  names the UI library as a class, not a component kit and a styling system.
  Frontend §1 named Shadcn/ui and Tailwind, so an application built on the
  organization's design system could not satisfy `FE-12` or `FE-14` whatever
  its own code did. Each application records its UI library, and the
  component library it is built on, in `DECISIONS.md`. Amendment A-06 in
  `enforcement.md` records why.
- `FE-32`: a UI-class package - one whose purpose is presentation, such as a
  component, styling, icon or class-name package - is a dependency only if the
  UI library depends on it, and application code imports no UI-class package
  but the UI library and its component library. Packages the application uses
  in its own right, such as its runtime, are outside the rule.
- §2 removes `shared/ui/`. `FE-31`: features and `app/` use the UI library
  directly, `shared/` imports neither it nor its component library, and a
  module that re-exports, wraps, adapts or restyles a UI-library component is
  forbidden.
- `FE-12` keeps React Hook Form + Zod. A UI-library input is bound with
  `Controller`, and the UI library's own validator is forbidden.
- `FE-14`: stylesheets enter only through the global stylesheet, which imports
  the UI library's stylesheet entry, its component library's theme and its
  icon stylesheet, plus one layout partial that uses only the library's tokens
  and mixins and never targets a library component. R-14 is reworded to match.
- Each consumer adds five lists to its lint configuration, beside the
  identity-library lists from 1.6.0:
  - the UI library's packages;
  - its component library's packages;
  - the UI-class packages;
  - the library's validator;
  - the library's class prefixes.
- The `Controller` binding under `FE-12`, and a wrapper written inside a
  feature under `FE-31`, are reached by review. Frontend rows go from 30 to 32.

## 1.6.0

- The law names the identity library as a class, not MSAL. A-03 took the
  provider's name out and left its library in, so frontend §9 described a
  library that applications in the organization's host shell do not use.
  Those applications read tokens from a session store the shell writes and
  shares, and could not satisfy `FE-25` whatever their own code did.
  Amendment A-05 in `enforcement.md` records why.
- `FE-24` covers the identity library's session store as well as its calls:
  nothing outside the one module reads or writes it. `FE-25` forbids the
  application retaining a token beyond the request, including in a
  module-level cache. The library's own store is the one place a token may
  live, and the application configures the shortest-lived store the library
  supports - memory, for an application that runs its own sign-in.
- An application in a host shell runs no sign-in of its own, and its `401`
  goes to the shell's sign-in page (`FE-27`). The grant it requests - scopes,
  a service name, or any other - is declared once and is the narrowest it
  needs. Sign-out has the library clear its store.
- §11 lets the identity library's store hold the user's own profile, and
  forbids the application writing to it (`FE-23`).
- `BE-104`: a claim read before the token's signature is validated may only
  select how the token is validated, and must agree with the validated token.
  A required test in backend §14 **Authorization** carries it.
- Backend §6's "External permission authorities" becomes "External
  authorities". `BE-99` and `BE-100` now cover a remote service that
  validates the token, not only one that answers for permissions.

## 1.5.1

- `BE-53`'s check bans the class of claim types, not two members of it. It
  named `ClaimsPrincipal` and `ClaimTypes`, so a check built to the note let
  `Claim`, `ClaimsIdentity` and `JwtRegisteredClaimNames` through - the gap
  A-04 closed for the rules in 1.5.0, left open in a check note. The rule is
  unchanged; only the check it prescribes is widened.
- `BE-94`'s mechanism is `contract`, not `arch`. A declared dimension key is a
  `const string` the compiler inlines, so in IL it is the same `ldstr` as an
  arbitrary literal and ArchUnitNET cannot tell the two apart. The check reads
  the committed call sites instead. The rule and its status are unchanged.

## 1.5.0

- The law states classes, not lists. A machine-checked rule is read as
  exhaustive, so a rule that enumerates grants every member it forgot to name.
  `SKILL.md` §1 now requires a rule to state its class, with members named
  only as illustrations. §10 adds the converse: a check stricter than its rule
  is the same contradiction, and the rule is widened or the check narrowed in
  the same change. Amendment A-04 in `enforcement.md` records why.
- `BE-98` forbids code outside the composition root from learning which
  environment it runs in, by any means. The check bans the environment types
  rather than two of the four predicates, so `IsStaging()`,
  `IsEnvironment(string)` and `EnvironmentName` comparisons are caught too.
- `BE-97` forbids a secret in any committed file, not only in the three places it
  listed. A committed credential slot, such as `nuget.config`'s
  `<packageSourceCredentials>`, holds a `%VAR%` reference and never a
  placeholder to be overwritten.
- The same fix for rules with the same gap: `BE-95` covers environment-variable
  reads, `BE-39` any substitute test database provider, `BE-80` every route
  builder verb, `FE-01` `@ts-nocheck`, `FE-16` the URL fragment, `FE-19`
  `vi.doMock`, `FE-23` and `FE-25` any browser storage, and §15 every ambient
  clock read.

## 1.4.5

- `vaca`'s task reader skips non-task files. The `[0-9]*.md` glob also matches
  files such as `12-analysis.md`, which `read-tasks.awk` reported as a task
  numbered `12-analysis`. A file whose name is not all digits is now skipped
  whole: it emits no record, and none of its frontmatter lines reach the
  next task.

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
