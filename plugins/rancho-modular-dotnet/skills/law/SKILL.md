---
name: law
description: The modular .NET law - modules as services, project layout and references, where each kind of type lives, the domain and application model, reactions and sagas, persistence, the API and its wire contract, security, data classification, and verification. The tree's own reference code is the guide for everything else. Read in full before writing, reviewing, or placing code in a tree whose CLAUDE.md names `Stack law: /rancho-modular-dotnet:law`.
---

# Modular .NET Law

This law states how code here is structured, and the few decisions this codebase makes differently from a sensible default. The codebase is the reference for everything else.

## 0. The codebase is the reference

- The tree's `CLAUDE.md` has a `## Reference` section. For each kind of code, such as a command, a query, an endpoint, a reaction, a saga or an integration test, it names the implementation to follow. Before writing code of a listed kind, read its reference and match it: structure, naming, and tests.
- Reference code is held to this law. Where the two disagree, the law wins: say so, and fix the reference in the same change or report it.
- For a kind with no reference yet, build it from this law. It is reviewed as the new reference, and its path is added to the list in the same change.
- Code not on the list is precedent, not reference. Follow it where the law and the references are silent and a second occurrence confirms it. Otherwise ask.
- The guardrails live in the codebase too. Architecture tests enforce §2 and §3, and a failing `dotnet build` or `dotnet test` is the law speaking: fix the code, not the check. A rule that can be checked gets its test in the same change that introduces the first code it governs.
- If two rules leave no compliant answer, stop and ask.

## 1. Stack

.NET 10 · EF Core on SQL Server · Mediator 3.x (source-generated) · FluentValidation · Microsoft.Extensions.Compliance · Serilog behind `ILogger` · OpenTelemetry · Azure Key Vault · xUnit v3 · NSubstitute · Shouldly · Testcontainers · ArchUnitNET

- `TreatWarningsAsErrors` and `Nullable` are on, once, in `Directory.Build.props`.
- Create solutions, projects, and references with the `dotnet` CLI (`dotnet new`, `dotnet sln add`, `dotnet reference add`, `dotnet package add`), never by writing the file.

## 2. Layout and references

```
src/
  Modules/<Module>/
    <Module>.Domain/          aggregates, value objects, domain events, repository ports
    <Module>.Application/     one folder per feature: message, handler, validator, DTO, operation
    <Module>.Infrastructure/  DbContext, repositories, query handlers, adapters
    <Module>.Contracts/       what the module publishes: wire ids, integration events, command messages, published queries
  Shared/SharedKernel/        base types; no business rules
  Platform/Platform.Application/     Result, message abstractions, page and cursor, pipeline behaviors
  Platform/Platform.Infrastructure/  outbox, audit trail, cursor codec
  Api/                        composition root and endpoints
tests/
```

A module is a bounded context. Each of its projects exists at most once; omit the ones it doesn't need.

Four facts decide every reference:

1. A module reaches another module only through that module's `Contracts`.
2. Inside a module, references point inward: `Infrastructure` → `Application` → `Domain` and `Contracts`. `Domain` and `Contracts` don't reference each other.
3. Shared projects (`SharedKernel`, `Platform.*`) reference no module.
4. Nothing references `Api`.

What each project may depend on:

| Project | Projects | Packages |
|---|---|---|
| `SharedKernel`, `<Module>.Domain` | `SharedKernel` | BCL, `Microsoft.Extensions.Compliance.Abstractions` |
| `<Module>.Contracts` | `SharedKernel`, `Platform.Application` | none |
| `<Module>.Application`, `Platform.Application` | per the facts above | Mediator and FluentValidation abstractions, `Microsoft.Extensions.*.Abstractions`; `Platform.Application` also ASP.NET Core authorization |
| `<Module>.Infrastructure`, `Platform.Infrastructure` | per the facts above | EF Core, ASP.NET Core authorization and Data Protection, SDKs of the systems they adapt, Options |
| `Api` | anything | ASP.NET Core, hosting, OpenAPI, telemetry exporters, logging provider |

Each project declares its kind as an MSBuild property; an architecture test checks the table over the resolved package closure.

## 3. Where each type lives

Placement follows dependencies, never the name.

| Interface in | Implementation in | What it is |
|---|---|---|
| `Domain` | `Domain` | a business rule spanning aggregates |
| `Domain` | `Infrastructure` | a repository port, one per aggregate root |
| `Application` | `Application` | use-case logic with no external dependency |
| `Application` | `Infrastructure` | an adapter: anything that crosses a process boundary (an HTTP client, a queue, a blob store, an identity provider) |

