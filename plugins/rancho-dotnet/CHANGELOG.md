# Changelog

## 4.2.0

- **A Stop hook builds before a turn ends.** In a tree holding
  `build/ProjectKinds.targets` with uncommitted code changes, it runs
  `dotnet build` and the architecture tests, and hands a failure back to
  Claude to fix. A turn that changed no code costs nothing.
- **The `law-reviewer` agent**, read-only, reviews a change against the
  judgment rules no check can see (§3 to §10), and ignores what the build
  enforces. §11 says to run it once a change builds and its tests pass.
- **The template ships as a NuGet template package**,
  `Vensure.Rancho.Templates`, versioned with this law (`templates/pack`).
  Its identity is now `Vensure.Rancho.Dotnet`.
- **The template's CI** (`.github/workflows/ci.yml`): build, every test,
  and both base-branch checks on every pull request.
- **New checks in the template.** A service made from an earlier version
  can take each one over from `tests/Architecture.Tests`:
  - `DiscoveryTests`: every production project is scanned, every set a
    rule walks is found, and only a known generator marks code generated.
  - `Repositories_expose_no_query` (was `Repositories_expose_no_IQueryable`,
    now also `Expression` and `IAsyncEnumerable`),
    `One_repository_per_aggregate_root`, and
    `Only_a_repository_loads_an_entity_and_only_its_own_root`.
  - `Every_read_applies_the_principals_tenant` and
    `Every_aggregate_root_is_stored_with_its_tenant`.
  - `A_module_reaches_its_context_only_from_persistence_and_queries`.
  - The query rules cover every type in `Queries/`; domain types carry no
    serialization attributes; six more broker SDKs are refused.
  - SQL Server is pinned to `2022-CU28-ubuntu-22.04` (`SqlServer.Image`).

## 4.1.0

- **Several aggregates in one transaction, through a domain service.**
  §5 no longer says a command changes exactly one aggregate. A rule over
  several aggregates of the module, such as a transfer or a bulk
  correction, is a domain service the handler calls: it loads, changes
  and saves them, and they commit together. §3 already placed a rule
  spanning aggregates in `Domain` as a domain service, so §5 contradicted
  it. Dropped "If two aggregates must always change together, they are
  one aggregate: redraw the boundary". Across modules nothing changed: a
  command can't reach another module's aggregates, so modules still meet
  only through messages and sagas.
- **Upgrade:** none for a tree that changes one aggregate per command.
  In the `rancho-dotnet` template, a domain service implements
  `IDomainService`, and its command is an `IMultiAggregateCommand`: it
  carries no `If-Match`, each aggregate's version is checked at commit,
  and a lost race answers 409. `RenameProducts` is the example.

## 4.0.0

- **Breaking.** The modular .NET law (formerly `rancho-modular-dotnet`) replaces the 3.x law, rewritten to hold only what the `rancho-dotnet` template's checks and reference code can't show. The annexes are gone; caching, model calls, real-time push and uploads are "stop and ask". Bind trees with `Stack law: /rancho-dotnet:law`.

Earlier versions are under the `law-archive-4.1` tag.
