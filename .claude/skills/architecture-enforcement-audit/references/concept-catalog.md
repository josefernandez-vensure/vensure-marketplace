# Concept Catalog

Each concept has an ID for referencing in the report, what "enforced" looks like, the ideal tier, and the common ways it is silently weak. Translate every concept to the stack discovered in Phase 0.

## Contents
- L: Layering and dependency direction
- P: Ports and adapters
- R: Data access through repositories
- D: DDD tactical design
- T: Integration testing against real infrastructure
- M: Meta-rules (the quality of the rule suite itself)
- A: Assistant layer and distribution

---

## L: Layering and dependency direction

**L1. Domain depends on nothing else in the solution and on no infrastructure library.**
Ideal tier: compile (no project references) plus test (no third-party persistence, serialization or framework types).
Weak when: the Domain project references a shared "Common" project that itself references infrastructure libraries, so the dependency arrives transitively.

**L2. Application depends only on Domain.**
Ideal tier: compile plus test.
Weak when: Application references an infrastructure package "just for an interface" (for example an ORM abstractions package).

**L3. Infrastructure depends inward only; nothing inward depends on Infrastructure.**
Ideal tier: compile.

**L4. Only the composition root (host or startup project) references Infrastructure, and dependency registration of adapters happens there or in Infrastructure-owned registration extensions.**
Ideal tier: compile plus test.
Weak when: Application registers infrastructure services, or presentation code instantiates adapters directly.

**L5. Every production project is classified into exactly one layer.**
Ideal tier: test that fails when an unclassified project appears.
Weak when: rules enumerate assemblies by hand, so a new project is silently exempt from every rule.

## P: Ports and adapters

**P1. Inbound and outbound ports are distinguishable in code** (marker interface, base interface, namespace, or attribute), not only by convention in people's heads.
Ideal tier: compile-visible convention plus test.
Weak when: the only signal is a folder name that a test does not check.

**P2. Outbound ports are declared in Application (or Domain) and implemented only in Infrastructure.**
Ideal tier: test.
Weak when: the test checks namespaces rather than the marker, so an implementation placed in a differently named folder escapes. Also weak when generic port interfaces are not matched (open-generic comparison missing).

**P3. Inbound ports (use cases) are implemented in Application.** This is legitimate and must not be flagged by P2. A rule that cannot tell P2 from P3 will either produce false positives or have been loosened until it catches nothing.

**P4. Port signatures contain no infrastructure types** (ORM contexts and sets, lazy query types, HTTP response types, driver-specific types, framework request objects).
Ideal tier: test over method signatures of port interfaces.

**P5. Application code depends on ports, never on concrete adapters.**
Ideal tier: compile (adapters are internal or in a project Application cannot reference) plus test.

## R: Data access through repositories

**R1. Only the persistence part of Infrastructure may depend on data-access libraries** (ORM, micro-ORM, raw database drivers, database-specific clients). This applies to reads as much as writes.
Ideal tier: test that inspects method bodies, not only signatures (see dotnet-notes.md if .NET).
Weak when: the library list is incomplete (for example the ORM is banned but the raw driver is not), or only constructor dependencies are checked.

**R2. Read-only access also goes through a repository** (for example read-model or query repositories behind ports). Query handlers, controllers and background jobs do not take the ORM context directly.
Ideal tier: covered by R1 if R1 is complete; verify with a seeded read-only violation.

**R3. Repositories do not leak deferred-execution or ORM types** (lazy query interfaces, ORM sets, expression-accepting methods that let callers build queries).
Ideal tier: test over repository interface return types and parameters.

**R4. Raw SQL and stored-procedure calls exist only in persistence.**
Ideal tier: test (via R1) plus a text-search hook for raw SQL strings during editing.

**R5. Transaction and unit-of-work boundaries are owned by Application through a port, not opened ad hoc in handlers.**
Ideal tier: test for direct transaction API use outside persistence; review for correctness of boundaries.

## D: DDD tactical design

