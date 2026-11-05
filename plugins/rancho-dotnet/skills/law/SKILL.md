---
name: law
description: The .NET server stack law - project references, domain and application layers, EF Core persistence, authorization, data classification, AI agents, quotas, observability, configuration, real-time, and the API layer. Subordinate to the core law in /rancho:law. Read in full, after the core law, before writing, reviewing, or placing code in a tree whose CLAUDE.md names `Stack law: /rancho-dotnet:law`.
---

# .NET Stack Law

The core law, `/rancho:law`, governs: read it first. This is a stack law under core §2. It adds the .NET rules, and it carries the core law's mechanism in every tree bound to it. Core §N is §N of the core law. `⚙` = can be made to fail the build (core §1). Every `⚙` here, and every core rule this role carries, is inventoried in `references/enforcement.md`, beside this file: consult it by rule id rather than reading it whole.

Three annexes, beside the inventory in `references/`, hold rules that bind a tree only once the tree meets the annex's trigger: `annex-agents.md`, `annex-caching.md` and `annex-realtime.md`. Each is named, with its trigger, where its rules would otherwise stand. Once the tree meets a trigger, the annex binds the whole tree, exactly as this file does.

A tree is bound to this stack law by one line in the `CLAUDE.md` at its root:

```
Stack law: /rancho-dotnet:law
```

If `/rancho:law` is not available in the session, stop and say so. This file alone is not the law.

## Role

`server`. A tree bound here publishes the API contract (core §4), owns its modules and their persistence, and owns every egress boundary in core §8.

## Core map

| Core | Carried here by |
|---|---|
| §3 Module Boundary Law | §2 Project Reference Law · §4 Transactions and consistency, Integration event contracts · §5 Persistence |
| §4 API Contract | §13 API Layer · §4 Pagination, Idempotency · §9 Transport |
| §5 Validation | §4 Pipeline behaviors |
| §6 Error Contract | §2 (`Result` placement) · §3 Aggregates · §4 Commands and Queries, Pipeline behaviors · §5 Persistence · §13 API Layer · §15 Forbidden |
| §7 Security Baseline | §6 Authorization · §8 AI Agents · §10 Observability (*Audit trail*) · §11 Configuration and Secrets · §13 API Layer |
| §8 Data Boundaries | §7 Data Classification · §5 Read-side caching · §12 Real-time |
| §9 Observability | §10 Observability |
| §10 Verification | §14 Testing · §16 Verification · `references/enforcement.md` |

Gaps: none.

## Layout

Paths are relative to the tree root - the directory whose `CLAUDE.md` binds this stack law.

```
src/
  Modules/<Module>/
    <Module>.Domain/          BCL only. No framework references.
    <Module>.Application/     Commands, queries, handlers, behaviors.
    <Module>.Infrastructure/  EF Core, adapters, DbContext.
    <Module>.Contracts/       Public surface. The only project other modules may reference.
  Shared/SharedKernel/        Base types only. No business rules.
  Api/                        Composition root, endpoints, auth wiring.
tests/
```

§2 states what each project may reference.

## 1. Stack

.NET 10 (LTS) · EF Core · SQL Server · Redis · SignalR · Mediator 3.x, source-generated · FluentValidation · Serilog · OpenTelemetry · Azure Key Vault · xUnit v3 · NSubstitute · Shouldly · Testcontainers · ArchUnitNET

`TreatWarningsAsErrors` and `Nullable` are enabled solution-wide and MUST stay enabled. Suppressing a warning requires a comment naming the rule and the reason; where the warning enforces a rule of the law, the reason is its exceptions-register entry's id (core §11).

Verify third-party licenses before pinning. MediatR and FluentAssertions moved to commercial licensing; the picks above avoid that.

## 2. Project Reference Law

Four facts decide every reference. The table below is their consequence, not a second set of rules:

1. A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference a module or `Api`.
2. A module reaches another module only through that module's `Contracts`.
3. Inside a module, dependencies point inward: `Infrastructure` to `Application` to `Domain` and `Contracts`. `Domain` and `Contracts` are peers and MUST NOT reference each other.
4. `Api` composes; nothing references `Api`.

| Project | Projects it may reference | Packages it may declare |
|---|---|---|
| `SharedKernel` | none | none - BCL only |
| `<Module>.Domain` | `SharedKernel` | none - BCL only |
| `Platform.Application` | `SharedKernel` | dispatch, validation, authorization and logging abstractions; `Microsoft.Extensions.*.Abstractions` |
| `<Module>.Contracts` | `SharedKernel`, `Platform.Application` | none of its own |
| `<Module>.Application` | own Domain + Contracts, other modules' Contracts, `Platform.Application` | validation; `Microsoft.Extensions.*.Abstractions` |
| `Platform.Infrastructure` | `SharedKernel`, `Platform.Application` | EF Core, messaging, telemetry instrumentation, ASP.NET Core Data Protection, ASP.NET Core authorization, the SDK of any external system it adapts; `Microsoft.Extensions.*.Abstractions`; `Microsoft.Extensions.Options` with its configuration extensions |
| `<Module>.Infrastructure` | own Application, Domain, Contracts, `Platform.Infrastructure` | EF Core, messaging, ASP.NET Core authorization, the SDK of any external system it adapts; `Microsoft.Extensions.*.Abstractions`; `Microsoft.Extensions.Options` with its configuration extensions |
| `Api` | any `*.Contracts`, any `<Module>.Application`, any `<Module>.Infrastructure`, `Platform.Infrastructure`, `Platform.Application`, `SharedKernel` | ASP.NET Core, hosting, OpenAPI, the dispatcher's source generator, telemetry exporters, EF Core's design-time tooling as a private asset - never a package an Infrastructure project declares |
| `<Anything>.Tests` | any project | test packages |

Every project MUST declare its kind as an MSBuild property whose value is one of the rows above. A project whose kind is missing, misspelled, or absent from the table may reference nothing and be referenced by nothing: adding a kind of project means adding a row, derived from the four facts. ⚙ BE-01

A test project MUST NOT be referenced by a project of any other kind. A fixture built to prove a check fails MUST NOT appear in the solution file or under the production source root.

The table is an allowlist over **direct** `ProjectReference` entries, not over the transitive closure. A project MAY declare a reference it could also reach transitively; it MUST NOT declare one its row omits. ⚙ BE-01

