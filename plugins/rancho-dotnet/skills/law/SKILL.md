---
name: law
description: The .NET server stack law - project references, domain and application layers, EF Core persistence, authorization, data classification, AI agents, quotas, observability, configuration, real-time, and the API layer. Subordinate to the core law in /rancho:law. Read in full, after the core law, before writing, reviewing, or placing code in a tree whose CLAUDE.md names `Stack law: /rancho-dotnet:law`.
---

# .NET Stack Law

Read the core law, `/rancho:law`, first; it governs. This stack law (core §2) adds the .NET rules and carries the core law's mechanism in every tree bound to it. Core §N is the core law's §N. `⚙` = can be made to fail the build (core §1). `references/enforcement.md` inventories every `⚙` here and every core rule this role carries: consult it by rule id rather than reading it whole.

Three annexes, beside the inventory in `references/`, hold rules that bind a tree only once the tree meets the annex's trigger:

- `annex-agents.md`: the first code that sends data to a model or exposes an operation to one as a tool - an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other.
- `annex-caching.md`: the first code that reads or writes a cache - a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other.
- `annex-realtime.md`: the first code that pushes messages to a client over a persistent connection - a SignalR hub, server-sent events, a raw WebSocket, or any other.

Each is named where its rules would otherwise stand. Once the tree meets a trigger, the annex binds the whole tree, exactly as this file does.

A tree is bound to this stack law by one line in the `CLAUDE.md` at its root:

```
Stack law: /rancho-dotnet:law
```

If `/rancho:law` is not available in the session, stop and say so. This file alone is not the law.

## Role

`server`. A tree bound here publishes the API contract (the contract law, `references/contract.md` in `/rancho:law`), owns its modules and their persistence, and owns every egress boundary in core §4.

## Core map

| Core | Carried here by |
|---|---|
| §3 Access | §6 Authorization · §4 Pipeline behaviors (validation runs on the message) · §8 AI Agents · §11 Configuration and Secrets · §13 API Layer (uploads) |
| §4 Data Boundaries | §7 Data Classification, with its egress owners · §5 Read-side caching · §12 Real-time |
| §5 Observability | §10 Observability, with its audit trail |
| §6 Verification | §14 Testing · §16 Verification · `references/enforcement.md` |
| Contract law | §13 API Layer · §4 Pagination, Idempotency · §9 Transport · §7 Data Classification (`x-data-class`) · §10 Observability (*Correlation*) |

Gaps: none.

## Layout

Paths are relative to the tree root - the directory whose `CLAUDE.md` binds this stack law.

```
src/
  Modules/<Module>/
    <Module>.Domain/          BCL and `Microsoft.Extensions.Compliance.Abstractions` only. No other framework reference.
    <Module>.Application/     Commands, queries, handlers, behaviors.
    <Module>.Infrastructure/  EF Core, adapters, DbContext.
    <Module>.Contracts/       Public surface. The only project other modules may reference.
  Shared/SharedKernel/        Base types only. No business rules.
  Api/                        Composition root, endpoints, auth wiring.
tests/
```

§2 states what each project may reference.

## 1. Stack

.NET 10 (LTS) · EF Core · SQL Server · Redis · SignalR · Mediator 3.x, source-generated · FluentValidation · Serilog · Microsoft.Extensions.Compliance · OpenTelemetry · Azure Key Vault · Microsoft Agent Framework (agents annex) · xUnit v3 · NSubstitute · Shouldly · Testcontainers · ArchUnitNET

- `TreatWarningsAsErrors` and `Nullable` are enabled solution-wide, once in `Directory.Build.props`, never per project, and MUST stay enabled.
- Suppressing a warning requires a comment naming the rule and the reason. Where the warning enforces a rule of the law, the reason is its exceptions-register entry's id (core §7).
- Verify third-party licenses before pinning.
- Create solutions, projects, project references and package references with the `dotnet` CLI, never by writing the file:
  - `dotnet new sln`, which writes an `.slnx` file.
  - `dotnet new` with the project's template, such as `classlib`, `webapi`, or xUnit v3's `xunit3` from `xunit.v3.templates`.
  - `dotnet sln add` and `dotnet reference add`.
  - `dotnet package add`, which resolves the version from the configured package sources.
- Edit a project file by hand only for what the CLI cannot express, such as the project's kind or an analyzer reference's assets (§2).

## 2. Project Reference Law

Four facts decide every reference. The table below is their consequence, not a second set of rules:

1. A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference a module or `Api`.
2. A module reaches another module only through that module's `Contracts`.
3. Inside a module, dependencies point inward: `Infrastructure` to `Application` to `Domain` and `Contracts`. `Domain` and `Contracts` are peers and MUST NOT reference each other.
4. `Api` composes; nothing references `Api`.

| Project | Projects it may reference | Packages it may declare |
|---|---|---|
| `SharedKernel` | none | the BCL, and `Microsoft.Extensions.Compliance.Abstractions` |
| `<Module>.Domain` | `SharedKernel` | the BCL, and `Microsoft.Extensions.Compliance.Abstractions` |
| `Platform.Application` | `SharedKernel` | dispatch, validation, authorization and logging abstractions - `Mediator.Abstractions`, `FluentValidation`, `Microsoft.AspNetCore.Authorization`; `Microsoft.Extensions.*.Abstractions` |
| `<Module>.Contracts` | `SharedKernel`, `Platform.Application` | none of its own |
| `<Module>.Application` | own Domain + Contracts, other modules' Contracts, `Platform.Application` | validation; `Microsoft.Extensions.*.Abstractions` |
| `Platform.Infrastructure` | `SharedKernel`, `Platform.Application` | EF Core, messaging, telemetry instrumentation, ASP.NET Core Data Protection, ASP.NET Core authorization, the SDK of any external system it adapts; `Microsoft.Extensions.*.Abstractions`; `Microsoft.Extensions.Options` with its configuration extensions |
| `<Module>.Infrastructure` | own Application, Domain, Contracts, `Platform.Infrastructure` | EF Core, messaging, ASP.NET Core authorization, the SDK of any external system it adapts; `Microsoft.Extensions.*.Abstractions`; `Microsoft.Extensions.Options` with its configuration extensions |
| `Api` | any `*.Contracts`, any `<Module>.Application`, any `<Module>.Infrastructure`, `Platform.Infrastructure`, `Platform.Application`, `SharedKernel` | ASP.NET Core, hosting, OpenAPI, the dispatcher's source generator, telemetry exporters, the logging provider and `Microsoft.Extensions.Telemetry` with `Microsoft.Extensions.Compliance.Redaction`, EF Core's design-time tooling as a private asset - never a package an Infrastructure project declares |
| `<Anything>.Tests` | any project | test packages |

