# Architecture Enforcement Audit

Scope: `templates/rancho-dotnet/` (the .NET checks) and the plugins that teach the assistant (`rancho`, `rancho-modular-dotnet`), at commit `13c38f4` on `law-5.0-phase1-dotnet-checks`, 2026-11-16. Phase 3 (seeded violations) was **not run**: the user chose a read-only audit. A blind spot below is proven by reading the rule's code, not by a seed. The repository's own seed proof (`templates/check-rancho-dotnet.py`, 165 seeds per `LAW-REDESIGN-dotnet-triage.md`) was not re-run.

## Summary

The enforcement system is unusually strong. The reference table is a compile-time MSBuild check, banned APIs and analyzers fail the build, and 63 architecture tests use ArchUnitNET, so method bodies are seen. The behaviour tests run against real SQL Server, and a seed-proof script exists for the rules themselves. The whole suite passes on the unmodified code, and I found no current violations. The most serious gap is coverage holes in rules that look complete: the reflection-based rules skip **nested types**, **Api** can reach the database and brokers with no rule against it, **Application** can do network and file I/O through the BCL, and **no rule asserts its candidate set is non-empty**. Several central law rules are guidance only, though each is cheap to check: one aggregate per command, value-object immutability, no `*Raw` SQL, and tenant filtering on new queries. The single highest-leverage fix is a small set of architecture-test changes in `Solution.cs`/`ReferenceTests.cs`: include nested types, extend the data-access/I/O/broker bans to `Api` and `Application`, and add a discovery test. Together these close most of the blind spots in about a day.

## Stack and layout map

| Item | Finding |
|---|---|
| Repository | A Claude Code plugin marketplace. Guidance ships as plugins (`plugins/`); the .NET checks ship as a `dotnet new` template (`templates/rancho-dotnet/`). |
| Language / runtime | C#, .NET 10 (SDK 10.0.404 here; `global.json` 10.0.100, `rollForward: latestFeature`), `Nullable`, `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild` |
| Build | `Starter.slnx`; `Directory.Build.props/.targets`; `build/ProjectKinds.targets` (Roslyn code task) |
| Tests | xUnit v3 on Microsoft.Testing.Platform, Shouldly; ArchUnitNET 0.13.5 (dependencies, IL bodies) plus reflection (shape); Testcontainers.MsSql 4.16.0 |
| Dispatch | Mediator 3 (source-generated), FluentValidation, pipeline behaviors in `Platform.Application/Behaviors` |
| Persistence / I/O | EF Core 10.0.15 on SQL Server; `Database.SqlQuery<T>` with interpolation in query handlers; outbox in `ModuleDbContext`; no HTTP clients, brokers or caches yet |

Projects, kinds and layers (every project declares `<ProjectKind>`; the build fails without one):

| Project | Kind | Layer | May reference (build-enforced) |
|---|---|---|---|
| `SharedKernel` | SharedKernel | Domain (shared) | nothing; package `Microsoft.Extensions.Compliance.Abstractions` |
| `Catalog.Domain`, `Ordering.Domain` | Domain | Domain | SharedKernel |
| `Catalog.Contracts`, `Ordering.Contracts` | Contracts | Published surface | SharedKernel, Platform.Application |
| `Platform.Application` | Platform.Application | Application (shared) | SharedKernel; Mediator/FluentValidation/`Microsoft.Extensions.*.Abstractions`/ASP.NET authorization |
| `Catalog.Application`, `Ordering.Application` | Application | Application | SharedKernel, Platform.Application, own Domain, any Contracts |
| `Platform.Infrastructure` | Platform.Infrastructure | Infrastructure (shared) | SharedKernel, Platform.Application; any package except ASP.NET Core beyond Authorization/DataProtection |
| `Catalog.Infrastructure`, `Ordering.Infrastructure` | Infrastructure | Infrastructure | above plus own Domain/Application, any Contracts |
| `Api` | Api | Host / composition root / presentation | everything; any package; ASP.NET Core |
| `tests/*` (5 projects) | Tests | Tests | everything |

Conventions: aggregate roots derive `AggregateRoot<TId>` / implement `IAggregateRoot`. Repository ports are `IRepository<TRoot, TId> where TRoot : IAggregateRoot`, declared in Domain and implemented `internal` in `Infrastructure/Persistence`. Platform ports (`IUnitOfWorkFactory`, `IOutbox`, `ICursorCodec`) sit in `Platform.Application/Abstractions`. Outbound ports have no marker: by design, "placement follows dependencies, never the name" (law §3). **Read side:** query handlers are adapters in `<Module>.Infrastructure/Queries/<Feature>/` and take the module's `DbContext` directly. This is a deliberate CQRS choice: Application never sees the ORM, so it satisfies R1 and the spirit of R2.

## Enforcement inventory