The Domain project MUST NOT reference EF Core, Mediator, ASP.NET, `IHttpContextAccessor`, or any application type. ⚙ BE-02 Checkable by `.csproj` and by an architecture test.

**`SharedKernel` and the two `Platform.*` projects are shared, not modules.** `Platform.*` carries the dispatch pipeline - `Result`, the message abstractions, the solution-wide page shape and cursor type, and the behaviors. A platform project that names a module has stopped being platform, by fact 1.

`Result` lives in `Platform.Application`, not in `SharedKernel`. `Domain` does not reference `Platform.Application`, so "the domain MUST NOT reference `Result`" (core §6) is carried by the compiler rather than by an analyzer.

`Contracts` references `Platform.Application` because a published query message MUST name both its `Result` response and the abstraction it is dispatched by. That is the only reason for the reference: nothing else from `Platform.Application` may appear in a contract's public surface, so the committed contract snapshot (§4) is coupled to the message abstraction and to nothing else.

`Api` names only the registration entry points of Infrastructure projects. Each module's Infrastructure exposes exactly one, `Add<Module>Module(IServiceCollection, IConfiguration)`. `Platform.Infrastructure` exposes one `Add<Feature>` extension per platform feature, such as the outbox or correlation logging. `Api` MUST NOT name any other Infrastructure type - no client, credential, `DbContext`, repository, adapter, SDK type, or any other - and after startup MUST NOT name or invoke an Infrastructure type directly - dispatch reaches an Infrastructure handler through the dispatcher, not through `Api`. Each Infrastructure project builds and registers its own clients, including its `DbContext`. ⚙ BE-105, BE-106 Code the dispatcher's source generator emits into `Api` is written by the generator, not in `Api`, and is outside this rule.

The **composition root** is `Api` together with the registration entry points it calls. Wherever this law confines something to the composition root - reading configuration (§11), learning the environment (§11), resolving services from the container (§15), or registering decorators (§5) - both are admitted, and nothing else is.

A module is a Bounded Context. Its projects exist **at most once per module**, never per feature: a module with no model of its own omits `Domain`, one that publishes nothing - to its siblings or over the wire - omits `Contracts`, and `Application` is what makes a module dispatchable at all. A module has one Domain, shared by all of its features - features are use cases over that model, not copies of it. Vertical slicing applies to the **Application project only**.

| Project | Organized by | Contains |
|---|---|---|
| `Domain` | Aggregate | `<Aggregate>/` - root, its entities, value objects, outcomes, domain events, specifications, repository port |
| `Application` | Feature (use case) | `<Feature>/` - command or query, handler, validator, response DTO, operation declaration |
| `Infrastructure` | Technical concern | `Persistence/`, `Messaging/`, `External/` |
| `Contracts` | Published surface | `IntegrationEvents/`, `Queries/`, and the module's wire ids (§3) |
| `Platform.Application` | Technical concern | `Abstractions/`, `Behaviors/` |
| `Platform.Infrastructure` | Technical concern | `Persistence/` - including the audit trail and the cursor codec - and `Messaging/` |

Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`, no `Domain/Entities/`, no `Domain/ValueObjects/`. ⚙ BE-03

Two features that use the same aggregate share it. Duplicating an aggregate across feature folders inside one module is a defect - it means the module has more than one model of the same concept.

Domain event reactions are Application slices too, named for what they do: `Application/<Reaction>/`.

## 3. Domain Layer

### Aggregates

- Aggregate state MUST be private. No public setters. Collections are exposed as `IReadOnlyCollection<T>` over a private backing field. ⚙ BE-04
- State MUST change only through methods named in domain vocabulary. `SetStatus` is a defect; `Approve` is the rule.
- An aggregate MUST be valid at every instant of its public lifetime. Constructors and methods enforce invariants. There is no "validate later" step and no `IsValid` property.
- An aggregate method MUST NOT return `Result`, a DTO, or any application type. It returns a domain outcome, a domain value, or nothing. ⚙ BE-05
- Expected business rejections MUST be domain outcome values - an enum or sealed outcome type in domain vocabulary. They MUST NOT be exceptions.
- Invariant breaches MUST throw.
- Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O. ⚙ BE-06
- An aggregate root MUST be marked as one at the type level. ⚙ BE-78 "Root" is not a folder position or a naming convention; it is a property a check can read, and several rules depend on being able to read it.
- An aggregate MUST NOT hold a reference to another aggregate. It holds that aggregate's id. ⚙ BE-79

```csharp
// Illustrative.
public <Outcome> <Operation>(<Args> args)
{
    if (<business condition>) return <Outcome>.<Rejection>;   // expected: a value
    ArgumentNullException.ThrowIfNull(args);                  // impossible: a throw

    <mutate private state>;
    Raise(new <Aggregate><Operation>ed(Id, _clock.UtcNow));
    return <Outcome>.<Success>;
}
```

### Value Objects

- Primitives in aggregates and entities SHOULD be replaced by Value Objects wherever the primitive carries meaning or constraints. A `DateOnly` date of birth becomes `Birthday`, which rejects future dates and implausible ages. A `string` email becomes `EmailAddress`.
- Value Objects MUST be immutable, MUST validate in their constructor, and MUST have structural equality.
- A Value Object MUST be impossible to construct in an invalid state.
- Every identifier MUST be a strongly-typed id - passing the wrong id must not compile.
- No public member of `Domain`, or of a command or query message, exposes a bare `Guid`, except a strongly-typed id's own constructor and value member - they are how the id is built from, and read as, the wire value its `Contracts` twin carries. ⚙ BE-08 `int` stays permitted for counts and quantities, so an `int` pressed into service as an identifier is caught in review rather than by the check.
- A command or query message, and a Response DTO, MUST NOT reference a `Domain` type. ⚙ BE-109 An id they carry is the module's published id type from `Contracts`, which is the one wire id for that concept; the handler converts it, and any other wire value, into domain values. A module with an endpoint therefore has a `Contracts` project.

### Entities and Domain Events

- Entities inside an aggregate MUST be reachable and modifiable only through the root.
- Domain events MUST be raised by the aggregate, recorded on it, and dispatched by infrastructure after persistence. A handler MUST NOT raise a domain event.
- Domain events SHOULD be named in the past tense.
- Domain events MUST carry ids and values, never entity references. ⚙ BE-10
- Domain events are intra-module and MUST NOT leave the module. ⚙ BE-07

### Specifications

- Specifications MAY encapsulate reusable query predicates.
- A Specification MUST NOT be the authority on whether a mutation is legal. That authority is the aggregate method. Never pre-check with a Specification and rely on the aggregate not to re-check - the two will drift, and the drift is a bypassed business rule.

## 4. Application Layer

### Commands and Queries

- A Command mutates. A Query reads. A Command MUST NOT return data beyond an identifier, its outcome, and the new version of the aggregate it changed (core §4 *Wire contract*). ⚙ BE-11
- A Query MUST NOT mutate: a query handler MUST NOT reference a repository port, and MUST NOT call `SaveChangesAsync`. ⚙ BE-12
- Command handlers MUST mutate state only by loading an aggregate and calling its methods. Writing through `DbContext` or SQL from a command handler is forbidden. ⚙ BE-13
- Query handlers MUST bypass the domain and project straight to Response DTOs, untracked. Materializing an aggregate in a query handler is forbidden - a read returns the fields the caller needs, never a whole aggregate. ⚙ BE-14
- The query message, its Response DTO, and the `IQueryHandler<,>` abstraction live in `Application`. **Every concrete query handler lives in `Infrastructure`, by convention.** A query handler that is pure computation is rare, and follows the same convention, so a handler's placement never depends on inspecting what it calls. Commands do not split this way: a command handler needs only the repository port declared in `Domain`, so it stays in `Application`. ⚙ BE-15
- A query handler reads and projects straight into its Response DTO, independent of the domain model: it MUST NOT reference a `Domain` type. ⚙ BE-108 It reads a relational database through keyless entity types its module's Infrastructure maps for reading, over a table or a view and queried with LINQ - EF never tracks a keyless type - or with parameterized SQL projected into the DTO, such as EF Core's `Database.SqlQuery<T>` with an interpolated string. A read type carries the module's global query filters, such as a tenant or soft-delete filter; SQL bypasses them and MUST apply them itself. A handler that reads with SQL is covered by a test against a real database (§14): SQL is not checked against the model at compile time, so that test is what catches a renamed column.
- SQL MUST NOT be built from input by any means but the parameterized form. The raw-string APIs - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw`, or any other - MUST NOT be used in a module. ⚙ BE-113 A migration's SQL (§5) is written, not built from input, and is outside this rule.
- A query handler MUST NOT use an aggregate repository. Repositories serve the write path only; reads project directly. ⚙ BE-16
- `Repository` is reserved for the aggregate write path. A read-side type that returns DTOs MUST NOT be called a repository - name it `<Feature>Reader`. Usually the query handler is the read data access and no separate type is warranted. ⚙ BE-17
- One handler per message. Handlers MUST NOT call other handlers. Shared logic belongs in the domain or a named application service.
- Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, or any transport type. ⚙ BE-18

