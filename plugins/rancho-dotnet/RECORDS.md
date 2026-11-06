# Records - .NET stack law

> Records that say a record is "in the core inventory" were written before 3.0 stage 1. The core records now live in `plugins/rancho/RECORDS.md`.

What the .NET stack law and its inventory no longer carry, because a coding agent does not need it to apply a rule: the reasoning behind wording that would otherwise look arbitrary, and the history of how the law came to read as it does. Nothing here is a rule. Every rule is stated in `skills/law/SKILL.md` or one of its annexes, `skills/law/references/annex-agents.md`, `annex-caching.md` and `annex-realtime.md`.

## Rationale moved from SKILL.md in 3.0 stage 1

Moved word for word, grouped by the section it came from. A section that 3.0 stage 1 moved to an annex keeps its number, and its rationale is listed under that number.

### §2 Project Reference Law

After "adding a kind of project means adding a row, derived from the four facts.", before BE-01's mark:

Kind is declared rather than inferred from the assembly name because `Platform.Application` and `<Module>.Application` are different rows that no name-suffix rule separates, and because a rule that guesses has no answer for a project it has never seen.

After BE-01's mark:

Read as the closure, every row would become the union of its dependencies' rows and the table would check nothing.

After BE-02's paragraph:

`SharedKernel` admits only the BCL and `Microsoft.Extensions.Compliance.Abstractions`, which carries the classification attributes and no behaviour, for the same reason one step removed: `Domain` references it, so a package added there reaches every aggregate in the solution.

After the definition of the composition root:

`Api` references `<Module>.Application` because an endpoint constructs the message it dispatches (§13). It uses `SharedKernel` and `Platform.Application` directly at the two boundaries the law assigns it - `Result` to `ProblemDetails` (core §6) and the correlation id (§10).

### §3 Domain Layer

After "Invariant breaches MUST throw.":

Reaching one means a guard failed upstream; that is a bug and is meant to be loud.

After BE-109's "the handler converts it, and any other wire value, into domain values.":

This is what lets an endpoint construct the message it dispatches (§13) and a query handler read without the domain model (§4).

### §4 Application Layer

After "**Every concrete query handler lives in `Infrastructure`, by convention.**":

A query reads from outside the process - a database, a third-party API, a file store, or any other source - and that dependency is an Infrastructure concern.

A bullet of its own, after BE-113's:

This split does not weaken the pipeline. Dispatch is by message type, so authorization, validation, and logging behaviors run identically wherever the handler is compiled.

After BE-23's mark:

Non-deterministic ordering makes both cursor and offset paging skip and duplicate rows, and does it silently - declaring the ordering is what makes that checkable instead of hopeful.

After "A total count MUST be opt-in per operation.":

Counting by default puts a `COUNT` over the whole filtered set on the hot path of every list screen.

After "An agent-reachable query MUST declare a lower maximum than the UI's, in the manifest (§8)." (now in `annex-agents.md`):

An unbounded search tool pulls a table into a vendor's context window.

Opening *Idempotency*, before "A command that executes twice on a retry is a defect in the command, not in the caller.":

Callers retry. A client times out and resubmits, a job re-runs, an agent re-invokes a tool it believes failed.

After "An integration event is a published contract.":

Another module already consumes it, and the outbox may hold instances written by code that no longer exists.

After BE-86's mark:

An aggregate serialized into a contract makes every future change to that aggregate a breaking change to another module.

After "An implementation that is pure logic MUST live in `Application`, except a concrete query handler, which §4 *Commands and Queries* places in `Infrastructure` by convention.":

Putting it in `Infrastructure` moves business rules into the adapter layer, where they get integration-tested instead of unit-tested and cannot be reused by another slice.

### §5 Persistence

After BE-87's mark:

It has run somewhere; changing it means two databases with the same migration history and different schemas.

After "A migration MUST be safe to run while the previous version of the application is still serving.":

Deployments overlap; the schema and the code are never swapped at the same instant.

After the destructive-change rule:

A migration that drops a column an older instance still reads is an outage with a deployment for a cause.

After BE-88's mark:

It runs without the application's services, against a schema that no longer matches today's model.

After the generic-repository rule:

A generic repository that accumulates query methods has become a data-access layer.

After BE-45's mark (now in `annex-caching.md`):

The rule is unconditional because "is this projection permission-filtered?" is not a question a check can answer.

Caching a filtered read under a principal-agnostic key serves one user's rows to another - and serves an agent rows its narrowed principal should never have seen. This is the single most likely way the authorization model in §6 and §8 gets bypassed. A genuinely public projection pays some hit rate for the rule being unconditional; that is the correct trade.

After "Old keys become unreachable and expire by TTL." (now in `annex-caching.md`):

Invalidation is therefore atomic and O(1) - it cannot half-succeed the way deleting N keys can.

After "Stamps MUST be incremented **after** the transaction commits, never before." (now in `annex-caching.md`):

Incrementing first lets a concurrent read repopulate the cache with pre-commit data, which then stays stale until TTL.

### §6 Authorization

After BE-52's mark:

Violating this silently replaces an agent's narrowed principal with the full user token and voids every agent restriction below.

After BE-102's mark:

This is what keeps the authority replaceable.

### §7 Data Classification

A bullet of its own, after BE-83's:

This is what §3's Value Object rule buys. A `string` holding a government identifier cannot be classified and fails the test; a `NationalId` value object carries its class into every DTO, log line, and cache key it ever reaches.

After BE-56's mark:

Interpolation into a log message is the most common way this data escapes, and it is close to invisible in review.

After "core §6 forbids leaking internals; this extends it to the data.":

A validator reporting that `123-45-6789` is not a valid national id has just written that value to a log, a browser, and an error tracker in one step.

After the free-text rule of *The model-visible surface* (now in `annex-agents.md`):

Its contents cannot be classified in advance, and in practice they routinely contain exactly what this section exists to hold back.

After "Integration events and outbox rows MUST NOT carry `Restricted` payloads.":

The outbox is durable, replayable, and operator-readable.

### §8 AI Agents

After "The scopes requested for a run MUST derive from that same manifest." (now in `annex-agents.md`):

Two hand-maintained lists will drift, and the drift is a privilege escalation.

### §9 Quotas and Rate Limits

After "Agent retries MUST be bounded and MUST reuse the original idempotency identity (§4)." (now in `annex-agents.md`):

An unbounded retry loop is indistinguishable from an attack, and with a fresh key per attempt it is also a duplicate-write bug.

After "Every limit MUST emit a metric when it is hit.":

A limit nobody can observe being hit is not operable - it gets discovered as an outage or as a bill.

### §10 Observability

After BE-90's mark:

A context that can be constructed without one will be.

After "The dispatcher MUST restore that correlation id when delivering.":

Without it the trace stops at the module boundary, which is precisely where cross-module debugging starts.

After BE-93's mark:

Handler logging duplicates what the pipeline already emits, and then drifts from it.

After BE-94's mark:

A user id, an aggregate id, or any unbounded value as a dimension is a cardinality incident waiting for traffic.

### §11 Configuration and Secrets

After BE-96's mark:

A misconfigured application fails to start; it does not fail on the first request that happens to touch the setting.

After "and MUST NOT carry a placeholder to be overwritten in place.":

A placeholder is one local edit away from a committed secret, and a check cannot tell a substituted value from the placeholder it replaced.

After BE-98's mark:

A branch on environment is a code path that no environment fully tests.

After "A feature flag is configuration: typed, validated, and removable.":

A flag with no removal plan is technical debt with a switch on it.

### §12 Real-time

After BE-74's mark (now in `annex-realtime.md`):

That is what "no business logic in a hub" means in a form a check can evaluate.

### §13 API Layer

After BE-80's mark:

A lambda's signature can be read from endpoint metadata, but its body compiles into another class, so a type-level check cannot attribute the body's references to the endpoint; a type makes them attributable.

## Restatements removed in 3.0 stage 1

Removed from `skills/law/SKILL.md` because the core law states the same obligation. Each left a pointer to the core section that states it.

- §8 *Effective permissions*: "An agent principal MUST be a strict narrowing of the calling user's. Never equal, never wider." Core §7 states it.
- §9 *Transport*: "A rejection is `429` with `Retry-After`, shaped as `ProblemDetails`." Core §4 *Limits* states it.
- §10 *Metrics and traces*: "OpenTelemetry. Logs, metrics, and traces MUST share one correlation identity, or they are three tools instead of one." and "Telemetry MUST NOT be optional per environment. Sampling rates MAY differ; emission MUST NOT." Core §9 states both.

## Records

Moved word for word from `skills/law/references/enforcement.md` in 3.0 stage 1. Paths in them are as written there, so `../SKILL.md` is this stack law, `skills/law/SKILL.md`. A record kept "here" or "in this file" is now here. Records the core law's inventory held are now in `rancho`'s `RECORDS.md`.

### Rewordings applied

Nine of the fifteen rewordings concern this stack law alone, and are kept here. R-01 to R-03 are in the core inventory, R-13 to R-15 in `rancho-react`'s. Each entry records the rule, why the original wording resisted checking, and what replaced it. Records written before 2.0.0 name this file's stack law `backend.md`, and call the core law the root.

**R-04 · BE-08 · "Raw `Guid` and `int` MUST NOT appear as identifier parameters."**
Requires knowing which parameters are identifiers. *Applied:* strengthen to the decidable form - *no public method, constructor, or property in `Domain` or `Application` exposes a bare `Guid`.* Stricter than intended and easy to check; `int` stays permitted since counts and quantities are legitimate.

**R-05 · BE-09 · "Domain events MUST be past tense."**
English morphology. A suffix denylist is brittle and produces false failures on irregular verbs. *Applied:* drop the ⚙, keep as SHOULD. BE-10 - events carry ids and values, not entity references - is the half that matters and is already checkable.

**R-06 · BE-12 · "A Query MUST NOT mutate."**
Needs effect analysis. *Applied:* reword to the proxy that actually holds - *a query handler MUST NOT reference a repository port, and MUST NOT call `SaveChangesAsync`.* First half is `arch`, second is `BannedApiAnalyzers` scoped to query handler types.

**R-07 · BE-23 · "Ordering MUST be deterministic."** *(a promotion, not a blocker - BE-23 is enforceable today by test)*
Buried in a LINQ chain, invisible to static analysis. *Applied:* require paginated queries to declare their sort as data - an ordering descriptor ending in a unique key - rather than expressing it only as `OrderBy`. The rule then becomes a `contract` check over the descriptors, and the badge becomes true.

**R-08 · BE-24 · "A cursor MUST NOT encode a filter, a sort, a principal."**
The contents of an opaque blob are not statically knowable. *Applied:* require one cursor type solution-wide whose payload is a record of exactly `(sortKey, id)`. Then `arch` checks the type, and the prohibition follows by construction.

