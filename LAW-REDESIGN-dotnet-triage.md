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

These test `Result`, the operation declaration, the pipeline, the page and cursor, the outbox, the endpoint abstraction, the EF model or the built host, so the platform is built first. The required behaviour tests become the template's own reference tests, which modular law §11 already lists. The work is split into six slices, all done.

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

### Slice 2e: concurrency and the remaining behaviour tests (done)

Versioned writes (`IVersionedCommand`, `ExpectedVersion` from `If-Match`, the precondition behavior, and the unit of work's check of the aggregate as loaded); create-only with `If-None-Match: *`; a tenant scope in the dispatch context, set on new products and applied by every read; a span for every pipeline step, with the handler's last; OpenTelemetry tracing and the trust boundary; the rate limiter moved ahead of authorization; and a domain test project. All 132 tests pass, and `check-rancho-dotnet.py` catches 133 of 133 seeds, 26 of them new.

| Mechanism | Rows |
|---|---|
| The types themselves | BE-115, refusal half: a versioned command carries `ExpectedVersion`; the precondition behavior refuses a missing or `*` tag (428) and one that can never match (412) before the handler; the unit of work refuses a stale one (412) against the aggregate as loaded, before anything commits, and turns a race lost at commit into 412. A command that changes a loaded aggregate without being an `IVersionedCommand` fails the dispatch. BE-141: a domain outcome is an enum, so CS8509 proves every translation. |
| `tests/Architecture.Tests` | BE-141 (a domain type named for an outcome is an enum) |
| `tests/Api.Tests`, against the built host | BE-127 (every rate-limit policy the routes use answers 429 problem details with `Retry-After`); BE-139 (a caller's `traceparent` gets a new trace id and a link to the caller's, with OpenTelemetry's instrumentation registered) |
| `tests/Catalog.IntegrationTests` | BE-115 (each versioned write: missing and `*` are 428, stale and weak tags 412; create-only for an existing id 412); BE-117 (another tenant's product is never read, listed, renamed, restocked or overwritten, and a principal with no tenant holds no policy); BE-122 (every route refuses a principal its policy doesn't admit, and records the refusal); BE-128 (the spans of every behavior the container resolves for a command and for a paged query are nested in pipeline order, the handler's last) |
| `tests/Domain.Tests` | BE-138: tests of every invariant and outcome, and a check that every public domain method has a test named for it, every outcome a `<Method>_Returns<Outcome>_` test, and every validating constructor a `New_` test |
| `tests/Platform.Tests` | The guard on a write to a loaded aggregate without a precondition |

Decided while building: tenancy is a scope, not a domain concept. The product's `TenantId` is an EF shadow property the repository sets on create and filters on load; the queries filter by it in their LINQ or SQL. The composition root reads the tenant from the token's `tenant` claim, and each Catalog policy requires one. The rate limiter now runs before authorization, so a refused request spends a permit; before, a caller could send refused requests without limit. And the service has one database: the composition root binds `ConnectionStrings:Database` once, `AddModuleDbContext` no longer takes a connection string, and a host test proves every module's context connects to that database. Each module keeps its own context and schema, as modular law §6 says; before, each module read a connection string of its own, so a second module could have split into a second database.

What differs from the old rows:

- **BE-115:** stale detection doesn't need each handler's code. The unit of work compares the expected version with every aggregate the handler loaded, so a handler that forgets can't let a stale write through. Which operations are versioned is still the message author's choice, kept honest by the guard on unversioned writes to loaded aggregates.
- **BE-117:** proven per operation, as the row asks. Nothing yet checks that a new query applies the tenant predicate; its tenancy test is the reference's pattern, and review reaches the rest.
- **BE-122:** one test over every mapped route, so a new operation is reached without a new test. It needs SQL Server, because a transport refusal is audited.
- **BE-127:** there is no `GlobalLimiter`; the OpenAPI endpoint, mapped only in Development, is under no limit.
- **BE-128:** the expected steps are read from the container for each message, so a new behavior is reached; one command and one paged query are dispatched.
- **BE-138:** the check is by test name: it proves a test exists for each member and outcome, not that the test asserts the right thing.
- **BE-139:** every caller is outside the trust boundary, because the template has one edge. A deployment with an internal edge whose callers it trusts decides that at the edge, which the template doesn't model.
- **BE-120:** deferred, below; the template has no upload endpoint.

### Slice 2f: outbox delivery, reactions and a saga (done)

The outbox dispatcher and the hosted service that runs it; reactions (`IReaction<TMessage, TCommand>`) and command messages (`ICommandMessage`); the module's service principal; the cause a reaction carries (`Causation`: the message and the initiating user) into its audit record and into every message it writes; the tenant scope carried on each outbox row; and the order saga, between Catalog and a minimal second module, `Ordering`, created with the `dotnet` CLI. All 176 tests pass, and `check-rancho-dotnet.py` catches 165 of 165 seeds, 32 of them new.

| Mechanism | Rows |
|---|---|
| The types themselves | §5, a reaction maps one message to one command: `IReaction<TMessage, TCommand>` has only a `static abstract Map`, so a reaction holds no dependency and can't load, write or dispatch; `TCommand` must be a command. The dispatcher sends the command through the pipeline, with the message's id as its idempotency key, in a scope of its own, and marks the message delivered only when every reaction succeeded (at least once). §5, a reaction runs as its module's service principal: `ServiceIdentity` is internal to `Platform.Infrastructure`, so only the dispatcher builds one and no authentication handler can produce it; `RequireServicePrincipal(module)` is a policy only that principal holds. Only the dispatcher enters a scope's dispatch context (`DispatchScope` is internal). §5, every outbox message carries the initiating user's id and its cause: `OutboxRows.For` writes them from the dispatch context, so a reaction passes both on to whatever it writes. §10, the reaction's audit record names the user and the message (`AuditEntry.OnBehalfOf`, `CausationId`). §5, a command message goes to the sending module's own outbox (`IOutbox.Send`). |
| `tests/Architecture.Tests` | §5: a reaction lives in its module's Application and maps to a command of its own module; it answers a domain event of its own module, any integration event, or a command message its module owns; its command is keyed, transactional and unversioned. BE-33 widened: command messages, like integration events, leave only through the outbox and carry their version in their name. BE-55/ROOT-16: command messages are outbox payloads, so nothing `Restricted` enters one. |
| `tests/Api.Tests`, against the built host | §5: every reaction in the solution is registered with the dispatcher; each reaction command's policy admits its own module's service principal and refuses every other module's and a caller holding every permission; a module's service principal holds no other policy; no route dispatches a reaction's command |
| `tests/Catalog.IntegrationTests` | BE-92: delivery restores the writing transaction's trace context: the delivery span is the writer's span's child, and the reaction's log line and audit record carry the writer's trace id. §5: a reaction runs as its module's service principal on behalf of the initiating user, and passes that user and its cause on; a redelivered message changes nothing (§11); a message whose reaction fails stays undelivered for the next pass; the host delivers on its own. The saga: an order whose stock is reserved is confirmed, one whose stock can't be is rejected, and a step that fails after a compensable one runs its compensation (§11); a confirmed order is past the point of no return. Ordering's versioned write answers 428 and 412 (BE-115), and another tenant's order is never read or cancelled (BE-117). |
| `tests/Platform.Tests` | A reaction that loses a race at commit isn't recorded under its key, so its retry runs again; its failure is audited with its cause |
| `tests/Domain.Tests` | Every outcome of `Product.ReserveStock` and `ReleaseStock` and of `Order` (BE-138 reaches the new module by itself) |

Decided while building:

- **The saga is choreographed** (modular law §5's default): an order is placed pending and sends Catalog's `ReserveStockV1`; Catalog replies with `StockReservedV1` or `StockReservationFailedV1`; Ordering confirms or rejects. Reserving is the compensable step and confirming the point of no return; an order cancelled in between is abandoned, and its confirmation sends `ReleaseStockV1`, which records the release and deletes nothing. A refusal is a reply, never a failed dispatch, so it commits and isn't retried. A reservation lives inside the product aggregate, since reserving changes the product's available stock in the same transaction.
- **The tenant scope travels with the message.** An outbox row records the writer's tenant, and the dispatcher scopes the reaction to it; without it, a reaction's repository would find nothing.
- **A reaction is exempt from the If-Match guard** (slice 2e): it carries no client's version and applies the state it loads; the aggregate's concurrency token still refuses a lost race at commit. An unversioned write that loses a race is not recorded under its key, since nothing happened; a reaction's retry under the same key runs again rather than replaying the conflict.
- **A reaction whose command fails leaves its message undelivered**, whatever the failure, so the next pass retries it; reactions are written so that a business refusal is a successful reply.
- An outbox row's `Type` is the message type's full name, not its assembly-qualified name, so a new assembly version still finds its reactions.
- **The seed proof runs in about 12 minutes, not 54.** Each seed has a copy of its own and runs only the tests it expects to fail, six seeds at a time. The SQL Server seeds share one server the script starts in Docker; each test run makes and drops a database of its own on it (`RANCHO_TEST_SQL`). Without the variable, the integration tests start their own container, as before.
- `Internals_are_visible_only_to_tests` now admits a friend whose name ends in `Tests`, so `Catalog.IntegrationTests` can drive the dispatcher; before, it required `.Tests`.

What differs from the law, or isn't checked:

- **No dead-letter and no attempt count.** A message whose reaction always fails is retried on every pass and logged each time; when the failure was recorded under its key, each retry replays it, so only an operator clears it. Each pass reads the oldest `BatchSize` undelivered messages of each module, so that many permanently failing messages would starve newer ones.
- **No lease on a message.** Two instances may deliver the same message at once; the idempotency key makes the second a replay, but both do the work of dispatching.
- **No orchestrator reference.** The saga has two steps and ordered compensations aren't needed, so the law's default applies. The orchestrator rules (more than three steps, or compensations in order) have no reference or check yet.
- "A step that must stop when the user loses access checks that explicitly" has no reference and no check; the reactions here act on stock and orders, which the user's access doesn't gate.
- "A read straight after a command may not show its reactions yet" is stated in `OrderResponse`'s doc, not checked.
- A domain event can't reach another module's reaction: the reference table keeps one module's Application from naming another's Domain, and reactions live only in Application. The architecture test still states it, as one arm of the test of what a reaction may hear.
- The SQL Server test project is still named `Catalog.IntegrationTests`, though it now tests Ordering too.

## Deferred: features modular law §12 doesn't cover yet

The rule for these is "stop and ask". Each row comes back as a check when the first project needs the feature, using the archived annex as source material.

| Feature | Rows |
|---|---|
| AI agents | BE-31, BE-58, BE-59, BE-62 to BE-69, BE-71, BE-72, BE-103, BE-114, BE-123 |
| Caching | BE-44 to BE-49, BE-60, BE-140 |
| Real-time | BE-74, BE-75, BE-121 |
| Uploads (modular law §12 doesn't name them yet) | test BE-120 |
| External authorities (modular §8 keeps the principle) | BE-99, BE-100, BE-102, BE-104 (BE-101 is covered in slice 2a) |

## Dropped: the check costs more than the mistake

| Row | Reason |
|---|---|
| BE-17 `<Feature>Reader` naming | Naming. The reference code shows it. |
| BE-36 Queries folders mirror Application folders | Naming. The reference code shows it. |
| BE-94 Metric tag keys from a closed set | It needs a custom analyzer, and a wrong tag is cheap to fix |
| BE-136 No `#region` | Cosmetic |
| BE-09 Domain events in the past tense | Already dropped in 4.1.0 |
