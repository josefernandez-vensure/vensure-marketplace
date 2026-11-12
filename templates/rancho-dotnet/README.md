# Rancho modular .NET template

A modular .NET service whose build enforces the `rancho-modular-dotnet` law. Rules a check can enforce live here as code, not as prose in the law.

```bash
dotnet new install ./templates/rancho-dotnet
```

```bash
dotnet new rancho-dotnet -n MyService
```

```bash
dotnet tool restore
```

## What enforces what

| Where | What it enforces |
|---|---|
| `build/ProjectKinds.targets` | The project reference table. Every project declares `<ProjectKind>`; a reference or package its kind doesn't allow fails the build. |
| `build/BannedSymbols*.txt` | APIs a kind may not call: ambient clock, environment, service location, the ambient principal, ambient transactions, Serilog's static logger |
| `.editorconfig` | Analyzer rules as errors: namespaces match folders, `[LoggerMessage]` logging, no swallowed `Exception`, no `async void`, no blocking on tasks, no concatenated raw SQL |
| The types themselves | A message must declare its operation (`static abstract Operation`), or it doesn't compile. `Operation.Query` can't ask for a transaction or a key. The registrar takes each endpoint's policy and rate limit from that declaration. An outcome switch with no discard arm fails the build when an outcome is added. A module's context derives from `ModuleDbContext`, whose sealed model maps the outbox, the idempotency records and the audit trail. Every module's context connects to the service's one database, `ConnectionStrings:Database`, each on its own schema: `AddModuleDbContext` takes no connection string, and a missing one stops startup. A paged query declares `MaxPageSize`; the paging behavior refuses a larger page or a foreign cursor, and its `Keyset` is the only source of its ordering. A value's data class is declared once, on its type, with `SharedKernel`'s attributes; a member projected from it names that type with `[ClassOf<T>]`. Generating the OpenAPI document fails on any property or parameter whose class can't be resolved, and logs erase every class but `Public` and `Internal`. A write to an existing aggregate is an `IVersionedCommand`: the pipeline refuses it without a usable `If-Match` (428) and when the aggregate has moved on (412), and a command that changes a loaded aggregate without one fails the dispatch. The dispatch context carries the principal's tenant; a principal without one holds no Catalog policy. A reaction is an `IReaction<TMessage, TCommand>` whose only member is a static `Map`, so it can do nothing but map one message to one command; the outbox dispatcher sends that command through the pipeline. A reaction's policy requires a `ServiceIdentity`, which only the dispatcher can build and no authentication handler can produce, and only the dispatcher can enter a scope's dispatch context. |
| `tests/Architecture.Tests` | Placement and dependency rules that need the compiled code (domain, application, infrastructure, operations, endpoints, the composition root, migrations), and each module's EF model, read without a database. Data classes: everything that leaves through a response, an event or the outbox resolves to one, every logging parameter declares one, and nothing `Restricted` enters the outbox. A domain outcome is an enum. The published contracts keep their shape, and no committed configuration file holds a secret. A reaction lives in its module's Application and maps to a command of its own module; it answers a domain event of its own module, any integration event, or a command message its module owns; its command is keyed, transactional and unversioned. Integration events and command messages carry their version in their name and leave only through the outbox. |
| `tests/Api.Tests` | What the built host does, without a database: every route is discovered and carries its operation's policy and rate limit; the fallback policy denies; options are validated on start; every module's context connects to the one database; the committed OpenAPI document is the one the host generates, with `x-data-class` on every property and parameter and `maxItems` on every page; a classified logging parameter reaches the log provider redacted; every rate-limit policy answers a request beyond it with 429 problem details and `Retry-After`; a caller's `traceparent` starts a new trace, linked to the caller's; every reaction in the solution is registered with the outbox dispatcher, each reaction's policy admits its own module's service principal and no other principal, that principal holds no other policy, and no route dispatches a reaction's command |
| `tests/Catalog.IntegrationTests` | What only real SQL Server shows (in Docker, through Testcontainers): idempotency, the unit of work, the audit trail, queries that read with SQL, keyset paging across ties, and migrations that run clean twice. Every route refuses a principal its policy doesn't admit, and records it; another tenant's product is never read, listed or changed; every versioned write answers 428 and 412; every step of the pipeline emits a span, nested in order, with the handler's last. The outbox is delivered under the trace that wrote it (the delivery span, the reaction's log line and its audit record), as the module's service principal on behalf of the user who started the chain. Every message a reaction writes names that user and the message that caused it. A redelivered message changes nothing, and a message whose reaction fails waits for the next pass. The host delivers on its own. The order saga confirms an order, rejects it, or releases the stock of an order abandoned after its stock was reserved. |
| `tests/Domain.Tests` | Every invariant and outcome of every module's domain, and a check that each domain method, constructor and outcome has its test |
| `tests/Platform.Tests` | Pipeline guards no module exercises yet, such as a command that declares it doesn't persist, and a reaction that loses a race at commit, which isn't recorded under its key, so its retry runs again |
| `openapi/v1.json`, `contracts/<Module>.json` | The committed wire contract and each module's published contracts. When the code no longer matches, the test regenerates the file and fails until it is committed. |
| `build/check-migrations.sh` | In CI: a migration already on the base branch is never edited |
| `build/check-contracts.sh` | In CI: no published contract breaks against the base branch's snapshot, so editing the snapshot can't hide a break |

