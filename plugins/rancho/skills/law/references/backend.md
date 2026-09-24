# Backend Law - .NET

`../SKILL.md` governs. This file adds backend rules. ⚙ = machine-checked.

## 1. Stack

.NET 10 (LTS) · EF Core · SQL Server · Redis · SignalR · source-generated Mediator · FluentValidation · Serilog · OpenTelemetry · Azure Key Vault · xUnit v3 · NSubstitute · Shouldly · Testcontainers · ArchUnitNET

`TreatWarningsAsErrors` and `Nullable` are enabled solution-wide and MUST stay enabled. Suppressing a warning requires a comment naming the rule and the reason.

Verify third-party licenses before pinning. MediatR and FluentAssertions moved to commercial licensing; the picks above avoid that.

## 2. Project Reference Law ⚙

Four facts decide every reference. The table below is their consequence, not a second set of rules:

1. A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference a module or `Api`.
2. A module reaches another module only through that module's `Contracts`.
3. Inside a module, dependencies point inward: `Infrastructure` to `Application` to `Domain` and `Contracts`. `Domain` and `Contracts` are peers and MUST NOT reference each other.
4. `Api` composes; nothing references `Api`.

| Project | Projects it may reference | Packages it may declare |
|---|---|---|
| `SharedKernel` | none | none - BCL only |
| `<Module>.Domain` | `SharedKernel` | none - BCL only |
| `Platform.Application` | `SharedKernel` | dispatch, validation, authorization and logging abstractions |
| `<Module>.Contracts` | `SharedKernel`, `Platform.Application` | none of its own |
| `<Module>.Application` | own Domain + Contracts, other modules' Contracts, `Platform.Application` | validation |
| `Platform.Infrastructure` | `SharedKernel`, `Platform.Application` | EF Core, messaging, telemetry |
| `<Module>.Infrastructure` | own Application, Domain, Contracts, `Platform.Infrastructure` | EF Core, messaging, external clients |
| `Api` | any `*.Contracts`, any `*.Infrastructure`, `Platform.Infrastructure`, `SharedKernel`, `Platform.Application` | ASP.NET Core, hosting, the source generator |
| `<Anything>.Tests` | any project | test packages |

Every project MUST declare its kind as an MSBuild property whose value is one of the rows above. A project whose kind is missing, misspelled, or absent from the table may reference nothing and be referenced by nothing: adding a kind of project means adding a row, derived from the four facts. Kind is declared rather than inferred from the assembly name because `Platform.Application` and `<Module>.Application` are different rows that no name-suffix rule separates, and because a rule that guesses has no answer for a project it has never seen.

A test project MUST NOT be referenced by a project of any other kind. A fixture built to prove a check fails MUST NOT appear in the solution file or under the production source root.

The table is an allowlist over **direct** `ProjectReference` entries, not over the transitive closure. A project MAY declare a reference it could also reach transitively; it MUST NOT declare one its row omits. Read as the closure, every row would become the union of its dependencies' rows and the table would check nothing.

The Domain project MUST NOT reference EF Core, Mediator, ASP.NET, `IHttpContextAccessor`, or any application type. Enforced by `.csproj` and by an architecture test. `SharedKernel` is BCL-only for the same reason one step removed: `Domain` references it, so a package added there reaches every aggregate in the solution.

**`SharedKernel` and the two `Platform.*` projects are shared, not modules.** `Platform.*` carries the dispatch pipeline - `Result`, the message abstractions, the solution-wide page shape and cursor type, and the behaviors. A platform project that names a module has stopped being platform, by fact 1.

`Result` lives in `Platform.Application`, not in `SharedKernel`. `Domain` does not reference `Platform.Application`, so "the domain MUST NOT reference `Result`" (`../SKILL.md` §6) is carried by the compiler rather than by an analyzer.

`Contracts` references `Platform.Application` because a published query message MUST name both its `Result` response and the abstraction it is dispatched by. That is the only reason for the reference: nothing else from `Platform.Application` may appear in a contract's public surface, so the committed contract snapshot (§4) is coupled to the message abstraction and to nothing else.

"For composition only" governs the reference, not the contents of `Api`: `Api` registers Infrastructure in the container and MUST NOT call into it. `SharedKernel` and `Platform.Application` are exempt from that qualifier, because `Api` uses them directly at the two boundaries the law assigns it - `Result` to `ProblemDetails` (`../SKILL.md` §6) and the correlation id (§10).

A module is a Bounded Context. Its projects exist **at most once per module**, never per feature: a module with no model of its own omits `Domain`, one that publishes nothing to its siblings omits `Contracts`, and `Application` is what makes a module dispatchable at all. A module has one Domain, shared by all of its features - features are use cases over that model, not copies of it. Vertical slicing applies to the **Application project only**.

