# Law 3.1 sentence ledger

Law 3.1 is `rancho-dotnet` 2.1.0. The core law (`rancho` 3.0.0) and `rancho-react` 2.0.0 are unchanged. This ledger lists every sentence of `plugins/rancho-dotnet/skills/law/SKILL.md` that 2.1.0 changes, and every inventory row it changes, against `main` at 58a5843 (the merge of law 3.0). Nothing else in the law files changed.

It was produced from the same edit list that made the changes, so an edit cannot be missing from it. Each entry gives the old text and the new text, or says the text was removed and why. Text moved to the records is quoted, word for word, in `plugins/rancho-dotnet/RECORDS.md`, `## 2.1.0`.

Kinds: **merge** - item 1, a duplicate rule merged into the one that survives; **rationale** - a reason moved to the records, the instruction kept; **dedupe** - a restatement of a sentence the law keeps elsewhere, replaced by a pointer or removed; **pointer** - a pointer to an annex shortened; **orientation** - a non-rule sentence shortened or removed.

Load per .NET stream (`python -I tools/check-law.py`): ~19.9k tokens before, ~19.4k after (core 4557 words, unchanged; .NET 9684 to 9325 words).

## SKILL.md

### M1 - §3 Aggregates; §5 EF Core mapping for DDD - merge

Old:

```text
- An aggregate MUST NOT hold a reference to another aggregate. It holds that aggregate's id. ⚙ BE-79
```

New:

```text
- An aggregate MUST NOT hold a reference to another aggregate, a navigation property between aggregate roots included. It holds that aggregate's id. ⚙ BE-79
```

Why: BE-42 merged into BE-79: a navigation property between roots is a reference one aggregate holds to another. BE-42's EF-model check joins BE-79's row.

### M1b - §5 EF Core mapping for DDD - merge

Old:

```text
- Navigation properties between aggregate roots are forbidden. One aggregate references another by id only. ⚙ BE-42
```

New: removed.

Why: Merged into BE-79 (§3 Aggregates), which states both sentences: no navigation property between roots, and one aggregate holds another's id.

### M2 - §4 Commands and Queries; §5 Repositories - merge

Old:

```text
- Command handlers MUST mutate state only by loading an aggregate and calling its methods. Writing through `DbContext` or SQL from a command handler is forbidden. ⚙ BE-13
```

New:

```text
- Command handlers MUST mutate state only by loading an aggregate through its repository, calling its methods, and persisting through the repository. There is no other write path: writing through `DbContext` or SQL from a command handler is forbidden. ⚙ BE-13
```

Why: BE-41 merged into BE-13: both state the one command write path. BE-41's check (no `Application` type references the unit-of-work port) joins BE-13's row.

### M2b - §5 Repositories and the aggregate graph - merge

Old:

```text
- Command handlers fetch through the repository, mutate through aggregate methods, and persist through the repository. There is no other write path. ⚙ BE-41
```

New: removed.

Why: Merged into BE-13 (§4 Commands and Queries), which now states fetch, mutate and persist, and "There is no other write path".

### M3 - §5 Persistence - merge

Old:

```text
- Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure. Mapping attributes on domain types are forbidden - the domain MUST NOT know it is persisted. ⚙ BE-38
```

New:

```text
- Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the domain MUST NOT know it is persisted: a Domain type MUST NOT carry a mapping attribute, an EF attribute included, and one the EF model maps MUST NOT have a public parameterless constructor or a public setter - use EF's private-constructor and backing-field support. ⚙ BE-38 If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type.
```

Why: BE-43 merged into BE-38: both forbid attributes on mapped Domain types; BE-43's constructor and setter bans and its last sentence move here unchanged. BE-43's check joins BE-38's row.

### M3b - §5 EF Core mapping for DDD - merge

Old:

```text
- A Domain type the EF model maps MUST NOT have a public parameterless constructor, a public setter, or an EF attribute; use EF's private-constructor and backing-field support. ⚙ BE-43 If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type.
```

New: removed.

Why: Merged into BE-38 (§5 Persistence), which states every clause of it.

### M4 - §4 Commands and Queries - merge

Old:

```text
- A Query MUST NOT mutate: a query handler MUST NOT reference a repository port, and MUST NOT call `SaveChangesAsync`. ⚙ BE-12
```