```csharp
// Illustrative. Translation happens here and nowhere else.
return aggregate.<Operation>(args) switch
{
    <Outcome>.<Success>   => Result.Success(),
    <Outcome>.<Rejection> => Result.Conflict(<Module>Errors.<Rejection>),
};
```

Non-exhaustive switches fail the build, so adding a domain outcome forces every handler to address it.

### Pagination

- Every query returning a collection whose size is not bounded by construction MUST be paginated, and one bounded by construction declares its bound (core §4). A handler returning an unbounded collection, such as a `List<T>` or an `IEnumerable<T>`, is a defect. ⚙ BE-19 A declared stream (core §4 *Wire contract*) is not a collection.
- One shared page shape for the whole solution. A per-feature paging envelope is a defect. ⚙ BE-20
- Every paginated query MUST declare a maximum page size, under a solution-wide ceiling. A request above the maximum MUST be rejected by the validation behavior. Silent clamping is forbidden: a caller that asked for 500, received 100, and was told nothing will conclude it has every row. ⚙ BE-21, BE-22
- Omitting the page size MUST yield the operation's declared default, never "all".
- Ordering MUST be deterministic. A paginated query MUST declare its ordering as a descriptor whose last term is a unique key, rather than expressing it only as an `OrderBy` chain. ⚙ BE-23
- Keyset (cursor) paging is the only paging (core §4 *Wire contract*). A bounded set a UI grid shows in full is a collection bounded by construction only where it declares its bound and rejects a request beyond it.
- A total count MUST be opt-in per operation.
- One cursor type solution-wide, whose payload is exactly the sort key and the id. ⚙ BE-24 A cursor therefore cannot carry a filter, a sort, or a principal: a client that could edit those would otherwise widen its own read.
- Cursors are opaque to the client and MUST be validated server-side.
- A cursor is encrypted and authenticated, so its sort key - which may be classified - is neither readable nor editable by the client. It is encoded and decoded only through a cursor codec port declared in `Platform.Application` and implemented in `Platform.Infrastructure` with ASP.NET Core Data Protection, under a purpose per query. A cursor that fails to decode is a validation failure, never an exception that escapes. The key ring is persisted and shared by every instance, so a cursor survives a restart and works on any instance. ⚙ BE-119

The agent rule of this subsection is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

### Pipeline behaviors

Fixed order. Changing it requires written justification.

1. **Logging / correlation**
2. **Authorization** - resolves the message's declared policy and evaluates it against the explicit principal. A message with no declared policy MUST fail to dispatch. ⚙ BE-25
3. **Validation** - FluentValidation. Failures short-circuit to a `Result` failure; the handler is never invoked.
4. **Idempotency** - commands only. Resolves the attempt identity and short-circuits a replay to its recorded result before the handler runs.
5. **Transaction** - commands only. For a command that persists aggregate state, it opens the unit of work, commits on success, and rolls back on any failure or exception. A concurrency conflict or unique-key violation at commit comes back from the unit of work as an outcome, which this behavior returns as a `Result` conflict (core §6) - or, for a stale expected version or a create-only unique-index violation, a precondition failure (§13). The outcome names the index a unique-key violation hit, so the idempotency behavior can tell a violation of its own record from a business conflict. The idempotency record is written through this unit of work, so the state change and the record proving it happened are one commit. For a command that does not persist, it opens nothing, and after the handler returns it fails the dispatch if the module's context holds a staged change.

### Idempotency

A command that executes twice on a retry is a defect in the command, not in the caller.