| Project | Organized by | Contains |
|---|---|---|
| `Domain` | Aggregate | `<Aggregate>/` - root, its entities, value objects, outcomes, domain events, specifications, repository port |
| `Application` | Feature (use case) | `<Feature>/` - command or query, handler, validator, response DTO, operation declaration |
| `Infrastructure` | Technical concern | `Persistence/`, `Messaging/`, `External/` |
| `Contracts` | Published surface | `IntegrationEvents/`, `Queries/` |
| `Platform.Application` | Technical concern | `Abstractions/`, `Behaviors/` |
| `Platform.Infrastructure` | Technical concern | `Persistence/`, `Messaging/` |

Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`, no `Domain/Entities/`, no `Domain/ValueObjects/`. ⚙

Two features that use the same aggregate share it. Duplicating an aggregate across feature folders inside one module is a defect - it means the module has more than one model of the same concept.

Domain event reactions are Application slices too, named for what they do: `Application/<Reaction>/`.

## 3. Domain Layer

### Aggregates

- Aggregate state MUST be private. No public setters. Collections are exposed as `IReadOnlyCollection<T>` over a private backing field. ⚙
- State MUST change only through methods named in domain vocabulary. `SetStatus` is a defect; `Approve` is the rule.
- An aggregate MUST be valid at every instant of its public lifetime. Constructors and methods enforce invariants. There is no "validate later" step and no `IsValid` property.
- An aggregate method MUST NOT return `Result`, a DTO, or any application type. It returns a domain outcome, a domain value, or nothing. ⚙
- Expected business rejections MUST be domain outcome values - an enum or sealed outcome type in domain vocabulary. They MUST NOT be exceptions.
- Invariant breaches MUST throw. Reaching one means a guard failed upstream; that is a bug and is meant to be loud.
- Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O. ⚙
- An aggregate root MUST be marked as one at the type level. ⚙ "Root" is not a folder position or a naming convention; it is a property a check can read, and several rules depend on being able to read it.
- An aggregate MUST NOT hold a reference to another aggregate. It holds that aggregate's id. ⚙

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
- A public method, constructor, or property in `Domain` or `Application` MUST NOT expose a bare `Guid`. ⚙ `int` stays permitted for counts and quantities, so an `int` pressed into service as an identifier is caught in review rather than by the check.

### Entities and Domain Events

- Entities inside an aggregate MUST be reachable and modifiable only through the root.
- Domain events MUST be raised by the aggregate, recorded on it, and dispatched by infrastructure after persistence. A handler MUST NOT raise a domain event.
- Domain events SHOULD be named in the past tense.
- Domain events MUST carry ids and values, never entity references. ⚙
- Domain events are intra-module and MUST NOT leave the module. ⚙

### Specifications

- Specifications MAY encapsulate reusable query predicates.
- A Specification MUST NOT be the authority on whether a mutation is legal. That authority is the aggregate method. Never pre-check with a Specification and rely on the aggregate not to re-check - the two will drift, and the drift is a bypassed business rule.

## 4. Application Layer

### Commands and Queries

- A Command mutates. A Query reads. A Command MUST NOT return data beyond an identifier and its outcome. ⚙
- A Query MUST NOT mutate: a query handler MUST NOT reference a repository port, and MUST NOT call `SaveChangesAsync`. ⚙
- Command handlers MUST mutate state only by loading an aggregate and calling its methods. Writing through `DbContext` or SQL from a command handler is forbidden. ⚙
- Query handlers MUST bypass the domain and project straight to Response DTOs, untracked. Materializing an aggregate in a query handler is forbidden - a read returns the fields the caller needs, never a whole aggregate. ⚙
- The query message, its Response DTO, and the `IQueryHandler<,>` abstraction live in `Application`. The **concrete query handler lives in `Infrastructure`**, because it depends on `DbContext`. Commands do not split this way: a command handler needs only the repository port declared in `Domain`, so it stays in `Application`. ⚙
- This split does not weaken the pipeline. Dispatch is by message type, so authorization, validation, and logging behaviors run identically wherever the handler is compiled.
- A query handler MUST NOT use an aggregate repository. Repositories serve the write path only; reads project directly. ⚙
- `Repository` is reserved for the aggregate write path. A read-side type that returns DTOs MUST NOT be called a repository - name it `<Feature>Reader`. Usually the query handler is the read data access and no separate type is warranted. ⚙
- One handler per message. Handlers MUST NOT call other handlers. Shared logic belongs in the domain or a named application service.
- Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, or any transport type. ⚙

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

- Every query returning a collection MUST be paginated. A handler returning an unbounded `List<T>` or `IEnumerable<T>` is a defect. ⚙
- One shared page shape for the whole solution. A per-feature paging envelope is a defect. ⚙
- Every paginated query MUST declare a maximum page size, under a solution-wide ceiling. A request above the maximum MUST be rejected by the validation behavior. Silent clamping is forbidden: a caller that asked for 500, received 100, and was told nothing will conclude it has every row. ⚙
- Omitting the page size MUST yield the operation's declared default, never "all".
- Ordering MUST be deterministic. A paginated query MUST declare its ordering as a descriptor whose last term is a unique key, rather than expressing it only as an `OrderBy` chain. ⚙ Non-deterministic ordering makes both cursor and offset paging skip and duplicate rows, and does it silently - declaring the ordering is what makes that checkable instead of hopeful.
- Keyset (cursor) paging is the default for unbounded and agent-reachable collections. Offset paging MAY be used for UI grids over bounded sets, and MUST still enforce the maximum and the tiebreaker.
- A total count MUST be opt-in per operation. Counting by default puts a `COUNT` over the whole filtered set on the hot path of every list screen.
- One cursor type solution-wide, whose payload is exactly the sort key and the id. ⚙ A cursor therefore cannot carry a filter, a sort, or a principal: a client that could edit those would otherwise widen its own read.
- Cursors are opaque to the client and MUST be validated server-side.
- An agent-reachable query MUST declare a lower maximum than the UI's, in the manifest (§8). An unbounded search tool pulls a table into a vendor's context window.

### Pipeline behaviors

Fixed order. Changing it requires written justification.

1. **Logging / correlation**
2. **Authorization** - resolves the message's declared policy and evaluates it against the explicit principal. A message with no declared policy MUST fail to dispatch. ⚙
3. **Validation** - FluentValidation. Failures short-circuit to a `Result` failure; the handler is never invoked.
4. **Idempotency** - commands only. Resolves the attempt identity and short-circuits a replay to its recorded result before the handler runs.
5. **Transaction** - commands only. Opens the unit of work, commits on success, rolls back on any failure or exception. The idempotency record is written through this unit of work, so the state change and the record proving it happened are one commit.

### Idempotency

Callers retry. A client times out and resubmits, a job re-runs, an agent re-invokes a tool it believes failed. A command that executes twice on a retry is a defect in the command, not in the caller.

- Every command MUST declare its idempotency mechanism in its operation declaration (§6): naturally idempotent, client-supplied aggregate id, or idempotency key. An undeclared command MUST fail to dispatch, exactly as an undeclared policy does. ⚙
- Where a key is used, the identity of an attempt is `(principal, operation, key)`. A replay MUST return the originally recorded result without re-executing the handler. The same key with a different payload MUST fail as a conflict, never overwrite.
- The idempotency record MUST be written through the same unit of work as the state change. Redis MUST NOT be the store - an expired cache entry must never make a command executable a second time (§5). ⚙
- The store MUST carry a unique index on the attempt identity. ⚙
- Two concurrent attempts with the same key MUST resolve as one execution and one replay, never as a `500`.
- Records MUST be retained longer than the longest caller retry window, and swept afterwards.
- Idempotency is a pipeline behavior. A per-endpoint or per-handler check is a defect - it will be forgotten on exactly the endpoint that needed it. ⚙
- Agent tool invocations MUST carry a key derived from the run and the step. An agent that re-invokes a tool after a timeout MUST NOT produce a second write. ⚙

### Transactions and consistency

- One transaction MUST modify exactly one aggregate. Needing two means either the boundary is wrong or the second change belongs in an event handler. Outbox rows and idempotency records are bookkeeping written by the pipeline, not aggregates, and do not count against this rule.
- A transaction MUST NOT span a module boundary. ⚙
- Integration events MUST be written to the outbox in the same transaction as the state change that caused them. Publishing directly from a handler is forbidden. ⚙
- Every integration event handler MUST be idempotent. Delivery is at-least-once; assume redelivery.
- Handlers MUST NOT call external systems inside a transaction.

### Integration event contracts

An integration event is a published contract. Another module already consumes it, and the outbox may hold instances written by code that no longer exists.

- Every integration event type MUST carry its version in its name: `<Event>V<N>`. ⚙
- The published contract surface MUST be snapshotted and committed. A change to the shape of an existing version MUST fail the build, the same way a change to the OpenAPI document does. ⚙
- Adding an optional field to an existing version is permitted. Renaming, removing, retyping, or changing the meaning of a field is not - that is a new version.
- Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes to it and no outbox row holds one. Both are published during the overlap.
- A consumer MUST tolerate fields it does not know and MUST NOT fail on one it does not use. Producers ship before consumers.
- Event payloads MUST be ids, primitives, and value objects that serialize stably. A domain type MUST NOT appear in an integration event. ⚙ An aggregate serialized into a contract makes every future change to that aggregate a breaking change to another module.

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

An implementation that is pure logic MUST live in `Application`. Putting it in `Infrastructure` moves business rules into the adapter layer, where they get integration-tested instead of unit-tested and cannot be reused by another slice.

**Only one direction of this error is caught by the compiler.** Infrastructure logic in `Application` fails to build - the reference is not there. Application logic in `Infrastructure` builds cleanly, because `Infrastructure` references `Application`. That direction is invisible to the build and MUST be caught in review.

- Every public type in `Infrastructure` MUST be an adapter: it implements a port declared in `Domain` or `Application`, or a framework extension point (`IEntityTypeConfiguration<T>`, `IHostedService`, `DbContext`). A free-standing Infrastructure service belonging to neither category is a defect. ⚙
- Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/`. A new top-level folder there requires justification. ⚙
- `Infrastructure/Queries/<Feature>/` MUST mirror the feature folder name used in `Application`, so the two halves of a read slice are findable from each other. ⚙