- Every project MUST declare its kind - one of the rows above - as an MSBuild property. A project whose kind is missing, misspelled or not in the table may reference nothing and be referenced by nothing. A new kind of project needs a new row, derived from the four facts. ⚙ BE-01
- The table allowlists **direct** `ProjectReference` entries, not the transitive closure. A project MAY declare a reference it could also reach transitively; it MUST NOT declare one its row omits. ⚙ BE-01
- A test project MUST NOT be referenced by a project of any other kind. A fixture built to prove a check fails MUST NOT appear in the solution file or under the production source root.
- An analyzer reference - one that contributes analyzers only, and no assembly the project compiles against or runs with - is admitted in every project and is outside the table. A package reference qualifies with `IncludeAssets="analyzers"` and `PrivateAssets="all"`; a project reference with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"`.
- Outside `Api`, a project references no ASP.NET Core type beyond the packages its row admits, even through a transitive reference: `HttpContext`, `IHttpContextAccessor` and every other transport type stay in `Api`. A module `Application` uses no data-access API, `System.Data` included, and does not reference the unit-of-work port. ⚙ BE-01
- `Platform.*` carries the dispatch pipeline: `Result`, the message abstractions, the solution-wide page shape and cursor type, and the behaviors. `Result` lives in `Platform.Application`, not in `SharedKernel`.
- `Contracts` references `Platform.Application`: a published query message MUST name both its `Result` response and the abstraction it is dispatched by. Nothing else from `Platform.Application` may appear in a contract's public surface, except the page and cursor types a published collection query returns.
- `Api` names only the registration entry points of Infrastructure projects. Each module's Infrastructure exposes exactly one, `Add<Module>Module(IServiceCollection, IConfiguration)`; `Platform.Infrastructure` exposes one `Add<Feature>` extension per platform feature, such as the outbox or correlation logging. `Api` MUST NOT name any other Infrastructure type - no client, credential, `DbContext`, repository, adapter, SDK type, or any other - and after startup MUST NOT name or invoke an Infrastructure type directly: dispatch reaches an Infrastructure handler through the dispatcher, not through `Api`. Each Infrastructure project builds and registers its own clients, its `DbContext` included. ⚙ BE-105, BE-106 Code the dispatcher's source generator emits into `Api` is outside this rule.
- The **composition root** is `Api` plus the registration entry points it calls. Wherever this law confines something to it - reading configuration (§11), learning the environment (§11), resolving services from the container (§15), registering decorators (§5) - both are admitted, nothing else.

A module is a Bounded Context. Its projects exist **at most once per module**, never per feature:

- a module with no model of its own omits `Domain`;
- a module that publishes nothing - to its siblings or over the wire - omits `Contracts`;
- `Application` is what makes a module dispatchable at all.

A module has one Domain, shared by all of its features: features are use cases over that model, not copies of it. Vertical slicing applies to the **Application project only**.

| Project | Organized by | Contains |
|---|---|---|
| `Domain` | Aggregate | `<Aggregate>/` - root, its entities, value objects, outcomes, domain events, specifications, repository port |
| `Application` | Feature (use case) | `<Feature>/` - command or query, handler, validator, response DTO, operation declaration |
| `Infrastructure` | Technical concern | `Persistence/`, `Queries/`, `Messaging/`, `External/` |
| `Contracts` | Published surface | `IntegrationEvents/`, `Queries/`, and the module's wire ids (§3) |
| `Platform.Application` | Technical concern | `Abstractions/`, `Behaviors/` |
| `Platform.Infrastructure` | Technical concern | `Persistence/` - including the audit trail and the cursor codec - and `Messaging/` |

- Grouping by pattern is forbidden: no `Application/Handlers/`, `Application/Commands/`, `Domain/Entities/` or `Domain/ValueObjects/`. ⚙ BE-03
- Duplicating an aggregate across feature folders inside one module is a defect.
- Domain event reactions are Application slices named for what they do: `Application/<Reaction>/`. A reaction loads an aggregate through its repository and calls its methods, as a command handler does, inside the command's unit of work. It handles no message and calls no handler.

## 3. Domain Layer

### Aggregates

- Aggregate state MUST be private: no public setters; collections exposed as `IReadOnlyCollection<T>` over a private backing field. ⚙ BE-04
- State MUST change only through methods named in domain vocabulary: `Approve` is the rule, `SetStatus` a defect.
- An aggregate MUST be valid at every instant of its public lifetime. Constructors and methods enforce invariants. No "validate later" step, no `IsValid` property.
- An aggregate method returns a domain outcome, a domain value, or nothing.
- Expected business rejections MUST be domain outcome values - an enum or sealed outcome type in domain vocabulary - and MUST NOT be exceptions.
- Invariant breaches MUST throw.
- Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O. ⚙ BE-06
- An aggregate root MUST be marked as one at the type level. ⚙ BE-78
- An aggregate MUST NOT hold a reference to another aggregate, a navigation property between aggregate roots included. It holds that aggregate's id. ⚙ BE-79

```csharp
// Illustrative.
public <Outcome> <Operation>(<Args> args, DateTimeOffset now)
{
    if (<business condition>) return <Outcome>.<Rejection>;   // expected: a value
    ArgumentNullException.ThrowIfNull(args);                  // impossible: a throw

    <mutate private state>;
    Raise(new <Aggregate><Operation>ed(Id, now));
    return <Outcome>.<Success>;
}
```

### Value Objects

- Primitives in aggregates and entities SHOULD be Value Objects wherever the primitive carries meaning or constraints: a `string` email becomes `EmailAddress`.
- Value Objects MUST be immutable, MUST validate in their constructor, MUST have structural equality, and MUST be impossible to construct in an invalid state.
- Every identifier MUST be a strongly-typed id, one type per concept, so passing the wrong id does not compile.
- No public member of `Domain`, or of a command or query message, exposes a bare `Guid`, except a strongly-typed id's own constructor, its value member, and its compiler-generated members, such as a positional record's `Deconstruct`. ⚙ BE-08 `int` stays permitted for counts and quantities.
- Command and query messages and Response DTOs MUST NOT reference a `Domain` type. ⚙ BE-109 An id they carry is the module's published id type from `Contracts`, the one wire id for that concept; the handler converts it, and every other wire value, into domain values. So a module with an endpoint has a `Contracts` project.

### Entities and Domain Events

- Entities inside an aggregate MUST be reachable and modifiable only through the root.
- Domain events MUST be raised by the aggregate and recorded on it. A handler MUST NOT raise a domain event.
- The unit of work dispatches domain events to their in-module reactions before it commits, inside the same transaction.
- Domain events SHOULD be named in the past tense.
- Domain events MUST carry ids and values, never entity references. ⚙ BE-10
- Domain events are intra-module and MUST NOT leave the module. ⚙ BE-07

### Specifications

- Specifications MAY encapsulate reusable query predicates.
- A Specification MUST NOT be the authority on whether a mutation is legal: the aggregate method is. Never pre-check with a Specification and rely on the aggregate not to re-check.