| Artifact | Path | Covers | Distribution |
|---|---|---|---|
| Project reference table | `templates/rancho-dotnet/build/ProjectKinds.targets` | L1–L5, P5, R1 (inner layers), T1 (in-memory/SQLite providers denied) | Template (copy on `dotnet new`) |
| Banned APIs | `build/BannedSymbols.txt`, `build/BannedSymbols.Inner.txt`, wired in `Directory.Build.targets` | R5 (`TransactionScope`), clock, environment, service location, ambient principal, Serilog static | Template |
| Analyzers as errors | `.editorconfig` | IDE0130, CA1848/CA2254, CA1031, EF1002/EF1003, VSTHRD | Template |
| Types by construction | `SharedKernel/IRepository.cs`, `Platform.Application/Abstractions/*`, `ModuleDbContext` | D1 (generic constraint), operation declaration, reactions, outbox | Template |
| Architecture tests (63) | `tests/Architecture.Tests/*.cs`, discovery in `Solution.cs` | L, P, R3, D2–D5, D7, paging, data classes, contracts, secrets | Template |
| Host tests (23) | `tests/Api.Tests` | routes, policies, rate limits, OpenAPI, redaction, reactions | Template |
| Integration tests (50) | `tests/Catalog.IntegrationTests` (`SqlServer.cs` assembly fixture) | T1, T3, idempotency, UoW, tenancy, outbox, saga | Template |
| Domain tests (35) | `tests/Domain.Tests` (`EveryRuleHasATest.cs`) | every domain method/outcome has a named test | Template |
| Platform tests (5) | `tests/Platform.Tests` | pipeline guards | Template |
| CI scripts | `build/check-migrations.sh`, `build/check-contracts.sh` | migration immutability, contract breaks vs. merge base | Template; **no pipeline invokes them** |
| Seed proof | `templates/check-rancho-dotnet.py` | M4: each check fails on a seeded violation | Marketplace repo only; not in the template, not in CI |
| Template CLAUDE.md | `templates/rancho-dotnet/CLAUDE.md` | law binding, `## Reference` list (all paths exist), verification commands | Template |
| Stack law (skill) | `plugins/rancho-modular-dotnet/skills/law/SKILL.md` (0.2.0) | layout, placement, domain, application, persistence, API, security, testing | Plugin, versioned |
| Prime directive | `plugins/rancho/context/prime-directive.md`, via `SessionStart` hook | reading rule; precedence; exceptions | Plugin `rancho` 4.1.0, prints in every repo |
| Hooks | `plugins/rancho/hooks/hooks.json` + `scripts/*.sh` | plugin-cache write guard, VACA session gate; **no build/test hook** | Plugin (bash) |
| Agents | `plugins/rancho/agents/vaca-stream.md` | workflow only; **no reviewer agent** | Plugin |
| Retired .NET law | `plugins/rancho-dotnet/skills/law/SKILL.md` (3.0.0) | superseded; not read in a tree bound to modular | Plugin, still published |
| CI pipelines | none (no `.github/`, no YAML anywhere) | — | — |

## Concept scorecard

"Proven by seed": **repo** means the repository's seed proof has a seed for it (by reading `check-rancho-dotnet.py`; not re-run). **none** means no seed exists. No new seeds were run in this audit.

| Concept | Rule exists | Tier | Proven by seed | Message quality | Verdict |
|---|---|---|---|---|---|
| L1 Domain depends on nothing | Yes | compile + test (`Domain_does_no_io`) | repo | Good | Enforced |
| L2 Application depends only inward | Yes | compile + test | repo | Good | Enforced, except BCL I/O (P2) |
| L3 Nothing inward depends on Infrastructure | Yes | compile | repo | Good | Enforced |
| L4 Only the host references Infrastructure | Yes | compile + test (`Api_names_only_registration_entry_points_of_infrastructure`) | repo | Good | Enforced |
| L5 Every project classified | Yes | compile (empty kind fails) | repo | Good | Weak: discovery can silently drop a project (finding 10) |
| P1 Ports distinguishable | Partly | compile (`IRepository<,>`), by design otherwise | — | — | Weak (by design) |
| P2 Adapters only in Infrastructure | Partly | compile (package allowlist) | none | — | Weak: BCL I/O in Application (finding 3) |
| P3 Use cases in Application | Yes | test (`Messages_and_handlers_live_where_they_belong`) | repo | Good | Enforced (nested gap, finding 1) |
| P4 No infra types in port signatures | Yes | compile | repo (indirect) | Good | Enforced |
| P5 Application depends on ports only | Yes | compile + `internal` adapters (test) | repo | Good | Enforced |
| R1 Data access only in Infrastructure | Yes | compile (inner) + test (`Module_application_uses_no_data_access`) | repo | Good | Weak: Api uncovered (finding 2) |
| R2 Reads through an adapter, not the ORM in handlers/controllers | Yes | compile + test | repo | Good | Enforced for Application; Api gap (finding 2) |
| R3 Repositories leak no deferred types | Yes | test (`Repositories_expose_no_IQueryable`) | repo | Good | Weak: `IQueryable` only (finding 12) |
| R4 Raw SQL only in persistence, parameterized | Partly | compile (inner) + analyzer EF1002/EF1003 | repo (EF1003) | Good | Guidance only for "never `*Raw`" (finding 5) |
| R5 UoW owned by the pipeline | Yes | banned `TransactionScope` + test (`Only_the_pipeline_touches_the_unit_of_work`) | repo | Good | Enforced; one-aggregate rule missing (finding 4) |
| D1 Repositories only for roots | Partly | compile (generic constraint) | repo | — | Weak: an interface not deriving the base is unchecked (finding 12) |
| D2 Aggregates protect invariants | Yes | test (roots) + model test (mapped types) | repo | Good | Enforced |
| D3 Reference by id | Yes | test (reflection + EF navigations) | repo | Good | Enforced |
| D4 Value objects immutable, value-equal | No | guidance (law §4) | none | — | Guidance only (finding 6) |
| D5 Persistence-ignorant domain | Yes | compile (EF attrs) + test (DataAnnotations) | repo | Good | Enforced; serialization attrs unchecked (Low) |
| D6 Rules in the domain | Partly | `EveryRuleHasATest` (tests exist per method) | — | — | Guidance only; no reviewer (finding 15) |
| D7 Events raised by aggregates, sent via outbox | Yes | `protected Raise` + test (`Published_messages_leave_only_through_the_outbox`) | repo | Good | Weak: Api not covered (finding 2) |
| D8 Aggregate boundaries | No | — | — | — | Guidance only (finding 15) |
| T1 Real infrastructure, no stand-ins | Yes | compile (InMemory/SQLite denied everywhere) | repo | Good | Enforced |
| T2 Every repository has an integration test | No | — | — | — | Missing (finding 16) |
| T3 Shared fixture | Yes | `[assembly: AssemblyFixture]`, migrations run twice | — | — | Enforced by structure |
| T4 Pinned images | No | — | — | — | Missing (`2022-latest`, finding 13) |
| T5 CI runs container tests and blocks merges | No | — | — | — | Missing (finding 8) |
| T6 Integration separable | Yes | separate project | — | — | Enforced |
| M1 No vacuous rules | No | — | none | — | Missing (finding 10) |
| M2 Complete discovery | Yes | metadata scan of build output | none | — | Weak (finding 10) |
| M3 Rules see bodies | Yes | ArchUnitNET | repo | — | Enforced; nested-type and helper gaps (findings 1, 17) |
| M4 Rules are tested | Yes | seed proof script | — | — | Weak: not shipped, not in CI (finding 14) |
| M5 Actionable messages | Yes | rule sentence + offending type + alternative | — | Good | Enforced |
| M6 Fast, categorized | Yes | arch tests ~10 s after a ~37 s build | — | — | Enforced (Low note on CLAUDE.md) |
| M7 One source of truth | Partly | — | — | — | Weak: copy-once template; law restates the table (findings 11, 14) |
| A1 CLAUDE.md short, points to references | Yes | — | — | — | Enforced |
| A2 Skills cover placement | Yes | law §2–§3 + `## Reference` | — | — | Enforced |
| A3 Guidance and tests agree | Partly | — | — | — | Contradictory on suppressions; overclaim on package closure (findings 7, 11) |
| A4 Hooks run checks | No | — | — | — | Guidance only (finding 9) |
| A5 Reviewer agent | No | — | — | — | Missing (finding 15) |
| A6 Scaffolding compliant | Yes | template passes every rule | — | — | Enforced |
| A7 Versioned, automatic distribution | Partly | plugins versioned; template not | — | — | Weak (finding 14) |

