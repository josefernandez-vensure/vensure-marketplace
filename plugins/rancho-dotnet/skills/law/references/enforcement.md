# Enforcement Inventory - .NET

Every rule marked ⚙ in the .NET stack law, every core rule its role carries, and the mechanism that enforces each.

`../SKILL.md` is the stack law, and `annex-agents.md`, `annex-caching.md` and `annex-realtime.md`, beside this file, are its annexes. The core law is `/rancho:law`. Core §1 defines ⚙ as **"machine-checked. Violating one is a build failure, not a review comment."** This file is the audit of that claim for this stack law: every rule still carrying a mark can be made to fail the build. A ⚙ with no entry here is unaccounted for and MUST be added. This stack law's records and row history are in `../../../RECORDS.md`.

A section an annex holds keeps its number: §8 is in `annex-agents.md`, §5 *Read-side caching* in `annex-caching.md`, and §12 in `annex-realtime.md`.

## How to read this

**Prefix:** `BE`. Ids, columns and statuses: `/rancho:law`'s inventory, *How to read an inventory*. Tests reference the id in their name: `BE_31_QueryHandlers_DoNotReferenceAggregates`.

**Mechanism** - what does the checking:

| Code | Mechanism | Fails at |
|---|---|---|
| `proj` | Project or build configuration - `.csproj`, `Directory.Build.props`, banned packages | Compile |
| `roslyn` | Roslyn analyzer: a built-in CA rule, `BannedApiAnalyzers`, or a custom analyzer | Compile |
| `arch` | ArchUnitNET test over types, assemblies, namespaces, attributes | Test run |
| `contract` | Reflection, DI container, EF model, or committed-file inspection - **one** test that discovers every instance of the rule automatically | Test run |
| `ci` | Repository-level script - path checks, generated-artifact freshness, diff checks | CI |
| `test` | A behavioural test that a human must write **per instance** | Test run |

## Summary

The stack law carries **97 ⚙ marks**, each naming the ids of the rows it marks. Its rules are **114 rows**: 99 `build`, 14 `required-test`, 1 `dropped`, and 0 `retired`. Its role, `server`, carries **15 core rules**, below.

**Every marked rule can be made to fail the build.** No rule claims an enforcement it cannot deliver. Nothing is enforced yet, though - `build` means the check is buildable, not built.

Five rules restate another rule in this file, and twelve core rules restate one. One check satisfies each pair; the canonical id is noted and the duplicate points at it.

## Core rules carried

Every core rule whose role is `server` or `both` and whose status is `build`. The section is the core law's; the rule is worded as this stack law expresses it.

| Id | Rule | Section | Mechanism | Status | Check | Gap | Source |
|---|---|---|---|---|---|---|---|
| ROOT-01 | No module references another module's Domain/Application/Infrastructure | §3 | `proj` + `arch` | build | Assembly dependency assertion; also expressible as a `ProjectReference` allowlist | — | — |
| ROOT-02 | Domain events MUST NOT cross a module boundary | §3 | `arch` | build | Canonical: BE-07 | — | — |
| ROOT-04 | One schema per module; no cross-schema joins, no cross-module FKs | §3 | `contract` | build | EF model inspection: every entity's schema equals the module's; no FK targets outside it | A query handler's SQL (§4) is not in the EF model, so a cross-schema reference there is a known gap, carried by the real-database test (BE-112) and by per-module database permissions where the project grants them | — |
| ROOT-07 | Endpoints take a Request DTO, return a Response DTO; no domain type reachable from HTTP | §4 | `arch` | build | Canonical: BE-77 | — | — |
| ROOT-08 | Query results projected directly to Response DTOs | §4 | `arch` | build | Canonical: BE-108 | — | — |
| ROOT-09 | OpenAPI document generated and committed | §4 | `ci` | build | Regenerate in CI, fail on any diff | — | — |
| ROOT-11 | Every collection endpoint paginated | §4 | `arch` | build | Canonical: BE-19 | — | — |
| ROOT-12 | One page shape across the API | §4 | `arch` | build | Canonical: BE-20 | — | — |
| ROOT-13 | Every state-changing endpoint declares an idempotency mechanism | §4 | `contract` | build | Canonical: BE-26 | — | — |
| ROOT-15 | Acting principal explicit; no ambient HTTP context in application or domain | §7 | `arch` | build | Canonical: BE-52 | — | — |
| ROOT-16 | Unclassified is `Restricted` | §8 | `arch` | build | Canonical: BE-55 | — | — |
| ROOT-17 | Integration events MUST NOT carry `Restricted` payloads | §8 | `arch` | build | Canonical: BE-61 | — | — |
| ROOT-18 | A field's class travels with its contract: the OpenAPI document carries the class of every request and response property and every parameter | §8 | `ci` | build | Canonical: BE-83. The client half is the client stack law's | — | — |
| ROOT-19 | Every unit of work carries a correlation id | §9 | `proj` | build | Canonical: BE-90 | — | — |
| ROOT-20 | A class declared on a domain type and on its `Contracts` twin agrees | §8 | `arch` | build | Canonical: BE-54 | — | — |