New: removed.

Why: Merged into the one query-side bullet (below, at BE-15's place). Its repository-port half is BE-108's (a port is a `Domain` type); its `SaveChangesAsync` half keeps BE-12.

### M4b - §4 Commands and Queries - merge

Old:

```text
- Query handlers MUST bypass the domain and project straight to Response DTOs, untracked. Materializing an aggregate in a query handler is forbidden. ⚙ BE-14
```

New: removed.

Why: Merged into the one query-side bullet; BE-14 retires into BE-108, whose sentence now says "bypasses the domain", "untracked" and "materialize an aggregate". BE-14's keyless-model check joins BE-108's row.

### M4c - §4 Commands and Queries - merge

Old:

```text
- The query message, its Response DTO, and the `IQueryHandler<,>` abstraction live in `Application`. **Every concrete query handler lives in `Infrastructure`, by convention.** A query handler that is pure computation follows the same convention. A command handler stays in `Application`. ⚙ BE-15 - A query handler reads and projects straight into its Response DTO, independent of the domain model: it MUST NOT reference a `Domain` type. ⚙ BE-108 It reads
```

New:

```text
- A Query MUST NOT mutate: its handler MUST NOT call `SaveChangesAsync`. ⚙ BE-12 A query handler MUST bypass the domain and project straight into its Response DTO, untracked: it MUST NOT reference a `Domain` type, materialize an aggregate, or use an aggregate repository. ⚙ BE-108 The query message, its Response DTO, and the `IQueryHandler<,>` abstraction live in `Application`. **Every concrete query handler lives in `Infrastructure`, by convention**, a pure-computation one included; a command handler stays in `Application`. ⚙ BE-15 A query handler reads
```

Why: The five query-side bullets (BE-12, BE-14, BE-15, BE-108, BE-16) become one, as the 2026-11-03 review asked. Rows kept: BE-12 (no mutation), BE-108 (bypasses the domain), BE-15 (placement); BE-14 and BE-16 retire into BE-108.

### M4d - §4 Commands and Queries - merge

Old:

```text
- A query handler MUST NOT use an aggregate repository. ⚙ BE-16
```

New: removed.

Why: Merged into the one query-side bullet: "or use an aggregate repository" under BE-108. BE-16's implementation check joins BE-108's row.

### T1 - Introduction - orientation

Old:

```text
This is a stack law under core §2. It adds the .NET rules, and it carries the core law's mechanism in every tree bound to it.
```

New:

```text
This stack law (core §2) adds the .NET rules and carries the core law's mechanism in every tree bound to it.
```

Why: Same statement in fewer words.

### T2 - Introduction - orientation

Old:

```text
is inventoried in `references/enforcement.md`, beside this file: consult it
```

New:

```text
is inventoried in `references/enforcement.md`: consult it
```

Why: "beside this file" repeats the path `references/`, which the annex paragraph below also states.

### T3 - §4 Commands and Queries - rationale

Old:

```text
Translation is a switch expression, never a switch statement: a switch statement gets no exhaustiveness check. A switch expression over an enum outcome that misses a named member raises CS8509, an error under `TreatWarningsAsErrors` (§1). CS8524, which the compiler raises for the enum's unnamed values, is disabled in the solution's build configuration with its reason (§1), so no discard arm is written and an added member fails the build. A hierarchy of outcome types cannot be proven exhaustive, so CS8509 fires on every switch over one: where outcomes are types, the switch ends in a discard arm that throws `UnreachableException`, and each outcome's translation is proven by a test (§14).
```

New:

```text
Translation is a switch expression, never a switch statement. Over an enum outcome, CS8524, raised for the enum's unnamed values, is disabled in the solution's build configuration with its reason (§1), and no discard arm is written, so a missed member raises CS8509, an error under `TreatWarningsAsErrors` (§1). Where outcomes are types, the switch ends in a discard arm that throws `UnreachableException`, and each outcome's translation is proven by a test (§14).
```

Moved to the records, word for word: "a switch statement gets no exhaustiveness check." / "A hierarchy of outcome types cannot be proven exhaustive, so CS8509 fires on every switch over one".

Why: The why of each instruction moves to RECORDS; every instruction stays: switch expression not statement; CS8524 disabled with its reason; no discard arm over an enum; CS8509 is an error; discard arm throwing `UnreachableException` over types; a test per outcome.

### T4 - §4 Pagination - pointer

Old:

```text
The agent rule of this subsection is in `references/annex-agents.md`, under its trigger (above).  ### Pipeline behaviors
```

New:

```text
Its agent rule: `annex-agents.md`.  ### Pipeline behaviors
```

Why: Pointer shortened. The introduction already says annexes sit in `references/` and bind under their trigger.

### T5 - §4 Idempotency - pointer

Old:

```text
The agent rule of this subsection is in `references/annex-agents.md`, under its trigger (above).  ### Transactions and consistency
```

New:

```text
Its agent rule: `annex-agents.md`.  ### Transactions and consistency
```

Why: Pointer shortened, as T4.

### T6 - §4 Placement of interfaces and implementations - dedupe

Old:

```text
A real-time push adapter - the implementation of an `Application` port that sends to a hub's clients through `IHubContext<T>` - is implemented in `Api`, which hosts the hubs, not in `Infrastructure`: `IHubContext<T>` ships only in the ASP.NET Core shared framework, which BE-01 admits in `Api` only (real-time annex).
```

New:

```text
A real-time push adapter - the implementation of an `Application` port that sends to a hub's clients through `IHubContext<T>` - is implemented in `Api`, which hosts the hubs, not in `Infrastructure` (real-time annex).
```

Moved to the records, word for word: "`IHubContext<T>` ships only in the ASP.NET Core shared framework, which BE-01 admits in `Api` only".

Why: Rationale; `annex-realtime.md` §12 states it word for word, so it stays in the law under that annex's trigger.

### T7 - §5 Redis - pointer

Old:

```text
*Redis* is in `references/annex-caching.md`, under its trigger (above).
```

New:

```text
In `annex-caching.md`.
```

Why: Pointer shortened, as T4.

### T8 - §5 Read-side caching - pointer

Old:

```text
*Read-side caching* is in `references/annex-caching.md`, with *Redis*, under its trigger (above).
```

New:

```text
In `annex-caching.md`.
```

Why: Pointer shortened, as T4.

### T9 - §6 External authorities - pointer

Old:

```text
Their intersection with an agent's manifest is in `references/annex-agents.md`, under its trigger (above).
```

New:

```text
Their intersection with an agent's manifest is in `annex-agents.md`.
```

Why: Pointer shortened, as T4.

### T10 - §7 Data Classification - orientation

Old:

```text
Core §8 defines the classes and the boundaries. This section is the mechanism.
```

New: removed.

Moved to the records, word for word: "Core §8 defines the classes and the boundaries. This section is the mechanism.".

Why: Orientation the Core map (§8 Data Boundaries row) already gives.

### T11 - §7 Logging - rationale

Old:

```text
except the first `ILogger`, `LogLevel` and `Exception` parameters, which the logging generator treats specially; the redactor applies only to classified parameters, so an unattributed one would be written in clear. An exception
```

New:

```text
except the first `ILogger`, `LogLevel` and `Exception` parameters. An exception
```

Moved to the records, word for word: "which the logging generator treats specially; the redactor applies only to classified parameters, so an unattributed one would be written in clear".

Why: Rationale for the attribute rule and its exception; the rule and the three excepted parameters stay.

### T12 - §7 Logging - rationale

Old:

```text
whose logging generator replaces the in-box one and reads the class attributes; the in-box generator ignores them, and writes a classified parameter in clear.
```

New:

```text
whose logging generator replaces the in-box one and reads the class attributes.
```

Moved to the records, word for word: "the in-box generator ignores them, and writes a classified parameter in clear".

Why: Rationale for the package reference, which stays.

### T13 - §7 The model-visible surface - pointer

Old:

```text
*The model-visible surface* is in `references/annex-agents.md`, under its trigger (above).
```

New:

```text
In `annex-agents.md`.
```

Why: Pointer shortened, as T4.

### T14 - §7 Cache, events, and storage - pointer

Old:

```text
The cache rules of this subsection are in `references/annex-caching.md`, under its trigger (above).
```

New:

```text
Its cache rules: `annex-caching.md`.
```

Why: Pointer shortened, as T4.

### T15 - §8 AI Agents - pointer

Old:

```text
§8 is in `references/annex-agents.md`, under its trigger (above). An agent manifest
```

New:

```text
In `annex-agents.md`. An agent manifest
```

Why: Pointer shortened, as T4.

### T16 - §9 Transport - rationale

Old:

```text
An endpoint carries one named policy beside the `GlobalLimiter`, so the stricter policy chains the write limiter with its own
```

New:

```text
The stricter policy chains the write limiter with its own
```

Moved to the records, word for word: "An endpoint carries one named policy beside the `GlobalLimiter`, so".

Why: Rationale (an ASP.NET Core fact) for chaining; the instruction to chain stays.

### T17 - §9 Agent runs - pointer

Old:

```text
*Agent runs* is in `references/annex-agents.md`, under its trigger (above).
```

New:

```text
In `annex-agents.md`.
```

Why: Pointer shortened, as T4.

### T18 - §10 Observability - orientation

Old:

```text
Core §9 governs. This is the mechanism.
```

New: removed.

Moved to the records, word for word: "Core §9 governs. This is the mechanism.".

Why: Orientation the Core map (§9 Observability row) already gives.

### T19 - §10 Correlation - rationale

Old:

```text
(`Sdk.SetDefaultTextMapPropagator`): under any other propagator that extracts `traceparent`, the SDK's default composite included, the ASP.NET Core instrumentation re-parents the request onto the caller's trace. An ingress that strips the header leaves nothing to link.
```

New:

```text
(`Sdk.SetDefaultTextMapPropagator`).
```

Moved to the records, word for word: "under any other propagator that extracts `traceparent`, the SDK's default composite included, the ASP.NET Core instrumentation re-parents the request onto the caller's trace. An ingress that strips the header leaves nothing to link.".

Why: Rationale for the plain `TraceContextPropagator`; the last sentence is core §9's own ("an ingress that strips the header leaves nothing to link").

### T20 - §10 Correlation - pointer

Old:

```text
The agent rule of this subsection is in `references/annex-agents.md`, under its trigger (above).  ### Logging
```

New:

```text
Its agent rule: `annex-agents.md`.  ### Logging
```

Why: Pointer shortened, as T4.

### T21 - §10 Logging - rationale

Old:

```text
replacing `ILoggerFactory` - as `UseSerilog()` does - is forbidden, because redaction runs in the logger factory `EnableRedaction()` configures.
```

New:

```text
replacing `ILoggerFactory` - as `UseSerilog()` does - is forbidden.
```

Moved to the records, word for word: "because redaction runs in the logger factory `EnableRedaction()` configures".

Why: Rationale for the prohibition, which stays.

### T22 - §12 Real-time - pointer

Old:

```text
§12 is in `references/annex-realtime.md`, under its trigger (above).
```

New:

```text
In `annex-realtime.md`.
```

Why: Pointer shortened, as T4.

### T23 - §13 API Layer - dedupe

Old:

```text
For a create-only message, a violation of the aggregate id's unique index at commit is a precondition failure, not a conflict. The unit-of-work behavior returns a commit-time precondition failure as a `Result` precondition failure, distinct from a business conflict, and the result translator maps every precondition failure to `412`.
```

New:

```text
For a create-only message, a violation of the aggregate id's unique index at commit is a precondition failure, not a conflict; the unit-of-work behavior returns it as a precondition failure (§4 *Pipeline behaviors*, item 4). The result translator maps every precondition failure to `412`.
```

Why: The unit-of-work behavior's return is stated in §4 *Pipeline behaviors*, item 4 ("as a precondition failure for a stale expected version or a create-only unique-index violation"); a pointer replaces the restatement. "Not a conflict" keeps the distinction from a business conflict, and the sentence still says the behavior returns a precondition failure.

### T24 - §13 API Layer - rationale

Old:

```text
with a keyed hash, such as an HMAC under a server key, so a retry finds the same file and no client can name another's; never from
```

New:

```text
with a keyed hash, such as an HMAC under a server key; never from
```

Moved to the records, word for word: "so a retry finds the same file and no client can name another's".

Why: Rationale for the derived name; the derivation stays.

### T25 - §4 Pipeline behaviors - dedupe

Old:

```text
A conflict or a unique-key violation at commit comes back from the unit of work as an outcome. On any failure result
```

New:

```text
On any failure result
```

Why: Restates §5 Persistence ("A concurrency conflict or a unique-key violation MUST surface as an outcome, never as an exception above the persistence adapter"), which stays; "one at commit" in the next sentence still names that case.

### T26 - §4 Idempotency - rationale

Old:

```text
A violation of that index at commit means a concurrent attempt executed first, and the unit-of-work behavior answers it by the attempt identity, as it answers any failure result (§4 *Pipeline behaviors*, item 4) - never as a conflict.
```

New:

```text
The unit-of-work behavior answers a violation of that index at commit by the attempt identity, as it answers any failure result (§4 *Pipeline behaviors*, item 4) - never as a conflict.
```

Moved to the records, word for word: "means a concurrent attempt executed first".

Why: Rationale; the instruction (answer by the attempt identity, never as a conflict) stays.

## Inventory, `plugins/rancho-dotnet/skills/law/references/enforcement.md`

Rows whose id is retired keep the row, with status `retired` and `Merged into X` in Check, as the inventory legend defines (`/rancho:law`'s inventory, *How to read an inventory*). A surviving row carries the check of every row merged into it.

### BE-04

Old:

```text
| BE-04 | Aggregate state private; no public setters; collections as `IReadOnlyCollection<T>` | §3 | `arch` | build | No property of a root-marked type (BE-78) has a public setter, an `init` accessor included, and it has no public field. Every public collection-typed property of a root-marked type is `IReadOnlyCollection<T>` | Entities inside the aggregate carry no marker, so they are not reached here; BE-43's setter ban reaches them. That each collection is "over a private backing field" is reached by review | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/PropertyMembers/PropertyMemberConditionsDefinition.cs |
```

New:

```text
| BE-04 | Aggregate state private; no public setters; collections as `IReadOnlyCollection<T>` | §3 | `arch` | build | No property of a root-marked type (BE-78) has a public setter, an `init` accessor included, and it has no public field. Every public collection-typed property of a root-marked type is `IReadOnlyCollection<T>` | Entities inside the aggregate carry no marker, so they are not reached here; BE-38's setter ban reaches them. That each collection is "over a private backing field" is reached by review | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/PropertyMembers/PropertyMemberConditionsDefinition.cs |
```

### BE-12

Old:

```text
| BE-12 | A query handler references no repository port and never calls `SaveChangesAsync` | §4 | `arch` | build | No query handler type references a repository port (BE-108 reaches this: a port is a `Domain` type), and none calls a method named `SaveChanges`, `SaveChangesAsync`, `ExecuteUpdate(Async)`, `ExecuteDelete(Async)` or `ExecuteSql(Async)` on any type (ArchUnitNET `NotCallAny`) | A write made through another Infrastructure type that the handler calls is not reached | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/MethodMembers/IAddMethodMemberCondition.cs, https://github.com/TNG/ArchUnitNET/releases/tag/0.13.5 |
```

New:

```text
| BE-12 | A Query never mutates: its handler never calls `SaveChangesAsync` | §4 | `arch` | build | No query handler type calls a method named `SaveChanges`, `SaveChangesAsync`, `ExecuteUpdate(Async)`, `ExecuteDelete(Async)` or `ExecuteSql(Async)` on any type (ArchUnitNET `NotCallAny`) | A write made through another Infrastructure type that the handler calls is not reached | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/MethodMembers/IAddMethodMemberCondition.cs, https://github.com/TNG/ArchUnitNET/releases/tag/0.13.5 |
```

### BE-13

Old:

```text
| BE-13 | Command handlers mutate only through aggregate methods; no `DbContext`/SQL | §4 | `arch` | build | Covered by BE-01: `Application` cannot reference EF. `arch`: no module `Application` type depends on a type in `System.Data` or below | A command handler that writes through an `Application`-declared port whose adapter runs SQL is not reached; review carries it, with BE-41 | https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand |
```

New:

```text
| BE-13 | Command handlers fetch through the repository, mutate only through aggregate methods, and persist through the repository; no other write path, no `DbContext`/SQL | §4 | `arch` | build | BE-01 keeps EF Core out of `Application`. `arch`: no module `Application` type depends on a type in `System.Data` or below, and none references the unit-of-work port, declared in `Platform.Application` | A command handler that writes through another port that Infrastructure implements, such as a writer that is not a repository or one whose adapter runs SQL, is not reached; review carries it | https://learn.microsoft.com/dotnet/api/system.data.common.dbcommand, https://archunitnet.readthedocs.io/en/latest/guide/ |
```

### BE-14

Old:

```text
| BE-14 | Query handlers project straight to DTOs, untracked; no aggregate materialization | §4 | `arch` + `contract` | build | Covered by BE-108: a handler that references no `Domain` type cannot materialize an aggregate, and SQL into a DTO is never tracked. `contract`: every entity type in a module's model that is not owned (`IsOwned`), is not a shared-type entity type (`HasSharedClrType`), and whose CLR type is neither a `Domain` type nor a pipeline bookkeeping type (outbox row, idempotency record, audit record) is keyless (`IsKeyless`) | — | https://learn.microsoft.com/ef/core/querying/tracking, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.iconventionentitytype.iskeyless?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlyentitytype.isowned?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlytypebase.hassharedclrtype?view=efcore-10.0 |
```

New:

```text
| BE-14 | Query handlers project straight to DTOs, untracked; no aggregate materialization | §4 | — | retired | Merged into BE-108 | — | — |
```

### BE-16

Old:

```text
| BE-16 | Query handlers MUST NOT use an aggregate repository | §4 | `arch` | build | Port: covered by BE-108, because a repository port is a `Domain` type. Implementation: `arch`. No `IQueryHandler<,>` implementation, and no compiler-generated type nested in one, depends on a class that implements an interface declared in a module `Domain` assembly. By §4's placement table, a repository implementation is the only Infrastructure type that does | A repository reached through an intermediate type the handler depends on is not reached, because ArchUnitNET asserts direct dependencies; review carries it | https://archunitnet.readthedocs.io/en/stable/guide/predicate/type/, https://archunitnet.readthedocs.io/en/stable/guide/condition/object/ |
```

New:

```text
| BE-16 | Query handlers MUST NOT use an aggregate repository | §4 | — | retired | Merged into BE-108 | — | — |
```

### BE-38

Old:

```text
| BE-38 | Mapping in `IEntityTypeConfiguration<T>`; no mapping attributes on domain types | §5 | `arch` | build | No attribute from `System.ComponentModel.DataAnnotations.Schema`, and none of `Key`, `Timestamp`, `ConcurrencyCheck`, `MaxLength`, `StringLength` or `Required` from `System.ComponentModel.DataAnnotations`, on any Domain type, property or field. EF Core's own attributes cannot reach Domain (BE-02) | Mapping written in `OnModelCreating` instead of an `IEntityTypeConfiguration<T>` is not checked; reached by review | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/ObjectConditionsDefinition.cs, https://learn.microsoft.com/ef/core/modeling/owned-entities |
```

New:

```text
| BE-38 | Mapping in `IEntityTypeConfiguration<T>`; no mapping attribute on a Domain type; a mapped Domain type has no public parameterless constructor or public setter | §5 | `arch` | build | No attribute from `System.ComponentModel.DataAnnotations.Schema`, and none of `Key`, `Timestamp`, `ConcurrencyCheck`, `MaxLength`, `StringLength` or `Required` from `System.ComponentModel.DataAnnotations`, on any Domain type, property or field. EF Core's own attributes cannot reach Domain (BE-02). Every Domain type that the EF model maps as an entity, owned or complex type has no public parameterless constructor, and no property has a public setter, `init` included. Skips compiler-generated types, marked with `CompilerGeneratedAttribute`, such as a lambda's closure class | Mapping written in `OnModelCreating` instead of an `IEntityTypeConfiguration<T>` is not checked; reached by review | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/ObjectConditionsDefinition.cs, https://learn.microsoft.com/ef/core/modeling/owned-entities, https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/MethodMembers/MethodMemberConditionsDefinition.cs, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlymodel.getentitytypes?view=efcore-10.0 |
```

### BE-41

Old:

```text
| BE-41 | Command handlers write only through the repository | §5 | `arch` | build | No type in a module `Application` references the unit-of-work port, declared in `Platform.Application` | A write through another port that Infrastructure implements, such as a writer that is not a repository, is not seen; reached by review | https://archunitnet.readthedocs.io/en/latest/guide/ |
```

New:

```text
| BE-41 | Command handlers write only through the repository | §4 | — | retired | Merged into BE-13 | — | — |
```

### BE-42

Old:

```text
| BE-42 | No navigation properties between aggregate roots | §5 | `contract` | build | EF model, Domain entity types only: no navigation or skip navigation (`GetNavigations`, `GetSkipNavigations`) leads from one aggregate to another. An entity type's aggregate is its root, found through the root marker and the Domain `<Aggregate>/` namespace | — | https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlyentitytype.getnavigations?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlynavigationbase.targetentitytype?view=efcore-10.0 |
```

New:

```text
| BE-42 | No navigation properties between aggregate roots | §3 | — | retired | Merged into BE-79 | — | — |
```

### BE-43

Old:

```text
| BE-43 | A Domain type the EF model maps has no public parameterless constructor, public setter, or EF attribute | §5 | `arch` | build | Extends BE-04 and BE-38. Every Domain type that the EF model maps as an entity, owned or complex type has no public parameterless constructor, and no property has a public setter, `init` included. Skips compiler-generated types, marked with `CompilerGeneratedAttribute`, such as a lambda's closure class | — | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Fluent/Syntax/Elements/Members/MethodMembers/MethodMemberConditionsDefinition.cs, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlymodel.getentitytypes?view=efcore-10.0 |
```

New:

```text
| BE-43 | A Domain type the EF model maps has no public parameterless constructor, public setter, or EF attribute | §5 | — | retired | Merged into BE-38 | — | — |
```

### BE-79

Old:

```text
| BE-79 | An aggregate holds another aggregate's id, never a reference | §3 | `arch` | build | No field or property of a root-marked type, or of a type in its aggregate's namespace, is typed as a root-marked type, its own included, generic arguments included | — | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Domain/ITypeInstance.cs |
```

New:

```text
| BE-79 | An aggregate holds another aggregate's id, never a reference; no navigation property between aggregate roots | §3 | `arch` + `contract` | build | `arch`: No field or property of a root-marked type, or of a type in its aggregate's namespace, is typed as a root-marked type, its own included, generic arguments included. `contract`: EF model, Domain entity types only: no navigation or skip navigation (`GetNavigations`, `GetSkipNavigations`) leads from one aggregate to another. An entity type's aggregate is its root, found through the root marker and the Domain `<Aggregate>/` namespace | — | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNET/Domain/ITypeInstance.cs, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlyentitytype.getnavigations?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlynavigationbase.targetentitytype?view=efcore-10.0 |
```

### BE-108

Old:

```text
| BE-108 | A query handler references no `Domain` type | §4 | `arch` | build | Type references of every `IQueryHandler<,>` implementation resolve outside `Domain`. Subsumes BE-16, and the repository-port half of BE-12 | — | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNETTests/Dependencies/GenericMemberDependenciesTests.cs, https://github.com/TNG/ArchUnitNET/releases/tag/0.13.5 |
```

New:

```text
| BE-108 | A query handler bypasses the domain: projects straight to its Response DTO, untracked; references no `Domain` type, materializes no aggregate, uses no aggregate repository | §4 | `arch` + `contract` | build | Type references of every `IQueryHandler<,>` implementation resolve outside `Domain`, so no handler materializes an aggregate or references a repository port, which is a `Domain` type. `arch`: No `IQueryHandler<,>` implementation, and no compiler-generated type nested in one, depends on a class that implements an interface declared in a module `Domain` assembly. By §4's placement table, a repository implementation is the only Infrastructure type that does. `contract`: every entity type in a module's model that is not owned (`IsOwned`), is not a shared-type entity type (`HasSharedClrType`), and whose CLR type is neither a `Domain` type nor a pipeline bookkeeping type (outbox row, idempotency record, audit record) is keyless (`IsKeyless`); SQL into a DTO is never tracked | A repository reached through an intermediate type the handler depends on is not reached, because ArchUnitNET asserts direct dependencies; review carries it | https://github.com/TNG/ArchUnitNET/blob/main/ArchUnitNETTests/Dependencies/GenericMemberDependenciesTests.cs, https://github.com/TNG/ArchUnitNET/releases/tag/0.13.5, https://learn.microsoft.com/ef/core/querying/tracking, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.iconventionentitytype.iskeyless?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlyentitytype.isowned?view=efcore-10.0, https://learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.metadata.ireadonlytypebase.hassharedclrtype?view=efcore-10.0, https://archunitnet.readthedocs.io/en/stable/guide/predicate/type/, https://archunitnet.readthedocs.io/en/stable/guide/condition/object/ |
```

### Summary

Old:

```text
The stack law carries **106 ⚙ marks**, each naming the ids of the rows it marks. Its rules are **141 rows**: 108 `build`, 32 `required-test`, 1 `dropped`, and 0 `retired`. Its role, `server`, carries **15 core rules**, below.
```

New:

```text
The stack law carries **101 ⚙ marks**, each naming the ids of the rows it marks. Its rules are **141 rows**: 103 `build`, 32 `required-test`, 1 `dropped`, and 5 `retired`. Its role, `server`, carries **15 core rules**, below. The five `retired` rows record rules merged into the row that states them: BE-14 and BE-16 into BE-108, BE-41 into BE-13, BE-42 into BE-79, BE-43 into BE-38.
```

Old:

```text
Four rules restate another rule in this file (BE-05, BE-13, BE-14, BE-32), and eleven core rules restate one, their Check naming it `Canonical:` (ROOT-02, ROOT-08, ROOT-11, ROOT-12, ROOT-13, ROOT-15, ROOT-16, ROOT-17, ROOT-18, ROOT-19, ROOT-20). ROOT-07's Check begins `Covered by`, naming several rows, and is not counted. One check satisfies each pair; the canonical id is noted and the duplicate points at it.
```

New:

```text
Two rules restate another rule in this file (BE-05, BE-32), and eleven core rules restate one, their Check naming it `Canonical:` (ROOT-02, ROOT-08, ROOT-11, ROOT-12, ROOT-13, ROOT-15, ROOT-16, ROOT-17, ROOT-18, ROOT-19, ROOT-20). ROOT-07's Check begins `Covered by`, naming several rows, and is not counted. One check satisfies each pair; the canonical id is noted and the duplicate points at it.
```

## `python -I tools/law-ledger.py main`

Every normative sentence of `main` that is no longer stated verbatim, and the edit above that changed it. The tool sees only sentences with MUST, SHOULD, MAY, "forbidden" or "defect"; the edits above list the rest.

| Line at `main` | Sentence | Edit |
|---|---|---|
| 141 | An aggregate MUST NOT hold a reference to another aggregate. | M1 |
| 183 | A Query MUST NOT mutate: a query handler MUST NOT reference a repository port, and MUST NOT call `SaveChangesAsync`. | M4 |
| 184 | Command handlers MUST mutate state only by loading an aggregate and calling its methods. | M2 |
| 184 | Writing through `DbContext` or SQL from a command handler is forbidden. | M2 |
| 185 | Query handlers MUST bypass the domain and project straight to Response DTOs, untracked. | M4b |
| 185 | Materializing an aggregate in a query handler is forbidden. | M4b |
| 187 | A query handler reads and projects straight into its Response DTO, independent of the domain model: it MUST NOT reference a `Domain` type. | M4c |
| 189 | A query handler MUST NOT use an aggregate repository. | M4d |
| 295 | Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure. | M3 |
| 295 | Mapping attributes on domain types are forbidden - the domain MUST NOT know it is persisted. | M3 |
| 327 | Navigation properties between aggregate roots are forbidden. | M1b |
| 328 | A Domain type the EF model maps MUST NOT have a public parameterless constructor, a public setter, or an EF attribute; use EF's private-constructor and backing-field support. | M3b |
| 456 | Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is the provider that writes the output, registered on the logging builder (`builder.Logging.AddSerilog(logger, dispose: true)`, with the l | T21 |

Total: 13, each an intended change.