## 5. Persistence

- EF Core, code-first. Migrations are generated, reviewed, and committed with the change that requires them.
- One `DbContext` per module, one schema per module. A `DbContext` MUST NOT map another module's tables. ⚙
- Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure. Mapping attributes on domain types are forbidden - the domain MUST NOT know it is persisted. ⚙
- Lazy loading MUST be disabled.
- Integration tests run against real SQL Server via Testcontainers. No other provider may stand in for it - the EF InMemory provider, SQLite in memory, or any other. ⚙
- Aggregates MUST carry a row version. Concurrency conflicts MUST surface as a domain-meaningful outcome, not a raw `DbUpdateConcurrencyException`.
- SQL Server is the default store. A second store requires a documented reason in the module's README.

### Migrations

- Every schema change ships as an EF Core migration, generated, reviewed, and committed with the change that requires it.
- A committed migration MUST NOT be edited. ⚙ It has run somewhere; changing it means two databases with the same migration history and different schemas. Correct it with a new migration.
- Migrations are forward-only. A down migration MUST NOT be relied on above local development - recovery is a new forward migration, written deliberately.
- A migration MUST be safe to run while the previous version of the application is still serving. Deployments overlap; the schema and the code are never swapped at the same instant.
- A destructive change - dropping a column or table, narrowing a type, adding a `NOT NULL` column with no default - MUST be split across releases: add the new, write both, backfill, stop reading the old, then drop. A migration that drops a column an older instance still reads is an outage with a deployment for a cause.
- A data migration MUST be idempotent and re-runnable, for the same reason a command is (§4).
- A migration MUST NOT contain business logic and MUST NOT call application code. ⚙ It runs without the application's services, against a schema that no longer matches today's model.
- Seed data MUST be limited to reference data the domain cannot function without. Demo records and test fixtures MUST NOT be seeded by a migration.

