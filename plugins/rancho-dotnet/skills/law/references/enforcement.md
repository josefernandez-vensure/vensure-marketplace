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

The stack law carries **95 ⚙ marks**. One sits on a section heading, §2, and covers everything beneath it. Its rules are **113 rows**: 99 `build`, 13 `required-test`, and 1 `dropped`. Its role, `server`, carries **14 core rules**, below.

**Every marked rule can be made to fail the build.** No rule claims an enforcement it cannot deliver. Nothing is enforced yet, though - `build` means the check is buildable, not built.

Five rules restate another rule in this file, and eleven core rules restate one. One check satisfies each pair; the canonical id is noted and the duplicate points at it.

## Core rules carried

Every core rule whose role is `server` or `both` and whose status is `build`. The section is the core law's; the rule is worded as this stack law expresses it.

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| ROOT-01 | No module references another module's Domain/Application/Infrastructure | §3 | `proj` + `arch` | build | S | Assembly dependency assertion; also expressible as a `ProjectReference` allowlist |
| ROOT-02 | Domain events MUST NOT cross a module boundary | §3 | `arch` | build | S | Canonical: BE-07 |
| ROOT-04 | One schema per module; no cross-schema joins, no cross-module FKs | §3 | `contract` | build | M | EF model inspection: every entity's schema equals the module's; no FK targets outside it. A query handler's SQL (§4) is not in the EF model, so a cross-schema reference there is a known gap, carried by the real-database test (BE-112) and by per-module database permissions where the project grants them |
| ROOT-07 | Endpoints take a Request DTO, return a Response DTO; no domain type reachable from HTTP | §4 | `arch` | build | M | Unblocked by P-01. Canonical: BE-77 |
| ROOT-08 | Query results projected directly to Response DTOs | §4 | `arch` | build | S | Canonical: BE-108 |
| ROOT-09 | OpenAPI document generated and committed | §4 | `ci` | build | S | Regenerate in CI, fail on any diff |
| ROOT-11 | Every collection endpoint paginated | §4 | `arch` | build | S | Canonical: BE-19 |
| ROOT-12 | One page shape across the API | §4 | `arch` | build | S | Canonical: BE-20 |
| ROOT-13 | Every state-changing endpoint declares an idempotency mechanism | §4 | `contract` | build | S | Canonical: BE-26 |
| ROOT-15 | Acting principal explicit; no ambient HTTP context in application or domain | §7 | `arch` | build | S | Canonical: BE-52 |
| ROOT-16 | Unclassified is `Restricted` | §8 | `arch` | build | L | Canonical: BE-55 |
| ROOT-17 | Integration events MUST NOT carry `Restricted` payloads | §8 | `arch` | build | M | Canonical: BE-61 |
| ROOT-18 | A field's class travels with its contract: the OpenAPI document carries the class of every request and response property and every parameter | §8 | `ci` | build | M | Added by P-03, widened by A-08. Canonical: BE-83. The client half is the client stack law's |
| ROOT-19 | Every unit of work carries a correlation id | §9 | `proj` | build | S | Canonical: BE-90 |

## Stack law - `../SKILL.md`

### §2 Project Reference Law

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-01 | Reference table: each project references, and declares packages, only as the table allows | `proj` | build | S | Allowlist keyed on the kind each project declares - an undeclared or unrecognised kind fails. Each direct `ProjectReference` checked against the row's project column, and each `PackageReference` against its package column; `Api` may declare no package an Infrastructure project declares, and admits EF Core's design-time tooling only with `PrivateAssets="all"`. Asserted in CI and by `arch`. Amended by A-09: `Api` admits `<Module>.Application`, and the package column is now checked - it had no check before |
| BE-02 | Domain references no EF Core, Mediator, ASP.NET, `IHttpContextAccessor`, application types | `proj` + `arch` | build | S | Banned assembly references on the Domain project |
| BE-03 | No grouping by pattern: no `Application/Handlers/`, `Domain/Entities/`, etc. | `ci` | build | S | Forbidden directory-name check (denylist only - the positive form is not checkable) |
| BE-105 | `Api` names only Contracts, Application, Platform and `SharedKernel` types and Infrastructure registration entry points, and after startup names or invokes no Infrastructure type directly | `arch` | build | M | Added by A-09. Type references of every `Api` type resolve to those sets, or to the BCL and the packages `Api`'s row admits. Types the dispatcher's source generator emits - marked with `GeneratedCodeAttribute` naming that generator, not any generator - are skipped - the rule puts them outside it |
| BE-106 | Each module's Infrastructure exposes exactly one registration entry point, `Add<Module>Module(IServiceCollection, IConfiguration)` | `arch` | build | S | Added by A-09. Exactly one public static extension method over `IServiceCollection` per module Infrastructure assembly, with that signature |
| BE-107 | `InternalsVisibleTo` names test assemblies only | `contract` | build | S | Added by A-09. Every `InternalsVisibleTo` attribute on every production assembly names a project whose declared kind is `<Anything>.Tests` |

