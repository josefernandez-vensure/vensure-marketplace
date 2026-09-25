# Enforcement Inventory

Every rule marked ⚙ in the law, and the mechanism that enforces it.

`../SKILL.md` §1 defines ⚙ as **"machine-checked. Violating one is a build failure, not a review comment."** This file is the audit of that claim, and the claim now holds: every rule still carrying a mark can be made to fail the build. A ⚙ with no entry here is unaccounted for and MUST be added.

## How to read this

**Ids are permanent.** `BE-31` stays `BE-31` when sections renumber. The Section column is maintained; the id never changes. Tests reference the id in their name: `BE_31_QueryHandlers_DoNotReferenceAggregates`.

**Mechanism** - what does the checking:

| Code | Mechanism | Fails at |
|---|---|---|
| `proj` | Project or build configuration - `.csproj`, `Directory.Build.props`, `tsconfig`, banned packages | Compile |
| `roslyn` | Roslyn analyzer: a built-in CA rule, `BannedApiAnalyzers`, or a custom analyzer | Compile |
| `arch` | ArchUnitNET test over types, assemblies, namespaces, attributes | Test run |
| `contract` | Reflection, DI container, EF model, or committed-file inspection - **one** test that discovers every instance of the rule automatically | Test run |
| `eslint` | ESLint rule, off-the-shelf or custom | Lint |
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

The law carries **129 ⚙ marks on rules**. Three sit on a section heading and cover everything beneath, so the marked rules expand to **132 rows**. Seventeen further rows record rules that carry no mark: thirteen named as required tests, four left to review.

| | Rows | `build` | `required-test` | `dropped` |
|---|---|---|---|---|
| `../SKILL.md` | 16 | 14 | 0 | 2 |
| `backend.md` | 103 | 92 | 10 | 1 |
| `frontend.md` | 30 | 26 | 3 | 1 |
| **Total** | **149** | **132** | **13** | **4** |

**Every one of the 132 marked rules can be made to fail the build.** No rule claims an enforcement it cannot deliver. Nothing is enforced yet, though - `build` means the check is buildable, not built.

Eleven root rules are **duplicated** by a subordinate file, and four backend rules restate another backend rule. One check satisfies each pair; the canonical id is noted and the duplicate points at it.

## Root - `../SKILL.md`

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| ROOT-01 | No module references another module's Domain/Application/Infrastructure | §3 | `proj` + `arch` | build | S | Assembly dependency assertion; also expressible as a `ProjectReference` allowlist |
| ROOT-02 | Domain events MUST NOT cross a module boundary | §3 | `arch` | build | S | Canonical: BE-07 |
| ROOT-03 | "An event with both audiences is two events" | §3 | — | dropped | — | R-01 applied: mark removed, rule stands for review |
| ROOT-04 | One schema per module; no cross-schema joins, no cross-module FKs | §3 | `contract` | build | M | EF model inspection: every entity's schema equals the module's; no FK targets outside it |
| ROOT-07 | Endpoints take a Request DTO, return a Response DTO; no domain type reachable from HTTP | §4 | `arch` | build | M | Unblocked by P-01. Canonical: BE-77 |
| ROOT-08 | Query results projected directly to Response DTOs | §4 | `arch` | build | S | Canonical: BE-14 |
| ROOT-09 | OpenAPI document generated and committed | §4 | `ci` | build | S | Regenerate in CI, fail on any diff |
| ROOT-11 | Every collection endpoint paginated | §4 | `arch` | build | S | Canonical: BE-19 |
| ROOT-12 | One page shape across the API | §4 | `arch` | build | S | Canonical: BE-20 |
| ROOT-13 | Every state-changing endpoint declares an idempotency mechanism | §4 | `contract` | build | S | Canonical: BE-26 |
| ROOT-14 | Deny by default, everywhere | §7 | — | dropped | — | R-03 applied: a principle; the rules implementing it carry their own marks |
| ROOT-15 | Acting principal explicit; no ambient HTTP context in application or domain | §7 | `arch` | build | S | Canonical: BE-52 |
| ROOT-16 | Unclassified is `Restricted` | §8 | `arch` | build | L | Canonical: BE-55 |
| ROOT-17 | Integration events MUST NOT carry `Restricted` payloads | §8 | `arch` | build | M | Canonical: BE-61 |
| ROOT-18 | A field's class travels with its contract, into the OpenAPI document and the generated types | §8 | `ci` | build | M | Added by P-03. Canonical: BE-83 |
| ROOT-19 | Every unit of work carries a correlation id | §9 | `proj` | build | S | Canonical: BE-90 |