## Findings

No Critical findings: every rule I traced can fail on the violation it names, and no guidance tells the assistant to do what a test punishes. Findings 1, 2 and 10 are the closest: rules that look complete but have a silent exemption.

### [High] 1. Reflection rules skip every nested type
- **Status**: Fixed in fa1d4e3. `Solution.TypesOf` now keeps nested types and drops only generated code and what is nested in it (`IsGeneratedOrInside`, moved from `CodeTests`). A seeded nested query handler in `Catalog.Application` passed all 63 rules at `HEAD`. It now fails `Messages_and_handlers_live_where_they_belong` and `A_query_never_loads_an_aggregate`, and also the classification rule. The seed is in `check-rancho-dotnet.py`.
- **Concept**: M3, P3, R2, A3 (rule coverage)
- **Evidence**: `tests/Architecture.Tests/Solution.cs:27-28`: `TypesOf` returns `OfKind(kinds).SelectMany(a => a.Assembly.GetTypes()).Where(t => !t.IsNested && !IsGenerated(t))`. Every rule that builds its candidate set from `TypesOf` ignores a type nested inside another. Affected rules include `Messages_and_handlers_live_where_they_belong` and `A_query_never_writes`. `A_query_never_loads_an_aggregate` and `Handlers_do_not_log` take their handlers from `TypesOf`, so `WithNested` is never reached for a nested handler. Also affected are `Every_message_declares_a_valid_operation`, the `PagingTests` rules, `Ids_are_strongly_typed`, `Messages_and_responses_carry_no_domain_types`, the `ReactionTests` rules, `EndpointTests.Endpoints_reference_only_their_messages_and_dtos`, and the `ClassificationTests` sets. By reading: a query handler written vertical-slice style, as `public static class GetProduct { public sealed class Handler : IQueryHandler<…> }` in `Catalog.Application`, loads `Product` through `IProductRepository`. It compiles, because Application may reference its own Domain, and neither placement nor "never loads an aggregate" sees it. Not seeded. Whether Mediator's generator registers nested handlers is not verified.
- **Why it matters**: Nested command/handler/validator classes are a very common slice style, and an assistant reaches for them readily. The rules then pass on code they were written to refuse.
- **Recommendation** (keeps tier `test`): in `Solution.TypesOf`, drop `!t.IsNested` and exclude only compiler-generated types (`IsGenerated` already does). Fix the few rules that then see legitimate private nested rows (e.g. `GetProductHandler.ProductRow`) by filtering on what they inspect, not on nesting. Add one seed per affected family to `check-rancho-dotnet.py`.