- `Application` is sliced by feature (`Application/<Feature>/`), never by pattern (`Handlers/`, `Commands/`). `Domain` is organized by aggregate, `Infrastructure` by technical concern (`Persistence/`, `Queries/`, `Messaging/`, `External/`).
- Infrastructure types are `internal`, except what the dispatcher's generated code must name. Each module's Infrastructure exposes one entry point, `Add<Module>Module(IServiceCollection, IConfiguration)`, and `Api` calls nothing else in it.

## 4. Domain

- An aggregate's state is private. It changes only through methods named in domain vocabulary (`Approve`, not `SetStatus`), and the aggregate is valid after every one.
- A business rejection is a returned outcome value (an enum in domain vocabulary), never an exception. A broken invariant throws.
- Ids are strongly typed, one type per concept. An aggregate holds another aggregate's id, never a reference to it.
- Primitives that carry meaning become value objects: immutable, validated in the constructor, structurally equal.
- Domain events are raised by the aggregate through a `protected` method on the root base type, and carry ids and values.
- Every aggregate carries a version it increments on every change; it is the concurrency token and the `ETag`.
- The aggregate receives the current time as an argument; the handler gets it from an injected `TimeProvider`.

## 5. Application

### Messages and handlers

- A command changes exactly one aggregate in one transaction: load it through its repository, call its method, save through the repository. It returns at most an id, an outcome, and the new version.
- If two aggregates must always change together, they are one aggregate: redraw the boundary, never widen the transaction.
- A query never loads an aggregate. Its handler lives in `Infrastructure/Queries/<Feature>/` and projects straight into the Response DTO, untracked, through LINQ over read types or parameterized SQL (`SqlQuery<T>`, never the `*Raw` APIs).
- Messages and DTOs carry the module's wire ids from `Contracts`, never domain types; the handler converts.
- The handler translates the outcome to `Result` with a switch expression over the enum, with no discard arm, so a new outcome fails the build.
- One handler per message; handlers don't call each other. To read another module, dispatch its published query.
- Handlers don't log; the logging behavior does.

### Reactions and processes

Treat every module as a separate service. Nothing changes a second aggregate inside the first one's transaction.

- The unit of work writes the aggregate's domain events to the outbox with the change. A change other modules must hear about also writes an integration event from `Contracts` there.
- A reaction is a command. The message's handler maps the message to one command of its own module and dispatches it through the pipeline, with the causing message's id as its idempotency key. It runs in its own transaction, and delivery is at least once.
- A reaction runs as the module's service principal, under a policy only that principal holds, so no HTTP caller can reach it. The original user is not re-authorized; a step that must stop when the user loses access checks that explicitly.
- Every outbox message carries, as metadata, the initiating user's id and the id of the message that caused it. A reaction passes both on to the messages it writes.
- Domain events reach reactions in their own module only. Other modules react to integration events and command messages.
- A read straight after a command may not show its reactions yet.

A module's `Contracts` publishes two kinds of message:

- An **integration event** says something happened. Any module may react to it.
- A **command message** asks the module to do something. The module that handles it owns it, just as it owns its published queries. Another module sends it by writing it to its own outbox, never by calling the module directly.

A process that spans modules is a saga:

- Use choreography by default: each module reacts to the others' events. Use an orchestrator when the process has more than three steps or its compensations must run in order. The orchestrator is an aggregate in the module that owns the process. It advances one step per transaction: each step sends the next participant's command message, and the participant's integration events (`StockReserved`, `StockReservationFailed`) are its replies.
- A failed step is undone by compensating commands, such as `Cancel`, `Refund` or `Release`. They record new state, never delete it, and are idempotent and retried until they succeed.
- Order the steps: those that can be compensated first, then the point of no return, then those that can only be retried. An effect that can't be undone, such as an email or a payment capture, comes after the point of no return.
- While a process is in flight, its aggregates show it in a pending state, such as `Reserved` or `PendingApproval`, that other operations respect.

### Operation declaration

Every message declares, once, in its slice:

```csharp
public static readonly Operation Approve = new(
    Id:          "<module>.<operation>",
    Policy:      Policies.<Module>.Write,
    Kind:        OperationKind.Command,
    Idempotency: IdempotencyMechanism.ClientSuppliedId, // Natural | ClientSuppliedId | Key
    Transaction: TransactionRequirement.Required,       // None: the effect is an external write
    RateLimit:   RateLimits.Default);
```

A message with no declaration fails to dispatch.

### Pipeline

In this order: **logging → authorization → validation → unit of work and idempotency** (commands only).