- Every command MUST declare its idempotency mechanism in its operation declaration (§6): naturally idempotent, client-supplied id, or idempotency key. An undeclared command MUST fail to dispatch, exactly as an undeclared policy does. ⚙ BE-26
- Where a key is used, the identity of an attempt is `(principal, operation, key)`. A replay MUST return the originally recorded result without re-executing the handler. The same key with a different payload MUST fail with `422`, never overwrite (core §4).
- The idempotency record MUST be written through the same unit of work as the state change. Redis MUST NOT be the store - an expired cache entry must never make a command executable a second time (§5). ⚙ BE-27
- The store MUST carry a unique index on the attempt identity. ⚙ BE-28 The idempotency record stores a fingerprint of the request payload: an HMAC under a server key, never a plain hash, and its class is `internal`. A violation of that index at commit means a concurrent attempt executed first: the idempotency behavior compares fingerprints, and answers with that attempt's recorded result as a replay when they match, or `422` when they differ (core §4) - never as a conflict. After any commit conflict or precondition failure, the idempotency behavior looks up the attempt identity before it answers: a recorded attempt is answered as a replay, or `422` when the fingerprints differ.
- Two concurrent attempts with the same key MUST resolve as one execution and one replay, never as a `500`.
- Records MUST be retained longer than the longest caller retry window, and swept afterwards.
- Idempotency is a pipeline behavior. A per-endpoint or per-handler check is a defect - it will be forgotten on exactly the endpoint that needed it. ⚙ BE-30

The agent rule of this subsection is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

### Transactions and consistency

- One transaction MUST modify exactly one aggregate. Needing two means either the boundary is wrong or the second change belongs in an event handler. Outbox rows, idempotency records and audit records are bookkeeping written by the pipeline, not aggregates, and do not count against this rule.
- A transaction MUST NOT span a module boundary. ⚙ BE-32
- Integration events MUST be written to the outbox in the same transaction as the state change that caused them. Publishing directly from a handler is forbidden. ⚙ BE-33
- Every integration event handler MUST be idempotent. Delivery is at-least-once; assume redelivery.
- Handlers MUST NOT call external systems inside a transaction.
- A command declares, in its operation declaration (§6), whether it persists aggregate state. Only a command that does opens the unit of work. A command whose effect is an external write runs without a transaction, and MUST NOT use a key-based idempotency mechanism or the outbox - the idempotency record and the outbox row are written through the unit of work it does not have. ⚙ BE-110 It MUST NOT stage a database change; the transaction behavior refuses one (§14). It is idempotent naturally or by a client-supplied id, such as a blob name derived from the aggregate id, so a retry repeats the same write rather than adding a second. It MAY load an aggregate to decide whether the write may proceed. Recording the result of the write is a separate command that persists.
- An external effect that a repeat would duplicate - such as sending an email or taking a payment - is not such a command. A command that persists records the intent and writes an outbox row, and the outbox delivery performs the effect, passing an idempotency key derived from the row wherever the external system accepts one.

### Integration event contracts

An integration event is a published contract.

- Every integration event type MUST carry its version in its name: `<Event>V<N>`. ⚙ BE-84
- The published contract surface MUST be snapshotted and committed. A change to the shape of an existing version MUST fail the build, the same way a change to the OpenAPI document does. ⚙ BE-85
- Adding an optional field to an existing version is permitted. Renaming, removing, retyping, or changing the meaning of a field is not - that is a new version.
- Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes to it and no outbox row holds one. Both are published during the overlap.
- A consumer MUST tolerate fields it does not know and MUST NOT fail on one it does not use. Producers ship before consumers.
- Event payloads MUST be ids, primitives, and value objects that serialize stably. A domain type MUST NOT appear in an integration event. ⚙ BE-86

### Placement of interfaces and implementations

Placement is decided by **dependencies, never by the name**. `Service`, `Manager`, `Provider`, and `Client` are not signals: `IPricingStrategy` and `IEmailSender` are both "services" and belong in different layers.

| Interface | Implementation | What it is |
|---|---|---|
| `Domain` | `Domain` | Domain service - a pure business rule spanning aggregates, belonging to no single root |
| `Domain` | `Infrastructure` | Repository port, expressed in domain vocabulary |
| `Application` | `Application` | Use-case logic with no external dependency: strategies, policies, orchestrators, decision tables, mappers |
| `Application` | `Infrastructure` | Adapter - anything crossing a process boundary or naming a technology, including concrete query handlers |

**Where the interface goes:** the layer that can express the signature in its own vocabulary. A port whose signature would force `Domain` to reference an application type MUST be declared in `Application`.

**Where the implementation goes** - both tests must agree:

1. Does it require a package or namespace `Application` may not reference? Then `Infrastructure`.
2. Can it be unit tested with no test doubles for anything but domain types? Then `Application`.

An implementation that is pure logic MUST live in `Application`, except a concrete query handler, which §4 *Commands and Queries* places in `Infrastructure` by convention.

**Only one direction of this error is caught by the compiler.** Infrastructure logic in `Application` fails to build - the reference is not there. Application logic in `Infrastructure` builds cleanly, because `Infrastructure` references `Application`. That direction is invisible to the build and MUST be caught in review.