### [High] 2. `Api` can query the database or a broker directly
- **Status**: Fixed at test tier in fa1d4e3. `Module_application_uses_no_data_access` is replaced by `Only_infrastructure_reaches_the_database`. It covers every kind except Infrastructure and Platform.Infrastructure, and adds `Dapper`. `Published_messages_leave_only_through_the_outbox` now includes `Api`. A seeded `SqlConnection` in `Api` passed at `HEAD` and now fails. The compile-tier option (`PrivateAssets="compile"`) is not done. The broker half has no seed, because a seed would need a broker package from NuGet.
- **Concept**: R1, R2, D7, L4
- **Evidence**: `build/ProjectKinds.targets:158` gives `Api` `Packages="*"`. `src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure.csproj` and `Platform.Infrastructure.csproj` reference `Microsoft.EntityFrameworkCore.SqlServer` without `PrivateAssets`, so EF Core and `Microsoft.Data.SqlClient` flow to `Api` at compile time. The two rules over `Api` (`ReferenceTests.cs:41-49`, `EndpointTests.cs:24-44`) only flag targets **owned by a production assembly** (`Owner(d.Target)?.Kind is …`), so `new SqlConnection(...)`, `DbContext` or Dapper in an `Api` class matches nothing. `Module_application_uses_no_data_access` covers only `Kinds.Application` (`ReferenceTests.cs:31`). `Published_messages_leave_only_through_the_outbox` checks broker SDKs in Domain, Application, Contracts, Infrastructure and Platform.Application but not `Api` (`PersistenceTests.cs:54`). Not seeded. No current violation: `grep` finds no EF, SqlClient or `System.Data` use in `src/Api`.
- **Why it matters**: The composition root is where a "quick report endpoint" or health query lands. It bypasses the pipeline, the tenant scope, authorization and the audit trail.
- **Recommendation**: Move to `test`, and optionally to `compile`. (a) Generalise the data-access rule to every kind except Infrastructure and Platform.Infrastructure, and add the broker list for `Api`:
  ```csharp
  DependenciesOf(ArchTypesOf(Kinds.All.Except([Kinds.Infrastructure, Kinds.PlatformInfrastructure]).ToArray()))
      .Where(d => DataAccess.Any(ns => InNamespace(d.Target, ns)))   // System.Data, Microsoft.Data, Microsoft.EntityFrameworkCore, Dapper
  ```
  The composition root's `AddModuleDbContext` calls go through registration entry points, so they need no exemption. (b) For compile tier, set `PrivateAssets="compile"` on the EF packages in Infrastructure projects, so EF types don't reach `Api`'s compiler. Test projects reference EF directly.

### [High] 3. Application and Platform.Application can do network and file I/O through the BCL
- **Concept**: P2, L2
- **Evidence**: `Domain_does_no_io` (`DomainTests.cs:33-42`) bans `System.Net`, `System.Data`, `Microsoft.Data`, EF and `System.IO.File*` for **Domain and SharedKernel only**. For Application, only `System.Data`/`Microsoft.Data`/EF are banned (`ReferenceTests.cs:28-35`). `ImplicitUsings` gives Application `global using System.Net.Http;` (`Catalog.Application/obj/.../Catalog.Application.GlobalUsings.g.cs:6`). Law §3 says an adapter, "anything that crosses a process boundary (an HTTP client…)", is implemented in Infrastructure. Not seeded.
- **Why it matters**: The package allowlist stops SDK adapters from landing in Application, but `new HttpClient()`, `File.ReadAllText` and sockets compile there. An outbound adapter in Application is exactly the P2 violation the catalog describes.
- **Recommendation** (`compile`): add to `build/BannedSymbols.Inner.txt`, which already applies to Domain, Contracts, Application and Platform.Application:
  ```
  T:System.Net.Http.HttpClient;Cross a process boundary through a port; its adapter lives in Infrastructure/External.
  T:System.Net.Sockets.Socket;…
  T:System.IO.File;…
  T:System.IO.Directory;…
  T:System.IO.FileStream;…
  ```
  Alternatively, extend `Domain_does_no_io` to `Kinds.Application, Kinds.PlatformApplication, Kinds.Contracts`.

- **Status**: Fixed at compile tier in 7ca13d6. `BannedSymbols.Inner.txt` bans the `System.Net` namespace (HTTP, sockets, mail and the rest), `File`, `FileInfo`, `FileStream`, `Directory`, `DirectoryInfo`, `DriveInfo`, `FileSystemWatcher`, `RandomAccess`, `System.IO.Pipes` and `Process` in SharedKernel, Domain, Contracts, Application and Platform.Application. The implicit `using System.Net.Http;` doesn't trip it. Seeds: `new HttpClient()` and `File.ReadAllText` in `Catalog.Application` both fail the build with RS0030. The analyzer flags uses, not signatures, so a member that only names `Socket` in its signature still compiles there; `Domain_does_no_io` still catches that in Domain and SharedKernel. Its own seed moved from `File.Exists` to `System.Data`, because the build now refuses the old one.

### [High] 4. "A command changes exactly one aggregate" is guidance only
- **Concept**: R5, D8
- **Evidence**: Law §5 says a command changes exactly one aggregate in one transaction, and "Nothing changes a second aggregate inside the first one's transaction". No test or runtime guard counts changed aggregates. `UnitOfWork.cs:38-45` already enumerates `ChangeTracker.Entries<IAggregateRoot>()` for versions and the unversioned-write guard. All 9 current command handlers call `.Save(` exactly once.
- **Why it matters**: This is the rule the whole saga/reaction design rests on, and it is cheap to check where the pipeline already looks.
- **Recommendation** (guidance → `test`/runtime): in `UnitOfWorkBehavior`, fail the dispatch when `Entries<IAggregateRoot>().Count(e => e.State is Added or Modified or Deleted) > 1`, with a `Platform.Tests` case that stages two roots, and a seed.

- **Status**: Resolved by changing the law, not by enforcing it. A guard that refused a second changed aggregate landed in 46055ff and was then removed: a rule over several aggregates of one module (a transfer, a bulk correction) is a domain service, and its aggregates commit together. `rancho-dotnet` 4.1.0 says so in §5 (§3 already placed a rule spanning aggregates in Domain as a domain service, so the law contradicted itself). Across modules nothing changes: a command can't reach another module's aggregates. The template adds `IDomainService`, `IMultiAggregateCommand` (no `If-Match`; versions checked at commit; a lost race answers 409), the `RenameProducts` slice through `ProductRenamer`, and the rule that a command over several aggregates is never also versioned (f86769e).