## Backend - `backend.md`

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
| BE-99 | An external permission authority's unavailability MUST deny; an expired cache entry is not served during an outage | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-100 | An externally-resolved permission cache declares a bounded lifetime in an options class | `contract` | build | S | Added by A-03. Options sweep, as BE-96: the lifetime exists on a validated options class and is bounded; a missing or unbounded value fails |
| BE-101 | An operation's authorization decision is reachable from the authorization behavior, not only a transport filter | `test` | required-test | M | Added by A-03. §14 **Authorization** |
| BE-102 | An external authority's types and codes MUST NOT be referenced outside the composition root | `arch` | build | S | Added by A-03. Same shape as BE-53: the authority client's permission types banned outside `Api` |
| BE-103 | Externally-resolved permissions are intersected with the agent manifest for an agent-actor principal | `test` | required-test | M | Added by A-03. §14 **Authorization** |

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
| BE-94 | Metric dimensions come from a declared closed set of keys | `arch` | build | S | No arbitrary string as a dimension key; bounds cardinality by construction |

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

## Frontend - `frontend.md`

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| FE-01 | `strict` on; no `any`; no type-checker suppression but a justified `@ts-expect-error` | §1 | `proj` + `eslint` | build | S | `tsconfig`; `no-explicit-any`; `ban-ts-comment` with every directive banned and `ts-expect-error` set to `allow-with-description` |
| FE-02 | `@ts-expect-error` requires a reason and an issue reference | §1 | `eslint` | build | S | `ban-ts-comment` with `descriptionFormat` |
| FE-03 | A feature MUST NOT import from another feature | §2 | `eslint` | build | S | `eslint-plugin-boundaries` |
| FE-04 | Dependency direction `shared` -> `features` -> `app`; no layer imports upward | §2 | `eslint` | build | S | `eslint-plugin-boundaries`, one `rules` entry per layer |
| FE-28 | Every feature is declared in `src/features/REGISTRY.md` | §2 | `ci` | build | S | Added by A-01, scoped by A-02. Listing of `src/features/` compared against the registry. No backend input |
| FE-05 | API types generated into `src/generated/` | §3 | `ci` | build | S | Regenerate and diff, with ROOT-09 |
| FE-06 | `src/generated/` MUST NOT be hand-edited | §3 | `ci` | build | S | Diff check against a fresh generation |
| FE-07 | A generated client response MUST NOT be cast, asserted, or re-declared | §3 | `eslint` | build | M | R-02 applied: `no-restricted-syntax` on casts of generated types |
| FE-08 | All server data through TanStack Query; no `useEffect` + `fetch` | §4 | `eslint` | build | M | Path-scoped `no-restricted-globals` on `fetch`; custom rule for the `useEffect` pattern |
| FE-09 | Query keys come from a per-feature factory | §4 | `eslint` | build | M | Custom rule: `queryKey` must be a call expression, never an array literal |
| FE-10 | Every list query paginated | §4 | — | dropped | — | R-13 applied: enforced server-side by BE-19. Client half is now FE-21 |
| FE-11 | Stores are feature-scoped | §5 | `eslint` | build | S | Path-scoped restriction on the `zustand` import |
| FE-12 | RHF + Zod only; no other form library | §6 | `eslint` | build | S | `no-restricted-imports` |
| FE-13 | Components MUST NOT call `fetch` or the generated client directly | §7 | `eslint` | build | S | Path-scoped `no-restricted-imports` |
| FE-14 | No `.css` import outside `shared/ui/`; no static object literal in `style` | §7 | `eslint` | build | S | R-14 applied |
| FE-15 | Exactly one `ProblemDetails` parser, in `shared/lib/` | §10 | `eslint` | build | S | Only that path may reference the type |
| FE-16 | Classified values MUST NOT appear in any part of a URL | §11 | `eslint` | build | M | Mark restored by P-03. Rule reads the class off the generated type |
| FE-17 | Analytics, telemetry, and error reporting MUST scrub classified values | §11 | `test` | required-test | M | frontend §13 **Data boundaries**. Mark stripped |
| FE-21 | No loop that exhausts `fetchNextPage` to assemble a whole collection | §4 | `eslint` | build | M | Added by R-13. `no-restricted-syntax`: `fetchNextPage` inside a loop or self-recursive call |
| FE-22 | Generated types carry the data class of every property | §3 | `ci` | build | S | Added by P-03. Generation output asserted to contain the class |
| FE-23 | Classified values MUST NOT be written to browser storage of any kind | §11 | `eslint` | build | M | Enabled by P-03. Same rule family as FE-16 |
| FE-24 | Token acquisition lives in exactly one place in `shared/lib/` | §9 | `eslint` | build | S | Path-scoped restriction on the MSAL import |
| FE-25 | A token MUST NOT be written to browser storage of any kind or a store | §9 | `eslint` | build | S | Same rule family as FE-23, and the same list of storage APIs: the two rules previously named different ones |
| FE-26 | The token is attached by exactly one interceptor | §9 | `eslint` | build | M | No literal `Authorization` header outside that module |
| FE-27 | `401` retries once silently then goes interactive; `403` never refreshes | §9 | `test` | required-test | M | frontend §13 **Authentication** |
| FE-29 | Sign-out ends the identity-provider session, not only local caches | §9 | `test` | required-test | M | Added by A-03. frontend §13 **Authentication** |
| FE-30 | A credential nested inside a token MUST NOT be extracted, stored, forwarded, or logged | §9 | `eslint` | build | S | Added by A-03. Same rule family as FE-25: reads of the configured nested-credential claim names off a decoded token |
| FE-18 | Components MUST NOT subscribe to hubs directly | §12 | `eslint` | build | S | Path-scoped restriction on the SignalR client import |
| FE-19 | MSW for all mocking; no module mock of the generated client | §13 | `eslint` | build | S | `no-restricted-syntax` on any `vi` module-mocking call with a generated-path argument |
| FE-20 | `pnpm lint && pnpm typecheck && pnpm test` gate every change | §14 | `ci` | build | S | CI pipeline |

