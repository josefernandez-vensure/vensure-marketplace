# Enforcement Inventory - .NET

Every rule marked ⚙ in the .NET stack law, every core rule its role carries, and the mechanism that enforces each.

`../SKILL.md` is the stack law. The core law is `/rancho:law`; its own inventory defines ids and status, and holds the records that concern the core law or more than one stack law. Core §1 defines ⚙ as **"machine-checked. Violating one is a build failure, not a review comment."** This file is the audit of that claim for this stack law: every rule still carrying a mark can be made to fail the build. A ⚙ with no entry here is unaccounted for and MUST be added.

## How to read this

**Prefix:** `BE`. Ids are permanent: `BE-31` stays `BE-31` when sections renumber. The Section column is maintained; the id never changes. Tests reference the id in their name: `BE_31_QueryHandlers_DoNotReferenceAggregates`.

**Mechanism** - what does the checking:

| Code | Mechanism | Fails at |
|---|---|---|
| `proj` | Project or build configuration - `.csproj`, `Directory.Build.props`, banned packages | Compile |
| `roslyn` | Roslyn analyzer: a built-in CA rule, `BannedApiAnalyzers`, or a custom analyzer | Compile |
| `arch` | ArchUnitNET test over types, assemblies, namespaces, attributes | Test run |
| `contract` | Reflection, DI container, EF model, or committed-file inspection - **one** test that discovers every instance of the rule automatically | Test run |
| `ci` | Repository-level script - path checks, generated-artifact freshness, diff checks | CI |
| `test` | A behavioural test that a human must write **per instance** | Test run |

**Status:**