### §3 Domain Layer

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-04 | Aggregate state private; no public setters; collections as `IReadOnlyCollection<T>` | `arch` | build | S | Public property setters on Domain types = fail |
| BE-05 | Aggregate methods MUST NOT return `Result`, a DTO, or any application type | `arch` | build | S | Return types of public Domain methods must be Domain or BCL |
| BE-06 | Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O | `arch` | build | S | Banned namespaces on Domain (`System.IO`, `System.Net`, EF) |
| BE-07 | Domain events are intra-module | `arch` | build | S | Domain event types not referenced outside their module's assemblies |
| BE-08 | No bare `Guid` in the public surface of `Domain` or of a command or query message | `arch` | build | S | R-04 applied. Narrowed by A-09 from all of `Application` to its messages: the rest of `Application` is reached through them |
| BE-109 | A command or query message, and a Response DTO, reference no `Domain` type; ids they carry are `Contracts` ids | `arch` | build | S | Added by A-09. Property and constructor-parameter types of every message and Response DTO, transitively, resolve outside `Domain` |
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
| BE-14 | Query handlers project straight to DTOs, untracked; no aggregate materialization | `arch` | build | S | Covered by BE-108: a handler that references no `Domain` type cannot materialize an aggregate. Read types are keyless, which EF never tracks, and SQL into a DTO is never tracked, so the context-wide `NoTracking` assertion is withdrawn by A-09 - the write side shares the context, and the assertion was stricter than the rule. Asserted instead: every read type mapped in a module's model is keyless |
| BE-15 | Query message and abstraction in Application; every concrete handler in Infrastructure, by convention | `arch` | build | S | Type-location assertion by interface: every `IQueryHandler<,>` implementation is in a module Infrastructure assembly, with no exception for a pure handler. Amended by A-09 |
| BE-108 | A query handler references no `Domain` type | `arch` | build | S | Added by A-09. Type references of every `IQueryHandler<,>` implementation resolve outside `Domain`. Subsumes BE-16, and the repository-port half of BE-12 |
| BE-112 | Every query handler that reads with SQL is tested against real SQL Server, including its query filters | `test` | required-test | M | Added by A-09. §14 **Queries**. SQL into a DTO is not checked against the model at compile time, and bypasses global query filters |
| BE-113 | No raw-string SQL API in a module; SQL is parameterized | `roslyn` | build | S | Added by A-09. `BannedApiAnalyzers` in every module project on EF Core's raw-string SQL methods - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw` and their async forms. A migration's `migrationBuilder.Sql` is outside the rule, which says so. Known gap: SQL set directly on an ADO.NET `DbCommand.CommandText` is not reached; review carries it |
| BE-16 | Query handlers MUST NOT use an aggregate repository | `arch` | build | S | Covered by BE-108: a repository port is a `Domain` type |
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
| BE-31 | An agent invocation of a keyed command carries a key derived from run and step; a client-supplied id is the same on every attempt | `arch` | build | S | By construction: the tool invoker is the only caller, and takes the key as a required argument for a command declaring one. Amended by A-09: a command idempotent naturally or by client-supplied id carries no key |
| BE-32 | A transaction MUST NOT span a module boundary | `arch` | build | S | Covered by BE-01 |
| BE-33 | Integration events written to the outbox in the same transaction; no direct publish | `arch` | build | S | Application handlers may reference the outbox port only, never a bus client |
| BE-110 | A command declares whether it persists aggregate state; one that does not uses no key-based idempotency and no outbox | `contract` + `arch` | build | S | Added by A-09. `contract`: the operation registry sweep (as BE-25) finds the declaration on every command, and refuses it on a query, together with an idempotency key, and with any mechanism but natural or client-supplied id. `arch`: no handler of a command declared not to persist references the outbox writer |
| BE-111 | A command that does not persist stages no database change | `test` | required-test | S | Added by A-09. §14 **Transactions**. The transaction behavior's guard is runtime; the test proves it |
| BE-34 | Every Infrastructure type is an adapter or a registration entry point, and is `internal` unless the dispatcher's generated code must name it | `arch` | build | M | Implements a Domain/Application port or a framework extension point, or is a registration entry point. Amended by A-09: a public Infrastructure type that is neither a registration entry point nor named by a generated type in `Api` fails. Mediator 3.x names each concrete handler in its generated registration, so concrete query handlers are public, as are the types their public constructors take, such as the module's `DbContext` and read types. `InternalsVisibleTo` is BE-107's |
| BE-35 | Infrastructure types live under `Persistence/`, `Queries/`, `Messaging/`, `External/` | `arch` | build | S | Namespace assertion |
| BE-36 | `Infrastructure/Queries/<Feature>/` mirrors the Application feature folder | `ci` | build | S | Directory comparison |
| BE-84 | Every integration event type carries its version in its name | `arch` | build | S | Naming assertion over the `Contracts` project |
| BE-85 | The published contract surface is snapshotted; a shape change to an existing version fails the build | `ci` | build | M | Snapshot and diff, the same mechanism as ROOT-09 |
| BE-86 | No domain type in an integration event payload | `arch` | build | S | Property types of contract types resolve to ids, primitives, value objects |