## 4. Application Layer

### Commands and Queries

- A Command mutates. A Query reads. A Command MUST NOT return data beyond an identifier, its outcome, and the new version of the aggregate it changed (contract §7 *Wire contract*). ⚙ BE-11
- A command handler MUST change state only by loading an aggregate through its repository, calling its methods, and persisting through the repository.
- Queries:
  - A Query MUST NOT mutate: its handler MUST NOT call `SaveChangesAsync`. ⚙ BE-12
  - A query handler MUST bypass the domain and project straight into its Response DTO, untracked: it MUST NOT reference a `Domain` type, materialize an aggregate, or use an aggregate repository. ⚙ BE-108
  - The query message, its Response DTO, and the `IQueryHandler<,>` abstraction live in `Application`. **Every concrete query handler lives in `Infrastructure`, by convention**, a pure-computation one included. A command handler stays in `Application`. ⚙ BE-15
  - A query handler reads a relational database through keyless entity types its module's Infrastructure maps for reading, over a table or a view and queried with LINQ, or with parameterized SQL projected into the DTO, such as EF Core's `Database.SqlQuery<T>` with an interpolated string.
  - A read type carries the module's global query filters, such as a tenant or soft-delete filter. SQL bypasses them and MUST apply them itself; a handler that reads with SQL is covered by a test against a real database (§14).