## The badge problem

**Resolved.** Six rules - BE-29, BE-49, BE-65, BE-66, BE-72, FE-17 - were provable only by a behavioural test written per instance. Nothing failed when that test was never written, which is precisely the failure ⚙ exists to prevent, so the badge was false on all six as §1 defines it.

The fix taken was to strip the mark and keep the obligation: each rule stays MUST, and each is now named in a Testing section - backend §14 under **Idempotency**, **Cache**, **Agents**, and **Budgets**, frontend §13 under **Data boundaries**. `../SKILL.md` §1 states the convention, so an unmarked rule of this kind reads as deliberate rather than forgotten. The alternative - a second marker meaning "enforced by a mandatory test" - was rejected: it would have put a second symbol on every page to describe six rules, and it would have left ⚙ meaning two different things.

One bullet had to be split to do this. BE-28 (a unique index on the attempt identity, a `contract` check) and BE-29 (what happens when two attempts race, only a test can say) shared a sentence and therefore a mark. They are now two rules, and only the first carries one.

Sections written after this decision apply it from the start: BE-89, BE-92, BE-99, BE-101, BE-103, FE-27, and FE-29 were never marked, because no build check can reach them. They arrived as required tests. That is the convention working rather than being repaired.

Promotion out of this category is possible and has happened once: BE-23's determinism was test-only until R-07 required paginated queries to declare their ordering as inspectable metadata rather than burying it in a LINQ chain. The same move may be available for some of the six - if it is, it is a change to the law under §10, not a change to a test.

