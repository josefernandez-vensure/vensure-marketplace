# .NET Notes

Read this only when the audited stack is .NET. These are the pitfalls most likely to make a hand-written LINQ/reflection rule, or a library-based rule, pass when it should fail. Check each one against the actual test code and confirm with a seeded violation.

## What reflection can and cannot see

Plain reflection (`Assembly.GetTypes()`, `Type.GetInterfaces()`, `GetConstructors()`, `GetMethods()`, `GetFields()`, `GetProperties()`) sees **signatures**: base types, implemented interfaces, fields, properties, constructor and method parameters, return types, attributes, generic arguments.

It does **not** see method bodies: a `new SqlConnection(...)`, a static call to a data-access helper, a service-locator lookup, or a local variable typed as an ORM context inside a method. A rule for R1 (data-access libraries only in persistence) built only on reflection will miss these.

Check how the suite handles this. Acceptable approaches:
- An IL-reading library (Mono.Cecil, or a library built on it such as NetArchTest or ArchUnitNET) for dependency rules, while keeping LINQ-style reflection for structural rules (port placement, setters, generic constraints), where it is excellent.
- Compile-time prevention instead: the project cannot reference the library at all, or `Microsoft.CodeAnalysis.BannedApiAnalyzers` with a `BannedSymbols.txt` bans the types in specific projects.

Seed a body-only violation (Phase 3) to prove which case applies.

## Compiler-generated types

Lambdas, local functions, iterators and `async` methods produce compiler-generated nested types (display classes, state machines). Their bodies hold code written in the outer type.
- If a rule **filters them out** (common, to reduce noise), dependencies used inside lambdas or async methods may be invisible.
- The better approach attributes compiler-generated types to their declaring type rather than dropping them.
Records also generate members (`EqualityContract`, `<Clone>$`) that can trip setter or mutability rules; check the rule handles them deliberately.

## Type loading

- `Assembly.GetTypes()` can throw `ReflectionTypeLoadException`. A rule that catches and swallows it may scan a partial set. It should fail loudly.
- `typeof(SomeMarker).Assembly` only covers assemblies the test project references. A new project that the test project does not reference is not scanned at all. Look for a test that discovers all production assemblies (from the solution, the build output, or a naming prefix) and fails on any that are not classified into a layer (catalog L5 and M2).

## Generic interfaces

Checks like `typeof(IRepository).IsAssignableFrom(t)` do not match `IRepository<T>`. Open-generic matching needs:

```csharp
t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>))
```

Seed a violation using a generic port to confirm.

## Vacuous rules

A rule shaped like `Assert.Empty(types.Where(...))` passes when the input set is empty because a namespace was renamed or an assembly was not loaded. Look for a preceding `Assert.NotEmpty` on the candidate set (catalog M1).

## Setters, init and ORM needs

- `init` accessors appear as setters in reflection (`SetMethod` is non-null; the return parameter carries the `IsExternalInit` modifier). Check whether the D2/D4 rule treats `init` as allowed or forbidden, and that this matches the skills' guidance.
- ORMs often need private parameterless constructors and private setters or backing fields. Rules should check for **public** setters, not any setter, or they will have been disabled to make the ORM work.

## Compile-time options worth checking for

- Separate projects per layer with one-way `ProjectReference`s (L1 to L4).
- Adapters declared `internal`, with `InternalsVisibleTo` only for the test and host assemblies (P5).
- A generic constraint such as `IRepository<T> where T : IAggregateRoot` (D1).
- `BannedApiAnalyzers` configured through `Directory.Build.props` so it applies solution-wide, with per-project banned lists (R1, T1).
- Central package management (`Directory.Packages.props`) makes it easy to audit which projects reference substitute database providers.

## Integration testing

- Container-based tests typically use Testcontainers for .NET with xUnit collection or class fixtures (`IAsyncLifetime`) and a reset tool such as Respawn between tests.
- Substitutes that violate T1 include the EF Core in-memory provider, SQLite used as a stand-in for a different production database, and mocked `DbContext`/`DbSet` helpers.
- Check that the fixture applies migrations the same way production does, and that image tags are pinned (T4).

## Hooks on .NET teams

Many .NET developers work on Windows. Hook scripts written only in bash will fail there. Check for PowerShell equivalents, a cross-platform .NET tool, or a documented requirement such as WSL. A typical end-of-task hook runs `dotnet test --filter "Category=Architecture"`; confirm the trait or category name the tests actually use, since a filter that matches nothing reports success.