- SQL MUST NOT be built from input by any means but the parameterized form. The raw-string APIs - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw`, or any other - MUST NOT be used in a module. ⚙ BE-113 A migration's SQL (§5) is written, not built from input, and is outside this rule.
- `Repository` is reserved for the aggregate write path. A read-side type that returns DTOs MUST NOT be called a repository: name it `<Feature>Reader`. Usually the query handler is the read data access, and no separate type is warranted. ⚙ BE-17
- One handler per message. Handlers MUST NOT call other handlers directly. Shared logic belongs in the domain or a named application service.
- Expected outcomes are `Result` values, never exceptions, and an exception is never control flow. Only an adapter MAY catch an exception its framework or SDK raises for an expected outcome - such as a concurrency conflict, a unique-key violation, or an unreachable or failing remote service - and it translates the exception into an outcome there. Nothing above the adapter catches one. Where the framework raises it at commit, the adapter is the unit of work, and the unit-of-work behavior translates its outcome (*Pipeline behaviors*, item 4). An exception that escapes a handler is a defect (§13).
- A change another module must make is that module's reaction to an integration event (*Transactions and consistency*). A handler MUST NOT dispatch another module's command or reach its write path inline.
- A handler MAY dispatch another module's published query through the dispatcher. That dispatch runs the full pipeline, on its own module's connection, never enlisted in the command's transaction.

```csharp
// Illustrative. Translation happens here and nowhere else: a switch expression over an enum outcome, with no discard arm.
return aggregate.<Operation>(args, now) switch
{
    <Outcome>.<Success>   => Result.Success(),
    <Outcome>.<Rejection> => Result.Conflict(<Module>Errors.<Rejection>),
};
```

Translation is a switch expression, never a switch statement.

- Over an enum outcome: write no discard arm. CS8524, raised for the enum's unnamed values, is disabled in the solution's build configuration with its reason (§1), so a missed member raises CS8509, an error under `TreatWarningsAsErrors` (§1).
- Where outcomes are types: end the switch in a discard arm that throws `UnreachableException`, and prove each outcome's translation by a test (§14).

### Pagination

- Every query returning a collection whose size is not bounded by construction MUST be paginated; one bounded by construction declares its bound (contract §2). A handler returning an unbounded collection, such as a `List<T>` or an `IEnumerable<T>`, is a defect. ⚙ BE-19 A declared stream (contract §7 *Wire contract*) is not a collection.
- A bounded set a UI grid shows in full is bounded by construction only where it declares its bound and rejects a request beyond it.
- One shared page shape for the whole solution. A per-feature paging envelope is a defect. ⚙ BE-20
- Every paginated query MUST declare a maximum page size, under a solution-wide ceiling. A request above the maximum MUST be rejected by the validation behavior. Silent clamping is forbidden. ⚙ BE-21, BE-22
- Omitting the page size MUST yield the operation's declared default, never "all".
- Ordering MUST be deterministic: a paginated query MUST declare it as a descriptor whose last term is a unique key, not only as an `OrderBy` chain. The executed `ORDER BY` and keyset predicate MUST be generated from the descriptor, never written beside it. ⚙ BE-23
- One cursor type solution-wide, whose payload is exactly the sort key and the id. ⚙ BE-24
- Cursors are opaque to the client (contract §7 *Wire contract*) and MUST be validated server-side.
- A cursor is encrypted and authenticated. It is encoded and decoded only through a cursor codec port declared in `Platform.Application` and implemented in `Platform.Infrastructure` with ASP.NET Core Data Protection, under a purpose per query. A cursor that fails to decode is a validation failure, never an exception that escapes. The key ring is persisted in the database through `PersistKeysToDbContext`, or in Blob storage with its keys protected by Key Vault, never in a cache, and is shared by every instance. ⚙ BE-119

Its agent rule: `annex-agents.md`.

### Pipeline behaviors

Fixed order. Changing it requires written justification.

1. **Logging / correlation**
2. **Authorization** - resolves the message's declared policy and evaluates it against the explicit principal. Every Command and Query MUST declare a named policy; a message with none MUST fail to dispatch. ⚙ BE-25
3. **Validation** - FluentValidation. A failure short-circuits to a `Result` failure; the handler is never invoked.
   - It runs on the message, so every caller gets it: an endpoint, an agent, a job, a test.
   - A Request DTO's annotations MAY repeat it, for early failure and for the OpenAPI document. They MUST NOT be the only validation.
   - A handler assumes its message is structurally valid, and MUST NOT re-validate its shape.
4. **Unit of work and idempotency** - commands only. One behavior.
   - A command that persists aggregate state: open the unit of work; then, if the command declares a key, resolve the attempt identity: where a record exists, answer with it as a replay, or `422` when the fingerprints differ, without invoking the handler.
     Where no record answers: run the handler, write the idempotency record where a key is used, dispatch domain events to their reactions, and commit on success. Roll back on any failure or exception.
   - After a successful commit: hand the committed domain events to their post-commit effects, such as cache invalidation. A failure there is logged as an error, and the committed result is still returned.
   - On any failure result - the handler's, or one at commit: look up the attempt identity first, as above, and answer with the record where one exists. Otherwise return the failure - a commit-time one as a `Result` conflict, or as a precondition failure for a stale expected version or a create-only unique-index violation (§13).
   - Where a key is used: a failure result the behavior returns is recorded after the rollback, in a transaction of its own on the module's context, so a replay returns it (contract §3). A violation of the unique index there is answered by the attempt identity, as above. An exception is not recorded.
   - A command that does not persist: open nothing. After the handler returns, fail the dispatch if the module's context holds a staged change.

### Idempotency

A command that executes twice on a retry is a defect in the command, not in the caller.

- Every command MUST declare its idempotency mechanism in its operation declaration (§6): naturally idempotent, client-supplied id, or idempotency key. An undeclared command MUST fail to dispatch, exactly as an undeclared policy does. ⚙ BE-26
- Where a key is used, the identity of an attempt is `(principal, operation, key)`. The same key with a different payload MUST fail with `422`, never overwrite (contract §3).
- The idempotency record MUST be written through the same unit of work as the state change. Redis MUST NOT be the store. ⚙ BE-27
- The store MUST carry a unique index on the attempt identity. ⚙ BE-28 The unit-of-work behavior answers a violation of that index at commit by the attempt identity, as it answers any failure result (§4 *Pipeline behaviors*, item 4) - never as a conflict.
- The idempotency record stores a fingerprint of the request payload: an HMAC under a server key, never a plain hash, and the fingerprint's class is `Internal`.
- Two concurrent attempts with the same key MUST resolve as one execution and one replay, never as a `500`.
- Records MUST be retained longer than the longest caller retry window, and swept afterwards.
- Idempotency is a pipeline behavior. A per-endpoint or per-handler check is a defect. ⚙ BE-30

Its agent rule: `annex-agents.md`.

### Transactions and consistency

- One transaction SHOULD modify exactly one aggregate. A second, changed by a domain-event reaction in the same transaction, needs the PR to name the invariant that requires it. Outbox rows, idempotency records and audit records are pipeline bookkeeping, not aggregates, and do not count against this rule.
- A transaction MUST NOT span a module boundary. ⚙ BE-32
- Integration events MUST be written to the outbox in the same transaction as the state change that caused them. Publishing directly from a handler is forbidden. ⚙ BE-33
- Every integration event handler MUST be idempotent. Delivery is at-least-once; assume redelivery.
- Handlers MUST NOT call external systems inside a transaction.
- A command declares, in its operation declaration (§6), whether it persists aggregate state. Only a command that does opens the unit of work. A command whose effect is an external write runs without a transaction, and MUST NOT use a key-based idempotency mechanism or the outbox. ⚙ BE-110 Such a command:
  - MUST NOT stage a database change - the unit-of-work behavior refuses one (§4 *Pipeline behaviors*, item 4; its test, §14);
  - is idempotent naturally or by a client-supplied id, such as a blob name derived from the aggregate id;
  - MAY load an aggregate to decide whether the write may proceed;
  - does not record the write's result: a separate command that persists does.
- An external effect a repeat would duplicate, such as sending an email or taking a payment, is not such a command: a persisting command records the intent and writes an outbox row, and the outbox delivery performs the effect, passing a row-derived idempotency key wherever the external system accepts one.

### Integration event contracts

An integration event is a published contract.

- Every integration event type MUST carry its version in its name: `<Event>V<N>`. ⚙ BE-84
- The published contract surface MUST be snapshotted and committed. A change to the shape of an existing version MUST fail the build, as a change to the OpenAPI document does. ⚙ BE-85
- Adding an optional field to an existing version is permitted. Renaming, removing, retyping, or changing the meaning of a field is not: that is a new version.
- Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes to it and no outbox row holds one. Both are published during the overlap.
- A consumer MUST tolerate fields it does not know and MUST NOT fail on one it does not use. Producers ship before consumers.
- An event with both audiences - a reaction in its own module and another module - is two types, a domain event and an integration event, never one shared type.
- Event payloads MUST be ids, primitives, and value objects that serialize stably. A domain type MUST NOT appear in an integration event. ⚙ BE-86

### Placement of interfaces and implementations

Placement is decided by **dependencies, never by the name**. `Service`, `Manager`, `Provider`, and `Client` are not signals.

| Interface | Implementation | What it is |
|---|---|---|
| `Domain` | `Domain` | Domain service - a pure business rule spanning aggregates, belonging to no single root |
| `Domain` | `Infrastructure` | Repository port, expressed in domain vocabulary |
| `Application` | `Application` | Use-case logic with no external dependency: strategies, policies, orchestrators, decision tables, mappers |
| `Application` | `Infrastructure` | Adapter - anything crossing a process boundary or naming a technology, including concrete query handlers |

- **The interface** goes in the layer that can express its signature in its own vocabulary. A port whose signature would force `Domain` to reference an application type MUST be declared in `Application`.
- **The implementation** - both tests must agree:
  1. Does it require a package or namespace `Application` may not reference? Then `Infrastructure`.
  2. Can it be unit tested with no test doubles for anything but domain types? Then `Application`.
- An implementation that is pure logic MUST live in `Application`, except a concrete query handler, which §4 *Commands and Queries* places in `Infrastructure` by convention.
- A real-time push adapter - the implementation of an `Application` port that sends to a hub's clients through `IHubContext<T>` - is implemented in `Api`, which hosts the hubs, not in `Infrastructure` (real-time annex).
- Application logic in `Infrastructure` is invisible to the build and MUST be caught in review.
- Every Infrastructure type MUST be an adapter, that is one of:
  - an implementation of a port declared in `Domain` or `Application`;
  - an implementation of a framework extension point, such as `IEntityTypeConfiguration<T>`, `IHostedService` or `DbContext`;
  - a registration entry point (§2);
  - a type the project maps or binds: an entity or read type in its model, such as the outbox row or the idempotency record, or an options class (§11).

  A free-standing Infrastructure service belonging to none of these is a defect. Infrastructure types are `internal`, except the registration entry points, any type the dispatcher's generated code in `Api` must name, and the types those expose in a public constructor or member - such as the module's `DbContext`, which a query handler's constructor takes, and the read types the `DbContext` exposes. The source-generated dispatcher names each concrete handler in its generated registration, so every concrete query handler is public. `InternalsVisibleTo` names only test assemblies and the test mocking library's proxy assembly, such as `DynamicProxyGenAssembly2`. ⚙ BE-34, BE-107
- Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/`, except the registration entry point, which sits at the project root. A new top-level folder there is added to the check's allowed set, with its justification in the PR. ⚙ BE-35
- `Infrastructure/Queries/<Feature>/` MUST mirror the feature folder name used in `Application`. ⚙ BE-36

## 5. Persistence

