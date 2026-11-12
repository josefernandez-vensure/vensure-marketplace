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
| The types themselves | A message must declare its operation (`static abstract Operation`), or it doesn't compile. `Operation.Query` can't ask for a transaction or a key. The registrar takes each endpoint's policy and rate limit from that declaration. An outcome switch with no discard arm fails the build when an outcome is added. A module's context derives from `ModuleDbContext`, whose sealed model maps the outbox, the idempotency records and the audit trail. A paged query declares `MaxPageSize`; the paging behavior refuses a larger page or a foreign cursor, and its `Keyset` is the only source of its ordering. |
| `tests/Architecture.Tests` | Placement and dependency rules that need the compiled code (domain, application, infrastructure, operations, endpoints, the composition root, migrations), and each module's EF model, read without a database |
| `tests/Api.Tests` | What the built host does, without a database: every route is discovered and carries its operation's policy and rate limit; the fallback policy denies; options are validated on start |
| `tests/Catalog.IntegrationTests` | What only real SQL Server shows (in Docker, through Testcontainers): idempotency, the unit of work, the audit trail, queries that read with SQL, keyset paging across ties, and migrations that run clean twice |
| `tests/Platform.Tests` | Pipeline guards no module exercises yet, such as a command that declares it doesn't persist |
| `build/check-migrations.sh` | In CI: a migration already on the base branch is never edited |

Adding a module needs no change to the checks. Create its projects with the `dotnet` CLI, declare each project's `<ProjectKind>`, derive its context from `ModuleDbContext`, and the tests pick it up.

## The platform

- `Platform.Application/Abstractions`: `Result`, problem types, the operation declaration, `ICommand`/`IQuery` and their handlers, the dispatch context, and the unit-of-work and outbox ports.
- `Platform.Application/Abstractions` also holds `Page<T>`, `IPagedQuery`, the `Keyset` ordering, and the cursor codec port.
- `Platform.Application/Behaviors`: the pipeline, in this order: logging → authorization → validation → paging (paged queries only) → unit of work and idempotency (commands only).
- `Platform.Infrastructure/Persistence`: `ModuleDbContext`, the unit of work, the outbox, and the cursor codec (Data Protection; the composition root chooses where keys persist).
- `Api/Endpoints`: the endpoint abstraction, the registrar that discovers it, and the result translator.
- `Api/Http`: problem details, deny-by-default (with every refusal audited), and rate limits.

A schema change is a migration, from the repository root:

```bash
dotnet ef migrations add <Name> --project src/Modules/<Module>/<Module>.Infrastructure --startup-project src/Api --context <Module>DbContext --output-dir Persistence/Migrations
```

## Status

Phase 1 of `LAW-REDESIGN.md`. Done: increment 1 (structural checks), slice 2a (the dispatch core), slice 2b (persistence), and slice 2c (paging). Still to come: 2d, data classification and OpenAPI; 2e, concurrency and the remaining behaviour tests; 2f, outbox delivery and reactions.