### Repositories and the aggregate graph

- A repository serves exactly one aggregate root. Its interface is declared in `Domain` beside the root it serves; its implementation lives in `Infrastructure` and depends on the module's `DbContext`.
- A repository MUST load the complete aggregate in one operation - the root, its entities, and its value objects. Partial loads are forbidden: an incompletely loaded aggregate cannot enforce its own invariants.
- The repository owns the graph shape. A command handler MUST NOT specify includes, projections, or tracking behavior, and MUST NOT know how the aggregate is assembled.
- Persistence is whole-aggregate. Add, update, and remove operate on the root; the aggregate's internals are persisted and deleted with it.
- A repository MUST NOT expose `IQueryable`, `DbSet`, or any other EF type. ⚙
- A generic `IRepository<TRoot, TId>` MAY provide the universal operations: get by id, add, remove. Anything beyond that MUST be declared on a per-aggregate interface with an intention-revealing name. A generic repository that accumulates query methods has become a data-access layer.
- Command handlers fetch through the repository, mutate through aggregate methods, and persist through the repository. There is no other write path. ⚙

### EF Core mapping for DDD

- Value Objects MUST be mapped as owned types or complex types, never as entities with their own identity.
- Strongly-typed ids MUST be mapped with value converters.
- Private collections MUST be mapped to their backing field.
- Navigation properties between aggregate roots are forbidden. One aggregate references another by id only. ⚙
- Domain types MUST NOT acquire public parameterless constructors, public setters, or EF attributes to satisfy the ORM. Use EF's private-constructor and backing-field support. **If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type.** ⚙

### Redis

- Redis is a cache, a SignalR backplane, and short-lived coordination state. It MUST NOT be a source of truth.
- Every entry MUST have an explicit TTL. Unbounded entries are forbidden.
- Keys MUST be namespaced by module.
- The system MUST be correct with an empty cache. A cold cache is a performance event, not a failure.

### Read-side caching

Caching is a **decorator over `IQueryHandler<TQuery, TResponse>`**, never a branch inside a handler. One generic decorator serves every cached query: check Redis, fall through to the database handler on a miss, populate on the way out. Both paths return the same DTO.

- Decorators MUST be registered explicitly per query at the composition root, never auto-applied, so the cached set is auditable. ⚙
- The decorated and undecorated paths MUST return the same type and the same values. Removing the decorator changes latency, never behavior. Every cached query MUST have a test that exercises the handler undecorated.
- A decorator MUST contain cache logic only. Any business rule inside one is a defect - it is invisible to every test that runs the handler directly.
- Cache keys MUST be derived from the query message type and its parameter values, and MUST carry a schema version segment so a changed DTO shape cannot deserialize into a stale one.
- **Every cache key MUST include the acting principal**, added by the one key builder every cached query uses. ⚙ The rule is unconditional because "is this projection permission-filtered?" is not a question a check can answer. Caching a filtered read under a principal-agnostic key serves one user's rows to another - and serves an agent rows its narrowed principal should never have seen. This is the single most likely way the authorization model in §6 and §8 gets bypassed. A genuinely public projection pays some hit rate for the rule being unconditional; that is the correct trade.
**Invalidation.** A query populates the cache on a miss; a write to an aggregate invalidates every entry derived from it. Entries are not enumerable from an aggregate id, so invalidation uses **version stamps**, not key deletion.