## Rewordings applied

All fifteen are now in the law. Each entry records the rule, why the original wording resisted checking, and what replaced it. Kept as the reasoning behind wording that would otherwise look arbitrary - and as the record required by `../SKILL.md` §10, which says the law changes first.

**R-01 · ROOT-03 · "An event with both audiences is two events."**
A design judgment about intent. No mechanical test distinguishes one event serving two audiences from one event that legitimately has one. *Applied:* drop the ⚙ and keep the sentence as guidance. The enforceable half - domain events do not leave the module - is already ROOT-02/BE-07.

**R-02 · ROOT-10, FE-07 · "No hand-written interface describing a server payload."**
"Describing a server payload" is intent. A local interface that happens to match a DTO is indistinguishable from any other type. *Applied:* reword to the checkable form - *every call to the generated client MUST use the generated types; a type assertion or cast on a generated client response is forbidden.* That is an ESLint rule over a known import path.

**R-03 · ROOT-14 · "Deny by default, everywhere."**
A principle spanning authorization, rate limiting, classification, and configuration. Not one check. *Applied:* remove the ⚙ from the principle and let it stand as the stated intent of BE-50, BE-51, BE-70, BE-73, and ROOT-16, each of which is separately checkable.

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

**R-13 · FE-10 · "Every list query MUST be paginated."**
The client cannot verify this; it consumes whatever the generated client exposes. *Applied:* the frontend ⚙ is dropped, the law now says outright that enforcement is server-side (BE-19), and the genuinely client-side half became a marked rule of its own - *no loop that exhausts `fetchNextPage` to assemble a whole collection*, FE-21.

**R-14 · FE-14 · "Inline `style` only for genuinely dynamic values."**
"Genuinely dynamic" is judgment. *Applied:* split. `ci`/`eslint` enforces the checkable half - no `.css` imports outside `shared/ui/` and the global stylesheet - and a static object literal passed to `style` becomes a lint error. Anything computed passes, which is the intent.

**R-15 · FE-16 · "Classified values MUST NOT appear in URLs or route/search params."**
The frontend has no classification metadata today, so nothing can decide which values are classified: the mark was fiction. *Applied:* the ⚙ is removed and the law states the condition on which it returns - classification carried into the OpenAPI document and the generated types, P-03 below. The rule itself is untouched and still MUST. This is the one rewording that made a rule *less* enforced rather than more, because the alternative was leaving a mark that no check could ever honour.

## Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under `../SKILL.md` §11 - the law first, the codebase after.

**A-01 · ROOT-05 · "Frontend folders mirror backend modules one-to-one."**
The rule asserted a bijection the domain does not have. Backend modules are consistency boundaries; frontend features are user capabilities. One bounded context is usually several capabilities, and one screen usually reads several contexts - so the check failed in both directions: a backend module with no UI demanded an empty folder, and a UI-only capability could not have one at all. Worse, the frontend tree had `modules/`, `shared/` and `generated/` and no composition layer, while FE-03 forbade cross-module imports and FE-04 forbade `shared/` importing modules. A screen spanning two modules therefore had no legal home anywhere in the tree, which makes a suppression comment the only way to ship it. *Applied:* `frontend.md` §2 is rewritten around a one-way dependency graph, `shared` -> `features` -> `app`, with cross-feature composition in `app/routes/` - the structure Bulletproof React and Feature-Sliced Design converge on, and the one TanStack Router's generated route tree already assumes. The mirroring check becomes a registry check (FE-28): a feature declares the backend module it consumes and reuses its name, or declares itself UI-only with a reason. Vocabulary drift stays caught; the false failures go. Net effect on the inventory at the time: the mirroring row became a registry row and the frontend gained one. A-02 then moved it out of the root table altogether.