**R-09 · BE-45 · "A cache key for a permission-filtered projection MUST include the acting principal."**
"Permission-filtered" is not decidable. The law itself calls this the single most likely way the authorization model gets bypassed, and it is currently the least enforceable rule in the document. *Applied:* make it unconditional - *every cache key includes the acting principal*, built by one key builder that always includes it. Strictly safer, trivially checkable, and it costs only some cache-hit rate on genuinely public projections. **This is the most valuable rewording in this file.**

**R-10 · BE-58 · "The model-visible DTO set is distinct from the UI's where they differ in class."**
The conditional is the problem. *Applied:* split. Keep the ⚙ on the checkable half - *every model-visible DTO is explicitly marked* - and let BE-59 carry the actual protection. The "distinct set" guidance stays as prose.

**R-11 · BE-68 · "A tool MUST NOT accept a URL, route, table name, or query text."**
Distinguishing a behaviour-selecting string from a data string is semantic. *Applied:* require every tool string parameter to be a declared wrapper type - `SearchTerm`, `CommentBody` - with bare `string` forbidden in tool parameter position. `arch` then checks it, and adding a new wrapper becomes a deliberate, reviewable act.

**R-12 · BE-74 · "Hubs MUST NOT contain business logic."**
"Business logic" is not a static property. *Applied:* reword to the shape the rule is really describing - *a hub method MUST map its argument to a message, dispatch it, and return; it MUST NOT reference any type outside DTOs and the dispatcher.* That is an `arch` type-reference check.

### Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under core §11 - the law first, the codebase after. A-09 concerns this stack law alone and is kept here. A-08 spans both roles and is in the core inventory, with A-01 to A-05 and A-07; A-06 and A-10 are in `rancho-react`'s.

**A-09 · §1, §2, §3, §4, §5, §6, §7, §8, §9, §13, §14, §15, ROOT-04, ROOT-08, BE-01, BE-08, BE-14, BE-15, BE-16, BE-31, BE-34, BE-35, BE-37, BE-43, BE-52, BE-53, BE-54, BE-82, BE-83, BE-95, BE-98, BE-102, BE-105 · Rule combinations that left no compliant answer.**
A consuming project audited its code against the law on 2026-10-30 and found code shaped to pass a check while breaking the rule behind it, nearly always beside a comment arguing it was fine. Most of it was not carelessness: the law left no compliant answer. The audit's items 1 to 4, 6, 9 and 10 concern this stack law, and the owners' review of them found six more conflicts, resolved here in the same change. A whole-law review of the change before release (2026-11-03) found that it had created further conflicts of its own; those are resolved here too, and named in the bullets below.
- *`Api` and Application.* §4 puts messages in `<Module>.Application` and §13 has endpoints build them, but the `Api` row forbade that reference, so every endpoint named a type it could not reach. The workarounds were predictable: a transitive reference through Infrastructure, `InternalsVisibleTo` from Application to `Api`, and SDK clients and `DbContext` registrations moved into `Api` because "call into" had no edge. *Applied:* `Api` references `<Module>.Application`. It names only Infrastructure registration entry points - one `Add<Module>Module` per module, one `Add<Feature>` per platform feature, Microsoft's convention for registering a group of services - and never calls into Infrastructure after startup (BE-105, BE-106). Infrastructure types are `internal` (BE-34) and `InternalsVisibleTo` names test assemblies only (BE-107), so the boundary is one the compiler holds. A source generator in `Api` cannot name an `internal` type in another assembly, and Mediator 3.x, now pinned in §1, names each concrete handler in its generated registration - `typeof` the handler, registered under its `I…Handler<,>` interface - so concrete query handlers stay public, with the types their public constructors take, and generated code is outside BE-105. Where the compiler no longer holds the boundary, BE-105 does. A draft branch for a generator that never names handlers described no behavior of the named library and was removed before release. The registration entry point is added to BE-34's adapter kinds, which it otherwise failed. *The composition root.* `Add<Module>Module` takes `IConfiguration`, which §11 confined to the composition root, and a module's own registrations resolve factories from the container, which §15 confined there too. *Applied:* the composition root is defined as `Api` and the registration entry points it calls, and BE-52, BE-95 and BE-98 are scoped to it. BE-53's check, which banned the claims namespace everywhere outside `Api`, is narrowed to the rule's own scope, module `Domain` and `Application`: as written, the authorization behavior in `Platform.Application` could not compile.
- *Packages.* No row admitted `Microsoft.Extensions.*.Abstractions`, which dependency injection, options, logging and hosting need, nor `Microsoft.Extensions.Options` and its configuration extensions, which `ValidateOnStart` (§11) and `Bind` live in. The package column also had no check. *Applied:* the rows admit them, Infrastructure admits the SDK of any system it adapts, `Platform.Infrastructure` declares telemetry instrumentation and `Api` the exporters, and BE-01 now checks packages. With `Api` declaring no Infrastructure package, an external authority's SDK could live nowhere: BE-102 kept it in `Api`. *Applied:* the authority is reached through one Infrastructure adapter behind a port, which declares its SDK, translates its codes and its failures (core §6), and is the only project that references its types (BE-102).
- *Query handler placement.* BE-15 placed the concrete query handler in Infrastructure "because it depends on `DbContext`", which said nothing of a handler that calls an API or reads nothing, and the pure-logic rule then pulled the latter into Application. *Applied:* every concrete query handler is in Infrastructure by convention - one check instead of a judgement per handler - and reads independently of the domain model, as Microsoft's CQRS guidance does: no `Domain` type (BE-108), read through Infrastructure-mapped read types or parameterized SQL into the DTO, and a required test against a real database for SQL (BE-112), since SQL loses compile-time checking. A draft that required SQL for every read was narrowed before release: SQL bypasses global query filters, sits outside the EF model that ROOT-04 and BE-37 inspect, and cannot read a column encrypted in the mapping, and EF Core's own guidance treats it as a last resort. SQL must apply the module's filters itself, raw-string SQL APIs are banned (BE-113), and BE-14's context-wide `NoTracking` assertion - stricter than the rule once the write side shares the context - is withdrawn; read types are keyless instead, which EF never tracks. A strongly-typed id's own constructor and value member are exempt from BE-08, since converting between a `Domain` id and its `Contracts` twin, peers that cannot reference each other, goes through the wire value. `Api` admits EF Core's design-time tooling, which `dotnet ef` needs in the startup project.
- *Typed ids.* BE-08 forbade a bare `Guid` anywhere in Application, so a message carried a Domain id, and then the endpoint building it broke BE-77 and a query handler reading it broke BE-108. *Applied:* messages and Response DTOs carry the module's `Contracts` ids and no `Domain` type (BE-109); the handler converts. BE-08 narrows to `Domain` and the messages. A module with an endpoint therefore has a `Contracts` project.
- *Encryption.* §7 put the encryption of `Restricted` columns in the mapping, which SQL into a DTO bypasses. *Applied:* encryption lives in Infrastructure, in the mapping or below it in the driver, and is transparent to every read path: a read type carries the same converter, SQL may not read a column encrypted in the mapping, and the limits of driver-level encryption are stated: deterministic allows equality only, randomized nothing, and range, pattern and sort operations need a secure enclave.
- *Exceptions the framework requires.* EF Core signals a concurrency conflict or a unique-key race only by throwing, and the conflict at commit arrives after the handler has returned. A `BackgroundService` whose loop throws stops the host since .NET 6, and a singleton hosted service must create a scope to use scoped services, the pattern Microsoft documents. *Applied:* the adapter that receives the framework's exception translates it, and a conflict at commit becomes a `Result` conflict in the transaction behavior (core §6, A-08); §15 admits a declared fault-isolation boundary and `IServiceScopeFactory` in a hosted service.
- *A command whose work is an external write.* The transaction behavior wrapped every command, and a handler may not call an external system inside a transaction, so a command such as a blob upload had no legal shape. *Applied:* a command declares whether it persists. One that does not opens no unit of work, stages nothing, uses neither a key nor the outbox, and is idempotent naturally or by a client-supplied id (BE-110, BE-111). An agent's tool invocation carries a key only for a command that declares one (BE-31), so an agent can reach such a command; the mechanism list says client-supplied id, not aggregate id. A violation of the idempotency record's unique index at commit is answered as a replay, so two concurrent attempts still resolve as one execution and one replay. An effect a repeat would duplicate, such as an email or a payment, is not such a command: a persisting command writes an outbox row, and delivery performs the effect with an idempotency key derived from the row. §8's tool parameters, which named domain values, are now wire values from `Contracts`, matching BE-109.
- *Endpoint references.* BE-82 omitted the message an endpoint must build and its DTOs' own types; a consuming project moved the mapping onto the DTO to escape the check. *Applied:* both are admitted, with the types they are composed of, the endpoint abstraction, the `Result` the dispatcher returns, and framework primitives such as `Task`, `CancellationToken` and HTTP result types, all of which every endpoint names. The check reads the whole endpoint type.
- *BE-43* failed every lambda in a value object, through the compiler's closure class. Its check now skips compiler-generated types; the rule is unchanged.
- *A third check before release* found that the rewording of BE-34 had dropped "public" from "Every public type in `Infrastructure` MUST be an adapter", which made the outbox row, the idempotency record, read types and options classes illegal; types the project maps or binds are now an adapter kind. It also found that the mandatory registration entry point had no legal folder under BE-35, that the commit outcome did not say which index a violation hit, that BE-15's and BE-105's checks were stricter than their rules, that nothing checked an agent's client-supplied id (BE-114), and that a column could be encrypted in a form that refuses the range comparison keyset paging makes. Each is fixed here.

Backend rows go from 104 to 114, and marks from 90 to 95. The summary had counted 91 before this change: it counted the ⚙ that §14's opening sentence mentions in prose.

### Prerequisites (all adopted)

Three of the four prerequisites concern this stack law alone, and are kept here. P-03 spans both roles and is in the core inventory.

**P-01 · Endpoints are types, not raw lambdas.** *Adopted.* ArchUnitNET sees types; it cannot see the signature of a lambda handed to `MapPost`. `backend.md` §13 now requires every endpoint to be a type implementing the shared endpoint abstraction, registered by discovery (BE-80, BE-81). This unblocked ROOT-07 and BE-77, and made checkable a rule that had never carried a mark at all: BE-82, an endpoint referencing only its DTOs, the dispatcher, and the translator - the endpoint counterpart of what R-12 did for hubs.

**P-02 · Aggregate roots carry a marker.** *Adopted.* Nothing distinguished a root from an entity at the type level. `backend.md` §3 now requires the marker (BE-78). This unblocked BE-42 and made BE-79 checkable - *an aggregate holds another aggregate's id, never a reference*, which has been law since the first draft with no way to test it.

**P-04 · One cursor type.** *Adopted* as part of R-08.

### Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** `proj` and `roslyn` checks: BE-02, BE-39, BE-47, BE-57 (`CA2254` is a compiler switch), BE-50's banned `[Authorize]`, BE-98's banned environment types, BE-95's banned environment-variable reads, BE-97's secret scan, plus the §15 Forbidden list as `BannedApiAnalyzers` entries. **Enforced in about a day**, none requiring a decision.

**Second - the reflection sweeps.** One test each, discovering every instance automatically: BE-25 and BE-26 (declared policy, declared idempotency), BE-64, BE-67, BE-70, BE-73, BE-51, BE-96 (every options class validated on start), BE-100 (the external-authority cache lifetime declared and bounded), BE-107, BE-110, BE-113. These are the highest-leverage tests in the suite, because they cover every message that will ever be added, not just today's.

**Third - the classification graph.** BE-55 is the keystone: BE-59, BE-60, BE-61, BE-69, and ROOT-16 all reuse its property-graph walk. Write it once. It is the largest single piece of work here and the one that carries the most risk if it is skipped. The same pass emits the class into the OpenAPI document (BE-83), which is what lets a client stack law carry it into its generated types and enforce its data-boundary rules at all.

**Fourth - the structural arch tests.** BE-01, BE-04 through BE-07, BE-10, BE-13 through BE-20, BE-30, BE-33 through BE-38, BE-40, BE-43, BE-46, BE-48, BE-52, BE-53, BE-102, BE-63, BE-76, BE-78 through BE-82, BE-105, BE-106, BE-108 and BE-109. Individually cheap, collectively the bulk of the row count.

**Every decision is settled.** The fifteen rewordings, all four prerequisites, and the badge question are in the law. Every check above can be built against the current text, and nothing in this file is now waiting on an answer - only on the work. Building checks against the current text means building the wrong checks for some of them, and R-09 in particular should change before anything caches anything.

### Former status legend

The status legend and the effort note this inventory carried before 3.0 stage 1, which replaced them with the core inventory's *How to read an inventory* and dropped the Effort column. *The badge problem* is in `rancho`'s `RECORDS.md`.

**Status:**