### [High] 5. "Never the `*Raw` APIs" isn't enforced, and `A_query_never_writes` misses the Raw and Interpolated forms
- **Concept**: R4
- **Evidence**: Law §5: "parameterized SQL (`SqlQuery<T>`, never the `*Raw` APIs)". The check is EF1002/EF1003 (`.editorconfig:382-384`), which refuses only concatenated or interpolated SQL. `LAW-REDESIGN-dotnet-triage.md:22` says "A raw-SQL call with a constant string is no longer refused." `A_query_never_writes` (`ApplicationTests.cs:30-38`) matches `c.Method.StartsWith(w + "(")` for `ExecuteSql`/`ExecuteSqlAsync`, so `ExecuteSqlRawAsync("DELETE …")` and `ExecuteSqlInterpolatedAsync` in a query handler pass both checks. Not seeded.
- **Why it matters**: The law states a rule the build doesn't hold, against the redesign's own principle 1. A write hidden in a query handler escapes the unit of work, idempotency and audit.
- **Recommendation**: Ban the Raw APIs in `build/BannedSymbols.txt`, keeping the reason in the message (`compile`). Migrations use `migrationBuilder.Sql`, not these. Alternatively, drop the sentence from the law. In `A_query_never_writes`, match by prefix (`"ExecuteSql"`, `"ExecuteUpdate"`, `"ExecuteDelete"`, `"SaveChanges"`) instead of exact names.

- **Status**: Fixed in 7ca13d6. `BannedSymbols.txt` bans all seven overloads of `ExecuteSqlRaw`, `ExecuteSqlRawAsync`, `SqlQueryRaw<T>` and `FromSqlRaw<T>` in every non-test project (compile tier). A doc ID without parameters matches only a parameterless overload, so each overload is listed. A constant-string `ExecuteSqlRawAsync` fails the build with RS0030. `A_query_never_writes` now matches the prefixes `SaveChanges`, `ExecuteUpdate`, `ExecuteDelete` and `ExecuteSql`. A seeded `ExecuteSqlInterpolatedAsync` in `GetProductHandler` fails it. The law's sentence is now true as written, so it is unchanged.

### [High] 6. Value-object immutability and value equality are guidance only
- **Concept**: D4
- **Evidence**: Law §4 says value objects are "immutable, validated in the constructor, structurally equal". `Aggregate_state_is_private` covers only `IAggregateRoot` types (`DomainTests.cs:12-13`). `Mapped_domain_types_have_no_public_setter_or_parameterless_constructor` covers only EF **entity** types, and `ProductName` maps through a value converter (`ProductConfiguration.cs:15`), so it is not one. `ProductName` is compliant today (sealed record, get-only).
- **Why it matters**: A `{ get; set; }` on a value object, or a class without value equality, passes every check.
- **Recommendation** (`test`): for every non-root, non-event, non-interface Domain type, require no public setters (including `init`, matching the root rule), no public instance fields, and either `record` (`EqualityContract` present) or a declared `Equals(object)` override.

- **Status**: Fixed at test tier in 46055ff. The recommendation above was wrong: `StockReservation` is a child entity, mutable on purpose, and would have failed a value-equality rule. SharedKernel now has `Entity<TId>` (which `AggregateRoot<TId>` and `StockReservation` derive) and `ValueObject` (components-based equality; `ProductName` derives it instead of being a record, so no `with` bypasses its constructor). `Every_domain_type_is_a_building_block` requires every Domain type to be an entity, a value object, an event, an id, an enum, an interface or a static class; `Value_objects_are_immutable` refuses any setter (including `init`) and any non-readonly field; `Aggregate_state_is_private` now covers child entities. Seeds: a setter on `ProductName`, a `record Sku`, and a public setter on `StockReservation` each fail their rule.

### [High] 7. Contradictory guidance on suppressing an analyzer
- **Status**: Resolved on main by phases 4 and 5. The prime directive no longer names `LAW-EXCEPTIONS.md`; core law §1 and `rancho-dotnet` §1 both say a suppression carries its reason in a comment beside it.
- **Concept**: A3
- **Evidence**: The prime directive, printed at every `SessionStart` (`plugins/rancho/context/prime-directive.md`), says: "A comment arguing that a rule does not apply is a defect, never an exemption; the one exemption is an entry in `LAW-EXCEPTIONS.md`." The template does the opposite. `.editorconfig:379` says "A deliberate fault-isolation boundary suppresses this with its reason", and the reference code does so (`OutboxDelivery.cs:23`, `OutboxDispatcher.cs:71`, `[SuppressMessage("Design","CA1031", Justification=…)]`). `LAW-REDESIGN.md:78` adopts suppression comments. The modular law is silent. The prime directive's standalone clause ("give way to that law's own") doesn't settle a subject the law doesn't address.
- **Why it matters**: When CA1031 fires at a legitimate boundary, the build demands a suppression and the always-loaded directive calls one a defect. The assistant will either stall or add a `LAW-EXCEPTIONS.md` the new design drops.
- **Recommendation**: Add one line to modular law §0 or §13: "An analyzer error at a deliberate boundary is suppressed with `[SuppressMessage]` and its justification, never in `.editorconfig`." Then make the prime directive conditional, as phase 5 of `LAW-REDESIGN.md` plans.