- EF Core, code-first (migrations: §5 *Migrations*). SQL Server is the default store; a second store requires a documented reason in the module's README.
- One `DbContext` per module, on the module's own database schema. It maps no type another module declares, reads no other module's schema, and declares no foreign key to another module's table: another module's record is referenced by its id only. ⚙ BE-37
- Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure; the domain MUST NOT know it is persisted. A Domain type MUST NOT carry a mapping attribute, EF attributes included; a mapped one MUST NOT have a public parameterless constructor or a public setter - use EF's private-constructor and backing-field support. ⚙ BE-38 If a mapping needs the domain type changed, the mapping is wrong, not the domain type.
- Lazy loading MUST be disabled.
- Integration tests run against real SQL Server via Testcontainers. No other provider may stand in for it - the EF InMemory provider, SQLite in memory, or any other. ⚙ BE-39
- Every aggregate MUST carry an application-managed version, mapped as its concurrency token, which the aggregate increments on every change to itself or to anything inside it.
- A concurrency conflict or a unique-key violation MUST surface as an outcome, never as an exception above the persistence adapter: the repository or unit of work that receives `DbUpdateConcurrencyException`, or the provider's unique-key error, catches only that, where it is raised, and translates it (§4 *Commands and Queries*, *Pipeline behaviors*).

### Migrations

- Every schema change ships as an EF Core migration, generated, reviewed, and committed with the change that requires it.
- A committed migration MUST NOT be edited. ⚙ BE-87 Correct it with a new migration.
- Migrations are forward-only. A down migration MUST NOT be relied on above local development: recovery is a new forward migration, written deliberately.
- A migration MUST be safe to run while the previous version of the application is still serving.
- A destructive change - dropping a column or table, narrowing a type, adding a `NOT NULL` column with no default - MUST be split across releases: add the new, write both, backfill, stop reading the old, then drop.
- A data migration MUST be idempotent and re-runnable.
- A migration MUST NOT contain business logic and MUST NOT call application code. ⚙ BE-88
- Seed data MUST be limited to reference data the domain cannot function without. Demo records and test fixtures MUST NOT be seeded by a migration.

### Repositories and the aggregate graph

- A repository serves exactly one aggregate root. Its interface is declared in `Domain` beside that root; its implementation lives in `Infrastructure` and depends on the module's `DbContext`.
- A repository MUST load the complete aggregate - the root, its entities, and its value objects - in one operation. Partial loads are forbidden.
- The repository owns the graph shape. A command handler MUST NOT specify includes, projections, or tracking behavior, and MUST NOT know how the aggregate is assembled.
- Persistence is whole-aggregate. Add, update, and remove operate on the root; the aggregate's internals are persisted and deleted with it.
- A repository MUST NOT expose `IQueryable`. ⚙ BE-40
- A generic `IRepository<TRoot, TId>` MAY provide the universal operations: get by id, add, remove. Anything beyond them MUST be declared on a per-aggregate interface with an intention-revealing name.

### EF Core mapping for DDD

- Value Objects MUST be mapped as owned types, as complex types, or, for a value object with a single value, through a value converter - never as entities with their own identity.
- Strongly-typed ids MUST be mapped with value converters.
- Private collections MUST be mapped to their backing field.

### Redis

In `annex-caching.md`.

### Read-side caching

In `annex-caching.md`.

## 6. Authorization

Every dispatchable operation is declared once, in the slice that owns it.

```csharp
// Illustrative.
public static readonly Operation <Operation> = new(
    Id:          "<module>.<operation>",
    Policy:      Policies.<Module>.Write,
    Kind:        OperationKind.Command,
    Idempotency: IdempotencyMechanism.ClientSuppliedId,   // or Natural, or Key (§4)
    Transaction: TransactionRequirement.Required,         // None: an external write (§4)
    RateLimit:   RateLimits.<Stricter>,                   // optional (§9)
    Human:       HumanRequirement.Required);              // unreachable without a live user
```

- Bare `[Authorize]` and unattributed endpoints are defects. ⚙ BE-50
- `AuthorizationOptions.FallbackPolicy` MUST deny. ⚙ BE-51
- The authorization behavior evaluates policies via `IAuthorizationService`, against the explicit principal from the dispatch context.
- The object check (core §3) is made after the load and before the handler acts, through a resource-authorization port declared in `Platform.Application`:
  - the port takes the resource and the policy, and reads the acting principal from the dispatch context;
  - its implementation calls `IAuthorizationService.AuthorizeAsync(principal, resource, policy)`;
  - each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its aggregates - live in its `Infrastructure` as framework extension points (BE-34), read the aggregate through its public members, and are registered by its registration entry point.
- A query applies the principal's record scope in its read - a global query filter on its read types, or a predicate in its SQL - never by filtering the result. The scope (the records the principal may see, such as its tenant's or its own) is resolved into the dispatch context at the composition root or by the authority adapter; the read takes it from there.
- A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor package's equivalent - MAY enforce in addition, but MUST NOT be the only enforcement point. Every operation's policy decision MUST be reachable from the authorization behavior (core §3).
- Identity-provider claims are mapped to policies at the composition root, and externally-sourced permissions in the adapter that wraps the authority (below). Application and domain code MUST NOT read raw claims. ⚙ BE-53
- Where permissions come from - a token claim, a directory, a remote authorization service - is a composition-root concern. Application and domain code name a policy, never a permission source or a role literal.
- A claim read from a token before its signature is validated MAY select how the token is validated - such as which issuer's keys, or which remote validation endpoint - and MUST NOT be trusted for anything else: not identity, not tenancy, not a permission lookup. Once the token is validated, the claim that made the selection MUST agree with the validated token, or the token is refused.
- An external authority is reached through one Infrastructure adapter, behind a port, that wraps its client and declares its SDK. The adapter translates the authority's permission codes into the application's policy vocabulary, and its failures into outcomes (§4 *Commands and Queries*). The authority's types and codes MUST NOT be referenced outside that adapter's project. ⚙ BE-102

### External authorities

Which provider and which authority the organization runs is recorded in `DECISIONS.md`, not here.

- When a request depends on a source outside the process - one that validates the token or answers for permissions - its unavailability MUST deny. An unreachable, timing-out or erroring authority MUST NOT grant, MUST NOT accept the token unvalidated, and MUST NOT fall back to a default permission set.
- An external authority's decision - a validation result or a permission decision - MAY be cached, keyed by the acting principal. Its lifetime MUST be bounded and declared in an options class. ⚙ BE-100 The lifetime SHOULD be configurable. Where the authority's client caches for a fixed period it does not expose, the declared value MUST be that period.
- An unexpired entry MAY be served while the authority is unreachable. An expired entry MUST NOT be served, and its lifetime MUST NOT be extended because the authority cannot be reached.
- Externally-resolved permissions are the user's permissions, not the caller's. The lookup MUST be keyed on the acting principal from the dispatch context, never on the HTTP request. Their intersection with an agent's manifest is in `annex-agents.md`.

## 7. Data Classification

### Declaring a class