| Status | Meaning |
|---|---|
| `build` | Enforceable as worded. A violation can be made to fail the build. |
| `required-test` | No build check can reach it. The ⚙ is stripped and the obligation is named in a Testing section, which is where it is now caught. See *The badge problem*, in `rancho`'s `RECORDS.md`. |
| `dropped` | The ⚙ was removed. The rule still stands - it is enforced by review, not by the build. |
| `reword` | Not decidable as worded. Resolved in [Rewordings applied](#rewordings-applied). None remain. |

**Effort** is for the check, not the rule: `S` under an hour, `M` a day, `L` more than a day.

## Row history

Notes taken out of the inventory's Check cells in 3.0 stage 1, word for word.

- ROOT-07: Unblocked by P-01.
- ROOT-18: Added by P-03, widened by A-08.
- ROOT-20: Added by A-08.
- BE-01: Amended by A-09: `Api` admits `<Module>.Application`, and the package column is now checked - it had no check before
- BE-105: Added by A-09. Narrowed before release from an allowlist of every type `Api` may name, which failed on types that flow in transitively, such as the dispatcher's
- BE-106: Added by A-09.
- BE-107: Added by A-09.
- BE-08: R-04 applied. Narrowed by A-09 from all of `Application` to its messages: the rest of `Application` is reached through them.
- BE-109: Added by A-09.
- BE-09: R-05 applied: now SHOULD, unmarked
- BE-78: Added by P-02.
- BE-79: Enabled by P-02
- BE-12: R-06 applied
- BE-14: Read types are keyless, which EF never tracks, and SQL into a DTO is never tracked, so the context-wide `NoTracking` assertion is withdrawn by A-09 - the write side shares the context, and the assertion was stricter than the rule. Asserted instead: every read type mapped in a module's model is keyless
- BE-15: Amended by A-09
- BE-108: Added by A-09.
- BE-112: Added by A-09.
- BE-113: Added by A-09.
- BE-23: R-07 applied: promoted from test-only.
- BE-24: R-08 applied
- BE-29: Mark stripped; split out of BE-28's bullet, which keeps its own
- BE-31: Amended by A-09: a command idempotent naturally or by client-supplied id carries no key; the client-supplied-id half is BE-114
- BE-114: Added by A-09. §14 **Idempotency** (the test moved to `annex-agents.md` in 3.0 stage 1)
- BE-110: Added by A-09.
- BE-111: Added by A-09.
- BE-34: Amended by A-09: a public Infrastructure type that is not a registration entry point, not named by a generated type in `Api`, and not exposed by such a type's public constructor or member, fails.
- BE-35: Amended by A-09: the mandatory entry point had no legal folder
- BE-39: Amended by A-04
- BE-41: No longer covered by BE-01: `Application` now references `Platform.Application`, where the unit-of-work port is declared. Assert directly - no type in a module `Application` references it
- BE-42: Unblocked by P-02.
- BE-43: Amended by A-09
- BE-45: R-09 applied
- BE-49: §14 **Cache**. Mark stripped (the test moved to `annex-caching.md` in 3.0 stage 1)
- BE-52: The composition root is `Api` and the registration entry points it calls (§2, A-09)
- BE-53: Rescoped by A-09: banning the namespace everywhere outside `Api` left the authorization behavior unable to compile, a check stricter than its rule.
- BE-99: Added by A-03, widened by A-05.
- BE-100: Added by A-03, widened by A-05.
- BE-101: Added by A-03.
- BE-102: Added by A-03. Amended by A-09: the authority's SDK is declared by the Infrastructure project holding its adapter, and its types are banned in every other assembly. `Api` may declare no package an Infrastructure project declares, so the old scope - `Api` - had no compliant answer
- BE-103: Added by A-03. §14 **Authorization** (the test moved to `annex-agents.md` in 3.0 stage 1)
- BE-104: Added by A-05.
- BE-54: Amended by A-09
- BE-58: R-10 applied: the "distinct set" half is now unmarked prose
- BE-65: §14 **Agents**. Mark stripped (the test moved to `annex-agents.md` in 3.0 stage 1)
- BE-66: §14 **Agents**. Mark stripped; the guard is runtime, the test proves it (the test moved to `annex-agents.md` in 3.0 stage 1)
- BE-68: R-11 applied
- BE-72: §14 **Budgets**. Mark stripped (the test moved to `annex-agents.md` in 3.0 stage 1)
- BE-83: Added by P-03, widened by A-08.
- BE-95: Amended by A-04. Rescoped by A-09: `Add<Module>Module` takes `IConfiguration`, so the composition root is `Api` plus the registration entry points (§2)
- BE-97: Amended by A-04
- BE-98: Amended by A-04
- BE-74: R-12 applied
- BE-77: Unblocked by P-01
- BE-80: Added by P-01.
- BE-81: Added by P-01.
- BE-82: Enabled by P-01. Amended by A-09: the message and the DTOs' own types were missing, so every endpoint failed it.

## 3.0 stage 3

- §4, BE-11: a Command may also return the new version of the aggregate it changed; the row's rule text and check follow. Closes A-18, A-35, NET-A-10.
- §4 *Pagination*, BE-19: only a query whose collection is not bounded by construction must be paginated, and a bounded one declares its bound; the row's rule text, check and gap follow. Closes CORE-16, A-15.
- §4 *Idempotency*: the same key with a different payload fails with `422`. Closes CORE-18.
- §5: every aggregate carries an application-managed version mapped as its concurrency token, replacing the row version. Closes A-18, A-35, NET-A-10.
- §10 *Correlation*: an inbound request adopts or starts its trace as core §9 states, and every problem details response carries `traceId`. Closes C-1, A-36, CORE-19.
- §13: success shapes admit a declared stream; a versioned write follows core §4 *Wire contract*; a new bullet sets the CORS headers for `traceparent`, `If-Match` and `ETag`. Closes A-18, A-35, NET-A-10.
- §7, BE-83: the class is emitted as `x-data-class`. Closes CORE-17.
- §14: new **Concurrency** required test (BE-115) and an extended **Idempotency** required test for `422` (BE-116), with their rows; the Summary counts follow. Closes A-18, A-35, NET-A-10 and CORE-18.
- Core rules carried: ROOT-07 and ROOT-11's rule text follows core §4. Closes CORE-14, CORE-16, A-15.
- Review fix: §4 *Pagination*: the sentence permitting offset paging for UI grids over bounded sets is removed, deliberately - keyset (cursor) paging is the only paging (core §4 *Wire contract*), and a bounded set a UI grid shows in full is a collection bounded by construction.
- Review fix: `annex-agents.md` §4 *Pagination*: an agent-reachable paginated query declares a lower maximum page size than the UI's, and a collection bounded by construction is returned whole and counts against the run's row budget.
- Review fix: §13 maps `If-Match` to the message's expected version, a stale version surfacing at commit as a `Result` precondition failure that the result translator maps to `412`; §4 *Pipeline behaviors* (Transaction) and §5 name the precondition failure beside the conflict.
- Review fix: §4 *Idempotency*: the idempotency record stores a payload fingerprint, and a concurrent attempt's index violation is answered as a replay when the fingerprints match, or `422` when they differ.
- Review fix: §10 *Correlation*: `traceId` is written from `Activity.Current.TraceId`, not from ASP.NET Core's default (`Activity.Id` or `TraceIdentifier`).
- Review fix: §10 *Correlation*: the dispatch context's correlation id is the current `Activity`'s trace id; an outbox row persists the trace context (`traceparent` and `tracestate`), which the dispatcher restores; a background job starts a trace per execution. BE-91's rule text and check, and BE-92's rule text, follow.
- Review fix: §10 *Metrics and traces*: metrics are exported with exemplars, sampled by the trace (`ExemplarFilterType.TraceBased`).
- Review fix: §13: every problem type a module returns is a declared URI with its status, title and user-facing flag, from which `x-problem-types` is generated; validation errors are keyed by the request's JSON property paths.
- Review fix: §7, BE-83: `x-data-class` is emitted on every property of every schema the document contains, problem details and the page included; the row's rule text and check follow.
- Review fix: §4 *Pagination*, BE-19: a declared stream is not a collection.
- Review fix: §13: the CORS bullet admits every request header the wire contract uses and exposes every response header a client reads.
- Review fix: inventory: BE-19's mechanism is `arch` + `ci`; BE-115's section is §13 and BE-116's §4, the sections of their rules.
- Review fix: stage 3 removed "Every response returns it" - the correlation id on success responses - from §10 *Correlation*, deliberately: an external caller's trace is linked rather than adopted (core §9), so the server's trace id is not the caller's, and it reaches the caller as `traceId` on a problem details response.
- Second review fix: §13: `If-None-Match: *` maps to the message's create-only flag; the repository's load reports a stale expected version as an outcome the handler translates to a precondition failure before it acts, a version changing between load and commit surfaces at commit, a create-only message's id-index violation is a precondition failure, and every precondition failure maps to `412`.
- Second review fix: §13: a versioned write returns the new `etag` in the body, and `Content-Location` is removed from the CORS exposed headers. The representation-variant ETag and `Content-Location` were dropped as unowned and against RFC 9110 §8.7 / §13.1.1.
- Second review fix: §4 *Idempotency*, BE-28: the payload fingerprint is an HMAC under a server key, never a plain hash, classed `internal`; after any commit conflict or precondition failure the idempotency behavior looks up the attempt identity and answers a recorded attempt as a replay, or `422` when the fingerprints differ.
- Second review fix: §10 *Correlation*: where outside requests reach the application directly with no ingress stripping `traceparent`, the composition root installs a `DistributedContextPropagator` that starts every request's trace and records the caller's as a link.
- Second review fix: §13: the result translator maps each validation failure's message path to its request path; `Platform` declares the pipeline's and the framework's problem types and overrides ASP.NET Core's per-status defaults through `CustomizeProblemDetails`.
- Second review fix: §4 *Pagination*: a bounded set a UI grid shows in full is bounded by construction only where it declares its bound and rejects a request beyond it.
- Second review fix: inventory, BE-11: the check admits `ICommand<T>` closed over a version, or a result carrying an id, a version, or both.
- Second review fix: §14 **Concurrency** (BE-115): the test also proves `If-Match: *` is refused with `428`, and a create with `If-None-Match: *` for an existing id with `412`.

## 3.0 stage 4

- §6: the object check (core §7) is in the handler - a command handler checks the loaded aggregate through a resource-authorization port in `Platform.Application` over `IAuthorizationService.AuthorizeAsync(principal, resource, policy)`; a query handler applies the record scope in its read. §14 **Authorization** gains a required test (BE-117), with its row. Closes S-1.
- §10 *Audit trail*: new subsection - an append-only store, a command's record written through its unit of work, denials by the authorization behavior, `Restricted` reads by the logging behavior, and a failed audit write fails the operation. §14 gains **Audit** (BE-118), with its row. Closes S-3.
- §4 *Pagination*: a cursor is encrypted and authenticated with ASP.NET Core Data Protection under a purpose per query. New ⚙ BE-119 (`arch`), with its row. Closes S-7, NET-A-17.
- §13: an upload endpoint stores the file in quarantine through a command that does not persist, and a persisting command records it once the scan passes. §14 gains **Uploads** (BE-120), with its row. Closes S-6.
- `annex-realtime.md` §12: a connection closes when its authentication expires and has a bounded lifetime no longer than the external authority's cache lifetime; every hub invocation is rate limited by a hub filter keyed on the principal; a client never names the group it joins. The annex's Testing gains **Real-time** (BE-121), with its row, and its intro, which said no required test belonged to the annex, now matches the other annexes'. Closes S-2, NET-B-6.
- Inventory Summary: 98 ⚙ marks; 121 rows - 100 `build`, 20 `required-test`.
- Review fix: §10 *Audit trail*: one port, `IAuditTrail`, declared in `Platform.Application` and implemented in `Platform.Infrastructure`; a persisting command's success is recorded through its unit of work, and every other record - a denial by the policy or the object check, a command's failure after its rollback, a command that does not persist, a read of `Restricted` data - through the port outside any unit of work; a denial stays a denial when its record fails. §4 *Transactions and consistency* counts audit records as bookkeeping. §14 **Audit** and BE-118's rule text follow.
- Review fix: §4 *Pagination*, BE-119: a cursor is encoded and decoded only through a cursor codec port in `Platform.Application`, implemented in `Platform.Infrastructure` with Data Protection under a purpose per query; a cursor that fails to decode is a validation failure; the key ring is persisted and shared by every instance. §2's `Platform.Infrastructure` row declares ASP.NET Core Data Protection. BE-119's rule text, check and gap follow.
- Review fix: §6: the object check is made through the resource-authorization port, which reads the principal from the dispatch context; each module's resource authorization handlers live in its `Infrastructure` and are registered by its entry point. A query's record scope is resolved at the composition root, or by the authority adapter, into the dispatch context. The authorization behavior carries every operation's policy decision. §14 **Authorization** (BE-117) exempts an operation whose policy grants every record.
- Review fix: §10 *Metrics and traces*: an OpenTelemetry processor, and its logging equivalent, removes `exception.message` from every exception whose type the solution does not declare (core §8). §10 *Logging*: the logging behavior is the only place a dispatch is logged. §15: the fault-isolation boundary logs within core §8's telemetry rule.
- Review fix: §13: the endpoint registrar sets `Cache-Control: no-store` on an endpoint whose Response DTO resolves to `Confidential` or `Restricted`, and `private` on every other principal-dependent endpoint.
- Review fix: §13: an upload's size limit is enforced at the request boundary before the body is buffered; the quarantined file's name is derived from the command's client-supplied id; the recording command runs under the uploader's principal recorded with the file.
- Review fix: `annex-realtime.md` §12: SignalR never revalidates a connection's principal; a connection's lifetime is bounded by the access token's, or by the external authority's cache lifetime where one answers for permissions; a rate-limited hub invocation fails with a `HubException` naming the limit.
- Review fix: Core map: core §7 is also carried by §10 Observability (*Audit trail*) and §13 API Layer.
- Targeted check fix: the authorization package admitted to both Infrastructure rows; the audit trail and cursor codec placed in `Platform.Infrastructure/Persistence/`; the audit port writes through its own context, not the module's, so a command that does not persist stages nothing; an upload's stored name derives from a parsed id, never the client's file name; which endpoints are principal-dependent is reached by review.

## 3.0 stage 5

- §2: the `SharedKernel` and `<Module>.Domain` rows declare the BCL and `Microsoft.Extensions.Compliance.Abstractions`; the shared-projects paragraph and the Layout's `Domain` line admit that one package, which carries the classification attributes and no behaviour. The §2 rationale above ("`SharedKernel` admits only ...") follows. Closes NET-B-11.
- §7 *Declaring a class*, BE-54: a class is a `DataClassification` from Microsoft.Extensions.Compliance; `SharedKernel` defines one attribute derived from `DataClassificationAttribute` per class of core §8; the illustration uses `[Restricted]`. BE-54's rule text, check and source follow. Closes NET-B-11.
- §7 *Logging*, BE-57: logging is structured through source-generated `LoggerMessage` methods with a constant template that interpolates no value; redaction is Microsoft's (`EnableRedaction()`, a redactor per class, the erasing redactor for `Restricted` and unresolved classes), replacing the destructuring policy. BE-57's rule text, check (CA2254 and CA1848 as errors) and source follow. Closes NET-B-11.
- §1: the Stack list adds Microsoft.Extensions.Compliance and Microsoft Agent Framework (agents annex). Closes NET-B-11, NET-B-13.
- §10 *Logging*: code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is the provider. Owner's decision E2 (2026-11-03).
- §3 *Entities and Domain Events*: the unit of work dispatches domain events to their in-module reactions before it commits, in the same transaction. §2: a reaction changes state as a command handler does, inside the command's unit of work, and calls no handler. §4 *Transactions and consistency*: one transaction SHOULD modify exactly one aggregate, and a second changed by a reaction needs the PR to name its invariant (the module-boundary sentence was not added: the next bullet states it with BE-32). Closes NET-A-8, A-19.
- §4 *Pipeline behaviors*: items 4 (idempotency) and 5 (transaction) become one item 4, the unit-of-work behavior, so the idempotency record and its change are one commit; "the idempotency behavior" and "the transaction behavior" become "the unit-of-work behavior" in §4 *Idempotency* and *Transactions and consistency*, §5 and §13, and in BE-111's check. The BE-28 bullet's lookup sentences now point at item 4. Closes N-1.
- §2: a contract's public surface admits the page and cursor types a published collection query returns. Closes N-3.
- §4 *Commands and Queries*: handlers MUST NOT call other handlers directly; a handler MAY dispatch another module's published query through the dispatcher, through the full pipeline. Closes N-5.
- `annex-realtime.md` §12, BE-74: a hub may reference its DTOs, the message it dispatches and their component types, the hub base type, the dispatcher and its `Result`, and framework primitives. BE-74's rule text follows. Closes N-7.
- §3 illustration: the operation takes `DateTimeOffset now`, with no `_clock`. §15: inject `TimeProvider` into the handler and pass the time to the aggregate as an argument. Closes N-11.
- §5 *EF Core mapping for DDD*: a single-value value object MAY be mapped through a value converter. Closes NET-A-12.
- §9 *Transport*, BE-70: every endpoint is covered by a `GlobalLimiter` or a named policy the registrar applies, and `[DisableRateLimiting]` is forbidden; the registrar applies the write policy to every command endpoint; a new bullet states when `Retry-After` is sent and that a limit is per instance unless shared. BE-70's rule text, check and source follow. Closes NET-B-8, A-23.
- §13: the registrar applies each endpoint's authorization and rate-limit metadata from its message's operation declaration. The §13 rationale above ("A lambda's signature ...") is corrected. Closes A-23, NET-B-7, A-22.
- §4 *Pagination*, BE-23: the `ORDER BY` and keyset predicate are generated from the descriptor. BE-23's rule text and check follow. Closes A-17.
- Inventory: BE-50's mechanism is `arch` + `roslyn` (every `AuthorizeAttribute` sets a policy name; `RequireAuthorization()` with no arguments banned); BE-96's check sweeps the solution's own options classes for a registered `IValidateOptions<T>` and `ValidateOnStart`; BE-12's mechanism is `arch` alone, with an ArchUnitNET method-call condition for `SaveChangesAsync`. Owner's decision E14 (2026-11-03).
- `annex-agents.md` §8 *Effective permissions*: the pointer to core §7 follows its new wording (a subset, never wider, always with the actor claim); an autonomous run acts as a service principal declared for its manifest. §9 *Agent runs*: invocation, wall-clock and spend budgets are enforced by the agent runtime's middleware, and the row budget by the dispatch pipeline. Closes C-2, NET-B-13.
- Cache invalidation (annex-caching, BE-48) runs after commit, from the committed domain events, not as an in-transaction reaction: E3 moved reactions before commit, and the annex's own rule increments stamps after commit so a concurrent read cannot repopulate the cache with pre-commit data.
- Review fix: BE-54's check also proves each logging-method parameter's class attribute equals its type's class (core §8).
- Review fix: §7 *Logging*: the erasing redactor covers `Confidential` and `Restricted`, and every parameter of a logging method carries its class attribute, since the redactor applies only to classified parameters; the per-class redactor and the unresolved-class clause are removed. BE-55's graph walk also covers every logging-method parameter.
- Review fix: §10 *Logging*: Serilog is registered on the logging builder with `AddSerilog()`; replacing `ILoggerFactory`, as `UseSerilog()` does, is forbidden because redaction runs in the factory `EnableRedaction()` configures. §14 *Classification*: a test proves a classified logging-method parameter is redacted in the provider's output.
- Review fix: §9 *Transport*: a stricter policy, and an expensive read's tighter policy, are named in the operation declaration, not chosen by the endpoint.
- Review fix: `annex-agents.md` §8 *Effective permissions*: for an autonomous run, the service principal's permissions stand in for the user's (core §7).
- Review fix: §4 *Pipeline behaviors*, item 4: the attempt identity is resolved, and the idempotency record written, only for a command that declares a key. After a successful commit the behavior hands the committed domain events to their post-commit effects, such as cache invalidation; a failure there is logged as an error and the committed result is returned. `annex-caching.md`: the invalidation-failure bullet says the committed result is returned and the TTL bounds the stale entry.
- Review fix: §13, BE-80: the shared endpoint abstraction takes the Request DTO, the message it dispatches and the Response DTO as type arguments, so the registrar can find the operation declaration.
- Review fix: §2: the `Api` row declares the logging provider and `Microsoft.Extensions.Telemetry` with `Microsoft.Extensions.Compliance.Redaction`; the Layout's `Domain` line names `Microsoft.Extensions.Compliance.Abstractions` as its one framework reference.
- Review fix: §4 *Commands and Queries*: a cross-module query a handler dispatches runs on its own module's connection, never enlisted in the command's transaction.
- Review fix: §4 illustration: the switch passes `now` to the aggregate operation, matching §3's signature.
- Review fix: §9 *Transport*: the `Retry-After` bullet states that ASP.NET Core writes neither the status nor the header; `OnRejected` sets `429` and copies `MetadataName.RetryAfter`.
- Review fix: `annex-agents.md` §9 *Agent runs*: function-invocation middleware reaches an agent only where it invokes tools through `FunctionInvokingChatClient`.
- Review fix: Inventory: BE-57, BE-23 and BE-70 state their gaps (Serilog's static `Log`, direct `ILogger.Log` and interpolated arguments; `ORDER BY` in SQL text; framework-mapped endpoints); BE-50's check names BE-25's sweep as its named-policy half; BE-48's check also proves no in-transaction reaction slice references `ICacheInvalidator`.

## Rationale moved in 3.0 stage 5

Reasons, consequences and examples removed from `skills/law/SKILL.md` when the law was shortened in stage 5, verbatim, grouped by section and prefixed with the rule they came from. No rule changed; the rule text stays in the law.

### §1 Stack

- **Verify third-party licenses before pinning**: MediatR and FluentAssertions moved to commercial licensing; the picks above avoid that.

### §2 Project Reference Law

- **BE-02**: Checkable by `.csproj` and by an architecture test.
- **SharedKernel and the two Platform.* projects are shared, not modules**: `SharedKernel` and `Domain` admit one package beyond the BCL, `Microsoft.Extensions.Compliance.Abstractions`, because it carries the classification attributes (§7) and no behaviour.
- **`Result` lives in `Platform.Application`**: `Domain` does not reference `Platform.Application`, so "the domain MUST NOT reference `Result`" (core §6) is carried by the compiler rather than by an analyzer.
- **`Contracts` references `Platform.Application`**: That is the only reason for the reference
- **`Contracts` references `Platform.Application`**: so the committed contract snapshot (§4) is coupled to the message abstraction and to nothing else.
- **Duplicating an aggregate across feature folders**: it means the module has more than one model of the same concept.

### §3 Domain Layer

- **BE-78** (Aggregates): "Root" is not a folder position or a naming convention; it is a property a check can read, and several rules depend on being able to read it.
- **Primitives in aggregates and entities SHOULD be replaced by Value Objects** (Value Objects): A `DateOnly` date of birth becomes `Birthday`, which rejects future dates and implausible ages.
- **BE-08** (Value Objects): they are how the id is built from, and read as, the wire value its `Contracts` twin carries.
- **BE-08** (Value Objects): so an `int` pressed into service as an identifier is caught in review rather than by the check.
- **BE-109** (Value Objects): A module with an endpoint therefore has a `Contracts` project.
- **Domain events MUST be raised by the aggregate** (Entities and Domain Events): so a reaction commits or rolls back with the command that caused it.
- **A Specification MUST NOT be the authority** (Specifications): the two will drift, and the drift is a bypassed business rule.

### §4 Application Layer

- **BE-14** (Commands and Queries): a read returns the fields the caller needs, never a whole aggregate.
- **BE-15** (Commands and Queries): A query handler that is pure computation is rare
- **BE-15** (Commands and Queries): so a handler's placement never depends on inspecting what it calls.
- **BE-15** (Commands and Queries): Commands do not split this way: a command handler needs only the repository port declared in `Domain`, so it stays in `Application`.
- **BE-108** (Commands and Queries): EF never tracks a keyless type
- **BE-108** (Commands and Queries): SQL is not checked against the model at compile time, so that test is what catches a renamed column.
- **Non-exhaustive switches fail the build** (Commands and Queries): so adding a domain outcome forces every handler to address it.
- **BE-21, BE-22** (Pagination): a caller that asked for 500, received 100, and was told nothing will conclude it has every row.
- **BE-24** (Pagination): A cursor therefore cannot carry a filter, a sort, or a principal: a client that could edit those would otherwise widen its own read.
- **BE-119** (Pagination): so its sort key - which may be classified - is neither readable nor editable by the client.
- **BE-119** (Pagination): so a cursor survives a restart and works on any instance.
- **Pipeline item 4, Unit of work and idempotency** (Pipeline behaviors): so the idempotency record and the change it proves are one commit.
- **BE-27** (Idempotency): an expired cache entry must never make a command executable a second time (§5).
- **BE-30** (Idempotency): it will be forgotten on exactly the endpoint that needed it.
- **BE-110** (Transactions and consistency): the idempotency record and the outbox row are written through the unit of work it does not have.
- **BE-110 (It is idempotent naturally or by a client-supplied id)** (Transactions and consistency): so a retry repeats the same write rather than adding a second.
- **Placement is decided by dependencies, never by the name** (Placement of interfaces and implementations): `IPricingStrategy` and `IEmailSender` are both "services" and belong in different layers.
- **An implementation that is pure logic MUST live in `Application`** (Placement of interfaces and implementations): **Only one direction of this error is caught by the compiler.** Infrastructure logic in `Application` fails to build - the reference is not there. Application logic in `Infrastructure` builds cleanly, because `Infrastructure` references `Application`.
- **BE-34, BE-107** (Placement of interfaces and implementations): The source-generated dispatcher names each concrete handler in its generated registration, so every concrete query handler is public. Where the compiler no longer holds the boundary for a public type, BE-105's check holds it.
- **BE-36** (Placement of interfaces and implementations): so the two halves of a read slice are findable from each other.

### §5 Persistence

- **Every aggregate MUST carry an application-managed version**: a database row version protects one row, not the aggregate.
- **A data migration MUST be idempotent** (Migrations): for the same reason a command is (§4).
- **A repository MUST load the complete aggregate** (Repositories and the aggregate graph): an incompletely loaded aggregate cannot enforce its own invariants.
- **BE-43** (EF Core mapping for DDD): **If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type.**

### §6 Authorization

- **BE-51**: so a forgotten attribute fails closed.
- **External authorities (introduction)** (External authorities): A token claim cannot be unavailable, and neither can a signature checked against keys the process holds. A remote service that validates the token or answers for permissions can be unreachable, its answers can outlive a revocation, and a permission service answers for the user when the caller is an agent. The rules below cover the first two cases, and key the third on the acting principal; the agent case itself is in `references/annex-agents.md`.
- **BE-100** (External authorities): and that value is the revocation window: a permission withdrawn at the authority continues to grant for that long.

### §7 Data Classification

- **BE-54** (Declaring a class): `Domain` and `Contracts` are peers (§2), so neither can inherit the other's declaration
- **BE-83** (Declaring a class): so it reaches every client with the contract.
- **Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a classified value** (Logging): core §6 forbids leaking internals; this extends it to the data.
- **`Restricted` columns MUST be encrypted at rest** (Cache, events, and storage): Driver-level encryption limits what the database can do with the column - deterministic encryption allows equality only, randomized encryption allows nothing, and range, pattern and sort operations need a secure enclave where the server version supports one
- **`Restricted` columns MUST be encrypted at rest** (Cache, events, and storage): for the same reason it MUST NOT know it is persisted (§5).

### §9 Quotas and Rate Limits

- **Limits attach to the caller and to the run**: HTTP rate limiting does not bound an agent, because agent dispatch never passes through the middleware pipeline.
- **Limits are keyed on the acting principal** (Transport): it is a shared bucket that one customer's office fills.

### §10 Observability

- **An expected business rejection MUST NOT be logged at `Error`** (Logging): and logging it as one teaches everybody to ignore the error log.
- **Every pipeline behavior MUST emit a span** (Metrics and traces): "the request was slow" is not a finding anyone can act on.
- **Metrics are exported with exemplars** (Metrics and traces): the SDK's default is off.
- **Liveness MUST NOT depend on a downstream** (Metrics and traces): a failing dependency restarts nothing.
- **A persisting command's success is recorded through its unit of work** (Audit trail): so the record commits with the change.

### §11 Configuration and Secrets

- **BE-95**: which is what makes BE-96's validation reach every setting.
- **BE-97**: connection strings, client secrets, signing keys, API keys and package-feed tokens are examples of the class, not the list of it, and `appsettings*.json`, environment files and source are where it has happened, not where the rule applies.
- **BE-98**: so nothing below it needs to ask.

### §13 API Layer

- **An endpoint maps `If-Match` to the message's expected version**: so a version that changes between load and commit surfaces when the unit of work commits.
- Restored after the tightening pass, because each tells an agent how to apply a rule: "A module with an endpoint therefore has a `Contracts` project", "every concrete query handler is public" (BE-34), and "If a mapping cannot be expressed without changing the domain type, the mapping is wrong" (BE-43). Their copies under *Rationale moved in 3.0 stage 5* are superseded.

## 3.0 stage 7

Every inventory row re-derived against the tool that would enforce it (stage 7 findings `s7-netA`, `s7-netB`, `s7-netC`, `s7-tests-s15`); each row's Source cites the documentation or source code the verdict rests on.

- §2: an analyzer - a package or project referenced only as an analyzer (`PrivateAssets="all"`; for a project, `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`) - is admitted in every project and is outside the table. BE-01's check skips analyzer references, and the inventory's `roslyn` legend names an analyzer package. Closes the finding that BE-01's allowlist failed every analyzer a `roslyn` row requires, BE-113's `BannedApiAnalyzers` included.
- §2 organization table: the `Infrastructure` row lists `Queries/`, which §4 *Placement of interfaces and implementations* (BE-35) and BE-15 already required. Closes the finding that the table omitted it.
- §3 *Value Objects*, BE-08: a strongly-typed id's compiler-generated members, such as a positional record's `Deconstruct`, are outside the rule, with its constructor and value member. The row's rule text and check follow. Closes the finding that a positional `record struct` id exposes a public `Deconstruct(out Guid)` the rule did not exempt.
- §4 *Placement of interfaces and implementations*, BE-107: `InternalsVisibleTo` may also name the proxy assembly of the test project's mocking library, such as `DynamicProxyGenAssembly2`. The row's rule text and check follow. NSubstitute requires it to substitute an `internal` type: https://nsubstitute.github.io/help/how-nsub-works
- §5 *EF Core mapping for DDD*, BE-43: "Domain types MUST NOT acquire ... to satisfy the ORM" becomes "A Domain type the EF model maps MUST NOT have a public parameterless constructor, a public setter, or an EF attribute". The motive clause could not be checked under any wording, and the rule bound unmapped types (specifications, domain events, outcome records) that get an implicit public constructor. The row's rule text and check follow.
- §14 **Domain**: the invariant-and-outcome test carries an id, BE-138, with its row.
- §14 **Application**: the policy-denial test proves a principal that does not hold the policy is denied, and carries an id, BE-122, with its row. Source: https://owasp.org/API-Security/editions/2023/en/0xa5-broken-function-level-authorization/
- §14 **Pagination**: a collection bounded by construction has a test proving a request beyond its bound is rejected, never truncated (core §4); the bullet carries an id, BE-124, with its row.
- §14 **Idempotency**: the replay test carries an id, BE-125, and a new test proves a request carrying no key is refused with `400` (core §4), BE-137; both with rows. An in-flight duplicate is not answered `409` here: the unit-of-work behavior answers it as a replay, which BE-29 proves.
- §14 **Classification**: the redaction test carries an id, BE-126, with its row. Source: https://learn.microsoft.com/dotnet/core/extensions/data-redaction
- §14 **Rate limits**: new required test, BE-127 - every rate-limit policy and the `GlobalLimiter` refuse beyond their limit with `429` problem details, with `Retry-After` where the limiter can estimate it. Not `contract`: `RateLimiterOptions` exposes no list of its policies. Source: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- §14 **Observability**: new required test, BE-128 - every registered pipeline behavior, and the handler, emits a span in pipeline order (§10 *Metrics and traces*).
- `annex-agents.md` **Testing**, **Agents**: the outside-the-manifest test carries an id, BE-123, with its row.
- §15: each item but commented-out code carries a mark, BE-129 to BE-136, with its row; commented-out code stays reached by review, because SonarAnalyzer S125 is a heuristic that would flag compliant comments. The fault-isolation marker is an attribute the solution declares once in `SharedKernel`. For BE-135 a wrapping `throw new X(…, ex)` counts as rethrowing; for BE-132 `SharedKernel`, `TimeProvider.System` and `Stopwatch` are in the banned set.
- Inventory *How to read this*: every `arch` check assumes ArchUnitNET 0.13.5 or later, or a Debug build. Source: https://github.com/TNG/ArchUnitNET/releases/tag/0.13.5, https://github.com/TNG/ArchUnitNET/issues/498, https://archunitnet.readthedocs.io/en/latest/limitations/debug_artifacts/ (finding `s7-netA`, cross-cutting fact).
- Inventory Summary: 106 ⚙ marks; 138 rows - 108 `build`, 29 `required-test`, 1 `dropped`, 0 `retired`.
- Rows, check corrected: ROOT-15, BE-01, BE-02, BE-106, BE-04, BE-05, BE-08, BE-109, BE-10, BE-79, BE-14, BE-15, BE-18, BE-21, BE-22, BE-24, BE-27, BE-28, BE-34, BE-36, BE-84, BE-37, BE-39, BE-40, BE-42, BE-43, BE-45, BE-87, BE-50, BE-51, BE-52, BE-54, BE-55, BE-56, BE-58, BE-63, BE-68, BE-69, BE-73, BE-93, BE-94, BE-96, BE-97, BE-98, BE-75, BE-76, BE-77, BE-81.
- Rows, gap stated or check narrowed to what the tool reaches: ROOT-04, ROOT-07, ROOT-19, BE-03, BE-06, BE-07, BE-78, BE-12, BE-13, BE-113, BE-19, BE-20, BE-23, BE-119, BE-30, BE-31, BE-32, BE-33, BE-110, BE-35, BE-85, BE-86, BE-38, BE-41, BE-44, BE-46, BE-47, BE-48, BE-88, BE-53, BE-100, BE-102, BE-57, BE-60, BE-61, BE-83, BE-62, BE-67, BE-70, BE-71, BE-91, BE-95, BE-74, BE-80, BE-82, BE-16.
- Rows, check confirmed as written (source added where one was cited): ROOT-01, ROOT-02, ROOT-08, ROOT-09, ROOT-11, ROOT-12, ROOT-13, ROOT-16, ROOT-17, ROOT-18, ROOT-20, BE-105, BE-107, BE-11, BE-108, BE-17, BE-25, BE-26, BE-59, BE-64, BE-90.
- Rows, mechanism changed to match the corrected check: BE-05 `proj`; BE-10, BE-58, BE-60, BE-86 `contract`; BE-14, BE-21, BE-27, BE-63 `arch` + `contract`; BE-23, BE-62, BE-81 `contract` + `arch`; BE-35, BE-76 `arch` + `roslyn`; BE-50 `contract` + `roslyn`; BE-94 `roslyn`; BE-98 `roslyn` + `arch`.
- Rows added: BE-122 to BE-128, BE-137 and BE-138 (`required-test`); BE-129 to BE-136 (`build`).
- Stricter than its rule, BE-04: failed a positional-record domain event, whose `init` accessors read as public setters; the check now binds root-marked types.
- Stricter than its rule, BE-05: failed an aggregate method returning a `SharedKernel` type or a `Microsoft.Extensions.Compliance.Abstractions` type.
- Stricter than its rule, BE-119: failed code that protects something other than a cursor with `IDataProtector`.
- Stricter than its rule, BE-30: failed the idempotency store's own implementation and its registration.
- Stricter than its rule, BE-110: failed a query whose operation declaration names `Transaction: None`, as §6's example gives every operation.
- Stricter than its rule, BE-34: failed the read types the `DbContext` exposes through public `DbSet<T>` properties, and compiler- and generator-emitted types.
- Stricter than its rule, BE-35: failed compiler-synthesized types and generator output outside the allowed namespaces.
- Stricter than its rule, BE-36: a two-way comparison failed a command-only feature folder in `Application`, which has no query handler.
- Stricter than its rule, BE-84: failed query messages, wire ids and nested payload records in `Contracts`, which the rule does not version.
- Stricter than its rule, BE-86: failed query DTOs in `Contracts` that carry page types.
- Stricter than its rule, BE-37: failed a `SharedKernel` value object mapped as owned, the `Platform.Infrastructure` outbox row and idempotency record, and many-to-many join entities typed as property bags.
- Stricter than its rule, BE-43: failed specifications, domain events and outcome records with an implicit public parameterless constructor the ORM never asked for.
- Stricter than its rule, BE-87: failed every new migration, which rewrites the model snapshot.
- Stricter than its rule, BE-88: failed every generated migration, whose Designer partial names its `DbContext` in `[DbContext(typeof(...))]`.
- Stricter than its rule, BE-50: failed `[Authorize(Roles = ...)]`, which is not bare.
- Stricter than its rule, BE-52: failed a registration entry point that wires `IHttpContextAccessor` in a factory lambda, compiled into a nested closure class.
- Stricter than its rule, BE-56: failed a plain `sealed class` with no `ToString` override, which prints only its type name.
- Stricter than its rule, BE-68: failed a declared wrapper parameter such as `SearchTerm`, whose own `Value` is a `string`.
- Stricter than its rule, BE-97: failed a literal `Username` or `ValidAuthenticationTypes` under `<packageSourceCredentials>`, which grant nothing, and a key generated for a test.
- BE-47's rule is widened to forbid enumerating keys at all: banning `IServer.Keys` also stopped a read-only `SCAN`, which the old rule allowed and no use the law gives Redis needs.
- BE-57's rule now says a log scope is begun with a state object (`ILogger.BeginScope<TState>`), never a template: CA1848 fails `LoggerExtensions.BeginScope` with a constant template, which the old rule allowed.
- ROOT-07, ROOT-09, ROOT-12 and ROOT-19 reconciled with the core-row research: ROOT-07 names BE-80, BE-77 and BE-109 with a Request DTO walk, since BE-77 alone covers only the endpoint's own references; ROOT-09 uses `git status --porcelain`, since `git diff` misses an untracked document; ROOT-12 adds a document check for a non-generic envelope; ROOT-19 adds BE-91 for outbox deliveries.
- Review fix: §2: an analyzer is a package or project reference that contributes analyzers only, and no assembly the project compiles against or runs with; a package reference qualifies with `IncludeAssets="analyzers"` and `PrivateAssets="all"`, a project reference with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`. `PrivateAssets="all"` alone still flows compile and runtime assets to the project itself. BE-01 skips a reference as an analyzer only where `project.assets.json` resolves it to no `compile` or `runtime` assets. Source: https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files#controlling-dependency-assets
- Review fix: BE-95, BE-98: `BannedApiAnalyzers` runs only in the project kinds that hold no entry point - `Domain`, `Application`, `Contracts`, `SharedKernel`, `Platform.Application`; in `Infrastructure` and `Platform.Infrastructure`, `arch` admits the banned members and types only in registration entry points and types nested in them. Every banned symbol reports RS0030, so exempting a file for one ban would remove every ban from it; the analyzer cannot be scoped to allow the registration entry points.
- Review fix: BE-43 Check: no property of a mapped Domain type has a public setter, `init` included.
- Review fix: BE-57 Gap: a scope begun with a `string` state through `ILogger.BeginScope<string>` is reached by review.
- Review fix: §14 **Application**, BE-122: the denial test is required for every declared policy that some principal does not satisfy; a policy every principal satisfies has no principal to deny. Row Rule follows.
- Review fix: §14 **Pagination**, BE-124: the beyond-the-bound test is required only for a collection bounded by construction whose size a request determines. Row Rule follows.
- Review fix: §15: a post-commit effect (§4) is named as a fault-isolation boundary beside a hosted-service iteration and an outbox delivery. BE-135's Rule lists none, and is unchanged.
- Review fix: §10 *Logging*: Serilog is registered with `builder.Logging.AddSerilog(logger, dispose: true)`, the logger built from a `LoggerConfiguration` and never assigned to the static `Log.Logger`.
- Review fix: BE-53 Check: restated as the `arch` dependency rule it is - no module `Domain` or `Application` type depends on a type in `System.Security.Claims` or another claims type such as `JwtRegisteredClaimNames`; the reference to `BannedApiAnalyzers`, which does not look at parameter or field types, is removed from the Check and Source.
- Review fix: BE-96 Check: the options types are each options type `T` declared in a solution assembly and bound by an `IConfigureOptions<T>` registration; the startup validator is `IStartupValidator` (.NET 8+), not `IAsyncStartupValidator`. Source: https://learn.microsoft.com/dotnet/api/microsoft.extensions.options.istartupvalidator
- Review fix: Inventory Summary: recounted - four rules restate another rule in this file (BE-05, BE-13, BE-14, BE-32), and eleven core rules restate one (ROOT-02, ROOT-08, ROOT-11, ROOT-12, ROOT-13, ROOT-15, ROOT-16, ROOT-17, ROOT-18, ROOT-19, ROOT-20). ROOT-07's Check begins "Covered by" and is not counted among core rules.
- Review fix: BE-135 Check: also accepts a `[LoggerMessage]` method whose level is a parameter, called with `LogLevel.Error`.
- Review fix: BE-129 Gap: reflection over `DynamicAttribute` on members and parameters would reach a `dynamic` declaration with no dynamic operation.

## 3.0 stage 8

Release-review fixes (findings `s8-review-dotnet`, ids ".NET Bn/Sn/Nn"; `s8-review-core`, ids "core Bn/Sn/Nn"; closure audit `s8-closure`).

- §4 *Commands and Queries*: "Non-exhaustive switches fail the build" is replaced with what C# does. Translation is a switch expression, never a switch statement, which gets no exhaustiveness check. A switch expression over an enum outcome that misses a named member raises CS8509, an error under `TreatWarningsAsErrors`; CS8524, raised for an enum's unnamed values, is disabled in the solution's build configuration, so no discard arm is written and an added member fails the build. A hierarchy of outcome types gets no exhaustiveness check, so each outcome's translation has a required test, BE-141, with its row (§14 **Application**). The illustration's comment says it has no discard arm; with CS8524 disabled it compiles. Closes release review core B1.
- Source for the CS8509 and CS8524 behaviour of a switch expression: https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/pattern-matching-warnings
- Source for a switch statement falling through with no case matched and no exhaustiveness diagnostic: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements
- §13: the quarantined upload is stored under a name the server derives from the uploader's principal and the command's client-supplied id with a keyed hash, so a retry finds the same file and no client can name another's. Closes release review core B3.
- §13: a write that leaves a versioned resource in place returns its new `etag` in a body; a delete returns `204`. Closes the .NET half of release review core S1.
- §10 *Correlation*: the application adopts no outside caller's trace wherever the caller's `traceparent` reaches it, directly or through an ingress that forwards the header; an ingress that strips it leaves nothing to link. The composition root also sets OpenTelemetry's default propagator to a plain `TraceContextPropagator` (`Sdk.SetDefaultTextMapPropagator`): the SDK's default is a composite of `TraceContextPropagator` and `BaggagePropagator`, and under any propagator that is not a plain `TraceContextPropagator` the ASP.NET Core instrumentation re-extracts the incoming context and starts a sibling activity parented to the caller's. New required test, BE-139, with its row (§14 **Correlation**). Closes release review .NET B4 and the .NET half of core B2.
- Source for the OpenTelemetry SDK's default propagator and `Sdk.SetDefaultTextMapPropagator`: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry/Sdk.cs
- Source for the ASP.NET Core instrumentation's re-parenting under a propagator that is not `TraceContextPropagator`: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/Implementation/HttpInListener.cs
- Source for `DistributedContextPropagator`, the propagator ASP.NET Core extracts an incoming trace context with: https://learn.microsoft.com/dotnet/api/system.diagnostics.distributedcontextpropagator
- Source for recording the caller's trace as a link (`Activity.AddLink`): https://learn.microsoft.com/dotnet/api/system.diagnostics.activity.addlink
- `annex-agents.md` §7 and §8: a tool's output is a tool DTO, a projection of the dispatched message's Response DTO made only by the tool projector; the model-visible DTOs are the tool DTO and the tool's Request DTO, never the UI Response DTO. BE-58 marks those two; BE-59 forbids `Confidential` and `Restricted` in the tool DTO and `Restricted` in the tool Request DTO; a `Confidential` tool argument is admitted where the operation's policy grants it (core §8); BE-69 checks that the projection takes the dispatched message's Response DTO. Agent reads still go through the same query handlers. Rows BE-58, BE-59, BE-69 follow. Closes release review .NET B1 and B2.
- §4 *Placement of interfaces and implementations* and `annex-realtime.md`: a real-time push adapter is implemented in `Api`, which hosts the hubs; BE-01's check says why and BE-105's check puts the adapter outside its rule. Closes release review .NET B3.
- Source for `IHubContext<T>` shipping in the ASP.NET Core shared framework (`Microsoft.AspNetCore.App.Ref`) from 3.0: https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.signalr.ihubcontext-1?view=aspnetcore-10.0
- Source for the standalone `Microsoft.AspNetCore.SignalR.Core` package stopping at 1.x: https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Core
- BE-45 Check: no type other than the cache decorator writes a query response to the cache store; the invalidator, the stamp store, an authority adapter's decision cache, a declared coordination-state adapter and the registration entry points may depend on the store, by name. Closes release review .NET B5.
- BE-14 Check: owned entity types (`IsOwned`) and shared-type entity types (`HasSharedClrType`) are exempt from the keyless sweep, as BE-37's already were. Closes release review .NET B6.
- BE-11 Check: also admits a `Result` carrying only its outcome. Closes release review .NET B7.
- BE-100 Check: binds an authority adapter that depends on a cache type. Closes release review .NET B8.
- BE-119 Check: matches only the Base64 members and a `JsonSerializer` call applied to the cursor type. Closes release review .NET B9.
- BE-75 Check: binds method-level `[Authorize]` only; a class-level connection policy, which BE-51's fallback requires, is outside it. Closes release review .NET B10.
- §7 *Logging*, BE-54, BE-55: the first `ILogger`, `LogLevel` and `Exception` parameters of a logging method carry no class attribute; BE-55's walk skips them and stops at a parameter that declares its class; an exception is governed by core §8's exception rule and the §10 processor. Closes release review .NET B11.
- Source for the logging generator's special treatment of the first `ILogger`, `LogLevel` and `Exception` parameters: https://learn.microsoft.com/dotnet/core/extensions/logging/source-generation
- §7 *Declaring a class*, BE-54: a projected property names its classified source by its `Contracts` twin. Closes release review .NET S1.
- Not applied: release review .NET S2 (the authority decision cache as a second owner of the cache boundary). The reviewer's first fix names it in core §8, which is the core plugin's file; listed for the core agent.
- BE-20 Check: the paging members are `NextCursor` and `TotalCount`. Closes release review .NET S3.
- §13: the framework endpoints - the probes, `MapHub` and the OpenAPI document - are a declared list outside BE-80 and BE-81, and stay under the rate limit and the fallback policy. BE-81's Check points at the list. Closes release review .NET S4. Core §4's "Every endpoint" (ROOT-07) is the core plugin's.
- §2 `Platform.Application` row names `Mediator.Abstractions`, `FluentValidation` and `Microsoft.AspNetCore.Authorization`; ROOT-15's Check says a module's `Application`. Closes release review .NET S5.
- Source for `Microsoft.AspNetCore.Authorization` being published as a package for the current major version: https://www.nuget.org/packages/Microsoft.AspNetCore.Authorization
- §4 *Pipeline behaviors*, item 4: every failure result looks up the attempt identity before it is returned; a keyed failure result is recorded after the rollback, in a transaction of its own on the module's context, so a replay returns it; an exception is not recorded. §4 *Idempotency* follows. Closes release review .NET S6.
- §9 *Transport*: a stricter policy chains the write limiter inside the named policy with `RateLimiter.CreateChained`, since an endpoint carries one named policy beside the `GlobalLimiter`. Closes release review .NET S7.
- Source for one endpoint policy replacing another beside the global limiter, and for chaining limiters in a named policy: https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0
- `annex-caching.md`: the undecorated-handler test moves to **Testing**, **Cache**, as BE-140, with its row. Closes release review .NET S8.
- The unmarked tool claims below gain sources. Closes release review .NET S9 and the .NET half of core S11.
- Source for ASP.NET Core writing neither `429` nor `Retry-After` by default, `OnRejected` setting both, and the concurrency limiter giving no `RetryAfter` (§9): https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0
- Source for ASP.NET Core's default `traceId`, `Activity.Current?.Id ?? httpContext.TraceIdentifier` (§10): https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs
- Source for `AddSerilog(logger, dispose: true)` on the logging builder adding a provider whose disposal disposes the logger (§10): https://github.com/serilog/serilog-extensions-logging/blob/dev/src/Serilog.Extensions.Logging/SerilogLoggingBuilderExtensions.cs
- Source for `UseSerilog()`, through `AddSerilog` on the service collection, registering its own `ILoggerFactory` (§10): https://github.com/serilog/serilog-extensions-hosting/blob/dev/src/Serilog.Extensions.Hosting/SerilogServiceCollectionExtensions.cs
- Source for `EnableRedaction()` on the logging builder and redaction applying to classified parameters (§7): https://learn.microsoft.com/dotnet/core/extensions/data-redaction
- Source for SignalR not revalidating a connection's credentials and `CloseOnAuthenticationExpiration` (real-time annex); authentication refresh exists only from .NET 11, outside the .NET 10 stack: https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0
- Source for function-calling middleware requiring `FunctionInvokingChatClient` (agents annex §9): https://learn.microsoft.com/agent-framework/concepts/agents/middleware/
- Source for `ExemplarFilterType.TraceBased` (§10): https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/docs/metrics/exemplars/README.md
- §4 *Pagination*: the key ring is persisted in the database through `PersistKeysToDbContext`, or in Blob storage with its keys protected by Key Vault, never in a cache. `PersistKeysToStackExchangeRedis` would make Redis a source of truth, against the caching annex. Closes release review .NET S10.
- Source for `PersistKeysToDbContext`, `PersistKeysToAzureBlobStorage` and the Redis key store: https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-storage-providers?view=aspnetcore-10.0
- §7 *Logging*: every project that declares a logging method references `Microsoft.Extensions.Telemetry.Abstractions`, whose generator replaces the in-box one and reads the class attributes. Closes release review .NET S11.
- Source for `Microsoft.Extensions.Telemetry.Abstractions` replacing the default logging generator: https://www.nuget.org/packages/Microsoft.Extensions.Telemetry.Abstractions
- Source for that generator reading `DataClassificationAttribute`: https://github.com/dotnet/extensions/blob/main/src/Generators/Microsoft.Gen.Logging/Parsing/SymbolLoader.cs
- Source for the in-box generator reading no attribute but `LoggerMessageAttribute`: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Abstractions/gen/LoggerMessageGenerator.Parser.cs
- `annex-agents.md` §8, BE-62: a hosted tool the runtime adds outside the manifest is disabled - a `HarnessAgent` sets `DisableWebSearch = true` - checked by a custom analyzer; mechanism `contract` + `arch` + `roslyn`. Closes release review .NET S12.
- Source for `HarnessAgent` adding `HostedWebSearchTool` by default and `DisableWebSearch`: https://learn.microsoft.com/agent-framework/agents/tools/web-search
- Inventory *How to read this*: the example test name is `BE_108_QueryHandlers_DoNotReferenceDomain`. Closes release review .NET N1 and the .NET half of core N1.
- Inventory: what each annex holds is listed in full. Closes release review .NET N2.
- §6: the operation declaration example names its idempotency mechanism and optional rate-limit policy, and its type is `Operation`. Closes release review .NET N3.
- §4 *Transactions and consistency*: the refusal points at §4 *Pipeline behaviors*, item 4, and its test at §14. Closes release review .NET N4.
- `annex-caching.md`: an invalidation failure is logged at `Error`, never silently discarded. Closes release review .NET N5.
- §10 *Correlation*: a background job logs a start event once; the enricher supplies its trace id. Closes release review .NET N6.
- §10 *Audit trail*: the store is one table in each module's schema, for the success records its unit of work writes, and one the port's own context writes. Closes release review .NET N9.
- §13 and `annex-realtime.md`: "the Application message it dispatches" becomes "the message it dispatches", since a published query's message is in `Contracts`. Closes release review .NET N10.
- Inventory Summary: eleven core rules restate one, their Check naming it `Canonical:`; ROOT-07's Check begins `Covered by` and is not counted. Closes the .NET half of release review .NET N8 and core S6.
- Nit not applied: release review .NET N7 (state that validation precedes the idempotency check, so a reused key with an invalid payload gets `400`): it would add a rule.
- §10 *Audit trail*: an export, report or file handed to a user is recorded through the port (core §8). Closes release review core S9.
- §11 and BE-97: a test key is exempt where it is listed, by file and key, in `tests/test-keys.txt`, the list the rule names. Closes release review core S7.
- §4 *Idempotency*: the fingerprint's class is `Internal`. Closes release review core N7.
- `annex-agents.md` §8: an agent principal MUST hold a subset of permissions and MUST carry the actor claim. Closes release review core N6.
- BE-90 Gap: a default or empty trace id passes the constructor; it is reached by the correlation test (BE-92). Closes closure audit BE-90.
- Inventory Summary: 106 ⚙ marks; 141 rows - 108 `build`, 32 `required-test`, 1 `dropped`, 0 `retired`.
- Rows added: BE-139, BE-140, BE-141 (`required-test`).
- Rows, check corrected: ROOT-15, BE-01, BE-105, BE-11, BE-14, BE-20, BE-119, BE-45, BE-100, BE-54, BE-55, BE-58, BE-59, BE-62, BE-69, BE-75, BE-81, BE-97. Gap stated: BE-90, BE-59.
- Deferred: the duplicate merges the review asked for (BE-13/BE-41, BE-79/BE-42, BE-38/BE-43, the five query-side bullets) are linked by `Covered by`, not merged; and the .NET stream loads ~19.8k tokens after the release fixes, against the ~16k target. Both are left for a later release.
- Re-review fix: a switch expression over a hierarchy of outcome types always raises CS8509, since C# cannot prove it exhaustive, so it ends in a discard arm that throws `UnreachableException` and BE-141's test proves each outcome; the illustration's no-discard-arm rule is for an enum outcome. BE-141's check no longer says CS8509 reaches only an enum's members.
- Re-review fix: the framework endpoints each carry a named policy, such as an anonymous one for the probes, or fall to the fallback, which denies them; a probe with no credentials could otherwise never succeed.
- Re-review fix: the propagator sentence names a propagator that extracts `traceparent`.

## 2.1.0

Law 3.1. Closes the two items 3.0 deferred (`## 3.0 stage 8`, "Deferred"). Every sentence and row changed is listed, old and new, in `RECORDS-3.1-ledger.md` at the repository root.

### Duplicate rules merged

The 2026-11-03 review (`RECORDS-3.0-review.md` §6, item 4) asked for these merges; 3.0 linked them with `Covered by`. One rule and one row now carry each obligation; the other id is `retired`, `Merged into` the survivor, as the inventory legend defines.

- BE-42 into BE-79 (§3 *Aggregates*): "a navigation property between aggregate roots included". BE-79's row gains BE-42's EF-model check; mechanism `arch` + `contract`.
- BE-41 into BE-13 (§4 *Commands and Queries*): a command handler loads through its repository, calls the aggregate's methods and persists through the repository; "There is no other write path". BE-13's row gains BE-41's check (no `Application` type references the unit-of-work port), and states BE-01's part directly rather than as `Covered by`. The contradiction the review found between the two rows (net review §5) no longer arises.
- BE-43 into BE-38 (§5 *Persistence*): no mapping attribute, an EF attribute included, on a Domain type; no public parameterless constructor or public setter on a mapped one; the last sentence of BE-43 moves with it. BE-38's row gains BE-43's check. BE-04's Gap now points at BE-38's setter ban.
- The five query-side bullets of §4 *Commands and Queries* (BE-12, BE-14, BE-15, BE-108, BE-16) become one. BE-12 keeps "A Query MUST NOT mutate" and `SaveChangesAsync`; its repository-port half is BE-108's, since a port is a `Domain` type. BE-108 states "MUST bypass the domain", "untracked", no `Domain` type, no aggregate materialized, no aggregate repository, and gains BE-14's keyless-model check and BE-16's repository-implementation check and Gap. BE-15 is unchanged. BE-14 and BE-16 retire into BE-108.
- Inventory Summary: 101 ⚙ marks; 141 rows - 103 `build`, 32 `required-test`, 1 `dropped`, 5 `retired`. Two rules restate another rule in the file (BE-05, BE-32); BE-13 and BE-14 no longer do.

### Rationale moved from SKILL.md in 2.1.0

Moved word for word, by the section it came from. Each instruction it explained stays in the law.

§4 Commands and Queries (T3):

> a switch statement gets no exhaustiveness check.

> A hierarchy of outcome types cannot be proven exhaustive, so CS8509 fires on every switch over one

§4 Placement of interfaces and implementations (T6):

> `IHubContext<T>` ships only in the ASP.NET Core shared framework, which BE-01 admits in `Api` only

§7 Data Classification (T10):

> Core §8 defines the classes and the boundaries. This section is the mechanism.

§7 Logging (T11):

> which the logging generator treats specially; the redactor applies only to classified parameters, so an unattributed one would be written in clear

§7 Logging (T12):

> the in-box generator ignores them, and writes a classified parameter in clear

§9 Transport (T16):

> An endpoint carries one named policy beside the `GlobalLimiter`, so

§10 Observability (T18):

> Core §9 governs. This is the mechanism.

§10 Correlation (T19):

> under any other propagator that extracts `traceparent`, the SDK's default composite included, the ASP.NET Core instrumentation re-parents the request onto the caller's trace. An ingress that strips the header leaves nothing to link.

§10 Logging (T21):

> because redaction runs in the logger factory `EnableRedaction()` configures

§13 API Layer (T24):

> so a retry finds the same file and no client can name another's

§4 Idempotency (T26):

> means a concurrent attempt executed first

### Load

- Load per .NET stream, as `python -I tools/check-law.py` measures it: ~19.9k tokens before, ~19.4k after. The core law is unchanged at 4557 words; the .NET law goes from 9684 to 9325 words. Annex pointers are shortened, since the introduction already says where the annexes are and when they bind; two orientation lines the Core map already gives are removed; two restatements become pointers to the sentence that stays (§4 *Pipeline behaviors* item 4, §5 *Persistence*).
- Not reached: the ~16k target. After 2.1.0 the .NET law is rules, their conditions and exceptions, the illustrations, the Core map AUTHORING.md requires, and the Testing section; the sentences in it that restate a core rule are marked rules of their own. Reaching ~16k means taking about 2,300 more words out of what every stream loads, which no further move of rationale can do. It needs a decision on structure, such as moving more topics behind annex triggers, or moving the Core map and Role out of the loaded file (AUTHORING.md §2 now requires them at the top). Left for that decision.

### Review

- An independent reviewer, given the old and new law text and inventory but not these records or the ledger, found no obligation, condition, exception, named tool, live mark or Testing entry lost, and no new rule combination without a compliant answer. Fixed from its findings: §13 says the unit-of-work behavior returns the create-only unique-index violation "as a precondition failure", not "as one", which could be read as a conflict; the merged query bullet keeps "MUST bypass the domain and project ... untracked" as a MUST; the inventory Summary carries no release history. Left as at `main`: BE-13's "no other write path" strains against a command that does not persist and creates nothing to load, exactly as BE-13 and BE-41 did before the merge.

## 2.2.0

Law 3.2. The .NET law is rewritten as short instructions, after the owner asked whether its prose could be shorter, straighter and clearer. No rule changes. The ledger and the item-by-item proof are in `RECORDS-3.2-ledger.md` at the repository root.

- Long paragraphs become lists: §2's module projects, §4 *Pipeline behaviors* item 4, the query-side rules, the external-write command, the Infrastructure adapter kinds, the audit records, §13's preconditions, problem types and uploads, and §15, one prohibition per line.
- Each §14 Testing entry with several obligations becomes sub-bullets, each a full sentence naming what MUST have a test and its id; Idempotency says "four tests, one proving each".
- Restored after the check: the old wording wherever the shorter one dropped "only", "exactly", "not through `Api`", "a handler", "there", "against this rule" or a closed list ("Three annexes", "The framework endpoints ... and the OpenAPI document"), or moved a ⚙ mark off the sentence it marks.
- Not changed, though the checklist noticed it: Layout says `<Module>.Application/` holds "Commands, queries, handlers, behaviors", while §2 puts the behaviors in `Platform.Application` and §4 puts concrete query handlers in `Infrastructure`. Correcting it changes what the Layout states, so it is left for a change that may.
- Load per .NET stream: ~19.4k tokens before, ~19.3k after. The ~16k target still needs a decision on structure (`## 2.1.0`, *Load*).

### Rationale moved from SKILL.md in 2.2.0

§2, from "Code the dispatcher's source generator emits into `Api` is written by the generator, not in `Api`, and is outside this rule.":

> is written by the generator, not in `Api`,