- Redis holds a stamp per aggregate type (`ver:<module>:<aggregate>`) and, for queries targeting a single instance, per id.
- Every cache key MUST embed the stamps of the aggregates its result depends on, and a cached query MUST declare those dependencies explicitly. ⚙
- A write increments the relevant stamps. Old keys become unreachable and expire by TTL. Invalidation is therefore atomic and O(1) - it cannot half-succeed the way deleting N keys can.
- Stamps MUST be incremented **after** the transaction commits, never before. Incrementing first lets a concurrent read repopulate the cache with pre-commit data, which then stays stale until TTL.
- `KEYS` and `SCAN`-and-delete invalidation are forbidden. ⚙
- Command handlers MUST NOT touch the cache. Invalidation runs in an Application slice reacting to the domain event, through an `ICacheInvalidator` port implemented in Infrastructure. ⚙
- An invalidation failure MUST be logged as an error and MUST NOT be swallowed. The mandatory TTL is the backstop bounding how long a failed invalidation serves stale data - that is why unbounded entries are forbidden.
- A flow requiring read-your-own-writes MUST NOT be cached unless invalidation completes before the response returns. TTL expiry is not a substitute for invalidation.
- A Redis failure MUST be treated as a miss. An unreachable cache degrades latency, never availability.
- Failures and empty results SHOULD NOT be cached, unless the negative result is itself expensive to compute and then only with a short TTL.

## 6. Authorization

Every dispatchable operation is declared once, in the slice that owns it.

```csharp
// Illustrative.
public static readonly AgentOperation <Operation> = new(
    Id:     "<module>.<operation>",
    Policy: Policies.<Module>.Write,
    Kind:   OperationKind.Command,
    Human:  HumanRequirement.Required);   // unreachable without a live user
```

- Every Command and Query MUST declare a named policy. Bare `[Authorize]` and unattributed endpoints are defects. ⚙
- `AuthorizationOptions.FallbackPolicy` MUST deny, so a forgotten attribute fails closed. ⚙
- Policies are evaluated by the authorization behavior via `IAuthorizationService`, against the explicit principal from the dispatch context.
- A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor package's equivalent - MAY enforce in addition, but MUST NOT be the only enforcement point. Every operation's decision MUST be reachable from the authorization behavior, so a caller that bypasses HTTP receives the identical answer (`../SKILL.md` §7).
- The acting principal MUST travel in the dispatch context. `IHttpContextAccessor` MUST NOT be referenced outside the composition root. ⚙ Violating this silently replaces an agent's narrowed principal with the full user token and voids every agent restriction below.
- Identity-provider claims and externally-sourced permissions are mapped to policies at the composition root. Application and domain code MUST NOT read raw claims. ⚙
- Where permissions come from - a token claim, a directory, a remote authorization service - is a composition-root concern. Application and domain code name a policy, never a permission source or a role literal.
- An external authority's permission codes MUST be translated to the application's policy vocabulary at the composition root. The external authority's types and codes MUST NOT be referenced outside `Api`. ⚙ This is what keeps the authority replaceable.

### External permission authorities

A token claim cannot be unavailable. A remote authorization service can be unreachable, its answers can outlive a revocation, and it answers for the user when the caller is an agent. The rules below cover those three cases. Which provider and which authority the organization runs is recorded in `DECISIONS.md`, not here.

- When permissions are resolved from a source outside the process, that source's unavailability MUST deny. An unreachable, timing-out, or erroring authority MUST NOT grant and MUST NOT fall back to a default permission set.
- An externally-resolved permission decision MAY be cached, keyed by the acting principal. Its lifetime MUST be bounded and declared in an options class, and that value is the revocation window: a permission withdrawn at the authority continues to grant for that long. ⚙ The lifetime SHOULD be configurable. Where the authority's client caches for a fixed period it does not expose, the declared value states that period and MUST NOT differ from it. A lifetime nobody can state is a defect.
- An unexpired entry MAY be served while the authority is unreachable. An expired entry MUST NOT be served, and its lifetime MUST NOT be extended because the authority cannot be reached. An outage never lengthens the revocation window.
- Externally-resolved permissions are the user's permissions, not the caller's. For an agent-actor principal they MUST be intersected with the agent's manifest exactly as §8 requires. The lookup MUST be keyed on the acting principal from the dispatch context, never on the HTTP request.

## 7. Data Classification

`../SKILL.md` §8 defines the classes and the boundaries. This section is the mechanism.

### Declaring a class