### §5 Persistence

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-37 | One `DbContext` per module; MUST NOT map another module's tables | `contract` | build | S | EF model: every entity type, read types included, originates in the owning module's assembly. SQL is outside the model: see ROOT-04 |
| BE-38 | Mapping in `IEntityTypeConfiguration<T>`; no mapping attributes on domain types | `arch` | build | S | No EF attributes present on any Domain type |
| BE-39 | Integration tests run against SQL Server; no other provider stands in for it | `proj` | build | S | A package allowlist, not a denylist: SQL Server is the only EF provider a test project may reference, so SQLite and InMemory both fail without being named. Amended by A-04 |
| BE-40 | Repositories MUST NOT expose `IQueryable`, `DbSet`, or EF types | `arch` | build | S | Signature assertion on repository ports |
| BE-41 | Command handlers write only through the repository | `arch` | build | S | No longer covered by BE-01: `Application` now references `Platform.Application`, where the unit-of-work port is declared. Assert directly - no type in a module `Application` references it |
| BE-42 | No navigation properties between aggregate roots | `contract` | build | M | Unblocked by P-02. EF model navigation targets, resolved through the root marker |
| BE-43 | Domain types acquire no public parameterless ctors, public setters, or EF attributes | `arch` | build | S | Extends BE-04 and BE-38. Skips compiler-generated types, marked with `CompilerGeneratedAttribute`, such as a lambda's closure class. Amended by A-09 |
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
| BE-52 | `IHttpContextAccessor` MUST NOT be referenced outside the composition root | `arch` | build | S | Assembly-scoped type reference assertion. The composition root is `Api` and the registration entry points it calls (§2, A-09) |
| BE-53 | Application and domain code MUST NOT read raw claims | `arch` | build | S | Every type raw claims can be read through, such as `ClaimsPrincipal`, `ClaimsIdentity`, `Claim`, `ClaimTypes` or `JwtRegisteredClaimNames`, banned in every module `Domain` and `Application` assembly - the rule's own scope. `Platform.Application`'s authorization behavior passes the principal to `IAuthorizationService` without reading a claim, and the authority adapter (BE-102) reads the subject claim to look permissions up; both are outside the ban. Rescoped by A-09: banning the namespace everywhere outside `Api` left the authorization behavior unable to compile, a check stricter than its rule. Ban the `System.Security.Claims` namespace rather than the members, as BE-98 bans types rather than predicates |
| BE-99 | An external authority's unavailability - one that validates the token or answers for permissions - MUST deny; an expired cache entry is not served during an outage | `test` | required-test | M | Added by A-03, widened by A-05. §14 **Authorization** |
| BE-100 | An external authority's cached decision - a validation result or a permission decision - declares a bounded lifetime in an options class | `contract` | build | S | Added by A-03, widened by A-05. Options sweep, as BE-96: the lifetime exists on a validated options class and is bounded; a missing or unbounded value fails |
| BE-101 | An operation's authorization decision is reachable from the authorization behavior, not only a transport filter | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-102 | An external authority's types and codes MUST NOT be referenced outside the one adapter project that wraps it | `arch` | build | S | Added by A-03. Amended by A-09: the authority's SDK is declared by the Infrastructure project holding its adapter, and its types are banned in every other assembly. `Api` may declare no package an Infrastructure project declares, so the old scope - `Api` - had no compliant answer |
| BE-103 | Externally-resolved permissions are intersected with the agent manifest for an agent-actor principal | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-104 | A claim read before the token is validated only selects how it is validated, and must agree with the validated token | `test` | required-test | M | Added by A-05. §14 **Authorization**: a token whose selecting claims disagree with its validated issuer or endpoint is refused, and no claim read before validation reaches the principal |