- Every Infrastructure type MUST be an adapter: it implements a port declared in `Domain` or `Application`, or a framework extension point such as `IEntityTypeConfiguration<T>`, `IHostedService` or `DbContext`, or it is a registration entry point (§2), or it is a type the project maps or binds - an entity or read type in its model, such as the outbox row or the idempotency record, or an options class (§11). A free-standing Infrastructure service belonging to none of these is a defect. Infrastructure types are `internal`, except the registration entry points, any type the dispatcher's generated code in `Api` must name, and the types those expose in a public constructor or member - such as the module's `DbContext`, which a query handler's constructor takes, and the read types the `DbContext` exposes. The source-generated dispatcher names each concrete handler in its generated registration, so every concrete query handler is public. Where the compiler no longer holds the boundary for a public type, BE-105's check holds it. `InternalsVisibleTo` names test assemblies only. ⚙ BE-34, BE-107
- Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/`, except the registration entry point, which sits at the project root. A new top-level folder there is added to the check's allowed set, with its justification in the PR. ⚙ BE-35
- `Infrastructure/Queries/<Feature>/` MUST mirror the feature folder name used in `Application`, so the two halves of a read slice are findable from each other. ⚙ BE-36

## 5. Persistence

- EF Core, code-first. Migrations are generated, reviewed, and committed with the change that requires them.
- One `DbContext` per module, one schema per module. A `DbContext` MUST NOT map another module's tables. ⚙ BE-37
- Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure. Mapping attributes on domain types are forbidden - the domain MUST NOT know it is persisted. ⚙ BE-38
- Lazy loading MUST be disabled.
- Integration tests run against real SQL Server via Testcontainers. No other provider may stand in for it - the EF InMemory provider, SQLite in memory, or any other. ⚙ BE-39
- Every aggregate MUST carry an application-managed version, which the aggregate increments on every change to itself or to anything inside it, mapped as its concurrency token; a database row version protects one row, not the aggregate. A concurrency conflict or a unique-key violation MUST surface as an outcome, never as an exception above the persistence adapter: the repository or unit of work that receives `DbUpdateConcurrencyException`, or the provider's unique-key error, catches that exception and nothing else, where it is raised, and translates it (core §6). A conflict raised at commit becomes a `Result` conflict in the transaction behavior (§4) - or, for a stale expected version or a create-only unique-index violation, a precondition failure (§13).
- SQL Server is the default store. A second store requires a documented reason in the module's README.

### Migrations

- Every schema change ships as an EF Core migration, generated, reviewed, and committed with the change that requires it.
- A committed migration MUST NOT be edited. ⚙ BE-87 Correct it with a new migration.
- Migrations are forward-only. A down migration MUST NOT be relied on above local development - recovery is a new forward migration, written deliberately.
- A migration MUST be safe to run while the previous version of the application is still serving.
- A destructive change - dropping a column or table, narrowing a type, adding a `NOT NULL` column with no default - MUST be split across releases: add the new, write both, backfill, stop reading the old, then drop.
- A data migration MUST be idempotent and re-runnable, for the same reason a command is (§4).
- A migration MUST NOT contain business logic and MUST NOT call application code. ⚙ BE-88
- Seed data MUST be limited to reference data the domain cannot function without. Demo records and test fixtures MUST NOT be seeded by a migration.

### Repositories and the aggregate graph

- A repository serves exactly one aggregate root. Its interface is declared in `Domain` beside the root it serves; its implementation lives in `Infrastructure` and depends on the module's `DbContext`.
- A repository MUST load the complete aggregate in one operation - the root, its entities, and its value objects. Partial loads are forbidden: an incompletely loaded aggregate cannot enforce its own invariants.
- The repository owns the graph shape. A command handler MUST NOT specify includes, projections, or tracking behavior, and MUST NOT know how the aggregate is assembled.
- Persistence is whole-aggregate. Add, update, and remove operate on the root; the aggregate's internals are persisted and deleted with it.
- A repository MUST NOT expose `IQueryable`, `DbSet`, or any other EF type. ⚙ BE-40
- A generic `IRepository<TRoot, TId>` MAY provide the universal operations: get by id, add, remove. Anything beyond that MUST be declared on a per-aggregate interface with an intention-revealing name.
- Command handlers fetch through the repository, mutate through aggregate methods, and persist through the repository. There is no other write path. ⚙ BE-41

### EF Core mapping for DDD

- Value Objects MUST be mapped as owned types or complex types, never as entities with their own identity.
- Strongly-typed ids MUST be mapped with value converters.
- Private collections MUST be mapped to their backing field.
- Navigation properties between aggregate roots are forbidden. One aggregate references another by id only. ⚙ BE-42
- Domain types MUST NOT acquire public parameterless constructors, public setters, or EF attributes to satisfy the ORM. Use EF's private-constructor and backing-field support. **If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type.** ⚙ BE-43

### Redis

*Redis* is in `references/annex-caching.md`, which binds once the tree meets its trigger: the first code that reads or writes a cache - a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other.

### Read-side caching

*Read-side caching* is in `references/annex-caching.md`, with *Redis*, which binds once the tree meets its trigger: the first code that reads or writes a cache - a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other.

## 6. Authorization

Every dispatchable operation is declared once, in the slice that owns it.

```csharp
// Illustrative.
public static readonly AgentOperation <Operation> = new(
    Id:     "<module>.<operation>",
    Policy: Policies.<Module>.Write,
    Kind:   OperationKind.Command,
    Transaction: TransactionRequirement.Required,   // None: an external write (§4)
    Human:  HumanRequirement.Required);   // unreachable without a live user