| Status | Meaning |
|---|---|
| `build` | Enforceable as worded. A violation can be made to fail the build. |
| `required-test` | No build check can reach it. The ⚙ is stripped and the obligation is named in a Testing section, which is where it is now caught. See [The badge problem](#the-badge-problem). |
| `dropped` | The ⚙ was removed. The rule still stands - it is enforced by review, not by the build. |
| `reword` | Not decidable as worded. Resolved in [Rewordings applied](#rewordings-applied). None remain. |

**Effort** is for the check, not the rule: `S` under an hour, `M` a day, `L` more than a day.

## Summary

The stack law carries **91 ⚙ marks**. One sits on a section heading, §2, and covers everything beneath it. Its rules are **104 rows**: 92 `build`, 11 `required-test`, and 1 `dropped`. Its role, `server`, carries **14 core rules**, below.

**Every marked rule can be made to fail the build.** No rule claims an enforcement it cannot deliver. Nothing is enforced yet, though - `build` means the check is buildable, not built.

Four rules restate another rule in this file, and eleven core rules restate one. One check satisfies each pair; the canonical id is noted and the duplicate points at it.

## Core rules carried

Every core rule whose role is `server` or `both` and whose status is `build`. The section is the core law's; the rule is worded as this stack law expresses it.

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| ROOT-01 | No module references another module's Domain/Application/Infrastructure | §3 | `proj` + `arch` | build | S | Assembly dependency assertion; also expressible as a `ProjectReference` allowlist |
| ROOT-02 | Domain events MUST NOT cross a module boundary | §3 | `arch` | build | S | Canonical: BE-07 |
| ROOT-04 | One schema per module; no cross-schema joins, no cross-module FKs | §3 | `contract` | build | M | EF model inspection: every entity's schema equals the module's; no FK targets outside it |
| ROOT-07 | Endpoints take a Request DTO, return a Response DTO; no domain type reachable from HTTP | §4 | `arch` | build | M | Unblocked by P-01. Canonical: BE-77 |
| ROOT-08 | Query results projected directly to Response DTOs | §4 | `arch` | build | S | Canonical: BE-14 |
| ROOT-09 | OpenAPI document generated and committed | §4 | `ci` | build | S | Regenerate in CI, fail on any diff |
| ROOT-11 | Every collection endpoint paginated | §4 | `arch` | build | S | Canonical: BE-19 |
| ROOT-12 | One page shape across the API | §4 | `arch` | build | S | Canonical: BE-20 |
| ROOT-13 | Every state-changing endpoint declares an idempotency mechanism | §4 | `contract` | build | S | Canonical: BE-26 |
| ROOT-15 | Acting principal explicit; no ambient HTTP context in application or domain | §7 | `arch` | build | S | Canonical: BE-52 |
| ROOT-16 | Unclassified is `Restricted` | §8 | `arch` | build | L | Canonical: BE-55 |
| ROOT-17 | Integration events MUST NOT carry `Restricted` payloads | §8 | `arch` | build | M | Canonical: BE-61 |
| ROOT-18 | A field's class travels with its contract, into the OpenAPI document and the generated types | §8 | `ci` | build | M | Added by P-03. Canonical: BE-83. The client half is the client stack law's |
| ROOT-19 | Every unit of work carries a correlation id | §9 | `proj` | build | S | Canonical: BE-90 |

## Stack law - `../SKILL.md`

### §2 Project Reference Law

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-01 | Reference table: each project references only what the table allows | `proj` | build | S | Allowlist keyed on the kind each project declares - an undeclared or unrecognised kind fails. Asserted in CI and by `arch` |
| BE-02 | Domain references no EF Core, Mediator, ASP.NET, `IHttpContextAccessor`, application types | `proj` + `arch` | build | S | Banned assembly references on the Domain project |
| BE-03 | No grouping by pattern: no `Application/Handlers/`, `Domain/Entities/`, etc. | `ci` | build | S | Forbidden directory-name check (denylist only - the positive form is not checkable) |

### §3 Domain Layer

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-04 | Aggregate state private; no public setters; collections as `IReadOnlyCollection<T>` | `arch` | build | S | Public property setters on Domain types = fail |
| BE-05 | Aggregate methods MUST NOT return `Result`, a DTO, or any application type | `arch` | build | S | Return types of public Domain methods must be Domain or BCL |
| BE-06 | Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O | `arch` | build | S | Banned namespaces on Domain (`System.IO`, `System.Net`, EF) |
| BE-07 | Domain events are intra-module | `arch` | build | S | Domain event types not referenced outside their module's assemblies |
| BE-08 | No bare `Guid` in the `Domain` or `Application` public surface | `arch` | build | S | R-04 applied |
| BE-09 | Domain events past tense | — | dropped | — | R-05 applied: now SHOULD, unmarked |
| BE-10 | Domain events carry ids and values, never entity references | `arch` | build | S | Property types of event types must not be entity types |
| BE-78 | An aggregate root is marked as one at the type level | `arch` | build | S | Added by P-02. Marker interface present on every root |
| BE-79 | An aggregate holds another aggregate's id, never a reference | `arch` | build | S | Enabled by P-02: no property of a root-marked type is another root-marked type |

### §4 Application Layer

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-11 | Command return type limited to identifier and outcome | `arch` | build | S | `ICommand<T>` closed over `Unit` or an id type |
| BE-12 | A query handler references no repository port and never calls `SaveChangesAsync` | `arch` + `roslyn` | build | S | R-06 applied |
| BE-13 | Command handlers mutate only through aggregate methods; no `DbContext`/SQL | `arch` | build | S | Covered by BE-01: Application cannot reference EF |
| BE-14 | Query handlers project straight to DTOs, untracked; no aggregate materialization | `arch` + `contract` | build | M | Query handler types must not reference aggregates; `QueryTrackingBehavior.NoTracking` asserted on the context |
| BE-15 | Query message and abstraction in Application, concrete handler in Infrastructure | `arch` | build | S | Type-location assertion by interface |
| BE-16 | Query handlers MUST NOT use an aggregate repository | `arch` | build | S | No repository port referenced from query handler types |
| BE-17 | `Repository` reserved for the write path; read-side types named `<Feature>Reader` | `arch` | build | S | Naming plus interface-origin assertion |
| BE-18 | Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, transport types | `arch` | build | S | Same check as BE-52 |
| BE-19 | Every collection query paginated | `arch` | build | S | Query handler return types: `Page<T>` only, never `List<T>`/`IEnumerable<T>` |
| BE-20 | One shared page shape | `arch` | build | S | Exactly one generic page type exists solution-wide |
| BE-21 | Every paginated query declares a maximum page size | `arch` | build | S | Paginated query types must expose `MaxPageSize` |
| BE-22 | A request above the maximum is rejected, never clamped | `contract` | build | M | Reflection over paginated query types; assert the validator rejects `max + 1` |
| BE-23 | Ordering declared as a descriptor whose last term is a unique key | `contract` | build | M | R-07 applied: promoted from test-only. Descriptor inspection over all paginated queries |
| BE-24 | One cursor type solution-wide, payload exactly sort key plus id | `arch` | build | S | R-08 applied: the prohibition follows by construction |
| BE-25 | A message with no declared policy MUST fail to dispatch | `contract` | build | S | Reflection over all message types at test time; runtime guard in the behavior |
| BE-26 | Every command declares an idempotency mechanism | `contract` | build | S | Same reflection sweep as BE-25 |
| BE-27 | Idempotency record written through the same unit of work; Redis MUST NOT be the store | `arch` | build | S | Idempotency store type depends on `DbContext`, never on `IConnectionMultiplexer` |
| BE-28 | Unique index on the attempt identity | `contract` | build | S | EF model inspection: index exists and `IsUnique` |
| BE-29 | Concurrent duplicates resolve as one execution and one replay | `test` | required-test | M | §14 **Idempotency**. Mark stripped; split out of BE-28's bullet, which keeps its own |
| BE-30 | Idempotency is a pipeline behavior, never per-endpoint | `arch` | build | S | Only the behavior may reference the idempotency store |
| BE-31 | Agent tool invocations carry a key derived from run and step | `arch` | build | S | By construction: the tool invoker is the only caller, and takes the key as a required argument |
| BE-32 | A transaction MUST NOT span a module boundary | `arch` | build | S | Covered by BE-01 |
| BE-33 | Integration events written to the outbox in the same transaction; no direct publish | `arch` | build | S | Application handlers may reference the outbox port only, never a bus client |
| BE-34 | Every public Infrastructure type is an adapter | `arch` | build | M | Implements a Domain/Application port or a known framework extension point |
| BE-35 | Infrastructure types live under `Persistence/`, `Queries/`, `Messaging/`, `External/` | `arch` | build | S | Namespace assertion |
| BE-36 | `Infrastructure/Queries/<Feature>/` mirrors the Application feature folder | `ci` | build | S | Directory comparison |
| BE-84 | Every integration event type carries its version in its name | `arch` | build | S | Naming assertion over the `Contracts` project |
| BE-85 | The published contract surface is snapshotted; a shape change to an existing version fails the build | `ci` | build | M | Snapshot and diff, the same mechanism as ROOT-09 |
| BE-86 | No domain type in an integration event payload | `arch` | build | S | Property types of contract types resolve to ids, primitives, value objects |

### §5 Persistence

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-37 | One `DbContext` per module; MUST NOT map another module's tables | `contract` | build | S | EF model: every entity type originates in the owning module's assembly |
| BE-38 | Mapping in `IEntityTypeConfiguration<T>`; no mapping attributes on domain types | `arch` | build | S | No EF attributes present on any Domain type |
| BE-39 | Integration tests run against SQL Server; no other provider stands in for it | `proj` | build | S | A package allowlist, not a denylist: SQL Server is the only EF provider a test project may reference, so SQLite and InMemory both fail without being named. Amended by A-04 |
| BE-40 | Repositories MUST NOT expose `IQueryable`, `DbSet`, or EF types | `arch` | build | S | Signature assertion on repository ports |
| BE-41 | Command handlers write only through the repository | `arch` | build | S | No longer covered by BE-01: `Application` now references `Platform.Application`, where the unit-of-work port is declared. Assert directly - no type in a module `Application` references it |
| BE-42 | No navigation properties between aggregate roots | `contract` | build | M | Unblocked by P-02. EF model navigation targets, resolved through the root marker |
| BE-43 | Domain types acquire no public parameterless ctors, public setters, or EF attributes | `arch` | build | S | Extends BE-04 and BE-38 |
| BE-44 | Cache decorators registered explicitly per query, never auto-applied | `contract` | build | M | Container inspection: no open-generic decorator registration over `IQueryHandler<,>` |
| BE-45 | **Every** cache key includes the acting principal | `arch` | build | S | R-09 applied: unconditional, built by the one key builder |
| BE-46 | Every cache key embeds its aggregates' version stamps, declared explicitly | `arch` + `contract` | build | M | Cached queries implement a dependency-declaring interface; key builder consumes it |
| BE-47 | `KEYS` and `SCAN`-and-delete invalidation forbidden | `roslyn` | build | S | `BannedApiAnalyzers` on the Redis APIs |
| BE-48 | Command handlers MUST NOT touch the cache | `arch` | build | S | No cache type referenced from command handler types |
| BE-49 | A Redis failure MUST be treated as a miss | `test` | required-test | S | §14 **Cache**. Mark stripped |
| BE-87 | A committed migration MUST NOT be edited | `ci` | build | S | Diff every migration file against its committed history |
| BE-88 | A migration references no application code | `arch` | build | S | Migration types depend on EF only |
| BE-89 | Migrations run clean twice; a destructive one keeps the previous version serving | `test` | required-test | M | §14 **Migrations** |

### §6 Authorization

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-50 | Every Command and Query declares a named policy; no bare `[Authorize]` | `contract` + `roslyn` | build | S | Reflection sweep (as BE-25); `BannedApiAnalyzers` on parameterless `[Authorize]` |
| BE-51 | `FallbackPolicy` MUST deny | `contract` | build | S | Startup configuration assertion |
| BE-52 | `IHttpContextAccessor` MUST NOT be referenced outside the composition root | `arch` | build | S | Assembly-scoped type reference assertion |
| BE-53 | Application and domain code MUST NOT read raw claims | `arch` | build | S | Every type raw claims can be read through, such as `ClaimsPrincipal`, `ClaimsIdentity`, `Claim`, `ClaimTypes` or `JwtRegisteredClaimNames`, banned outside `Api`. Ban the `System.Security.Claims` namespace rather than the members, as BE-98 bans types rather than predicates |
| BE-99 | An external authority's unavailability - one that validates the token or answers for permissions - MUST deny; an expired cache entry is not served during an outage | `test` | required-test | M | Added by A-03, widened by A-05. §14 **Authorization** |
| BE-100 | An external authority's cached decision - a validation result or a permission decision - declares a bounded lifetime in an options class | `contract` | build | S | Added by A-03, widened by A-05. Options sweep, as BE-96: the lifetime exists on a validated options class and is bounded; a missing or unbounded value fails |
| BE-101 | An operation's authorization decision is reachable from the authorization behavior, not only a transport filter | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-102 | An external authority's types and codes MUST NOT be referenced outside the composition root | `arch` | build | S | Added by A-03. Same shape as BE-53: the authority client's permission types banned outside `Api` |
| BE-103 | Externally-resolved permissions are intersected with the agent manifest for an agent-actor principal | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-104 | A claim read before the token is validated only selects how it is validated, and must agree with the validated token | `test` | required-test | M | Added by A-05. §14 **Authorization**: a token whose selecting claims disagree with its validated issuer or endpoint is refused, and no claim read before validation reaches the principal |

### §7 Data Classification

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-54 | A class is declared once, on the value type; attribute lives in `SharedKernel` | `arch` | build | S | Attribute defined in `SharedKernel`; no redundant redeclaration on DTO properties |
| BE-55 | Every property reachable from an egress boundary resolves to a class | `arch` | build | L | Transitive property-graph walk from each boundary root. **The keystone check** |
| BE-56 | `Confidential`/`Restricted` types redact in `ToString()`, no implicit string conversion | `arch` | build | S | Overrides `ToString`; declares no `op_Implicit` to `string` |
| BE-57 | No interpolation of a classified value into a log message template | `roslyn` | build | S | Enable `CA2254` as error, plus a custom rule for classified arguments |
| BE-58 | Every model-visible DTO is explicitly marked | `arch` | build | S | R-10 applied: the "distinct set" half is now unmarked prose |
| BE-59 | No model-visible DTO contains `Confidential`/`Restricted`, transitively | `arch` | build | M | Reuses BE-55's graph walk |
| BE-60 | `Restricted` MUST NOT be written to Redis in any form | `arch` | build | M | Reuses BE-55's graph walk over cache value types |
| BE-61 | Integration events and outbox rows MUST NOT carry `Restricted` | `arch` | build | M | Reuses BE-55's graph walk over contract types |
| BE-83 | The class is emitted into the OpenAPI document on every Response DTO property | `ci` | build | M | Added by P-03. Emitter fails on a property whose class it cannot resolve |

### §8 AI Agents

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-62 | Tools projected from the manifest; hand-registration forbidden | `contract` | build | M | Container inspection: every tool descriptor originates from the projector |
| BE-63 | Run scopes derive from the same manifest | `arch` | build | S | No scope literal outside the manifest projection |
| BE-64 | Every manifest entry resolves to a real handler with a matching policy | `contract` | build | S | Reflection test over manifest entries |
| BE-65 | The principal carries an actor claim identifying the agent | `test` | required-test | S | §14 **Agents**. Mark stripped |
| BE-66 | An agent-actor principal MUST NOT start another agent run | `test` | required-test | S | §14 **Agents**. Mark stripped; the guard is runtime, the test proves it |
| BE-67 | Autonomous runs need a separate manifest, no `HumanRequirement.Required` entries | `contract` | build | S | Reflection over manifests |
| BE-68 | No bare `string` in a tool parameter; declared wrapper types only | `arch` | build | S | R-11 applied |
| BE-69 | Tool output MUST be a Response DTO; no domain objects or raw rows | `arch` | build | S | Reuses BE-55/BE-59 machinery |

### §9 Quotas and Rate Limits

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-70 | The global rate limiter's default policy covers every endpoint | `contract` | build | S | Endpoint data source inspection: rate-limit metadata present on every endpoint |
| BE-71 | Every agent run carries a budget fixed at start | `proj` | build | S | By construction: budget is a required constructor parameter of the run |
| BE-72 | Exceeding a budget aborts; MUST NOT silently truncate | `test` | required-test | M | §14 **Budgets**. Mark stripped |
| BE-73 | Limits are configuration and fail closed | `contract` | build | S | Options `ValidateOnStart`; test proving a missing value denies |

### §10 Observability

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-90 | The correlation id is a required constructor parameter of the dispatch context | `proj` | build | S | By construction: the context cannot be built without one |
| BE-91 | Outbox rows persist the correlation id of the writing transaction | `contract` | build | S | EF model: the column exists on the outbox entity |
| BE-92 | The dispatcher restores the correlation id on delivery | `test` | required-test | M | §14 **Correlation** |
| BE-93 | A handler MUST NOT reference `ILogger` | `arch` | build | S | Dispatch logging belongs to the pipeline behavior alone |
| BE-94 | Metric dimensions come from a declared closed set of keys | `contract` | build | S | Committed-file inspection of every metric call site: each dimension key is a member of the declared key set, never a literal or a computed string; bounds cardinality by construction. Not `arch`: a declared key is a `const string`, which the compiler inlines, so in IL a declared key and an arbitrary literal are the same `ldstr` and a type-level assertion cannot tell them apart |

### §11 Configuration and Secrets

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-95 | Configuration MUST NOT be read from its source outside the composition root | `arch` + `roslyn` | build | S | `arch`: type reference assertion on `IConfiguration`, as BE-52 does for `IHttpContextAccessor`. `roslyn`: `BannedApiAnalyzers` on `System.Environment.GetEnvironmentVariable` and `GetEnvironmentVariables`, scoped to allow `Api`. Amended by A-04 |
| BE-96 | Every options class has a validator and is registered with `ValidateOnStart` | `contract` | build | S | Reflection sweep over registered options types |
| BE-97 | No secret in any committed file; a committed credential slot holds a reference, never a value | `contract` + `ci` | build | S | `contract`: a sweep of the committed configuration files - `appsettings*.json`, environment files, `nuget.config` - failing on a denylisted key name with a literal value, and on any `<packageSourceCredentials>` value that is not a `%VAR%` macro. It runs in the test suite, so it fails before a push. `ci`: entropy and provider-pattern secret scanning over the whole history, which no unit test can reach. Amended by A-04 |
| BE-98 | Outside the composition root, code MUST NOT learn which environment it is running in | `roslyn` | build | S | `BannedApiAnalyzers` on the *types* that answer the question - `IHostEnvironment`, `IWebHostEnvironment`, both obsolete `IHostingEnvironment`s, and the `Environments` constants - scoped to allow `Api`. Banning the types rather than the predicates reaches `IsStaging()`, `IsEnvironment(string)` and `EnvironmentName == "..."` without naming them; an environment variable read is BE-95's. Amended by A-04 |

### §12-§13 Real-time and API

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-74 | A hub references only DTOs and the dispatcher | `arch` | build | S | R-12 applied |
| BE-75 | Hub authorization uses the same named policies as the HTTP operation | `contract` | build | S | By construction if hubs dispatch messages: the policy resolves from the message |
| BE-76 | SignalR messages carry DTOs; no domain types serialized | `arch` | build | S | Hub method signatures |
| BE-77 | Endpoints MUST NOT reference domain types | `arch` | build | S | Unblocked by P-01 |
| BE-80 | Every endpoint is a type implementing the shared endpoint abstraction | `arch` | build | S | Added by P-01. No call outside the discovery registrar to any `EndpointRouteBuilderExtensions` overload taking a handler delegate - every verb, not a list of them |
| BE-81 | Endpoints registered by discovery; hand-registration forbidden | `contract` | build | S | Added by P-01. Container inspection, as BE-62 does for tools |
| BE-82 | An endpoint references only its DTOs, the dispatcher, and the result translator | `arch` | build | S | Enabled by P-01. The endpoint counterpart of BE-74 |

## Rewordings applied

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

## Prerequisites (all adopted)

Three of the four prerequisites concern this stack law alone, and are kept here. P-03 spans both roles and is in the core inventory.

**P-01 · Endpoints are types, not raw lambdas.** *Adopted.* ArchUnitNET sees types; it cannot see the signature of a lambda handed to `MapPost`. `backend.md` §13 now requires every endpoint to be a type implementing the shared endpoint abstraction, registered by discovery (BE-80, BE-81). This unblocked ROOT-07 and BE-77, and made checkable a rule that had never carried a mark at all: BE-82, an endpoint referencing only its DTOs, the dispatcher, and the translator - the endpoint counterpart of what R-12 did for hubs.

**P-02 · Aggregate roots carry a marker.** *Adopted.* Nothing distinguished a root from an entity at the type level. `backend.md` §3 now requires the marker (BE-78). This unblocked BE-42 and made BE-79 checkable - *an aggregate holds another aggregate's id, never a reference*, which has been law since the first draft with no way to test it.

**P-04 · One cursor type.** *Adopted* as part of R-08.

## Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** `proj` and `roslyn` checks: BE-02, BE-39, BE-47, BE-57 (`CA2254` is a compiler switch), BE-50's banned `[Authorize]`, BE-98's banned environment types, BE-95's banned environment-variable reads, BE-97's secret scan, plus the §15 Forbidden list as `BannedApiAnalyzers` entries. **Enforced in about a day**, none requiring a decision.

**Second - the reflection sweeps.** One test each, discovering every instance automatically: BE-25 and BE-26 (declared policy, declared idempotency), BE-64, BE-67, BE-70, BE-73, BE-51, BE-96 (every options class validated on start), BE-100 (the external-authority cache lifetime declared and bounded). These are the highest-leverage tests in the suite, because they cover every message that will ever be added, not just today's.

**Third - the classification graph.** BE-55 is the keystone: BE-59, BE-60, BE-61, BE-69, and ROOT-16 all reuse its property-graph walk. Write it once. It is the largest single piece of work here and the one that carries the most risk if it is skipped. The same pass emits the class into the OpenAPI document (BE-83), which is what lets a client stack law carry it into its generated types and enforce its data-boundary rules at all.

**Fourth - the structural arch tests.** BE-01, BE-04 through BE-07, BE-10, BE-13 through BE-20, BE-30, BE-33 through BE-38, BE-40, BE-43, BE-46, BE-48, BE-52, BE-53, BE-102, BE-63, BE-76, and BE-78 through BE-82. Individually cheap, collectively the bulk of the row count.

**Every decision is settled.** The fifteen rewordings, all four prerequisites, and the badge question are in the law. Every check above can be built against the current text, and nothing in this file is now waiting on an answer - only on the work. Building checks against the current text means building the wrong checks for some of them, and R-09 in particular should change before anything caches anything.