### [High] 8. No CI: migration-immutability and contract-break checks never run
- **Status**: Fixed (uncommitted). The template ships `.github/workflows/ci.yml`: build, every test with SQL Server on the runner, and both base-branch checks on every pull request; a service generated from the package carries it, renamed to its solution. This repository's `.github/workflows/templates.yml` builds and tests each template, packs the .NET one and runs both seed proofs when `templates/` changes. The React job needs a `NODE_AUTH_TOKEN` repository secret. Not yet run on GitHub.
- **Concept**: T5, M6
- **Evidence**: No `.github/` or pipeline file exists in the repository or the template (`find` for `*.yml`/`*.yaml` returns nothing). `build/check-migrations.sh` and `build/check-contracts.sh` say "Run in CI on every pull request", and the template README's "What enforces what" table lists them as enforcing BE-87 and BE-85. Nothing invokes them. The seed proof isn't run automatically either.
- **Why it matters**: As shipped, a new service has two rules that look enforced but can't fail until someone wires a pipeline. The Testcontainers suite also blocks no merge.
- **Recommendation**: Ship `templates/rancho-dotnet/.github/workflows/ci.yml`: `dotnet build`, `dotnet test --solution Starter.slnx` on `ubuntu-latest` (Docker is available), and both scripts on `pull_request` with `fetch-depth: 0`. Add a workflow to this repository that runs `python -I templates/check-rancho-dotnet.py` when `templates/**` changes.

### [High] 9. No hook runs the build or the architecture tests
- **Status**: Fixed at hook tier (uncommitted). `rancho-dotnet` gains a Stop hook (`scripts/verify-checks.sh`): in each tree with `build/ProjectKinds.targets` and uncommitted code changes it runs `dotnet build` and the architecture tests, and exits 2 with the failing lines; it skips when `stop_hook_active`, so it can't loop. Tested on a clean tree (exit 0), a rule violation and a compile error (exit 2 with the message).
- **Concept**: A4
- **Evidence**: `plugins/rancho/hooks/hooks.json` has `SessionStart`, `PreToolUse` (plugin-cache guard, VACA stream), `SubagentStop` and `UserPromptSubmit` (session gate). Nothing builds or tests. Law §13 ("`dotnet build` and `dotnet test` pass before a change is done") and template `CLAUDE.md` are text only. Measured cost: build 37 s, architecture tests 10 s.
- **Why it matters**: The redesign's premise is "the failing build tells the agent". That holds only if the agent runs the build, and today nothing makes it.
- **Recommendation** (guidance → `hook`): a `Stop` hook in `rancho-modular-dotnet`. It activates only when the working tree holds `build/ProjectKinds.targets`, runs `dotnet build` and `dotnet test --project tests/Architecture.Tests --no-build`, and exits 2 with the failing lines. Bash works on Windows through Git Bash, which Claude Code requires, and matches the existing hooks.

### [High] 10. Every rule can go vacuous, and discovery can silently drop a project
- **Status**: Fixed at test tier (uncommitted). `Every_production_project_is_scanned` reads the `.slnx` and each project's `<ProjectKind>`, and names a project the solution doesn't list, one outside `src/`, or one whose assembly lost its kind metadata (`GenerateAssemblyInfo` false). `Every_rule_has_something_to_check` fails when any set a rule walks is empty. Four seeds.
- **Concept**: M1, M2, L5
- **Evidence**: No rule asserts a non-empty candidate set: `grep NotEmpty|ShouldNotBeEmpty` in `tests/Architecture.Tests` finds nothing, and `ShouldHaveNoViolations` (`Solution.cs:98-104`) only checks for offenders. Discovery (`Solution.cs:108-127`) keeps an assembly only if it carries `[AssemblyMetadata("ProjectKind", …)]`. Those attributes come from `AssemblyAttribute` items, which the SDK writes only when `GenerateAssemblyInfo` is true (`Microsoft.NET.GenerateAssemblyInfo.targets:57-60`). A project, or a consumer's `Directory.Build.props`, setting `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` is a common legacy pattern with a shared `AssemblyInfo.cs`. Its assemblies then pass the build-time kind check but drop out of all 63 architecture tests, which stay green. Separately, `Architecture.Tests.csproj` references only `../../src/**/*.csproj`, so a project outside `src/` is never scanned. Not seeded.
- **Why it matters**: A green architecture suite is then proof of nothing, and no message says so.
- **Recommendation** (`test` + `compile`):
  ```csharp
  [Fact] public void Every_production_project_is_scanned() {
      // every project in Starter.slnx whose <ProjectKind> isn't Tests appears in Solution.Assemblies,
      // and every kind in Kinds.All has at least one assembly
  }
  ```
  In `RanchoCheckProjectKind`, fail when `'$(GenerateAssemblyInfo)' == 'false'`. Give `ShouldHaveNoViolations` an overload that takes the candidate set and fails when it is empty, for rules whose set must exist (aggregates, messages, handlers, endpoints, contexts).

### [Medium] 11. The law overclaims the package check and restates a table that has drifted
- **Status**: Resolved on main by phase 2. The 4.x law no longer restates the table or claims a closure check; it names `build/ProjectKinds.targets`.
- **Concept**: A3, M7
- **Evidence**: Law §2 (`SKILL.md:67`) says "an architecture test checks the table over the resolved package closure". The check is MSBuild (`ProjectKinds.targets:189-267`) over **direct** `PackageReference`s only, and no architecture test checks packages. The law's table also differs from the targets file. It lists Infrastructure packages as "EF Core, ASP.NET Core authorization and Data Protection, SDKs…, Options", where the targets allow `*` minus ASP.NET Core. It lists Application's FluentValidation as "abstractions", where the targets admit the full `FluentValidation`.
- **Why it matters**: The assistant is told a transitive leak would be caught when it wouldn't. Two copies of the table drift, against the redesign's principle 3.
- **Recommendation**: Replace the law's table with "The table is `build/ProjectKinds.targets`; the build enforces it", or make the task read `obj/project.assets.json` to check the closure.

