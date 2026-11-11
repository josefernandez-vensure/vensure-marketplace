# Rancho modular .NET template

A modular .NET service whose build enforces the `rancho-modular-dotnet` law. Rules a check can enforce live here as code, not as prose in the law.

```bash
dotnet new install ./templates/rancho-dotnet
```

```bash
dotnet new rancho-dotnet -n MyService
```

## What enforces what

| Where | What it enforces |
|---|---|
| `build/ProjectKinds.targets` | The project reference table. Every project declares `<ProjectKind>`; a reference or package its kind doesn't allow fails the build. |
| `build/BannedSymbols*.txt` | APIs a kind may not call: ambient clock, environment, service location, the ambient principal, ambient transactions, Serilog's static logger |
| `.editorconfig` | Analyzer rules as errors: namespaces match folders, `[LoggerMessage]` logging, no swallowed `Exception`, no `async void`, no blocking on tasks, no concatenated raw SQL |
| The types themselves | A message must declare its operation (`static abstract Operation`), or it doesn't compile. `Operation.Query` can't ask for a transaction or a key. The registrar takes each endpoint's policy and rate limit from that declaration. An outcome switch with no discard arm fails the build when an outcome is added. |
| `tests/Architecture.Tests` | Placement and dependency rules that need the compiled code: domain, application, infrastructure, operations, endpoints, the composition root |
| `tests/Api.Tests` | What the built host does: every route is discovered and carries its operation's policy and rate limit; the fallback policy denies; the pipeline refuses without a transport filter; options are validated on start |

Adding a module needs no change to the checks. Create its projects with the `dotnet` CLI, declare each project's `<ProjectKind>`, and the tests pick it up.

## The platform

- `Platform.Application/Abstractions`: `Result`, problem types, the operation declaration, `ICommand`/`IQuery` and their handlers, and the dispatch context.
- `Platform.Application/Behaviors`: the pipeline, in this order: logging → authorization → validation.
- `Api/Endpoints`: the endpoint abstraction, the registrar that discovers it, and the result translator.
- `Api/Http`: problem details, deny-by-default, rate limits.

## Status

Phase 1 of `LAW-REDESIGN.md`. Increment 1 (structural checks) and slice 2a (the dispatch core) are done. Still to come: 2b, persistence (unit of work, outbox, idempotency, audit, migrations); 2c, paging; 2d, data classification and OpenAPI; 2e, concurrency and the remaining behaviour tests. Until 2b, a command saves through its repository directly.