- A class is declared on the value's type - the Value Object or strongly-typed id in `Domain` - and on the module's published type for the same concept in `Contracts`; an architecture test MUST prove the two agree (core §4). A property projected from a classified source, such as a column a query handler reads into a `string`, names that source by its `Contracts` twin (§3) in its declaration; the same test proves they agree. Classes are `DataClassification`s from Microsoft.Extensions.Compliance, defined once in `SharedKernel`: one attribute derived from `DataClassificationAttribute` per class of core §4 - `Public`, `Internal`, `Confidential`, `Restricted`. ⚙ BE-54
- An architecture test MUST assert that every property reachable from an egress boundary resolves to a class. A property whose type carries no declaration is `Restricted`, and fails at the boundary. ⚙ BE-55
- The OpenAPI document MUST carry the class as `x-data-class` (contract §7 *Wire contract*) on every property of every schema it contains - Request and Response DTOs, problem details, the page - and on every parameter's schema. The emitter MUST fail rather than emit a property whose class it cannot resolve. ⚙ BE-83

```csharp
// Illustrative.
[Restricted]
public sealed record NationalId
{
    public override string ToString() => "***";   // never the raw value
}
```

- A `Confidential` or `Restricted` type MUST redact in `ToString()`. It MUST NOT define an implicit conversion to `string`. ⚙ BE-56

- A property whose type carries no class of its own - such as a page size, a cursor, a computed flag, or a field of a shared platform type - declares its class on the property. Every other type that carries a classified value inherits its class and MUST NOT redeclare it: not on a DTO, a message, an event, or a call site.

### Egress owners

Each egress boundary of core §4 has one owner here, and no other path across it:

- a Response DTO to an HTTP client: the endpoint, through the registrar (§13); to a real-time client: the hub (`annex-realtime.md`);
- a model's context: the agent tool projection (`annex-agents.md`);
- a log, metric, trace or error report: the logging behavior (§4), and the processors and exporters every signal passes through, automatic instrumentation included (§10);
- a cache entry: the cache decorator (`annex-caching.md`), and for an external authority's decisions, the adapter for that authority (§6);
- a payload to a third-party integration: its Infrastructure adapter, with a per-integration allowlist of fields, declared and reviewed;
- an export, report or file handed to a user: its export endpoint, under the policy of the equivalent read, and audited (§10 *Audit trail*).

### Logging

- Logging is structured, through source-generated `LoggerMessage` methods. A message template MUST be a constant and MUST NOT interpolate a value. A log scope is begun with a state object (`ILogger.BeginScope<TState>`), never a template. ⚙ BE-57
- Redaction is Microsoft's: `EnableRedaction()` on the logging builder, and the erasing redactor for `Confidential` and `Restricted`.
- Every parameter of a logging method carries its class attribute, except the first `ILogger`, `LogLevel` and `Exception` parameters. An exception passed to a logging method is governed by core §4's exception rule and the processor in §10, not by redaction.
- Every project that declares a logging method references `Microsoft.Extensions.Telemetry.Abstractions`, whose logging generator replaces the in-box one and reads the class attributes.
- `PersonId` is loggable; `NationalId` is not (core §4).
- Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a classified value.

### The model-visible surface

In `annex-agents.md`.

### Cache, events, and storage

Its cache rules: `annex-caching.md`.

- Integration events and outbox rows MUST NOT carry `Restricted` payloads. Reference by id. ⚙ BE-61
- `Restricted` columns MUST be encrypted at rest:
  - the encryption MUST live in Infrastructure - in the mapping, or below it in the database driver, such as SQL Server Always Encrypted - and MUST be transparent to every path that reads the column;
  - a read type that maps an encrypted column carries the same converter, and SQL (§4) MUST NOT read a column encrypted in the mapping;
  - a column MUST NOT be encrypted in a form that does not support every operation a query performs on it, such as the range comparison keyset paging makes on its sort key (§4);
  - the domain type MUST NOT know it is encrypted.

## 8. AI Agents

In `annex-agents.md`. An agent manifest is the one list of operations an agent may invoke.

## 9. Quotas and Rate Limits

Limits attach to the caller and to the run, not to the transport. Both are required, and neither substitutes for the other.

### Transport

- Every endpoint MUST be covered by a rate limit: a `GlobalLimiter`, or a named policy the endpoint registrar applies to every endpoint (§13). `[DisableRateLimiting]`, and any other way of removing an endpoint from its limit, is forbidden. ⚙ BE-70
- Limits are keyed on the acting principal, falling back to a client identity for anonymous endpoints. Keying on IP alone is not a limit.
- The endpoint registrar applies the write policy to every command endpoint. An operation declaration MAY name a stricter policy and MUST NOT remove the write policy: the stricter policy chains the write limiter with its own through `RateLimiter.CreateChained`, inside the named policy, rather than replacing it.
- An expensive read's operation declaration MUST name a tighter policy than the default.
- A rejection (contract §4 *Limits*) carries `Retry-After` wherever the limiter can estimate when permits return - a token bucket, a fixed or a sliding window; a concurrency limiter's does not. ASP.NET Core writes neither: `OnRejected` sets `429` and copies `MetadataName.RetryAfter` into the header.
- A limit is enforced per instance unless a shared limiter enforces it. A limit stated for the whole service is divided by the instance count or enforced by a shared limiter.

### Agent runs

In `annex-agents.md`.

### Configuration

- Limits and budgets MUST be configuration, not constants in code, and MUST fail closed: a missing or unparseable limit denies rather than admits. ⚙ BE-73
- Every limit MUST emit a metric when it is hit.

## 10. Observability

### Correlation

- The correlation id MUST live in the dispatch context beside the principal, as a required constructor parameter. ⚙ BE-90 Its value is the current `Activity`'s trace id.
- Every problem details response carries the trace id as `traceId`, written from `Activity.Current.TraceId`, not from ASP.NET Core's default (`Activity.Id` or `TraceIdentifier`) (contract §7 *Wire contract*).
- Where a request from outside the trust boundary can reach the application with the caller's `traceparent` - directly, or through an ingress that forwards the header - the application adopts no caller's trace. The composition root:
  - installs a `DistributedContextPropagator` that starts every such request's trace and records the caller's `traceparent` as a link (`Activity.AddLink`);
  - sets OpenTelemetry's default propagator to a plain `TraceContextPropagator` (`Sdk.SetDefaultTextMapPropagator`).
- An outbox row MUST persist the trace context - `traceparent` and `tracestate` - of the transaction that wrote it. ⚙ BE-91
- The dispatcher MUST restore that trace context when delivering.
- A background job MUST start a trace per execution and log a start event once; the enricher supplies its trace id.

Its agent rule: `annex-agents.md`.

### Logging

- Code logs through `ILogger` and source-generated `LoggerMessage` methods. Serilog writes the output, registered on the logging builder: `builder.Logging.AddSerilog(logger, dispose: true)`, the logger built from a `LoggerConfiguration` and never assigned to the static `Log.Logger`. Replacing `ILoggerFactory`, as `UseSerilog()` does, is forbidden.
- The logging behavior (§4) is the only place a message's dispatch is logged. A handler MUST NOT reference `ILogger`. ⚙ BE-93
- Levels have fixed meanings:
  - `Error`: a defect that needs someone;
  - `Warning`: degraded but handled;
  - `Information`: a business-meaningful event;
  - `Debug`: developer detail, and MUST NOT be enabled in production.