### [Medium] 12. Repository rules cover only interfaces that use the base, and only `IQueryable`
- **Status**: Fixed at test tier (uncommitted), narrowed: law §3 on main lets Domain declare non-repository ports, so the rule is that only a repository port returns an entity, only its own root (`Only_a_repository_loads_an_entity_and_only_its_own_root`), one port per root (`One_repository_per_aggregate_root`), and no repository takes or returns `IQueryable`, `Expression` or `IAsyncEnumerable` (`Repositories_expose_no_query`). Four seeds.
- **Concept**: D1, R3
- **Evidence**: The `IRepository<TRoot,TId> where TRoot : IAggregateRoot` constraint binds only interfaces that derive from it. `Repositories_expose_no_IQueryable` (`DomainTests.cs:89-99`) scans only those and only for `IQueryable`. A Domain interface `IStockReservationRepository { Task<StockReservation?> Find(…) }` for a non-root entity, or a method taking `Expression<Func<Product,bool>>`, passes. Law §3: a Domain interface implemented in Infrastructure is "a repository port, one per aggregate root". Not seeded.
- **Recommendation** (`test`): every Domain interface with an implementation in an Infrastructure assembly derives `IRepository<,>`, and at most one exists per root. No repository member takes or returns `Expression<>`, `IQueryable` or `IAsyncEnumerable<TRoot>`.

### [Medium] 13. Container images are not pinned
- **Status**: Fixed (uncommitted). `SqlServer.Image` pins `2022-CU28-ubuntu-22.04` (16.0.4325.4); the seed script reads that line. The cached `2022-latest` on this machine was an older build, 16.0.4315.2.
- **Concept**: T4
- **Evidence**: `tests/Catalog.IntegrationTests/SqlServer.cs:40` and `templates/check-rancho-dotnet.py:359` both use `mcr.microsoft.com/mssql/server:2022-latest`.
- **Recommendation**: Pin a CU tag that matches production (e.g. `2022-CU<n>-ubuntu-22.04`) in one constant, and have the seed script read the same value.

### [Medium] 14. The checks are copy-once and unversioned, and the seed proof doesn't ship
- **Status**: Fixed in part (uncommitted). `templates/pack` packs the template as `Vensure.Rancho.Templates`, its version read from `plugins/rancho-dotnet/.claude-plugin/plugin.json`; identity is now `Vensure.Rancho.Dotnet`. A service generated from the package builds and passes 216 of 216. The seed proof stays in this repository rather than in the template: its seeds edit the sample modules, which a service deletes. A service made from an earlier version still keeps its copy of the checks; the changelog lists the checks each version adds. A shared checks package remains the longer-term fix.
- **Concept**: A7, M4, M7
- **Evidence**: The template installs from a clone (`README.md`: `dotnet new install ./templates/rancho-dotnet`). `template.json` has no version, and nothing ties it to `rancho-modular-dotnet` 0.2.0. A service made from it keeps its copy of `Architecture.Tests` and `ProjectKinds.targets`, so a fix to a rule, such as findings 1–10, never reaches existing services. `check-rancho-dotnet.py` sits outside the template directory, so consumers don't get the rules' own test, and it checks only that the named test fails, not its message. This is open decision 2 in `LAW-REDESIGN.md`.
- **Recommendation**: Short term, pack the template as a versioned NuGet template package released with the plugin version, and move the seed script into the template (`build/prove-checks.py`). Longer term, move `Solution.cs`, the rules and `ProjectKinds.targets` into a package (`Rancho.Architecture` + an MSBuild SDK) that services update.

### [Medium] 15. No reviewer for the judgment rules
- **Status**: Fixed (uncommitted). `plugins/rancho-dotnet/agents/law-reviewer.md`: read-only, its checklist is the law's judgment rules (§3 to §10), and it explicitly excludes what the build checks. Law §11 says to run it once a change builds and its tests pass.
- **Concept**: A5, D6, D8
- **Evidence**: The only agent is `plugins/rancho/agents/vaca-stream.md`, a workflow runner. Unchecked judgment rules in the law include: rules in the domain (§4), aggregate boundaries (§5 "redraw the boundary"), saga step ordering and compensations (§5), record-level authorization after the policy (§8), and destructive migrations split across releases (§6, triage notes).
- **Recommendation**: A read-only `law-reviewer` agent in `rancho-modular-dotnet`, whose checklist is limited to exactly these items and explicitly excludes what the build checks.

### [Medium] 16. Tenant filtering and per-repository integration coverage aren't generalized
- **Status**: (a) and (b) fixed at test tier (uncommitted): `Every_read_applies_the_principals_tenant` (each Infrastructure query handler and repository calls `DispatchContext.Tenant`) and `Every_aggregate_root_is_stored_with_its_tenant` (a Guid `TenantId` in the model). Two seeds. (c), one other-tenant test over every GET route, is open: each route needs a record to look up.
- **Concept**: T2, security (law §8)
- **Evidence**: The triage notes "Nothing yet checks that a new query applies the tenant predicate" (`LAW-REDESIGN-dotnet-triage.md:116`). `TenancyTests.cs` names seven operations one by one, while authorization (BE-122) is one test over every mapped route. No rule ties a new repository or module to an integration test. All three current query handlers and both repositories do filter by tenant.
- **Recommendation**: (a) An architecture test that every `IQueryHandler` in Infrastructure, and every repository implementation, depends on `DispatchContext.get_Tenant`, using ArchUnitNET's method-call dependency. (b) A model test that every aggregate entity has the `TenantId` shadow property. (c) One integration test over every GET route that asserts another tenant's id returns 404. That reaches new modules and repositories without new tests, and covers T2 in practice.