**D1. Repositories exist only for aggregate roots.**
Ideal tier: compile (generic constraint on the repository base interface) plus test that no repository is declared outside that base.

**D2. Aggregates protect their invariants**: no public setters on state, collections exposed read-only, state changes through intention-revealing methods.
Ideal tier: test for public setters and mutable collection exposure on aggregate and entity types; review for whether the methods actually enforce invariants.

**D3. Aggregates reference other aggregates by identity, not by object reference.**
Ideal tier: test that no aggregate has a property or field typed as another aggregate root.

**D4. Value objects are immutable and compared by value.**
Ideal tier: test (no settable state; value-equality type or overridden equality).

**D5. The Domain is persistence-ignorant**: no ORM mapping attributes, no serialization attributes, no persistence base classes. Mapping lives in Infrastructure.
Ideal tier: test.

**D6. Business rules live in the Domain, not in application services** (no anemic model).
Ideal tier: review. This is judgment; a reviewer checklist and skill examples are the right mechanism. Flag if it is enforced by nothing.

**D7. Domain events are raised by aggregates and dispatched by Application/Infrastructure**, not published directly from domain code via an infrastructure client.
Ideal tier: test for broker or bus client usage in Domain (covered by L1 if complete); review for usage patterns.

**D8. Aggregate boundaries and sizes are sensible.**
Ideal tier: review only.

## T: Integration testing against real infrastructure

**T1. Persistence adapters are tested against real infrastructure in containers**, not in-memory providers, file-based substitutes, or mocked ORM contexts.
Ideal tier: test or build check that bans substitute provider packages in test projects, plus hook during editing.

**T2. Every repository implementation has at least one integration test**, using the shared container fixture.
Ideal tier: test that enumerates repository implementations and checks for a corresponding test type.
Weak when: the mapping is by naming convention only and a renamed class escapes.

**T3. A shared fixture manages container lifetime and database reset**, so tests are isolated and reasonably fast (container shared per collection or assembly, state reset between tests, migrations applied the same way as production).
Ideal tier: shared package or base class plus review.

**T4. Container images are pinned to explicit versions that match production.**
Ideal tier: test or text check; review.

**T5. CI can run container-based tests** (Docker or equivalent available) and those tests block merges.
Ideal tier: pipeline configuration; verify by reading the pipeline and recent runs if accessible.

**T6. Integration tests are separable from unit and architecture tests** (categories or separate projects), so fast feedback stays fast.

## M: Meta-rules (quality of the rule suite)

**M1. No vacuous rules.** Every rule that filters a set of types asserts the set is non-empty before asserting there are no offenders. A rule over zero types passes forever.

**M2. Complete assembly/module discovery.** Rules find production code automatically and fail on anything unclassified (ties to L5).

**M3. Rules see method bodies where it matters.** Signature-only inspection misses dependencies created inside methods.

**M4. Rules are themselves tested.** Ideally a permanent fixture of deliberately bad code that every rule must flag, so a refactor that weakens a rule turns the build red.

**M5. Failure messages are actionable**: offending type and location, the rule broken, and the correct alternative.

**M6. Rules are fast and categorized** so they can run at end of every assistant task and on every CI build.

**M7. One source of truth.** Rules live in one shared, versioned place, not copied between repositories with drift.

## A: Assistant layer and distribution

**A1. CLAUDE.md is short and points to skills**; it states the layer map and the handful of rules that matter everywhere.

**A2. Skills cover the "where does this go" decisions** (layer placement table, port vs adapter, repository patterns, aggregate design, integration test fixture usage) with examples from the real codebase.

**A3. Guidance and tests agree.** Any contradiction is Critical.

**A4. Hooks give fast feedback during editing and run architecture tests before the assistant declares a task done**, with blocking exit codes and cross-platform scripts.

**A5. A reviewer agent covers judgment concerns** (D6, D8, R5 boundaries) and nothing the tests already cover.

**A6. Scaffolding produces compliant code by default**, including the integration test.

**A7. Distribution is versioned and automatic**: rules package, assistant plugin, and service template are released together, and repositories opt in through committed configuration.