- An expected business rejection MUST NOT be logged at `Error`. A `Result` failure is an outcome, not a fault.
- Module, message type, principal id, and correlation id are supplied by the enricher. A call site MUST NOT restate them.

### Metrics and traces

- OpenTelemetry (core §5).
- An OpenTelemetry processor, and its equivalent in the logging pipeline, removes `exception.message` from every exception whose type the solution does not declare, keeping `exception.type` and the stack trace (core §4). Automatic instrumentation passes through the same processors.
- Every pipeline behavior MUST emit a span. Authorization, validation, and handler time MUST be separable in a trace.
- Metrics MUST cover dispatch count and duration per message type, authorization denials, validation failures, outbox depth and delivery lag, cache hit rate, limit and budget rejections (§9), and agent run duration and spend.
- Metric dimensions MUST come from a declared closed set of keys. ⚙ BE-94
- Metrics are exported with exemplars, sampled by the trace (`ExemplarFilterType.TraceBased`).
- Liveness MUST NOT depend on a downstream. Readiness MUST check the database and the outbox dispatcher.

### Audit trail

- The audit trail (core §5) is a store no application role may update or delete, written through one port, `IAuditTrail`, declared in `Platform.Application` and implemented in `Platform.Infrastructure`: one table in each module's schema, for the success records that module's unit of work writes (§5), and one the port's own context writes.
- A persisting command's success is recorded through its unit of work, as the outbox row is. Every other record is written through the port, outside any unit of work:
  - a denial, whether by the policy or by the object check;
  - a command's failure, written after its rollback;
  - a command that does not persist (§4), through a context or connection of the port's own, never the module's;
  - a read of `Restricted` data;
  - an export, report or file handed to a user (core §4).
- A failure to write an audit record fails the operation it records. A denial stays a denial.

## 11. Configuration and Secrets

- Configuration binds to strongly-typed options classes. Outside the composition root, code MUST NOT read configuration from its source - `IConfiguration`, a process environment variable, or any other. ⚙ BE-95 It receives a bound options class.
- Every options class MUST have a validator and MUST be registered with `ValidateOnStart`. ⚙ BE-96
- An options default MUST NOT silently weaken a control. A missing limit, timeout, or policy denies (§9); it never falls back to permissive.
- Secrets come from Key Vault through managed identity in every environment above local development. A secret MUST NOT appear in any committed file. ⚙ BE-97 A secret is any value that grants access on its own, such as a connection string, a package-feed token, or any other. A key generated for a test and trusted by nothing outside it is not a secret where it is listed, by file and key, in the reviewed test-key list `tests/test-keys.txt`. A committed file that needs a credential MUST reference it - an environment-variable macro such as `nuget.config`'s `%NUGET_PAT%`, or a credential provider - and MUST NOT carry a placeholder to be overwritten in place.
- Local development uses user secrets. A developer MUST NOT need a production credential to run the application.
- Configuration that differs by environment MUST differ by value, never by code path. Outside the composition root, code MUST NOT learn which environment it runs in, by any means. ⚙ BE-98 Where telemetry needs the environment's name, the composition root puts it there - a resource attribute, a log enricher.
- A feature flag is configuration: typed, validated, and removable.

## 12. Real-time

In `annex-realtime.md`.

## 13. API Layer

- Every endpoint MUST be a type implementing the shared endpoint abstraction, with its Request DTO, dispatched message and Response DTO as type arguments. A lambda or method group passed as a route handler to a minimal-API route builder - `MapGet`, `MapPost`, `MapMethods`, or any other - is forbidden. ⚙ BE-80
- Endpoints are registered by discovery over those types at the composition root. Hand-registering one is a defect. ⚙ BE-81
- The framework endpoints - the readiness and liveness probes (§10), a hub's `MapHub` (real-time annex) and the OpenAPI document - are mapped by the framework, not by an endpoint type: a declared list at the composition root, outside the two rules above. They stay under the rate limit (§9, the `GlobalLimiter`). Each carries a named policy (§6), such as an anonymous one for the probes, or falls to the denying fallback.
- The registrar applies each endpoint's authorization and rate-limit metadata from the operation declaration of the message it dispatches; an endpoint declares neither itself.
- An endpoint MUST map its Request DTO to a message, dispatch it, and translate `Result` to HTTP (contract §5). It MUST NOT reference any type other than its DTOs, the message it dispatches, and every type those are composed of; the shared endpoint abstraction; the dispatcher and the `Result` it returns; the result translator; and the framework's primitives, such as `Task`, `CancellationToken`, and HTTP result types. No type reachable from an endpoint's signature, a Request DTO's property graph included, is a `Domain` type. ⚙ BE-82
- Success shapes: `200` with a Response DTO, `201` with a location, `204` for no content, or a stream the endpoint declares. A write that leaves a versioned resource in place returns a body carrying its new `etag`, never `204`; a delete returns `204` (contract §7 *Wire contract*). Failures are `ProblemDetails`.
- Preconditions:
  - An endpoint maps `If-Match` to the message's expected version, and `If-None-Match: *` to the message's create-only flag, as it maps any other request field.
  - The repository's load reports a stale expected version as an outcome, which the handler translates to a `Result` precondition failure before it acts. The repository also uses the expected version as the concurrency token's original value.
  - For a create-only message, a violation of the aggregate id's unique index at commit is a precondition failure, not a conflict; the unit-of-work behavior returns it as a precondition failure (§4 *Pipeline behaviors*, item 4).
  - The result translator maps every precondition failure to `412`.
- Problem types:
  - Every problem type a module returns is a URI the module declares, with its status, title and whether it is user-facing. The OpenAPI document's `x-problem-types` is generated from those declarations (contract §7 *Wire contract*).
  - `Platform` declares the problem types the pipeline and the framework return - validation, authorization, idempotency, preconditions, rate limits, unsupported media, and any other - and overrides ASP.NET Core's per-status defaults with them through `CustomizeProblemDetails`.
  - The result translator maps each validation failure's message path to the request path it came from (contract §7 *Wire contract*): a message property keeps its request path unless the endpoint's mapping declares a different one.