## Stack law - `../SKILL.md` and its annexes

| Id | Rule | Section | Mechanism | Status | Check | Gap | Source |
|---|---|---|---|---|---|---|---|
| BE-01 | Reference table: each project references, and declares packages, only as the table allows | §2 | `proj` | build | Allowlist keyed on the kind each project declares - an undeclared or unrecognised kind fails. Each direct `ProjectReference` checked against the row's project column, and each `PackageReference` against its package column; `Api` may declare no package an Infrastructure project declares, and admits EF Core's design-time tooling only with `PrivateAssets="all"`. Asserted in CI and by `arch` | — | — |
| BE-02 | Domain references no EF Core, Mediator, ASP.NET, `IHttpContextAccessor`, application types | §2 | `proj` + `arch` | build | Banned assembly references on the Domain project | — | — |
| BE-03 | No grouping by pattern: no `Application/Handlers/`, `Domain/Entities/`, etc. | §2 | `ci` | build | Forbidden directory-name check (denylist only - the positive form is not checkable) | — | — |
| BE-105 | `Api` names no Infrastructure type but a registration entry point, and after startup invokes none directly | §2 | `arch` | build | No type in `Api` references an Infrastructure type other than a registration entry point. Types the dispatcher's source generator emits - marked with `GeneratedCodeAttribute` naming that generator, not any generator - are skipped; the rule puts them outside it | — | — |
| BE-106 | Each module's Infrastructure exposes exactly one registration entry point, `Add<Module>Module(IServiceCollection, IConfiguration)` | §2 | `arch` | build | Exactly one public static extension method over `IServiceCollection` per module Infrastructure assembly, with that signature | — | — |
| BE-107 | `InternalsVisibleTo` names test assemblies only | §2 | `contract` | build | Every `InternalsVisibleTo` attribute on every production assembly names a project whose declared kind is `<Anything>.Tests` | — | — |
| BE-04 | Aggregate state private; no public setters; collections as `IReadOnlyCollection<T>` | §3 | `arch` | build | Public property setters on Domain types = fail | — | — |
| BE-05 | Aggregate methods MUST NOT return `Result`, a DTO, or any application type | §3 | `arch` | build | Return types of public Domain methods must be Domain or BCL | — | — |
| BE-06 | Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O | §3 | `arch` | build | Banned namespaces on Domain (`System.IO`, `System.Net`, EF) | — | — |
| BE-07 | Domain events are intra-module | §3 | `arch` | build | Domain event types not referenced outside their module's assemblies | — | — |
| BE-08 | No bare `Guid` in the public surface of `Domain` or of a command or query message, except a strongly-typed id's own constructor and value member | §3 | `arch` | build | A type whose only state is one `Guid` it wraps is a strongly-typed id, identified structurally, and its constructor and value member are exempt | — | — |
| BE-109 | A command or query message, and a Response DTO, reference no `Domain` type; ids they carry are `Contracts` ids | §3 | `arch` | build | Property and constructor-parameter types of every message and Response DTO, transitively, resolve outside `Domain` | — | — |
| BE-09 | Domain events past tense | §3 | — | dropped | — | — | — |
| BE-10 | Domain events carry ids and values, never entity references | §3 | `arch` | build | Property types of event types must not be entity types | — | — |
| BE-78 | An aggregate root is marked as one at the type level | §3 | `arch` | build | Marker interface present on every root | — | — |
| BE-79 | An aggregate holds another aggregate's id, never a reference | §3 | `arch` | build | No property of a root-marked type is another root-marked type | — | — |
| BE-11 | Command return type limited to identifier and outcome | §4 | `arch` | build | `ICommand<T>` closed over `Unit` or an id type | — | — |
| BE-12 | A query handler references no repository port and never calls `SaveChangesAsync` | §4 | `arch` + `roslyn` | build | `arch`: no repository port referenced from query handler types. `roslyn`: `BannedApiAnalyzers` on `SaveChangesAsync`, scoped to query handler types | — | — |
| BE-13 | Command handlers mutate only through aggregate methods; no `DbContext`/SQL | §4 | `arch` | build | Covered by BE-01: Application cannot reference EF | — | — |
| BE-14 | Query handlers project straight to DTOs, untracked; no aggregate materialization | §4 | `arch` | build | Covered by BE-108: a handler that references no `Domain` type cannot materialize an aggregate. Read types are keyless, which EF never tracks, and SQL into a DTO is never tracked. Asserted: every read type mapped in a module's model is keyless | — | — |
| BE-15 | Query message and abstraction in Application; every concrete handler in Infrastructure, by convention | §4 | `arch` | build | Type-location assertion by interface: every `IQueryHandler<,>` implementation is in a module Infrastructure assembly, with no exception for a pure handler, except a read-side cache decorator - one that takes an `IQueryHandler<,>` to wrap - which is in `Platform.Infrastructure` | — | — |
| BE-108 | A query handler references no `Domain` type | §4 | `arch` | build | Type references of every `IQueryHandler<,>` implementation resolve outside `Domain`. Subsumes BE-16, and the repository-port half of BE-12 | — | — |
| BE-112 | Every query handler that reads with SQL is tested against real SQL Server, including its query filters | §4 | `test` | required-test | §14 **Queries**. SQL into a DTO is not checked against the model at compile time, and bypasses global query filters | — | — |
| BE-113 | No raw-string SQL API in a module; SQL is parameterized | §4 | `roslyn` | build | `BannedApiAnalyzers` in every module project on EF Core's raw-string SQL methods - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw` and their async forms. A migration's `migrationBuilder.Sql` is outside the rule, which says so | SQL set directly on an ADO.NET `DbCommand.CommandText` is not reached; review carries it | — |
| BE-16 | Query handlers MUST NOT use an aggregate repository | §4 | `arch` | build | Covered by BE-108: a repository port is a `Domain` type | The rule also forbids the concrete repository in `Infrastructure`, which is not a `Domain` type; BE-108 does not reach it. 3.0 stage 7 re-derives this check. | — |
| BE-17 | `Repository` reserved for the write path; read-side types named `<Feature>Reader` | §4 | `arch` | build | Naming plus interface-origin assertion | — | — |
| BE-18 | Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, transport types | §4 | `arch` | build | Same check as BE-52 | — | — |
| BE-19 | Every collection query paginated | §4 | `arch` | build | Query handler return types: `Page<T>` only, never `List<T>`/`IEnumerable<T>` | — | — |
| BE-20 | One shared page shape | §4 | `arch` | build | Exactly one generic page type exists solution-wide | — | — |
| BE-21 | Every paginated query declares a maximum page size | §4 | `arch` | build | Paginated query types must expose `MaxPageSize` | — | — |
| BE-22 | A request above the maximum is rejected, never clamped | §4 | `contract` | build | Reflection over paginated query types; assert the validator rejects `max + 1` | — | — |
| BE-23 | Ordering declared as a descriptor whose last term is a unique key | §4 | `contract` | build | Descriptor inspection over all paginated queries | — | — |
| BE-24 | One cursor type solution-wide, payload exactly sort key plus id | §4 | `arch` | build | The prohibition follows by construction | — | — |
| BE-25 | A message with no declared policy MUST fail to dispatch | §4 | `contract` | build | Reflection over all message types at test time; runtime guard in the behavior | — | — |
| BE-26 | Every command declares an idempotency mechanism | §4 | `contract` | build | Same reflection sweep as BE-25 | — | — |
| BE-27 | Idempotency record written through the same unit of work; Redis MUST NOT be the store | §4 | `arch` | build | Idempotency store type depends on `DbContext`, never on `IConnectionMultiplexer` | — | — |
| BE-28 | Unique index on the attempt identity | §4 | `contract` | build | EF model inspection: index exists and `IsUnique` | — | — |
| BE-29 | Concurrent duplicates resolve as one execution and one replay | §4 | `test` | required-test | §14 **Idempotency** | — | — |
| BE-30 | Idempotency is a pipeline behavior, never per-endpoint | §4 | `arch` | build | Only the behavior may reference the idempotency store | — | — |
| BE-31 | An agent invocation of a keyed command carries a key derived from run and step | §4 | `arch` | build | By construction: the tool invoker is the only caller, and takes the key as a required argument for a command declaring one | — | — |
| BE-114 | An agent's re-invocation of a command declaring a client-supplied id carries the same id and writes once | §4 | `test` | required-test | `annex-agents.md` **Testing**, **Idempotency** | — | — |
| BE-32 | A transaction MUST NOT span a module boundary | §4 | `arch` | build | Covered by BE-01 | — | — |
| BE-33 | Integration events written to the outbox in the same transaction; no direct publish | §4 | `arch` | build | Application handlers may reference the outbox port only, never a bus client | — | — |
| BE-110 | A command declares whether it persists aggregate state; one that does not uses no key-based idempotency and no outbox | §4 | `contract` + `arch` | build | `contract`: the operation registry sweep (as BE-25) finds the declaration on every command, and refuses it on a query, together with an idempotency key, and with any mechanism but natural or client-supplied id. `arch`: no handler of a command declared not to persist references the outbox writer | — | — |
| BE-111 | A command that does not persist stages no database change | §4 | `test` | required-test | §14 **Transactions**. The transaction behavior's guard is runtime; the test proves it | — | — |
| BE-34 | Every Infrastructure type is an adapter or a registration entry point, and is `internal` unless the dispatcher's generated code must name it | §4 | `arch` | build | Implements a Domain/Application port or a framework extension point, is a registration entry point, or is a type the project maps or binds - an entity or read type in its EF model, or a bound options class. A public Infrastructure type that is not a registration entry point, not named by a generated type in `Api`, and not exposed by such a type's public constructor or member, fails. Mediator 3.x names each concrete handler in its generated registration, so concrete query handlers are public, as are the types those expose - the module's `DbContext`, which a handler's constructor takes, and the read types it exposes. `InternalsVisibleTo` is BE-107's | — | — |
| BE-35 | Infrastructure types live under `Persistence/`, `Queries/`, `Messaging/`, `External/`; the registration entry point at the project root | §4 | `arch` | build | Namespace assertion over an allowed set of top-level namespaces, which a new folder joins in its PR | — | — |
| BE-36 | `Infrastructure/Queries/<Feature>/` mirrors the Application feature folder | §4 | `ci` | build | Directory comparison | — | — |
| BE-84 | Every integration event type carries its version in its name | §4 | `arch` | build | Naming assertion over the `Contracts` project | — | — |
| BE-85 | The published contract surface is snapshotted; a shape change to an existing version fails the build | §4 | `ci` | build | Snapshot and diff, the same mechanism as ROOT-09 | — | — |
| BE-86 | No domain type in an integration event payload | §4 | `arch` | build | Property types of contract types resolve to ids, primitives, value objects | — | — |
| BE-37 | One `DbContext` per module; MUST NOT map another module's tables | §5 | `contract` | build | EF model: every entity type, read types included, originates in the owning module's assembly | SQL is outside the model: see ROOT-04 | — |
| BE-38 | Mapping in `IEntityTypeConfiguration<T>`; no mapping attributes on domain types | §5 | `arch` | build | No EF attributes present on any Domain type | — | — |
| BE-39 | Integration tests run against SQL Server; no other provider stands in for it | §5 | `proj` | build | A package allowlist, not a denylist: SQL Server is the only EF provider a test project may reference, so SQLite and InMemory both fail without being named | — | — |
| BE-40 | Repositories MUST NOT expose `IQueryable`, `DbSet`, or EF types | §5 | `arch` | build | Signature assertion on repository ports | — | — |
| BE-41 | Command handlers write only through the repository | §5 | `arch` | build | No type in a module `Application` references the unit-of-work port, declared in `Platform.Application` | — | — |
| BE-42 | No navigation properties between aggregate roots | §5 | `contract` | build | EF model navigation targets, resolved through the root marker | — | — |
| BE-43 | Domain types acquire no public parameterless ctors, public setters, or EF attributes | §5 | `arch` | build | Extends BE-04 and BE-38. Skips compiler-generated types, marked with `CompilerGeneratedAttribute`, such as a lambda's closure class | — | — |
| BE-44 | Cache decorators registered explicitly per query, never auto-applied | §5 | `contract` | build | Container inspection: no open-generic decorator registration over `IQueryHandler<,>` | — | — |
| BE-45 | **Every** cache key includes the acting principal | §5 | `arch` | build | Unconditional, built by the one key builder | — | — |
| BE-46 | Every cache key embeds its aggregates' version stamps, declared explicitly | §5 | `arch` + `contract` | build | Cached queries implement a dependency-declaring interface; key builder consumes it | — | — |
| BE-47 | `KEYS` and `SCAN`-and-delete invalidation forbidden | §5 | `roslyn` | build | `BannedApiAnalyzers` on the Redis APIs | — | — |
| BE-48 | Command handlers MUST NOT touch the cache | §5 | `arch` | build | No cache type referenced from command handler types | — | — |
| BE-49 | A Redis failure MUST be treated as a miss | §5 | `test` | required-test | `annex-caching.md` **Testing**, **Cache** | — | — |
| BE-87 | A committed migration MUST NOT be edited | §5 | `ci` | build | Diff every migration file against its committed history | — | — |
| BE-88 | A migration references no application code | §5 | `arch` | build | Migration types depend on EF only | — | — |
| BE-89 | Migrations run clean twice; a destructive one keeps the previous version serving | §5 | `test` | required-test | §14 **Migrations** | — | — |
| BE-50 | Every Command and Query declares a named policy; no bare `[Authorize]` | §6 | `contract` + `roslyn` | build | Reflection sweep (as BE-25); `BannedApiAnalyzers` on parameterless `[Authorize]` | — | — |
| BE-51 | `FallbackPolicy` MUST deny | §6 | `contract` | build | Startup configuration assertion | — | — |
| BE-52 | `IHttpContextAccessor` MUST NOT be referenced outside the composition root | §6 | `arch` | build | Assembly-scoped type reference assertion. The composition root is `Api` and the registration entry points it calls (§2) | — | — |
| BE-53 | Application and domain code MUST NOT read raw claims | §6 | `arch` | build | Every type raw claims can be read through, such as `ClaimsPrincipal`, `ClaimsIdentity`, `Claim`, `ClaimTypes` or `JwtRegisteredClaimNames`, banned in every module `Domain` and `Application` assembly - the rule's own scope. `Platform.Application`'s authorization behavior passes the principal to `IAuthorizationService` without reading a claim, and the authority adapter (BE-102) reads the subject claim to look permissions up; both are outside the ban. Ban the `System.Security.Claims` namespace rather than the members, as BE-98 bans types rather than predicates | — | — |
| BE-99 | An external authority's unavailability - one that validates the token or answers for permissions - MUST deny; an expired cache entry is not served during an outage | §6 | `test` | required-test | §14 **Authorization** | — | — |
| BE-100 | An external authority's cached decision - a validation result or a permission decision - declares a bounded lifetime in an options class | §6 | `contract` | build | Options sweep, as BE-96: the lifetime exists on a validated options class and is bounded; a missing or unbounded value fails | — | — |
| BE-101 | An operation's authorization decision is reachable from the authorization behavior, not only a transport filter | §6 | `test` | required-test | §14 **Authorization** | — | — |
| BE-102 | An external authority's types and codes MUST NOT be referenced outside the one adapter project that wraps it | §6 | `arch` | build | The authority's SDK is declared by the Infrastructure project holding its adapter, and its types are banned in every other assembly | — | — |
| BE-103 | Externally-resolved permissions are intersected with the agent manifest for an agent-actor principal | §6 | `test` | required-test | `annex-agents.md` **Testing**, **Authorization** | — | — |
| BE-104 | A claim read before the token is validated only selects how it is validated, and must agree with the validated token | §6 | `test` | required-test | §14 **Authorization**: a token whose selecting claims disagree with its validated issuer or endpoint is refused, and no claim read before validation reaches the principal | — | — |
| BE-54 | A class is declared on the domain value type and on its published `Contracts` twin, and the two agree; attribute lives in `SharedKernel` | §7 | `arch` | build | Attribute defined in `SharedKernel`. Each `Contracts` type's class equals its `Domain` twin's; a property whose type carries no class declares one, and a projected property's class equals that of the source it names; no other redeclaration on a DTO, message or event property | — | — |
| BE-55 | Every property reachable from an egress boundary resolves to a class | §7 | `arch` | build | Transitive property-graph walk from each boundary root. **The keystone check** | — | — |
| BE-56 | `Confidential`/`Restricted` types redact in `ToString()`, no implicit string conversion | §7 | `arch` | build | Overrides `ToString`; declares no `op_Implicit` to `string` | — | — |
| BE-57 | No interpolation of a classified value into a log message template | §7 | `roslyn` | build | Enable `CA2254` as error, plus a custom rule for classified arguments | — | — |
| BE-58 | Every model-visible DTO is explicitly marked | §7 | `arch` | build | — | No check is described; the marker's presence is the checkable half (R-10). 3.0 stage 7 derives one. | — |
| BE-59 | No model-visible DTO contains `Confidential`/`Restricted`, transitively | §7 | `arch` | build | Reuses BE-55's graph walk | — | — |
| BE-60 | `Restricted` MUST NOT be written to Redis in any form | §7 | `arch` | build | Reuses BE-55's graph walk over cache value types | — | — |
| BE-61 | Integration events and outbox rows MUST NOT carry `Restricted` | §7 | `arch` | build | Reuses BE-55's graph walk over contract types | — | — |
| BE-83 | The class is emitted into the OpenAPI document on every Request and Response DTO property and on every parameter's schema | §7 | `ci` | build | Emitter fails on a property or parameter whose class it cannot resolve. On the parameter's schema, not the Parameter Object, because that is what a client generator's schema transform sees | — | — |
| BE-62 | Tools projected from the manifest; hand-registration forbidden | §8 | `contract` | build | Container inspection: every tool descriptor originates from the projector | — | — |
| BE-63 | Run scopes derive from the same manifest | §8 | `arch` | build | No scope literal outside the manifest projection | — | — |
| BE-64 | Every manifest entry resolves to a real handler with a matching policy | §8 | `contract` | build | Reflection test over manifest entries | — | — |
| BE-65 | The principal carries an actor claim identifying the agent | §8 | `test` | required-test | `annex-agents.md` **Testing**, **Agents** | — | — |
| BE-66 | An agent-actor principal MUST NOT start another agent run | §8 | `test` | required-test | `annex-agents.md` **Testing**, **Agents**. The guard is runtime, the test proves it | — | — |
| BE-67 | Autonomous runs need a separate manifest, no `HumanRequirement.Required` entries | §8 | `contract` | build | Reflection over manifests | — | — |
| BE-68 | No bare `string` in a tool parameter; declared wrapper types only | §8 | `arch` | build | No tool parameter, or property of one, is typed `string`; each string-valued parameter is a declared wrapper type | — | — |
| BE-69 | Tool output MUST be a Response DTO; no domain objects or raw rows | §8 | `arch` | build | Reuses BE-55/BE-59 machinery | — | — |
| BE-70 | The global rate limiter's default policy covers every endpoint | §9 | `contract` | build | Endpoint data source inspection: rate-limit metadata present on every endpoint | — | — |
| BE-71 | Every agent run carries a budget fixed at start | §9 | `proj` | build | By construction: budget is a required constructor parameter of the run | — | — |
| BE-72 | Exceeding a budget aborts; MUST NOT silently truncate | §9 | `test` | required-test | `annex-agents.md` **Testing**, **Budgets** | — | — |
| BE-73 | Limits are configuration and fail closed | §9 | `contract` | build | Options `ValidateOnStart`; test proving a missing value denies | — | — |
| BE-90 | The correlation id is a required constructor parameter of the dispatch context | §10 | `proj` | build | By construction: the context cannot be built without one | — | — |
| BE-91 | Outbox rows persist the correlation id of the writing transaction | §10 | `contract` | build | EF model: the column exists on the outbox entity | — | — |
| BE-92 | The dispatcher restores the correlation id on delivery | §10 | `test` | required-test | §14 **Correlation** | — | — |
| BE-93 | A handler MUST NOT reference `ILogger` | §10 | `arch` | build | Dispatch logging belongs to the pipeline behavior alone | — | — |
| BE-94 | Metric dimensions come from a declared closed set of keys | §10 | `contract` | build | Committed-file inspection of every metric call site: each dimension key is a member of the declared key set, never a literal or a computed string; bounds cardinality by construction. Not `arch`: a declared key is a `const string`, which the compiler inlines, so in IL a declared key and an arbitrary literal are the same `ldstr` and a type-level assertion cannot tell them apart | — | — |
| BE-95 | Configuration MUST NOT be read from its source outside the composition root | §11 | `arch` + `roslyn` | build | `arch`: type reference assertion on `IConfiguration`, as BE-52 does for `IHttpContextAccessor`. `roslyn`: `BannedApiAnalyzers` on `System.Environment.GetEnvironmentVariable` and `GetEnvironmentVariables`, scoped to allow `Api` and the registration entry points it calls | — | — |
| BE-96 | Every options class has a validator and is registered with `ValidateOnStart` | §11 | `contract` | build | Reflection sweep over registered options types | — | — |
| BE-97 | No secret in any committed file; a committed credential slot holds a reference, never a value | §11 | `contract` + `ci` | build | `contract`: a sweep of the committed configuration files - `appsettings*.json`, environment files, `nuget.config` - failing on a denylisted key name with a literal value, and on any `<packageSourceCredentials>` value that is not a `%VAR%` macro. It runs in the test suite, so it fails before a push. `ci`: entropy and provider-pattern secret scanning over the whole history, which no unit test can reach | — | — |
| BE-98 | Outside the composition root, code MUST NOT learn which environment it is running in | §11 | `roslyn` | build | `BannedApiAnalyzers` on the *types* that answer the question - `IHostEnvironment`, `IWebHostEnvironment`, both obsolete `IHostingEnvironment`s, and the `Environments` constants - scoped to allow `Api` and the registration entry points it calls. Banning the types rather than the predicates reaches `IsStaging()`, `IsEnvironment(string)` and `EnvironmentName == "..."` without naming them; an environment variable read is BE-95's | — | — |
| BE-74 | A hub references only DTOs and the dispatcher | §12 | `arch` | build | Type-reference assertion over hub types, the hub counterpart of BE-82 | — | — |
| BE-75 | Hub authorization uses the same named policies as the HTTP operation | §12 | `contract` | build | By construction if hubs dispatch messages: the policy resolves from the message | — | — |
| BE-76 | SignalR messages carry DTOs; no domain types serialized | §12 | `arch` | build | Hub method signatures | — | — |
| BE-77 | Endpoints MUST NOT reference domain types | §13 | `arch` | build | — | No check is described (P-01). 3.0 stage 7 derives one. | — |
| BE-80 | Every endpoint is a type implementing the shared endpoint abstraction | §13 | `arch` | build | No call outside the discovery registrar to any `EndpointRouteBuilderExtensions` overload taking a handler delegate - every verb, not a list of them | — | — |
| BE-81 | Endpoints registered by discovery; hand-registration forbidden | §13 | `contract` | build | Container inspection, as BE-62 does for tools | — | — |
| BE-82 | An endpoint references only its DTOs and the types they are composed of, the message it dispatches, the dispatcher, and the result translator | §13 | `arch` | build | The endpoint counterpart of BE-74. Checks the whole endpoint type, including any method on a DTO it calls to build the message | — | — |