**A-02 · ROOT-05, ROOT-06, ROOT-10 · Frontend rules stated in the root law.**
The root law carried three rules that bind only the frontend tree: how its folders are named, that its features may not import each other, and that a generated client response may not be cast. All three were already stated in full in `frontend.md`, so the root copy governed nothing the subordinate file did not already govern - it only made the root read as though one decomposition covered both trees. A-01 made this worse by writing the registry rule into §3, a section whose every other bullet is about .NET project references, EF schemas, and the outbox. *Applied:* the three rules leave the root law and the root table; §3 opens by scoping itself to backend modules and pointing at `frontend.md` §2; §4 keeps the boundary fact - frontend types are generated from the OpenAPI document - and delegates what the frontend does with them. The line drawn: **the root states obligations that span the boundary or bind both trees; each subordinate file states what is internal to its own.** FE-28's check was rescoped to match, reading only `src/features/` and the registry, with backend name reuse demoted from a cross-tree ⚙ to a SHOULD. Root drops from 19 rows to 16.

**A-03 · §6 Authorization, frontend §9 · The law named an identity provider instead of a shape.**
Backend §6 said "Entra ID claims are mapped to policies", and frontend §9 opened with "Entra ID through MSAL". The organization's applications authenticate through a different, federating provider, and their tokens carry no roles: permissions come from a remote authorization service through a vendor package. The law's intent held - deny by default, named policies, no raw claims below `Api`, a browser that decides nothing - but by naming one provider it was silent on the three failure modes a remote authority has and a token claim does not. The authority can be unreachable, and nothing said an outage denies (BE-51 covers a forgotten attribute, not an unreachable authority). Its answers can be cached past a revocation, and nothing bounded how long. It answers for the user, and nothing said that answer is still intersected with an agent's manifest under §8. On the frontend, sign-out cleared local caches but could leave the provider session alive, so the next person at a shared workstation signed in silently as the previous one. *Applied:* the provider names leave both files, and provider choice is recorded in each application's `DECISIONS.md`. Backend §6 gains the rule that a permission source is a composition-root concern with the authority's codes confined to `Api` (BE-102), a subsection on external permission authorities - fail closed (BE-99), a bounded and declared cache lifetime that an outage never extends (BE-100, BE-99), intersection for agent principals (BE-103) - and the rule that a transport filter is never the only enforcement point, applying `../SKILL.md` §7 where the wiring happens (BE-101). Frontend §9 gains ending the provider session on sign-out (FE-29) and the rule that a credential nested inside a token is never extracted (FE-30). The cache lifetime MUST be bounded and declared and SHOULD be configurable: a vendor client that caches for a fixed period complies by stating that period, so the rule does not fail every consumer of a package that does not expose it, and a lifetime nobody can state is still a defect. The four runtime rules arrived as required tests, named in backend §14 and frontend §13. Backend rows go from 98 to 103, frontend from 28 to 30.

**A-04 · BE-39, BE-80, BE-95, BE-97, BE-98, FE-01, FE-16, FE-19, FE-23, FE-25, §15 · A ⚙ rule named members of its class instead of the class.**
A machine-checked rule is read as exhaustive: the check is taken to be the boundary, so whatever passes it is permitted. A rule that enumerates therefore grants every member it forgot to name. BE-98 named `IsProduction()` and `IsDevelopment()`; `IsStaging()`, `IsEnvironment("Development")` and a comparison on `EnvironmentName` are the same question, and a check built to the text let them through. BE-97 named four kinds of secret in three places; a consuming application's reference implementation commits a `nuget.config` whose `<packageSourceCredentials>` holds placeholders, one local substitution away from a committed package-feed token - a kind and a place the rule did not name. A sweep for the same shape found it again in rules nobody had tripped over yet. BE-95 banned `IConfiguration` but not the environment variables behind it. BE-39 banned the InMemory provider and left SQLite. BE-80 named `MapGet` and `MapPost`. FE-01 omitted `@ts-nocheck`. FE-25 and FE-23 listed different storage APIs for the same concern, and FE-25's list lacked IndexedDB. FE-16 omitted the fragment, FE-19 `vi.doMock`, and §15's clock item `DateTimeOffset.UtcNow` and `DateTime.Today`. *Applied:* each rule now states its class, with members kept only as marked illustrations. `../SKILL.md` §1 makes that the rule for writing rules, and §10 adds the converse case: a check stricter than its rule is the same contradiction, resolved by widening the rule or narrowing the check in the same change. BE-98's check moves from predicates to types, which closes the `EnvironmentName` gap a predicate ban leaves, and telemetry that needs the environment's name gets it from the composition root. BE-97 gains the requirement that a committed credential slot holds a reference, which is what makes the check decidable - a sweep cannot tell a substituted value from the placeholder it replaced. BE-97's row records its two genuinely different checks, `contract` for the committed set and `ci` for history, and the `contract` mechanism admits committed-file inspection. No rule id is added or retired and the row counts stand.