- A class is declared **once**, on the type that carries the value: the Value Object or the strongly-typed id. The attribute lives in `SharedKernel`, so `Domain` can carry it without acquiring a framework dependency. ⚙
- An architecture test MUST assert that every property reachable from an egress boundary resolves to a class. A property whose type carries no declaration is `Restricted`, and fails at the boundary. ⚙
- The class MUST be emitted into the OpenAPI document as a vendor extension on every property of every Response DTO, so it reaches the frontend with the contract. The emitter MUST fail rather than emit a property whose class it cannot resolve. ⚙
- This is what §3's Value Object rule buys. A `string` holding a government identifier cannot be classified and fails the test; a `NationalId` value object carries its class into every DTO, log line, and cache key it ever reaches.

```csharp
// Illustrative.
[DataClass(DataClass.Restricted)]
public sealed record NationalId
{
    public override string ToString() => "***";   // never the raw value
}
```

- A `Confidential` or `Restricted` type MUST redact in `ToString()` and MUST NOT define an implicit conversion to `string`. ⚙ Interpolation into a log message is the most common way this data escapes, and it is close to invisible in review.

### Logging

- Structured logging only. A message template MUST NOT interpolate a classified value - a destructuring policy cannot redact what interpolation has already flattened into a string. ⚙
- The destructuring policy redacts by class and MUST fail closed: a type it cannot resolve is redacted, not written.
- Log the identifier of a record, never its sensitive contents. `PersonId` is loggable; `NationalId` is not.
- Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a classified value. `../SKILL.md` §6 forbids leaking internals; this extends it to the data. A validator reporting that `123-45-6789` is not a valid national id has just written that value to a log, a browser, and an error tracker in one step.

### The model-visible surface

- Every model-visible DTO MUST be explicitly marked as such. ⚙
- Reusing a UI Response DTO for a tool because the shape happens to fit is how a `Confidential` field reaches a vendor. Where the two audiences differ in class, they are two types.
- An architecture test MUST assert that no model-visible DTO contains a `Confidential` or `Restricted` property, transitively through every nested type and collection. ⚙
- Free text a user can write into - notes, comments, descriptions, reasons - MUST be treated as `Confidential` when it is model-visible. Its contents cannot be classified in advance, and in practice they routinely contain exactly what this section exists to hold back.
- A tool that needs a sensitive value in order to work is the wrong design. Move the work behind an operation and give the model the outcome, not the input: a model that must "check the national id" instead invokes an operation that checks it.
- Redaction MUST happen in the projection to the tool DTO, never in a prompt instruction. Prompt text is not a control (§8).

### Cache, events, and storage

- `Restricted` MUST NOT be written to Redis in any form, including inside a serialized DTO. ⚙
- A classified value MUST NOT appear in a cache key in cleartext. Keys derive from ids, never from values.
- Integration events and outbox rows MUST NOT carry `Restricted` payloads. The outbox is durable, replayable, and operator-readable. Reference by id. ⚙
- `Restricted` columns MUST be encrypted at rest, and the encryption MUST live in the Infrastructure mapping. The domain type MUST NOT know it is encrypted, for the same reason it MUST NOT know it is persisted (§5).

## 8. AI Agents

An agent is a caller with a fixed toolset. Its limits are enforced by code. **Prompt text is not a security control.**

### Manifest

- Each agent MUST have exactly one manifest listing the operations it may invoke.
- Tools MUST be projected from the manifest at composition time. Hand-registering a tool is forbidden. ⚙
- The scopes requested for a run MUST derive from that same manifest. Two hand-maintained lists will drift, and the drift is a privilege escalation. ⚙
- An architecture test MUST assert every manifest entry resolves to a real handler whose declared policy matches. ⚙

### Effective permissions

```
effective = user's permissions  ∩  agent manifest  ∩  operations not marked HumanRequirement.Required
```

- An agent principal MUST be a strict narrowing of the calling user's. Never equal, never wider.
- Human-initiated runs use on-behalf-of: the agent carries a down-scoped principal derived from the caller's token.
- The principal MUST carry an actor claim identifying the agent. Writes audit the agent, not only the user.
- An agent-actor principal MUST NOT start another agent run. That is the recursion cap.
- Autonomous runs require a separately declared manifest. It MUST NOT be inherited from the human-initiated one and MUST NOT contain any operation marked `HumanRequirement.Required`. ⚙

### Tool shape

- Tool parameters MUST be typed domain values.
- A tool parameter MUST NOT be a bare `string`. Every string-valued parameter is a declared wrapper type - `SearchTerm`, `CommentBody` - so that admitting a new kind of free text is a deliberate, reviewable act. ⚙
- No wrapper type may be declared for a URL, route, endpoint name, table name, column name, or query text. Anything that selects behavior is not data.
- Agent reads MUST go through the same query handlers as the UI. An agent-only data path is forbidden; it is where every control above gets bypassed.
- Tool output returned to a model MUST be a Response DTO. Domain objects and raw rows MUST NOT be serialized to a model. ⚙

Verify Microsoft Agent Framework API specifics against official Microsoft documentation. Do not infer them.

## 9. Quotas and Rate Limits

Limits attach to the caller and to the run, not to the transport. HTTP rate limiting does not bound an agent, because agent dispatch never passes through the middleware pipeline. Both are required, and neither substitutes for the other.

### Transport