- Where the client is on another origin, the CORS policy admits the request headers the wire contract uses - `traceparent`, `tracestate`, `If-Match`, `If-None-Match`, `Idempotency-Key`, or any other it adds - and exposes the response headers a client reads, such as `ETag`, `Location` and `Retry-After`.
- The endpoint registrar sets `Cache-Control: no-store` on every endpoint whose Response DTO resolves to `Confidential` or `Restricted` (BE-55), and `Cache-Control: private` on every other endpoint whose representation depends on the principal (contract §7 *Wire contract*). Which endpoints depend on the principal is reached by review.
- Uploads:
  - An upload endpoint's Request DTO carries the file as a stream with its metadata. Its size limit is enforced at the request boundary - the endpoint's request size limit - before the body is buffered.
  - It dispatches a command that does not persist to store the file in quarantine, under a name the server derives from the uploader's principal and the command's client-supplied id - an id the validation behavior has parsed - with a keyed hash, such as an HMAC under a server key; never from the client's file name (§4).
  - A persisting command, run under the uploader's principal recorded with the quarantined file, records it once the scan passes.
  - Only a principal whose policy grants the upload may send one. The operation declares an allowlist of file types, enforced before anything is stored.
  - A file's type is proven from its content, never from its name or `Content-Type` alone, and never by a signature check alone.
  - The quarantine is outside any path the server serves, where a file cannot execute. A file is released only once its malware scan passes and, where its type allows, its content has been disarmed and reconstructed.
  - The operation that accepts a file declares the file's class (core §4); a file whose class is not declared is `Restricted`. A stored file is returned only by an operation whose policy grants that class, under core §4's export rule.
- An exception that escapes the pipeline is a defect. The global exception handler logs it with full context, within core §4's telemetry rule, and answers `500` as problem details. Code never catches a domain exception to turn it into a `4xx`; it adds the missing guard instead.
- A change to a Request or Response DTO that does not regenerate and commit the OpenAPI document is incomplete.

## 14. Testing

A missing required test is a defect exactly as a violated rule is (core §1). The required tests of a rule in an annex are in that annex's own Testing section.

- **Domain**: pure unit tests, no substitutes, no I/O. Every invariant and every domain outcome MUST have a test (BE-138).
- **Application**: handler tests with substituted infrastructure.
  - Every declared policy that some principal does not satisfy MUST have a test proving such a principal is denied, not only one proving success (BE-122).
  - Where a handler translates outcomes that are types, every outcome MUST have a test proving its translation (BE-141).
- **Integration**: real SQL Server via Testcontainers, real pipeline behaviors.
- **Queries**: every query handler that reads with SQL MUST have a test against real SQL Server proving its SQL projects into its Response DTO and applies the module's query filters (BE-112).
- **Transactions**: a command that runs without a transaction MUST have a test proving a staged database change fails the dispatch and is never committed (BE-111).
- **Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped or marked inconclusive to unblock a change.
- **Authorization**:
  - Every external authority MUST have a test proving that an unreachable, a timing-out, and an erroring authority each deny, and that an expired cache entry is not served while the authority is unreachable (BE-99).
  - Every token validation that reads a claim before the signature is checked MUST have a test proving that a token whose selecting claims disagree with its validated issuer or endpoint is refused, and that no claim read before validation reaches the principal (BE-104).
  - Every policy MUST have a test proving an in-process dispatch with no HTTP context receives the same decision as the HTTP path (BE-101).
  - Every operation that acts on a record the caller identifies, unless its policy grants every record, MUST have a test proving a principal that holds the policy but not the record is denied; every query, a test proving it returns no record the principal may not see (BE-117).
- **Audit**:
  - Every persisting command MUST have a test proving its success record commits with its change and its failure record is written after rollback (BE-118).
  - Every command that does not persist MUST have a test proving its record is written (BE-118).
  - The authorization behavior and the resource-authorization port MUST have a test proving a denial is recorded (BE-118).
- **Pagination**:
  - Every paginated query MUST have a test proving no row is skipped or repeated across a page boundary (BE-124).
  - Every collection bounded by construction whose size a request determines MUST have a test proving a request beyond its bound is rejected, never truncated (BE-124).
- **Idempotency**: every command declaring a key MUST have four tests, one proving each of:
  - a replayed key returns the recorded result without executing twice (BE-125);
  - two concurrent attempts under one key resolve as one execution and one replay, not an error (BE-29);
  - the same key with a different payload is refused with `422` (BE-116);
  - a request that carries no key is refused with `400` (BE-137).
- **Concurrency**: every versioned write MUST have a test proving a write without `If-Match` is refused with `428`, one with a stale version with `412`, `If-Match: *` with `428`, and a create with `If-None-Match: *` for an existing id with `412` (BE-115).
- **Uploads**: every upload endpoint MUST have a test proving a file whose content does not match its declared type is refused, a file above the maximum size is refused, and a stored file is not served before its scan passes (BE-120).
- **Rate limits**: every rate-limit policy, and the `GlobalLimiter`, MUST have a test proving a request beyond its limit is refused with `429` as problem details, carrying `Retry-After` where its limiter can estimate when permits return (BE-127).
- **Correlation**:
  - The outbox MUST have a test proving the trace context survives the round trip - written with the transaction, restored on delivery, present on the consumer's log line (BE-92).
  - A request from outside the trust boundary that carries a `traceparent` MUST have a test proving it gets a new trace id and a link to the caller's (BE-139).
- **Observability**: the dispatch pipeline MUST have a test proving every behavior registered in it, and the handler, emits a span, nested in pipeline order, so that authorization, validation and handler time are separable (BE-128).
- **Migrations**: every migration MUST be applied to a Testcontainers database in CI and proven to run clean twice. A destructive migration MUST also have a test proving the previous application version still serves against the new schema (BE-89).
- **Classification**: every `Confidential` and `Restricted` type MUST have a test proving it redacts in `ToString()` and in a serialized log event, and that a classified logging-method parameter is redacted in the provider's output (BE-126).
- Test names state the rule: `<Operation>_Returns<Rejection>_When<Condition>`.
- Tests MUST NOT assert on implementation details: no verifying private calls, no asserting on generated SQL.

## 15. Forbidden

- `dynamic` ⚙ BE-129
- Service location outside the composition root, except that a hosted service MAY create a scope through `IServiceScopeFactory` and resolve its per-iteration dependencies from it ⚙ BE-130
- Static mutable state ⚙ BE-131
- Reading the system clock in domain or application code - `DateTime.Now`, `DateTimeOffset.UtcNow`, `DateTime.Today`, or any other ambient read. Inject `TimeProvider` into the handler, and pass the time to the aggregate as an argument. ⚙ BE-132
- `async void` outside event handlers ⚙ BE-133
- `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` ⚙ BE-134
- Catching `Exception` without rethrowing, except at a declared fault-isolation boundary: one iteration of a hosted-service loop, one outbox delivery, a post-commit effect (§4), or any other unit whose failure must not stop the host. The boundary is declared by a marker on the method - an attribute the solution declares once in `SharedKernel` - and MUST log at `Error` with the exception, within core §4's telemetry rule, and MUST let `OperationCanceledException` propagate on shutdown. ⚙ BE-135
- `#region` ⚙ BE-136
- Commented-out code

## 16. Verification

`dotnet build` (warnings are errors) and `dotnet test` (includes architecture tests). Both MUST pass before a change is complete.