### §7 Data Classification

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-54 | A class is declared on the domain value type and on its published `Contracts` twin, and the two agree; attribute lives in `SharedKernel` | `arch` | build | M | Attribute defined in `SharedKernel`. Each `Contracts` type's class equals its `Domain` twin's; a property whose type carries no class declares one, and a projected property's class equals that of the source it names; no other redeclaration on a DTO, message or event property. Amended by A-09 |
| BE-55 | Every property reachable from an egress boundary resolves to a class | `arch` | build | L | Transitive property-graph walk from each boundary root. **The keystone check** |
| BE-56 | `Confidential`/`Restricted` types redact in `ToString()`, no implicit string conversion | `arch` | build | S | Overrides `ToString`; declares no `op_Implicit` to `string` |
| BE-57 | No interpolation of a classified value into a log message template | `roslyn` | build | S | Enable `CA2254` as error, plus a custom rule for classified arguments |
| BE-58 | Every model-visible DTO is explicitly marked | `arch` | build | S | R-10 applied: the "distinct set" half is now unmarked prose |
| BE-59 | No model-visible DTO contains `Confidential`/`Restricted`, transitively | `arch` | build | M | Reuses BE-55's graph walk |
| BE-60 | `Restricted` MUST NOT be written to Redis in any form | `arch` | build | M | Reuses BE-55's graph walk over cache value types |
| BE-61 | Integration events and outbox rows MUST NOT carry `Restricted` | `arch` | build | M | Reuses BE-55's graph walk over contract types |
| BE-83 | The class is emitted into the OpenAPI document on every Request and Response DTO property and every parameter | `ci` | build | M | Added by P-03, widened by A-08. Emitter fails on a property or parameter whose class it cannot resolve |

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
| BE-95 | Configuration MUST NOT be read from its source outside the composition root | `arch` + `roslyn` | build | S | `arch`: type reference assertion on `IConfiguration`, as BE-52 does for `IHttpContextAccessor`. `roslyn`: `BannedApiAnalyzers` on `System.Environment.GetEnvironmentVariable` and `GetEnvironmentVariables`, scoped to allow `Api` and the registration entry points it calls. Amended by A-04. Rescoped by A-09: `Add<Module>Module` takes `IConfiguration`, so the composition root is `Api` plus the registration entry points (§2) |
| BE-96 | Every options class has a validator and is registered with `ValidateOnStart` | `contract` | build | S | Reflection sweep over registered options types |
| BE-97 | No secret in any committed file; a committed credential slot holds a reference, never a value | `contract` + `ci` | build | S | `contract`: a sweep of the committed configuration files - `appsettings*.json`, environment files, `nuget.config` - failing on a denylisted key name with a literal value, and on any `<packageSourceCredentials>` value that is not a `%VAR%` macro. It runs in the test suite, so it fails before a push. `ci`: entropy and provider-pattern secret scanning over the whole history, which no unit test can reach. Amended by A-04 |
| BE-98 | Outside the composition root, code MUST NOT learn which environment it is running in | `roslyn` | build | S | `BannedApiAnalyzers` on the *types* that answer the question - `IHostEnvironment`, `IWebHostEnvironment`, both obsolete `IHostingEnvironment`s, and the `Environments` constants - scoped to allow `Api` and the registration entry points it calls. Banning the types rather than the predicates reaches `IsStaging()`, `IsEnvironment(string)` and `EnvironmentName == "..."` without naming them; an environment variable read is BE-95's. Amended by A-04 |

### §12-§13 Real-time and API