### [Low] 17. Body rules don't follow calls into helpers
- **Status**: Fixed (uncommitted). The query rules walk every type in an Infrastructure `Queries/` namespace, not only handlers. Two seeds.
- **Concept**: M3
- **Evidence**: `A_query_never_writes` and `A_query_never_loads_an_aggregate` scan `QueryHandlers.SelectMany(WithNested)`, the handler and its nested types only. A write or aggregate load moved into an `internal static class` in `Queries/` passes.
- **Recommendation**: Scan every type in `<Module>.Infrastructure.Queries.*`, not only the handler types.

### [Low] 18. Smaller gaps
- **Status**: Fixed (uncommitted): serialization attributes (System.Text.Json, DataContract, Newtonsoft) refused on Domain types; `A_module_reaches_its_context_only_from_persistence_and_queries`; `Only_a_generator_marks_code_generated` closes the `[GeneratedCode]` escape; six more broker namespaces (Redis left out, since it is also a cache); the Architecture.Tests inner-loop command in `CLAUDE.md`. Four seeds.
- **D5**: Only `System.ComponentModel.DataAnnotations` attributes are refused on Domain types (`DomainTests.cs:76-84`). `System.Text.Json.Serialization` and `DataContract` attributes are not, and domain events do reach the outbox serializer, so decide and state which is intended.
- **R1 within Infrastructure**: `DbContext` may be used from `Messaging/` or `External/`; nothing limits it to `Persistence/` and `Queries/`. The platform's outbox legitimately does this; a module's `External/` shouldn't.
- **Escape hatch**: `Solution.IsGenerated` drops any type marked `[GeneratedCode]`, so a hand-written type with that attribute skips every reflection rule.
- **Broker list** (`PersistenceTests.cs:52`) is hand-enumerated and misses e.g. `StackExchange.Redis` pub/sub, `Google.Cloud.PubSub` and `NATS`.
- **M6**: `CLAUDE.md` gives only the full suite, which needs Docker. Add the ~10 s `dotnet test --project tests/Architecture.Tests` as the inner-loop command.

## Current violations in the codebase

None found. The full suite passes on the unmodified template:

| Command | Result | Time |
|---|---|---|
| `dotnet build Starter.slnx` | 0 warnings, 0 errors | 37 s |
| `dotnet test --project tests/Architecture.Tests --no-build` | 63/63 passed | 10 s |
| `dotnet test --project tests/Domain.Tests --no-build` | 35/35 passed | 7 s |
| `dotnet test --project tests/Platform.Tests --no-build` | 5/5 passed | 6 s |
| `dotnet test --project tests/Api.Tests --no-build` | 23/23 passed | 10 s |
| `dotnet test --project tests/Catalog.IntegrationTests --no-build` (Docker) | 50/50 passed | 29 s |

Manual sweeps for what the rules miss (excluding `obj/`, `bin/` and migrations):

| Rule | Location | Caught by existing rules? |
|---|---|---|
| Data access, `HttpClient`, `System.IO.File`, `*Raw` SQL, `BeginTransaction` outside Infrastructure | none found in `src/` | n/a |
| Public setters / `init` on Domain or SharedKernel types | none found | n/a (D4 unchecked) |
| JSON/serialization attributes on Domain types | none found | n/a |
| More than one `.Save(` per command handler | none (9 handlers, 1 each) | n/a (one-aggregate rule unchecked) |
| Query handlers / repositories without tenant filter | none (3 handlers, 2 repositories filter) | partly (per-operation tests) |
| Suppressed analyzer at a boundary | `OutboxDelivery.cs:23`, `OutboxDispatcher.cs:71` | Not a violation under the template's convention; one under the prime directive's wording (finding 7) |

## Recommended roadmap

1. **Close the architecture-test blind spots** (S–M): include nested types in `TypesOf` (1). Apply the data-access and broker bans to `Api` (2). Add a discovery test plus the `GenerateAssemblyInfo` guard (10). Add a seed for each to `check-rancho-dotnet.py`.
2. **Move guidance-only rules into checks** (S each): ban BCL I/O types in `BannedSymbols.Inner.txt` (3). Ban the `*Raw` SQL APIs and fix the `A_query_never_writes` matcher (5). Add the one-aggregate guard to the unit of work (4). Add the value-object rule (6). Widen the repository rules (12).
3. **Wire the feedback loops** (S–M): a CI workflow in the template that runs the full suite and both scripts (8). A `Stop` hook that builds and runs the architecture tests (9). A repository workflow for the seed proof on `templates/**` (8, 14).
4. **Generalize tenancy** (M): architecture test on `DispatchContext.Tenant` use, plus a route-wide cross-tenant integration test (16).
5. **Fix the guidance** (S): one suppression line in modular law and a conditional prime directive (7). Replace the law's package table with a pointer to `ProjectKinds.targets` (11).
6. **Pin images** (S) (13).
7. **Reviewer agent for judgment rules** (S) (15).
8. **Distribution** (M–L): a versioned template package now, and an architecture-rules package later, so rule fixes reach existing services (14).

## Not verified

- **Seeded violations (Phase 3)**: not run, at the user's request. Findings 1, 2, 3, 5, 10, 12 and 17 rest on reading the rule code and the SDK targets. Each should be confirmed with a seed before or alongside its fix.
- **The repository's own seed proof** (`check-rancho-dotnet.py`, 165 seeds): not re-run, because it modifies copies of the template. Its "caught" results are taken from `LAW-REDESIGN-dotnet-triage.md`.
- **Whether Mediator's source generator registers nested handlers** (affects how realistic finding 1 is at runtime): not checked.
- **How production applies migrations**: `Program.cs` doesn't migrate, and the fixture uses `MigrateAsync`. Whether that matches production deployment (T3) is not in the repository.
- **The retired `rancho-dotnet` law and the React law**: out of scope. In a tree bound to `rancho-modular-dotnet`, neither is read.