- Validation failure returns a `Result` failure; the handler never runs. Handlers assume a valid message.
- The unit of work commits the aggregate, its outbox rows, the idempotency record, and the audit record in one transaction. A concurrency conflict or unique-key violation becomes a `Result` failure in the adapter that catches it, never an exception above it.
- With an idempotency key: the record's identity is `(principal, operation, key)` under a unique index; a replay returns the recorded result; the same key with a different payload is `422`; no key is `400`.
- A command declared `Transaction: None` stages no database change and is idempotent naturally or by a client-supplied id. An effect a repeat would duplicate (an email, a payment) goes through the outbox instead.

## 6. Persistence

- One `DbContext` and one schema per module. Mapping lives in `IEntityTypeConfiguration<T>` classes; domain types carry no mapping attributes. Value objects map as owned or complex types, ids through value converters.
- A repository loads and saves the whole aggregate and exposes no `IQueryable`.
- Every schema change is a committed EF migration, never edited after merge, and safe to run while the previous release still serves. Destructive changes are split across releases.

## 7. API

- Every endpoint is a type implementing the shared endpoint abstraction, discovered at startup. It maps the Request DTO to the message, dispatches it, and translates `Result` to HTTP. Nothing else.
- Success is `200` with a DTO, `201` with a location, or `204` for a delete. Every failure is RFC 9457 problem details with a `type` the module declares; `traceId` is the current trace id.
- The OpenAPI document is generated and committed with every DTO change.

### Wire contract

- **Page:** `{ items, nextCursor, totalCount? }`, requested with `pageSize` and `cursor`. Keyset paging over a declared ordering ending in a unique key. One cursor type, encrypted with Data Protection. A page size above the operation's maximum is rejected, never clamped. A collection that isn't paged declares its bound.
- **Concurrency:** reads return `ETag` (and `etag` in the DTO). Writes to an existing resource require `If-Match`: missing or `*` is `428`, stale is `412`. A create naming its own id sends `If-None-Match: *`.
- **Idempotency:** the `Idempotency-Key` header, where the operation declares a key.
- **Rate limits:** every endpoint is limited per principal; a rejection is `429` problem details with `Retry-After` where known.
- **Data class:** every property and parameter schema carries `x-data-class` (§9).

## 8. Security and configuration

- Deny by default: `FallbackPolicy` denies, and every operation names a policy, which the authorization behavior evaluates against the principal in the dispatch context. A transport-level `[Authorize]` may add to it, never replace it.
- After the policy, check the principal may act on the specific record it names. A query filters by the principal's scope where it reads (a global query filter or a SQL predicate), never afterwards.
- Application and domain code never read raw claims; claims map to policies at the composition root.
- An external authority that can't be reached denies. Never fall back to a default permission set.
- Configuration binds to options classes with a validator and `ValidateOnStart`; nothing outside the composition root reads `IConfiguration` or the environment. A missing limit denies.
- Secrets come from Key Vault (user secrets locally) and never appear in a committed file.

## 9. Data classification

Classes: `Public`, `Internal`, `Confidential` (attributable to a person, which they'd expect to control), `Restricted` (regulated, or enough on its own to impersonate or defraud). **Unclassified is `Restricted`.**

- Declare the class once, on the value object or id type in `Domain` and on its twin in `Contracts`, with attributes from `SharedKernel`. Everything else inherits it.
- `Confidential` and `Restricted` types redact in `ToString()` and have no implicit conversion to `string`.
- Log through `[LoggerMessage]` methods with redaction enabled. Log the id, never the value.
- `Restricted` never goes into a log, an integration event, a cache, or a model's context, and its columns are encrypted at rest.

## 10. Observability

- The correlation id is the W3C trace id. A request from outside the trust boundary starts a new trace linked to the caller's.
- Outbox rows store the writing transaction's `traceparent`, and delivery restores it.
- Every pipeline behavior emits a span.
- The audit trail records every denial, every command, and every `Restricted` read: principal, operation, record ids, time, outcome. A reaction's record also names the user it acted on behalf of and the message that caused it. It is append-only and holds ids, never values.

## 11. Testing

- Domain: plain unit tests of every invariant and outcome.
- Integration: real SQL Server through Testcontainers, never an in-memory provider.
- Architecture: the reference table (§2) and placement (§3), in ArchUnitNET.
- Behaviour only a test can prove, one test each: a principal without the policy is denied; a principal without the record is denied; a keyed command replays, rejects a changed payload with `422`, and survives two concurrent attempts; a stale write is `412` and an unconditional one `428`; paging skips and repeats no row; a saga step that fails after a compensable one runs its compensations; a redelivered event changes nothing.
- Don't write a test that proves only what the build proves (a type exists, a reference resolves).

## 12. Not yet covered

Caching, calls to AI models, and real-time push have no rules here yet. Before adding the first cache, model call, or persistent client connection to a tree, stop and ask.

## 13. Verification

`dotnet build` and `dotnet test` pass before a change is done.