- The global rate limiter MUST be configured with a default policy covering every endpoint. An endpoint outside a policy is a defect, exactly as an endpoint outside an authorization policy is. ⚙
- Limits are keyed on the acting principal, falling back to a client identity for anonymous endpoints. Keying on IP alone is not a limit; it is a shared bucket that one customer's office fills.
- Write endpoints and expensive reads MUST declare a tighter policy than the default.
- A rejection is `429` with `Retry-After`, shaped as `ProblemDetails`.

### Agent runs

- Every agent run MUST carry a budget fixed when the run starts: maximum tool invocations, maximum rows returned across the whole run, maximum wall-clock duration, and maximum model spend. ⚙
- Budgets MUST be enforced in the dispatch pipeline. A tool MUST NOT be trusted to count its own calls, and a model MUST NOT be asked to respect a limit stated in a prompt.
- Exceeding a budget MUST abort the run with a stated reason. It MUST NOT silently truncate: a partial read that looks complete is how a model reaches a confident wrong answer, and the caller cannot tell the difference.
- Commands already committed when a run aborts stay committed. The run outcome MUST report which operations completed.
- An autonomous run MUST have its own budget, declared with its own manifest (§8) and never inherited from the human-initiated one.
- Agent retries MUST be bounded and MUST reuse the original idempotency key (§4). An unbounded retry loop is indistinguishable from an attack, and with a fresh key per attempt it is also a duplicate-write bug.

### Configuration

- Limits and budgets MUST be configuration, not constants in code, and MUST fail closed: a missing or unparseable limit denies rather than admits. ⚙
- Every limit MUST emit a metric when it is hit. A limit nobody can observe being hit is not operable - it gets discovered as an outage or as a bill.

## 10. Observability

`../SKILL.md` §9 governs. This is the mechanism. What may be emitted is §7's business, not this section's: classification applies to every log line, metric label, and trace attribute.

### Correlation

- The correlation id MUST live in the dispatch context beside the principal, as a required constructor parameter. ⚙ A context that can be constructed without one will be.
- An inbound request adopts the caller's correlation id when it carries one and mints one otherwise. Every response returns it.
- An outbox row MUST persist the correlation id of the transaction that wrote it. ⚙
- The dispatcher MUST restore that correlation id when delivering. Without it the trace stops at the module boundary, which is precisely where cross-module debugging starts.
- An agent run MUST carry its own run id **and** the correlation id of the work that started it. Every tool invocation logs both, plus the step id that keys its idempotency (§4).
- A background job MUST mint a correlation id per execution and log it once at start.

### Logging

- Serilog, structured. The logging behavior (§4) is the only place a message's dispatch is recorded. A handler MUST NOT reference `ILogger`. ⚙ Handler logging duplicates what the pipeline already emits, and then drifts from it.
- Levels have fixed meanings. `Error` is a defect that needs someone. `Warning` is degraded but handled. `Information` is a business-meaningful event. `Debug` is developer detail and MUST NOT be enabled in production.
- An expected business rejection MUST NOT be logged at `Error`. A `Result` failure is an outcome, not a fault, and logging it as one teaches everybody to ignore the error log.
- Module, message type, principal id, and correlation id are supplied by the enricher. A call site MUST NOT restate them.

### Metrics and traces

- OpenTelemetry. Logs, metrics, and traces MUST share one correlation identity, or they are three tools instead of one.
- Every pipeline behavior MUST emit a span. Authorization, validation, and handler time MUST be separable in a trace: "the request was slow" is not a finding anyone can act on.
- Metrics MUST cover dispatch count and duration per message type, authorization denials, validation failures, outbox depth and delivery lag, cache hit rate, limit and budget rejections (§9), and agent run duration and spend.
- Metric dimensions MUST come from a declared closed set of keys. ⚙ A user id, an aggregate id, or any unbounded value as a dimension is a cardinality incident waiting for traffic.
- Telemetry MUST NOT be optional per environment. Sampling rates MAY differ; emission MUST NOT.
- Liveness MUST NOT depend on a downstream - a failing dependency restarts nothing. Readiness MUST check the database and the outbox dispatcher.

## 11. Configuration and Secrets

- Configuration binds to strongly-typed options classes. Outside the composition root, code MUST NOT read configuration from its source - `IConfiguration`, a process environment variable, or any other. ⚙ It receives a bound options class, which is what makes BE-96's validation reach every setting.
- Every options class MUST have a validator and MUST be registered with `ValidateOnStart`. ⚙ A misconfigured application fails to start; it does not fail on the first request that happens to touch the setting.
- An options default MUST NOT silently weaken a control. A missing limit, timeout, or policy denies (§9); it never falls back to permissive.
- Secrets come from Key Vault through managed identity in every environment above local development. A secret MUST NOT appear in any committed file. ⚙ A secret is any value that grants access on its own: connection strings, client secrets, signing keys, API keys and package-feed tokens are examples of the class, not the list of it, and `appsettings*.json`, environment files and source are where it has happened, not where the rule applies. A key generated for a test and trusted by nothing outside it is not a secret. A committed file that needs a credential MUST reference it - an environment-variable macro such as `nuget.config`'s `%NUGET_PAT%`, or a credential provider - and MUST NOT carry a placeholder to be overwritten in place. A placeholder is one local edit away from a committed secret, and a check cannot tell a substituted value from the placeholder it replaced.
- Local development uses user secrets. A developer MUST NOT need a production credential to run the application.
- Configuration that differs by environment MUST differ by value, never by code path. Outside the composition root, code MUST NOT learn which environment it is running in, by any means. ⚙ A branch on environment is a code path that no environment fully tests. Where the environment's name belongs in telemetry, the composition root puts it there - a resource attribute, a log enricher - so nothing below it needs to ask.
- A feature flag is configuration: typed, validated, and removable. A flag with no removal plan is technical debt with a switch on it.

