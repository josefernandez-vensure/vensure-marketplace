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
| `tests/Architecture.Tests` | Placement and dependency rules that need the compiled code: domain, application, infrastructure, composition root |

Adding a module needs no change to the checks. Create its projects with the `dotnet` CLI, declare each project's `<ProjectKind>`, and the architecture tests pick it up.

## Status

Increment 1 of phase 1 in `LAW-REDESIGN.md`: the structural checks. The platform (`Result`, the operation declaration, the pipeline, paging, the outbox, endpoints) and its checks come in increment 2. Until then the sample module's command saves through its repository directly.