| Id | Rule | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|
| BE-74 | A hub references only DTOs and the dispatcher | `arch` | build | S | R-12 applied |
| BE-75 | Hub authorization uses the same named policies as the HTTP operation | `contract` | build | S | By construction if hubs dispatch messages: the policy resolves from the message |
| BE-76 | SignalR messages carry DTOs; no domain types serialized | `arch` | build | S | Hub method signatures |
| BE-77 | Endpoints MUST NOT reference domain types | `arch` | build | S | Unblocked by P-01 |
| BE-80 | Every endpoint is a type implementing the shared endpoint abstraction | `arch` | build | S | Added by P-01. No call outside the discovery registrar to any `EndpointRouteBuilderExtensions` overload taking a handler delegate - every verb, not a list of them |
| BE-81 | Endpoints registered by discovery; hand-registration forbidden | `contract` | build | S | Added by P-01. Container inspection, as BE-62 does for tools |
| BE-82 | An endpoint references only its DTOs and the types they are composed of, the message it dispatches, the dispatcher, and the result translator | `arch` | build | S | Enabled by P-01. The endpoint counterpart of BE-74. Amended by A-09: the message and the DTOs' own types were missing, so every endpoint failed it. Checks the whole endpoint type, including any method on a DTO it calls to build the message |

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

## Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under core §11 - the law first, the codebase after. A-09 concerns this stack law alone and is kept here. A-08 spans both roles and is in the core inventory, with A-01 to A-05 and A-07; A-06 and A-10 are in `rancho-react`'s.

**A-09 · §2, §3, §4, §5, §6, §7, §8, §9, §13, §15, BE-01, BE-08, BE-14, BE-15, BE-31, BE-34, BE-43, BE-54, BE-82, BE-95, BE-98, BE-102 · Rule combinations that left no compliant answer.**
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

Backend rows go from 104 to 113, and marks from 90 to 95. The summary had counted 91 before this change: it counted the ⚙ that §14's opening sentence mentions in prose.

## Prerequisites (all adopted)

Three of the four prerequisites concern this stack law alone, and are kept here. P-03 spans both roles and is in the core inventory.

**P-01 · Endpoints are types, not raw lambdas.** *Adopted.* ArchUnitNET sees types; it cannot see the signature of a lambda handed to `MapPost`. `backend.md` §13 now requires every endpoint to be a type implementing the shared endpoint abstraction, registered by discovery (BE-80, BE-81). This unblocked ROOT-07 and BE-77, and made checkable a rule that had never carried a mark at all: BE-82, an endpoint referencing only its DTOs, the dispatcher, and the translator - the endpoint counterpart of what R-12 did for hubs.

**P-02 · Aggregate roots carry a marker.** *Adopted.* Nothing distinguished a root from an entity at the type level. `backend.md` §3 now requires the marker (BE-78). This unblocked BE-42 and made BE-79 checkable - *an aggregate holds another aggregate's id, never a reference*, which has been law since the first draft with no way to test it.

**P-04 · One cursor type.** *Adopted* as part of R-08.

## Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** `proj` and `roslyn` checks: BE-02, BE-39, BE-47, BE-57 (`CA2254` is a compiler switch), BE-50's banned `[Authorize]`, BE-98's banned environment types, BE-95's banned environment-variable reads, BE-97's secret scan, plus the §15 Forbidden list as `BannedApiAnalyzers` entries. **Enforced in about a day**, none requiring a decision.

**Second - the reflection sweeps.** One test each, discovering every instance automatically: BE-25 and BE-26 (declared policy, declared idempotency), BE-64, BE-67, BE-70, BE-73, BE-51, BE-96 (every options class validated on start), BE-100 (the external-authority cache lifetime declared and bounded), BE-107, BE-110, BE-113. These are the highest-leverage tests in the suite, because they cover every message that will ever be added, not just today's.

**Third - the classification graph.** BE-55 is the keystone: BE-59, BE-60, BE-61, BE-69, and ROOT-16 all reuse its property-graph walk. Write it once. It is the largest single piece of work here and the one that carries the most risk if it is skipped. The same pass emits the class into the OpenAPI document (BE-83), which is what lets a client stack law carry it into its generated types and enforce its data-boundary rules at all.

**Fourth - the structural arch tests.** BE-01, BE-04 through BE-07, BE-10, BE-13 through BE-20, BE-30, BE-33 through BE-38, BE-40, BE-43, BE-46, BE-48, BE-52, BE-53, BE-102, BE-63, BE-76, BE-78 through BE-82, BE-105, BE-106, BE-108 and BE-109. Individually cheap, collectively the bulk of the row count.

**Every decision is settled.** The fifteen rewordings, all four prerequisites, and the badge question are in the law. Every check above can be built against the current text, and nothing in this file is now waiting on an answer - only on the work. Building checks against the current text means building the wrong checks for some of them, and R-09 in particular should change before anything caches anything.