## 12. Real-time

- SignalR is a delivery channel, never a source of truth. Every state a client can receive over SignalR MUST be recoverable via REST.
- A hub method MUST map its argument to a message, dispatch it, and return. A hub MUST NOT reference any type other than DTOs and the dispatcher. ⚙ That is what "no business logic in a hub" means in a form a check can evaluate.
- Hub authorization MUST use the same named policies as the equivalent HTTP operation. ⚙
- Messages MUST carry DTOs. Domain types MUST NOT be serialized to clients. ⚙
- Redis backplane for scale-out.

## 13. API Layer

- Every endpoint MUST be a type implementing the shared endpoint abstraction, with its Request and Response DTOs as type arguments. A route handler passed as a lambda or method group to a minimal-API route builder - `MapGet`, `MapPost`, `MapMethods`, or any other - is forbidden. ⚙ A lambda has no type to inspect, so every rule about an endpoint's signature is unenforceable against one.
- Endpoints are registered by discovery over those types at the composition root. Hand-registering one is a defect. ⚙
- An endpoint MUST map its Request DTO to a message, dispatch it, and translate `Result` to HTTP. It MUST NOT reference any type other than its DTOs, the dispatcher, and the result translator. ⚙ An endpoint branching on business state is a defect.
- Endpoints MUST NOT reference domain types. ⚙
- Success shapes: `200` with a Response DTO, `201` with a location, `204` for no content. Failures are `ProblemDetails`.
- A change to a Request or Response DTO that does not regenerate and commit the OpenAPI document is incomplete.

## 14. Testing

A rule no build check can reach is carried by a named required test below rather than by a ⚙ it cannot honour. A missing required test is a defect exactly as a violated rule is.

- **Domain**: pure unit tests, no substitutes, no I/O. Every invariant and every domain outcome MUST have a test.
- **Application**: handler tests with substituted infrastructure. Every declared policy MUST have a test proving *denial*, not only one proving success.
- **Integration**: real SQL Server via Testcontainers, real pipeline behaviors.
- **Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped or marked inconclusive to unblock a change.
- **Authorization**: every external permission authority MUST have a test proving that an unreachable, a timing-out, and an erroring authority each deny, and that an expired cache entry is not served while the authority is unreachable. Every policy MUST have a test proving an in-process dispatch with no HTTP context receives the same decision as the HTTP path. An agent-actor principal MUST have a test proving an operation the user holds but the manifest omits is denied when permissions come from the external authority.
- **Agents**: every manifest MUST have a test asserting an operation outside it is rejected, a test proving the acting principal carries the agent's actor claim, and a test proving an agent-actor principal cannot start another agent run.
- **Budgets**: every budget MUST have a test proving a run that exceeds it aborts rather than truncates.
- **Pagination**: every paginated query MUST have a test proving a request above its maximum is rejected, and a test proving no row is skipped or repeated across a page boundary.
- **Idempotency**: every command declaring a key MUST have a test proving a replayed key returns the recorded result without executing twice, and a test proving two concurrent attempts under one key resolve as one execution and one replay rather than an error.
- **Cache**: the read-side decorator MUST have a test proving an unreachable Redis is served as a miss, not as a failure.
- **Correlation**: the outbox MUST have a test proving a correlation id survives the round trip - written with the transaction, restored on delivery, present on the consumer's log line.
- **Migrations**: every migration MUST be applied to a Testcontainers database in CI and proven to run clean twice. A destructive migration MUST additionally have a test proving the previous application version still serves against the new schema.
- **Classification**: every `Confidential` and `Restricted` type MUST have a test proving it redacts in `ToString()` and in a serialized log event.
- Test names state the rule: `<Operation>_Returns<Rejection>_When<Condition>`.
- Tests MUST NOT assert on implementation details - no verifying private calls, no asserting on generated SQL.

## 15. Forbidden

`dynamic` · service location outside the composition root · static mutable state · reading the system clock in domain or application code - `DateTime.Now`, `DateTimeOffset.UtcNow`, `DateTime.Today`, or any other ambient read (inject `TimeProvider`) · `async void` outside event handlers · `.Result`, `.Wait()`, `.GetAwaiter().GetResult()` · catching `Exception` without rethrowing · `#region` · commented-out code