Adding a module needs no change to the checks. Create its projects with the `dotnet` CLI, declare each project's `<ProjectKind>`, derive its context from `ModuleDbContext` and register it with `AddModuleDbContext`, and the tests pick it up. Its tables land in its own schema of the same database. If it reacts to messages, its registration entry point calls `AddReactions` with its Application assembly and declares its reactions' policy with `RequireServicePrincipal`. `Ordering` was added this way: it is the second module of the order saga.

## The platform

- `SharedKernel`: the data classes (`[Public]`, `[Internal]`, `[Confidential]`, `[Restricted]`, `[ClassOf<T>]`) and the resolver every egress check uses.
- `Platform.Application/Abstractions`: `Result`, problem types, the operation declaration, `ICommand`/`IQuery` and their handlers, the dispatch context, and the unit-of-work and outbox ports.
- `Platform.Application/Abstractions` also holds `Page<T>`, `IPagedQuery`, the `Keyset` ordering, and the cursor codec port; and `IReaction<TMessage, TCommand>`, `ICommandMessage`, and the `Causation` a reaction's dispatch context carries.
- `Platform.Application/Behaviors`: the pipeline, in this order: logging → authorization → validation → precondition (versioned writes only) → paging (paged queries only) → unit of work and idempotency (commands only) → the handler's span. Each step emits a span.
- `Platform.Infrastructure/Persistence`: `ModuleDbContext`, the unit of work, the outbox, and the cursor codec (Data Protection; the composition root chooses where keys persist).
- `Platform.Infrastructure/Messaging`: the outbox dispatcher and the hosted service that runs it (`Outbox:PollInterval`, `Outbox:BatchSize`), the service principal, and the reactions each module registers.
- `Api/Endpoints`: the endpoint abstraction, the registrar that discovers it, and the result translator.
- `Api/Http`: problem details, deny-by-default (with every refusal audited), and rate limits.
- `Api/OpenApi`: the document's transformers, which write `x-data-class` and a page's `maxItems`.
- `Api/Telemetry`: OpenTelemetry tracing and the trust boundary: no caller's trace is adopted, only linked.

A schema change is a migration, from the repository root:

```bash
dotnet ef migrations add <Name> --project src/Modules/<Module>/<Module>.Infrastructure --startup-project src/Api --context <Module>DbContext --output-dir Persistence/Migrations
```

## Status

Phase 1 of `LAW-REDESIGN.md`. Done: increment 1 (structural checks) and increment 2: slice 2a (the dispatch core), slice 2b (persistence), slice 2c (paging), slice 2d (data classification and OpenAPI), slice 2e (concurrency, tenancy and the remaining behaviour tests), and slice 2f (outbox delivery, reactions and a saga). The features modular law §12 doesn't cover yet (caching, AI models, real-time push, uploads) are deferred until a project needs them.
