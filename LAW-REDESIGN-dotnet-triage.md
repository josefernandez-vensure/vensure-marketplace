# Phase 1 triage: the .NET inventory, row by row

Working file for the law redesign (`LAW-REDESIGN.md`). It is deleted in phase 6; the PR keeps the record. It covers the 136 active rows of `rancho-dotnet`'s `enforcement.md` at rancho 4.1.0; the 12 retired rows are left out.

The checks land in `templates/rancho-dotnet/`. The law they serve is `rancho-modular-dotnet` (open decision 1, taken as recommended).

## Increment 1: structural checks on a skeleton solution (done)

These need only projects and types, not a running platform. Every check below was shown to fail on a seeded violation: `python -I templates/check-rancho-dotnet.py` reports 40 of 40 seeds caught.

| Mechanism | Rows |
|---|---|
| MSBuild, `build/ProjectKinds.targets`: each project's kind, and the references and packages that kind allows | BE-01 (project and package half). BE-39, narrowed: the in-memory and SQLite EF providers are denied. |
| `build/BannedSymbols*.txt` per project kind | BE-32 (ambient transactions), BE-57 (scope templates, Serilog's static logger), BE-95 and BE-98 (banned half), BE-130 (banned half), BE-132 (clock); `Thread.CurrentPrincipal` and `ClaimsPrincipal.Current` from BE-01 |
| `.editorconfig`: built-in analyzers as errors | BE-35 (IDE0130), BE-57 (CA1848, CA2254), BE-113 (EF1002/EF1003, EF Core's own analyzer), BE-133 (VSTHRD100/101), BE-134 (VSTHRD002/103), BE-135 (CA1031) |
| Generic constraint, by construction | BE-78: `IRepository<TRoot, TId>` requires `TRoot : IAggregateRoot` |
| `tests/Architecture.Tests` (26 tests) | BE-01 (type half), BE-03 (namespaces, since IDE0130 ties them to folders), BE-04, BE-06, BE-07, BE-08, BE-12, BE-15, BE-35 (namespaces), BE-38 (attribute half), BE-40, BE-53, BE-79 (type half), BE-84, BE-93, BE-95 and BE-98 (Infrastructure half), BE-105, BE-106, BE-107, BE-108 (type half), BE-109, BE-129, BE-130 (Infrastructure half), BE-131 |

Where the new check differs from the old row:

- **BE-01 packages:** Infrastructure kinds may take any package except ASP.NET Core beyond Authorization and Data Protection, because modular law §2 admits "SDKs of the systems they adapt", which no allowlist can name. Application may reference `SharedKernel` directly, which modular law's four facts allow.
- **BE-113:** EF1003 refuses raw SQL built by concatenation or interpolation, which is the injection risk itself. A raw-SQL call with a constant string is no longer refused.
- **BE-135:** CA1031 has no fault-isolation marker. A deliberate boundary suppresses it with its reason.
- **BE-08:** a strongly-typed id is recognised structurally, as a type whose only instance state is one `Guid`.
- **Untested backstops:** `Only_Api_uses_AspNetCore…` and `A_module_reaches_another_module…` have no seed, because the build-time reference table makes those violations impossible to compile. They guard against anything arriving transitively.

Moved to increment 2, because they need EF migrations: BE-87 (a committed migration isn't edited) and BE-88 (a migration references no application code).

## Increment 2: checks that need the platform

These test `Result`, the operation declaration, the pipeline, the page and cursor, the outbox, the endpoint abstraction, the EF model or the built host, so the platform is built first. The required behaviour tests become the template's own reference tests, which modular law §11 already lists. The work is split into five slices.

### Slice 2a: the dispatch core (done)

`Result` and problem types, the operation declaration, the dispatch context, the pipeline (logging → authorization → validation), the endpoint abstraction and its registrar, the result translator, deny-by-default, and rate limits.

| Mechanism | Rows |
|---|---|
| The types themselves | ROOT-13, BE-25 and BE-26: a message without a declaration doesn't compile, because `IDeclaresOperation.Operation` is `static abstract`. BE-110, declaration half: `Operation.Query` has no transaction or key, and `Operation.Command` refuses a key without a transaction. BE-90: `DispatchContext` can't be built without a trace id. BE-50 and BE-70: the registrar applies the declared policy and rate limit, so an endpoint can't omit them. |
| `tests/Architecture.Tests` | BE-11, BE-34, BE-80, BE-82; plus each operation's id, kind and uniqueness |
| `tests/Api.Tests`, against the built host | BE-50, BE-51, BE-70, BE-73, BE-81, BE-96, BE-101 (the authorization behavior refuses without any transport filter); plus every operation's policy is registered, and validation answers field errors by request path |

What differs from the old rows: BE-96 checks that a validator is registered, and BE-73's behaviour test shows that a missing or unusable rate limit stops startup. Neither adds an always-failing validator to prove that `ValidateOnStart` itself is wired.

### Slice 2b: persistence (done)

The unit-of-work behavior (commands only, last in the pipeline); idempotency by key, recorded and replayed; the audit trail, with denials recorded at both the pipeline and the transport; the outbox, written with each change; `ModuleDbContext`; the first migration; and SQL Server in Docker for the behaviour tests.

| Mechanism | Rows |
|---|---|
| The types themselves | BE-27, BE-28, BE-91: `ModuleDbContext` seals the model's base, so every module's context maps the outbox (with its trace columns), the idempotency records (with their unique identity index) and the audit trail, and commits them with its own changes. ROOT-19 is BE-90 (slice 2a) with BE-91. |
| `tests/Architecture.Tests`, over the EF model built without a database | BE-10, BE-27, BE-28, BE-37, BE-38 (EF half), BE-79 (EF half), BE-91, BE-108 (EF half); plus every model change has its migration (`HasPendingModelChanges`) |
| `tests/Architecture.Tests`, over types | BE-27 (no cache or second context), BE-30 (only the pipeline touches the unit of work), BE-33 (integration events leave only through the outbox), BE-88 |
| `tests/Catalog.IntegrationTests`, against SQL Server | BE-29, BE-89, BE-112, BE-116, BE-118 (a success, a failure, and a denial at the pipeline and at the transport, each recorded), BE-125, BE-137; plus a key belongs to its principal, and a stale write is a conflict, not an exception |
| `tests/Platform.Tests`, with fakes | BE-110 (staging half) and BE-111: a command declared `Transaction None` that stages a change fails the dispatch. BE-118: such a command is still recorded, and a failure is recorded only after what the handler staged is discarded. The template's domain rejects before it changes anything, so no real command can stage and then fail. |
| `build/check-migrations.sh`, in CI | BE-87 |

Found while writing and seeding the tests: an integration test named for the rollback never exercised one, because the domain rejects before changing anything; the rollback is now proven in `tests/Platform.Tests`. And a request the transport refuses never reaches the pipeline, so its denial went unrecorded. `AuditingAuthorizationResultHandler` now records it.

What differs from the old rows: the audit trail is append-only by convention only; no database permission or trigger enforces it yet. BE-89's "a destructive migration keeps the previous version serving" is left to review; the test proves only that migrations run clean twice.

### Slice 2c: paging (done)

`Page<T>`, `IPagedQuery` with a static `MaxPageSize`, the `Keyset` ordering descriptor, the cursor codec on Data Protection with a purpose per query, the paging behavior, and the `ListProducts` sample over a keyless read type.

| Mechanism | Rows |
|---|---|
| The types themselves | BE-22: the paging behavior refuses a page size above the query's maximum before the handler runs; no query can clamp it. BE-24: the codec port takes only `PageCursor`, which is exactly a sort key and an id. BE-23, generation half: `Keyset.Read` emits the ORDER BY and the keyset predicate, with the unique id as the last term. |
| `tests/Architecture.Tests` | ROOT-11 and BE-19 (a query returns a collection only as a page), ROOT-12 and BE-20, BE-21 (an `IPagedQuery` with a maximum within the ceiling), BE-23 (a paged handler calls no OrderBy or ThenBy and reads through a `Keyset`), BE-119 |
| `tests/Catalog.IntegrationTests` | BE-124 (no row skipped or repeated across boundaries that fall inside runs of equal names); BE-22 (51 refused, 50 served); BE-119 (a tampered cursor refused) |

What differs from the old rows: there is no "bounded by construction" collection type yet, so a query can't return a bare collection at all; the type comes with the first query that needs one, with BE-124's second half. The `maxItems` and `x-data-class` half of ROOT-11 and BE-19 is slice 2d's, with the OpenAPI document.

### Slice 2d: data classification and OpenAPI (done)

The four class attributes and `[ClassOf<T>]` in `SharedKernel`, with the resolver every egress check shares; log redaction by class at the composition root; typed endpoint results, so the document shows each Response DTO; the OpenAPI document's transformers; the committed `openapi/v1.json` and `contracts/<Module>.json`. All 92 tests pass, and `check-rancho-dotnet.py` catches 107 of 107 seeds, 19 of them new.

| Mechanism | Rows |
|---|---|
| The types themselves | BE-54, declaration half: the class attributes are `SharedKernel`'s and derive from `DataClassificationAttribute`; a projected member names its source with `[ClassOf<T>]` and takes its class, so it can't disagree with it. BE-83: the schema transformer throws on a property or parameter whose class it can't resolve, so the document can't be generated. ROOT-16, log half: the redactor's fallback erases, so an unmapped class is treated as `Restricted`. BE-86: the reference table already stops `Contracts` from reaching a `Domain` type, so its test was dropped (principle 3). |
| `tests/Architecture.Tests` | BE-54 (the attributes live only in `SharedKernel`; a `Contracts` twin agrees with its `Domain` type; a member redeclares no class its type carries; every logging parameter declares its class, and it agrees with its type's); BE-55 and ROOT-16 (everything in a module's `Contracts`, every domain event and every query response resolves to a class); BE-56; BE-61; BE-126, `ToString` half; BE-85, against the committed snapshot; BE-97, the sweep of committed configuration; plus every logging method is built by the redacting generator (`Microsoft.Gen.Logging`) |
| `tests/Api.Tests`, against the built host | ROOT-09 (the committed document is the one the host generates); ROOT-18 and BE-83 (every property and parameter schema carries `x-data-class`); ROOT-11 and BE-19, document half (every array in a success response carries `maxItems`, and a page's is at least its query's maximum page size); BE-126, provider half (a `[Confidential]` logging parameter reaches the log provider erased) |
| `build/check-contracts.sh`, in CI | BE-85, merge-base half |

Found while writing and seeding the tests: the first redaction map passed `Public` and `Internal` as one `DataClassificationSet`, which matches only a value carrying both classes, so every `Internal` log parameter was erased. The redaction test caught it; each class is now its own set.

What differs from the old rows:

- **ROOT-09:** a test compares the committed document with the one the host generates. On a difference it rewrites the file and fails, so a change shows in the diff and fails until it's committed; no `git status` step is needed in CI.
- **BE-55:** the architecture walk starts from each module's `Contracts`, domain events (the outbox carries them) and query responses. The Request and Response DTOs that `Api` declares are reached by the schema transformer instead. Hub, model-visible and cache roots wait for their features (deferred below); no project yet marks a third-party payload or an export.
- **BE-83:** a `$ref`'d property takes its component's class, as the old row's gap said. The registrar declares the platform's problem responses (400, 401, 403, 429) on every route, so problem details are in the document and classified; a module's own problem statuses aren't listed yet.
- **BE-85:** the snapshot is each `Contracts` type's JSON schema, as the outbox serializes it (default `System.Text.Json` options). A new type or a new optional property rewrites the snapshot and fails until it's committed. A change of meaning that keeps the shape is still left to review.
- **BE-97:** only the contract half. The history scan for high-entropy and provider-pattern secrets belongs to the repository's CI (GitHub secret scanning or a scanner such as gitleaks), not to the template. A secret in source code is reached only by that scan.
- **BE-126:** the `ToString` test plants values in each `Confidential` and `Restricted` type's string, `Guid` and numeric fields; a value held in a field of another type isn't planted. The template declares no such type yet, so the seeds add them. The log half is proven on the provider's structured state, not on a particular sink's serialization.

Noticed, outside 2d's rows: a wire id serializes as `{ "value": ... }` rather than a bare string; the Response DTOs' `ETag` serializes as `eTag`, where the contract says `etag` (2e); and no endpoint yet sends `Cache-Control: no-store` for a `Confidential` or `Restricted` response, nor does the document carry `x-problem-types` (contract law §6 and §7).

### Slices 2e and 2f (to do)

| Slice | Rows |
|---|---|
| 2e Concurrency and the remaining behaviour tests | tests BE-115, BE-117, BE-120, BE-122, BE-127, BE-128, BE-138, BE-139, BE-141 |
| 2f Outbox delivery and reactions (split from 2b) | test BE-92 (delivery restores the trace context); reactions and sagas, modular law §5 |

## Deferred: features modular law §12 doesn't cover yet

The rule for these is "stop and ask". Each row comes back as a check when the first project needs the feature, using the archived annex as source material.

| Feature | Rows |
|---|---|
| AI agents | BE-31, BE-58, BE-59, BE-62 to BE-69, BE-71, BE-72, BE-103, BE-114, BE-123 |
| Caching | BE-44 to BE-49, BE-60, BE-140 |
| Real-time | BE-74, BE-75, BE-121 |
| External authorities (modular §8 keeps the principle) | BE-99, BE-100, BE-102, BE-104 (BE-101 is covered in slice 2a) |

## Dropped: the check costs more than the mistake

| Row | Reason |
|---|---|
| BE-17 `<Feature>Reader` naming | Naming. The reference code shows it. |
| BE-36 Queries folders mirror Application folders | Naming. The reference code shows it. |
| BE-94 Metric tag keys from a closed set | It needs a custom analyzer, and a wrong tag is cheap to fix |
| BE-136 No `#region` | Cosmetic |
| BE-09 Domain events in the past tense | Already dropped in 4.1.0 |