```

- Every Command and Query MUST declare a named policy. Bare `[Authorize]` and unattributed endpoints are defects. ⚙ BE-50
- `AuthorizationOptions.FallbackPolicy` MUST deny, so a forgotten attribute fails closed. ⚙ BE-51
- Policies are evaluated by the authorization behavior via `IAuthorizationService`, against the explicit principal from the dispatch context.
- The object check (core §7) is made after the load and before the handler acts, through a resource-authorization port declared in `Platform.Application`: it takes the resource and the policy, and reads the acting principal from the dispatch context. Its implementation calls `IAuthorizationService.AuthorizeAsync(principal, resource, policy)`. Each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its aggregates - live in its `Infrastructure` as framework extension points (BE-34), read the aggregate through its public members, and are registered by its registration entry point.
- A query applies the principal's record scope in its read - a global query filter on its read types, or a predicate in its SQL - never by filtering the result. The scope - the records the principal may see, such as its tenant's or its own - is resolved at the composition root, or by the authority adapter, into the dispatch context, and the read takes it from there.
- A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor package's equivalent - MAY enforce in addition, but MUST NOT be the only enforcement point. Every operation's policy decision MUST be reachable from the authorization behavior, so a caller that bypasses HTTP receives the identical answer (core §7).
- The acting principal MUST travel in the dispatch context. `IHttpContextAccessor` MUST NOT be referenced outside the composition root. ⚙ BE-52
- Identity-provider claims are mapped to policies at the composition root, and externally-sourced permissions in the adapter that wraps the authority (below). Application and domain code MUST NOT read raw claims. ⚙ BE-53
- A claim read from a token before its signature is validated MAY select how the token is validated - such as which issuer's keys, or which remote validation endpoint - and MUST NOT be trusted for anything else: not identity, not tenancy, not a permission lookup. Once the token is validated, the claim that made the selection MUST agree with the validated token, or the token is refused.
- Where permissions come from - a token claim, a directory, a remote authorization service - is a composition-root concern. Application and domain code name a policy, never a permission source or a role literal.
- An external authority is reached through one Infrastructure adapter, behind a port, which wraps the authority's client and declares its SDK. The adapter translates the authority's permission codes to the application's policy vocabulary, and translates its failures into outcomes (core §6). The authority's types and codes MUST NOT be referenced outside that adapter's project. ⚙ BE-102

### External authorities

A token claim cannot be unavailable, and neither can a signature checked against keys the process holds. A remote service that validates the token or answers for permissions can be unreachable, its answers can outlive a revocation, and a permission service answers for the user when the caller is an agent. The rules below cover the first two cases, and key the third on the acting principal; the agent case itself is in `references/annex-agents.md`. Which provider and which authority the organization runs is recorded in `DECISIONS.md`, not here.

- When a request depends on a source outside the process - one that validates the token, or one that answers for permissions - that source's unavailability MUST deny. An unreachable, timing-out, or erroring authority MUST NOT grant, MUST NOT accept the token unvalidated, and MUST NOT fall back to a default permission set.
- An external authority's decision - a validation result or a permission decision - MAY be cached, keyed by the acting principal. Its lifetime MUST be bounded and declared in an options class, and that value is the revocation window: a permission withdrawn at the authority continues to grant for that long. ⚙ BE-100 The lifetime SHOULD be configurable. Where the authority's client caches for a fixed period it does not expose, the declared value states that period and MUST NOT differ from it. A lifetime nobody can state is a defect.
- An unexpired entry MAY be served while the authority is unreachable. An expired entry MUST NOT be served, and its lifetime MUST NOT be extended because the authority cannot be reached. An outage never lengthens the revocation window.
- Externally-resolved permissions are the user's permissions, not the caller's. The lookup MUST be keyed on the acting principal from the dispatch context, never on the HTTP request. Their intersection with an agent's manifest is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

## 7. Data Classification

Core §8 defines the classes and the boundaries. This section is the mechanism.

### Declaring a class

- A class is declared on the type that carries the value - the Value Object or the strongly-typed id in `Domain` - and on the module's published type for the same concept in `Contracts`. `Domain` and `Contracts` are peers (§2), so neither can inherit the other's declaration, and an architecture test MUST prove the two agree. Every other type that carries the value - a Response DTO, a message, a log event - inherits the class of the type it carries and MUST NOT redeclare it. A property whose type carries no class of its own - such as a page size, a cursor, a computed flag, a field of a `Platform` type, or a value projected from a classified source - declares its class on the property. Where it is projected from a classified source, such as a column a query handler reads into a `string`, the declaration names that source, and the same test proves the two agree. The attribute lives in `SharedKernel`, so `Domain` can carry it without acquiring a framework dependency. ⚙ BE-54
- An architecture test MUST assert that every property reachable from an egress boundary resolves to a class. A property whose type carries no declaration is `Restricted`, and fails at the boundary. ⚙ BE-55
- The class MUST be emitted into the OpenAPI document as `x-data-class` (core §4 *Wire contract*) on every property of every schema the document contains - Request and Response DTOs, problem details, the page - and on the schema of every parameter, so it reaches every client with the contract. The emitter MUST fail rather than emit a property whose class it cannot resolve. ⚙ BE-83

```csharp
// Illustrative.
[DataClass(DataClass.Restricted)]
public sealed record NationalId
{
    public override string ToString() => "***";   // never the raw value
}
```

- A `Confidential` or `Restricted` type MUST redact in `ToString()` and MUST NOT define an implicit conversion to `string`. ⚙ BE-56

### Logging

- Structured logging only. A message template MUST NOT interpolate a classified value - a destructuring policy cannot redact what interpolation has already flattened into a string. ⚙ BE-57
- The destructuring policy redacts by class and MUST fail closed: a type it cannot resolve is redacted, not written.
- Log the identifier of a record, never its sensitive contents. `PersonId` is loggable; `NationalId` is not.
- Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a classified value. core §6 forbids leaking internals; this extends it to the data.

### The model-visible surface

*The model-visible surface* is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

### Cache, events, and storage

The cache rules of this subsection are in `references/annex-caching.md`, which binds once the tree meets its trigger: the first code that reads or writes a cache - a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other.

- Integration events and outbox rows MUST NOT carry `Restricted` payloads. Reference by id. ⚙ BE-61
- `Restricted` columns MUST be encrypted at rest. The encryption MUST live in Infrastructure - in the mapping, or below it in the database driver, such as SQL Server Always Encrypted - and MUST be transparent to every path that reads the column. A read type that maps an encrypted column carries the same converter, and SQL (§4) MUST NOT read a column encrypted in the mapping. Driver-level encryption limits what the database can do with the column - deterministic encryption allows equality only, randomized encryption allows nothing, and range, pattern and sort operations need a secure enclave where the server version supports one - so a column MUST NOT be encrypted in a form that does not support every operation a query performs on it, such as the range comparison keyset paging makes on its sort key (§4). The domain type MUST NOT know it is encrypted, for the same reason it MUST NOT know it is persisted (§5).

## 8. AI Agents

§8 is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other. An agent manifest is the one list of operations an agent may invoke.

## 9. Quotas and Rate Limits

Limits attach to the caller and to the run, not to the transport. HTTP rate limiting does not bound an agent, because agent dispatch never passes through the middleware pipeline. Both are required, and neither substitutes for the other.

### Transport

- The global rate limiter MUST be configured with a default policy covering every endpoint. An endpoint outside a policy is a defect, exactly as an endpoint outside an authorization policy is. ⚙ BE-70
- Limits are keyed on the acting principal, falling back to a client identity for anonymous endpoints. Keying on IP alone is not a limit; it is a shared bucket that one customer's office fills.
- Write endpoints and expensive reads MUST declare a tighter policy than the default.
- A rejection is shaped as core §4 *Limits* states.

### Agent runs

*Agent runs* is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

### Configuration

- Limits and budgets MUST be configuration, not constants in code, and MUST fail closed: a missing or unparseable limit denies rather than admits. ⚙ BE-73
- Every limit MUST emit a metric when it is hit.

## 10. Observability

Core §9 governs. This is the mechanism. What may be emitted is §7's business, not this section's: classification applies to every log line, metric label, and trace attribute.

### Correlation

- The correlation id MUST live in the dispatch context beside the principal, as a required constructor parameter. ⚙ BE-90 Its value is the current `Activity`'s trace id; the dispatch context and the `Activity` are one source, not two.
- An inbound request adopts or starts its trace as core §9 states. Every problem details response carries the trace id as `traceId`, written from `Activity.Current.TraceId`, not from ASP.NET Core's default (`Activity.Id` or `TraceIdentifier`) (core §4 *Wire contract*).
- Where requests from outside the trust boundary reach the application directly, with no ingress in front of it that strips the caller's `traceparent`, the application adopts no caller's: the composition root installs a `DistributedContextPropagator` that starts every request's trace, and the caller's `traceparent` is recorded as a link.
- An outbox row MUST persist the trace context - `traceparent` and `tracestate` - of the transaction that wrote it. ⚙ BE-91
- The dispatcher MUST restore that trace context when delivering.
- A background job MUST start a trace per execution and log its trace id once at start.

The agent rule of this subsection is in `references/annex-agents.md`, which binds once the tree meets its trigger: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.

### Logging

- Serilog, structured. The logging behavior (§4) is the only place a message's dispatch is logged. A handler MUST NOT reference `ILogger`. ⚙ BE-93
- Levels have fixed meanings. `Error` is a defect that needs someone. `Warning` is degraded but handled. `Information` is a business-meaningful event. `Debug` is developer detail and MUST NOT be enabled in production.
- An expected business rejection MUST NOT be logged at `Error`. A `Result` failure is an outcome, not a fault, and logging it as one teaches everybody to ignore the error log.
- Module, message type, principal id, and correlation id are supplied by the enricher. A call site MUST NOT restate them.

### Metrics and traces

- OpenTelemetry. One correlation identity across logs, metrics, and traces, and emission in every environment: core §9.
- An OpenTelemetry processor, and its equivalent in the logging pipeline, removes `exception.message` from every exception whose type the solution does not declare, keeping `exception.type` and the stack trace (core §8). Automatic instrumentation passes through the same processors.
- Every pipeline behavior MUST emit a span. Authorization, validation, and handler time MUST be separable in a trace: "the request was slow" is not a finding anyone can act on.
- Metrics MUST cover dispatch count and duration per message type, authorization denials, validation failures, outbox depth and delivery lag, cache hit rate, limit and budget rejections (§9), and agent run duration and spend.
- Metric dimensions MUST come from a declared closed set of keys. ⚙ BE-94
- Metrics are exported with exemplars, sampled by the trace (`ExemplarFilterType.TraceBased`); the SDK's default is off.
- Liveness MUST NOT depend on a downstream - a failing dependency restarts nothing. Readiness MUST check the database and the outbox dispatcher.

### Audit trail

- The audit trail (core §9) is a store no application role may update or delete, written through one port, `IAuditTrail`, declared in `Platform.Application` and implemented in `Platform.Infrastructure`.
- A persisting command's success is recorded through its unit of work, as the outbox row is, so the record commits with the change. Every other record is written through the port outside any unit of work: a denial, whether by the policy or by the object check; a command's failure, written after its rollback; a command that does not persist (§4), through a context or connection of the port's own, never the module's; and a read of `Restricted` data.
- A failure to write an audit record fails the operation it records. A denial stays a denial.

## 11. Configuration and Secrets

- Configuration binds to strongly-typed options classes. Outside the composition root, code MUST NOT read configuration from its source - `IConfiguration`, a process environment variable, or any other. ⚙ BE-95 It receives a bound options class, which is what makes BE-96's validation reach every setting.
- Every options class MUST have a validator and MUST be registered with `ValidateOnStart`. ⚙ BE-96
- An options default MUST NOT silently weaken a control. A missing limit, timeout, or policy denies (§9); it never falls back to permissive.
- Secrets come from Key Vault through managed identity in every environment above local development. A secret MUST NOT appear in any committed file. ⚙ BE-97 A secret is any value that grants access on its own: connection strings, client secrets, signing keys, API keys and package-feed tokens are examples of the class, not the list of it, and `appsettings*.json`, environment files and source are where it has happened, not where the rule applies. A key generated for a test and trusted by nothing outside it is not a secret. A committed file that needs a credential MUST reference it - an environment-variable macro such as `nuget.config`'s `%NUGET_PAT%`, or a credential provider - and MUST NOT carry a placeholder to be overwritten in place.
- Local development uses user secrets. A developer MUST NOT need a production credential to run the application.
- Configuration that differs by environment MUST differ by value, never by code path. Outside the composition root, code MUST NOT learn which environment it is running in, by any means. ⚙ BE-98 Where the environment's name belongs in telemetry, the composition root puts it there - a resource attribute, a log enricher - so nothing below it needs to ask.
- A feature flag is configuration: typed, validated, and removable.

## 12. Real-time

§12 is in `references/annex-realtime.md`, which binds once the tree meets its trigger: the first code that pushes messages to a client over a persistent connection - a SignalR hub, server-sent events, a raw WebSocket, or any other.

## 13. API Layer

- Every endpoint MUST be a type implementing the shared endpoint abstraction, with its Request and Response DTOs as type arguments. A route handler passed as a lambda or method group to a minimal-API route builder - `MapGet`, `MapPost`, `MapMethods`, or any other - is forbidden. ⚙ BE-80
- Endpoints are registered by discovery over those types at the composition root. Hand-registering one is a defect. ⚙ BE-81
- An endpoint MUST map its Request DTO to a message, dispatch it, and translate `Result` to HTTP. It MUST NOT reference any type other than its DTOs, the Application message it dispatches, and every type those are composed of; the shared endpoint abstraction; the dispatcher and the `Result` it returns; the result translator; and the framework's primitives, such as `Task`, `CancellationToken`, and HTTP result types. ⚙ BE-82 An endpoint branching on business state is a defect.
- Endpoints MUST NOT reference domain types. ⚙ BE-77
- Success shapes: `200` with a Response DTO, `201` with a location, `204` for no content - except a write to a versioned resource, which returns a body carrying the new `etag` - or a stream the endpoint declares. A write to a versioned resource follows core §4 *Wire contract*: `If-Match` required, `428` and `412`, and the target resource's new `etag` in the body. Failures are `ProblemDetails`.
- An endpoint maps `If-Match` to the message's expected version, and `If-None-Match: *` to the message's create-only flag, as it maps any other request field. The repository's load reports a stale expected version as an outcome, which the handler translates to a `Result` precondition failure before it acts. The repository also uses the expected version as the concurrency token's original value, so a version that changes between load and commit surfaces when the unit of work commits. For a create-only message, a violation of the aggregate id's unique index at commit is a precondition failure, not a conflict. The transaction behavior returns a commit-time precondition failure as a `Result` precondition failure, distinct from a business conflict, and the result translator maps every precondition failure to `412`.
- Every problem type a module returns is a URI the module declares, with its status, title and whether it is user-facing; the OpenAPI document's `x-problem-types` is generated from those declarations (core §4 *Wire contract*). Validation errors are keyed by request paths (core §4 *Wire contract*); the result translator maps each validation failure's message path to the request path it came from: a message property keeps its request path unless the endpoint's mapping declares a different one. `Platform` declares the problem types the pipeline and the framework return - validation, authorization, idempotency, preconditions, rate limits, unsupported media, and any other - and overrides ASP.NET Core's per-status defaults with them through `CustomizeProblemDetails`.
- Where the client is on another origin, the CORS policy admits the request headers the wire contract uses - `traceparent`, `tracestate`, `If-Match`, `If-None-Match`, `Idempotency-Key`, or any other it adds - and exposes the response headers a client reads, such as `ETag`, `Location` and `Retry-After`.
- The endpoint registrar sets `Cache-Control: no-store` on every endpoint whose Response DTO resolves to `Confidential` or `Restricted` (BE-55), and `Cache-Control: private` on every other endpoint whose representation depends on the principal (core §4 *Wire contract*). Which endpoints depend on the principal is reached by review.
- An upload endpoint's Request DTO carries the file as a stream with its metadata, and its size limit is enforced at the request boundary - the endpoint's request size limit - before the body is buffered. It dispatches a command that does not persist to store the file in quarantine, under a name the server derives from the command's client-supplied id - an id the validation behavior has parsed, never the client's file name (§4). A persisting command, run under the uploader's principal recorded with the quarantined file, records it once the scan passes (core §7 *File uploads*).
- A change to a Request or Response DTO that does not regenerate and commit the OpenAPI document is incomplete.

## 14. Testing

A rule no build check can reach is carried by a named required test below, where a test can prove it, rather than by a `⚙` it cannot honour; otherwise review carries it (core §1). A missing required test is a defect exactly as a violated rule is. The required tests of a rule in an annex are in that annex's own Testing section.

- **Domain**: pure unit tests, no substitutes, no I/O. Every invariant and every domain outcome MUST have a test.
- **Application**: handler tests with substituted infrastructure. Every declared policy MUST have a test proving *denial*, not only one proving success.
- **Integration**: real SQL Server via Testcontainers, real pipeline behaviors.
- **Queries**: every query handler that reads with SQL MUST have a test against real SQL Server proving its SQL projects into its Response DTO and applies the module's query filters (BE-112).
- **Transactions**: a command that runs without a transaction MUST have a test proving that a staged database change fails the dispatch and is never committed (BE-111).
- **Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped or marked inconclusive to unblock a change.
- **Authorization**: every external authority MUST have a test proving that an unreachable, a timing-out, and an erroring authority each deny, and that an expired cache entry is not served while the authority is unreachable (BE-99). Every token validation that reads a claim before the signature is checked MUST have a test proving that a token whose selecting claims disagree with its validated issuer or endpoint is refused, and that no claim read before validation reaches the principal (BE-104). Every policy MUST have a test proving an in-process dispatch with no HTTP context receives the same decision as the HTTP path (BE-101). Every operation that acts on a record the caller identifies, unless its policy grants every record, MUST have a test proving a principal that holds the policy but not the record is denied, and every query a test proving it returns no record the principal may not see. (BE-117)
- **Audit**: every persisting command MUST have a test proving its success record commits with its change and its failure record is written after rollback; every command that does not persist, a test proving its record is written; and the authorization behavior and the resource-authorization port, a test proving a denial is recorded. (BE-118)
- **Pagination**: every paginated query MUST have a test proving a request above its maximum is rejected, and a test proving no row is skipped or repeated across a page boundary.
- **Idempotency**: every command declaring a key MUST have a test proving a replayed key returns the recorded result without executing twice, and a test proving two concurrent attempts under one key resolve as one execution and one replay rather than an error (BE-29), and a test proving the same key with a different payload is refused with `422` (BE-116).
- **Concurrency**: every versioned write MUST have a test proving a write without `If-Match` is refused with `428`, and one with a stale version with `412`, that `If-Match: *` is refused with `428`, and that a create with `If-None-Match: *` for an existing id is refused with `412`. (BE-115)
- **Uploads**: every upload endpoint MUST have a test proving a file whose content does not match its declared type is refused, a file above the maximum size is refused, and a stored file is not served before its scan passes. (BE-120)
- **Correlation**: the outbox MUST have a test proving the trace context survives the round trip - written with the transaction, restored on delivery, present on the consumer's log line (BE-92).
- **Migrations**: every migration MUST be applied to a Testcontainers database in CI and proven to run clean twice. A destructive migration MUST additionally have a test proving the previous application version still serves against the new schema (BE-89).
- **Classification**: every `Confidential` and `Restricted` type MUST have a test proving it redacts in `ToString()` and in a serialized log event.
- Test names state the rule: `<Operation>_Returns<Rejection>_When<Condition>`.
- Tests MUST NOT assert on implementation details - no verifying private calls, no asserting on generated SQL.

## 15. Forbidden

`dynamic` · service location outside the composition root, except that a hosted service MAY create a scope through `IServiceScopeFactory` and resolve its per-iteration dependencies from it · static mutable state · reading the system clock in domain or application code - `DateTime.Now`, `DateTimeOffset.UtcNow`, `DateTime.Today`, or any other ambient read (inject `TimeProvider`) · `async void` outside event handlers · `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` · catching `Exception` without rethrowing, except at a declared fault-isolation boundary - one iteration of a hosted-service loop, one outbox delivery, or any other unit whose failure must not stop the host - which is declared by a marker on the method, MUST log at `Error` with the exception, within core §8's telemetry rule, and MUST let `OperationCanceledException` propagate on shutdown · `#region` · commented-out code

## 16. Verification

`dotnet build` (warnings are errors) and `dotnet test` (includes architecture tests). Both MUST pass before a change is complete.