## Prerequisites (all adopted)

Four checks were blocked on an architectural decision rather than on the work of writing a test. All four decisions are now in the law.

**P-01 · Endpoints are types, not raw lambdas.** *Adopted.* ArchUnitNET sees types; it cannot see the signature of a lambda handed to `MapPost`. `backend.md` §13 now requires every endpoint to be a type implementing the shared endpoint abstraction, registered by discovery (BE-80, BE-81). This unblocked ROOT-07 and BE-77, and made checkable a rule that had never carried a mark at all: BE-82, an endpoint referencing only its DTOs, the dispatcher, and the translator - the endpoint counterpart of what R-12 did for hubs.

**P-02 · Aggregate roots carry a marker.** *Adopted.* Nothing distinguished a root from an entity at the type level. `backend.md` §3 now requires the marker (BE-78). This unblocked BE-42 and made BE-79 checkable - *an aggregate holds another aggregate's id, never a reference*, which has been law since the first draft with no way to test it.

**P-03 · Classification reaches the frontend.** *Adopted.* The browser had no way to know which fields were classified, which is why R-15 had to strip FE-16's mark. The class is now emitted into the OpenAPI document as a vendor extension (BE-83), carried into the generated types (FE-22), and readable by ESLint. That restored FE-16 and made FE-23 checkable. FE-17 stays `test`: a lint rule can catch a classified value handed to a telemetry call, but whether an SDK's scrubbing configuration is correct is a runtime fact, not a static one.

**P-04 · One cursor type.** *Adopted* as part of R-08.

## Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** `proj` and `roslyn` checks: BE-02, BE-39, BE-47, BE-57 (`CA2254` is a compiler switch), BE-50's banned `[Authorize]`, BE-98's banned environment types, BE-95's banned environment-variable reads, BE-97's secret scan, plus the §15 Forbidden list as `BannedApiAnalyzers` entries. Frontend: FE-01 through FE-04, FE-11 through FE-13, FE-15, FE-18, FE-19 are almost all off-the-shelf ESLint configuration. **Roughly 25 rules enforced in about a day**, none requiring a decision.

**Second - the reflection sweeps.** One test each, discovering every instance automatically: BE-25 and BE-26 (declared policy, declared idempotency), BE-64, BE-67, BE-70, BE-73, BE-51, BE-96 (every options class validated on start), BE-100 (the permission-cache lifetime declared and bounded). These are the highest-leverage tests in the suite, because they cover every message that will ever be added, not just today's.

**Third - the classification graph.** BE-55 is the keystone: BE-59, BE-60, BE-61, BE-69, and ROOT-16 all reuse its property-graph walk. Write it once. It is the largest single piece of work here and the one that carries the most risk if it is skipped. The same pass emits the class into the OpenAPI document (BE-83) and carries it into the generated types (FE-22), which is what makes FE-16 and FE-23 enforceable on the client at all.

**Fourth - the structural arch tests.** BE-01, BE-04 through BE-07, BE-10, BE-13 through BE-20, BE-30, BE-33 through BE-38, BE-40, BE-43, BE-46, BE-48, BE-52, BE-53, BE-102, BE-63, BE-76, and BE-78 through BE-82. Individually cheap, collectively the bulk of the row count.

**Every decision is settled.** The fifteen rewordings, all four prerequisites, and the badge question are in the law. Every check above can be built against the current text, and nothing in this file is now waiting on an answer - only on the work. Building checks against the current text means building the wrong checks for some of them, and R-09 in particular should change before anything caches anything.
