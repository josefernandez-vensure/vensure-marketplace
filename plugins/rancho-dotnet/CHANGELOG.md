# Changelog

## 3.0.0

- **Breaking.** Released together with `rancho` 4.0.0, whose core law it
  carries, and `rancho-react` 2.1.0. The case is in `RECORDS-4.0-ledger.md`
  at the repository root.
- **What changed:** this law now carries the server architecture that
  left the core law. That is:
  - cross-module writes as integration events, never an inline call;
  - the two-audience event as two types;
  - one schema per module, as BE-37;
  - validation on the message for every caller, with DTO annotations
    never the only validation;
  - outcomes as values, and exceptions translated only by an adapter;
  - the global `500` handler;
  - the whole upload rule;
  - a property's own class, and inheritance;
  - the owner of each core egress boundary (§7 *Egress owners*).

  BE-82 adds that no type reachable from an endpoint's signature is a
  `Domain` type, a Request DTO's property graph included. BE-01 and
  BE-37 take the checks of ROOT-01 and ROOT-04.
- **Core rules carried:** 7, down from 15. ROOT-01, -02, -04, -07, -08,
  -15, -17 and -20 retired into BE-01, BE-07, BE-37, BE-80, BE-108,
  BE-01, BE-61 and BE-54.
- The Core map maps the new core sections (§3–§6) and the contract law.
  Citations follow the new numbers.
- **Upgrade:** a project moves each check or test named for a retired
  ROOT id onto its BE row in `ENFORCEMENT-STATUS.md`. It adds BE-37's
  schema check and BE-82's Request DTO walk if it had them under ROOT-04
  and ROOT-07.
- §1: solutions, projects, project references and package references
  are created with the `dotnet` CLI - `dotnet new`, `dotnet sln add`,
  `dotnet reference add`, `dotnet package add` - never by writing the
  file. A project file is edited by hand only for what the CLI cannot
  express, such as the project's kind (§2). Solution-wide properties are
  set once in `Directory.Build.props`. No mark: reached by review.
- Carries core §6's new rule (§10 before 4.0): no test proves only what a successful
  `dotnet build` already proves.
- **What changed:** rules that only restated a broader rule are retired,
  each merged into the row that carries it: BE-02, BE-05, BE-13, BE-18
  and BE-52 into BE-01; BE-76 into BE-74; BE-77 into BE-82. Where a
  retired row checked something the broad row could not reach, that part
  moved into the broad row: BE-01 gains an `arch` half (no ASP.NET Core
  type outside `Api` beyond the admitted packages, even transitively; no
  `System.Data` and no unit-of-work port in a module `Application`), and
  BE-74 gains BE-76's client-interface and `IHubContext<T>` checks.
  Restating halves are trimmed from BE-37 (schema: ROOT-04), BE-40 (EF
  types: BE-01), BE-50 (named policy: now stated with BE-25), BE-56
  (`ToString()`: BE-126's test) and BE-124 (page size: BE-22). BE-32 no
  longer claims to be covered by BE-01, which does not reach an ambient
  transaction. 141 rows: 96 `build`, 12 `retired`; 95 marks.
- **What did not change:** no obligation is dropped; each is carried by
  the row named above. Rationale and row history are in `RECORDS.md`,
  `## 4.0: restatements retired`.
- **Upgrade:** a project folds any check or test named for BE-02, BE-05,
  BE-13, BE-18 or BE-52 into its BE-01 check, BE-76's into BE-74's and
  BE-77's into BE-82's, and updates those rows in its
  `ENFORCEMENT-STATUS.md`. The per-query page-size test BE-124 asked for
  is no longer required; BE-22's sweep carries it.

## 2.2.0

- Law 3.2. Released alone: `rancho` 3.0.0 and `rancho-react` 2.0.0 are
  unchanged. The ledger is `RECORDS-3.2-ledger.md` at the repository root.
- **What changed:** the .NET law is rewritten as short instructions. Long
  paragraphs become lists, multi-part rules become sub-bullets, each
  Testing entry names its own subject and id, and §15 lists one
  prohibition per line. Rule ids, ⚙ marks and inventory rows are
  unchanged.
- **What did not change:** no rule. An independent checklist of the old
  law's 924 obligations, conditions, exceptions, named tools, marks and
  Testing entries was checked item by item against the rewrite, and
  every finding was fixed and re-checked.
- **Upgrade:** nothing to do.

## 2.1.0

- Law 3.1. Released alone: `rancho` 3.0.0 and `rancho-react` 2.0.0 are
  unchanged. Every sentence and inventory row changed is listed in
  `RECORDS-3.1-ledger.md` at the repository root; the rationale is in
  this plugin's `RECORDS.md`, `## 2.1.0`.
- **What changed:**
  - Duplicate rules merged, one rule and one row per obligation: BE-42
    into BE-79, BE-41 into BE-13, BE-43 into BE-38, and the five
    query-side bullets of §4 into one (BE-14 and BE-16 into BE-108;
    BE-12 and BE-15 keep their rows). The retired ids keep their rows,
    status `retired`. 141 rows, 101 marks.
  - Rationale moved out of `SKILL.md` into `RECORDS.md`, annex pointers
    shortened, two restatements replaced by pointers. The load per .NET
    stream goes from ~19.9k to ~19.4k tokens; the ~16k target is not
    reached and needs a decision on structure (`RECORDS.md`).
- **What did not change:** no obligation, condition, exception, named
  tool or Testing entry was removed. A ⚙ mark left only with the rule it
  marked, merged into the row that survives.
- **Upgrade:** a project renames any test or check named for BE-14,
  BE-16, BE-41, BE-42 or BE-43 to the surviving id (BE-108, BE-13,
  BE-79, BE-38), and updates those rows in its `ENFORCEMENT-STATUS.md`.

## 2.0.0

- **Breaking.** Released together with `rancho` 3.0.0, whose core law it
  carries, and `rancho-react` 2.0.0. The rationale and history of each
  change are in this plugin's `RECORDS.md`, in its 3.0 stage sections
  (stage 0 to stage 8).
- **What changed:**
  - Agents, caching and real-time move to `references/annex-agents.md`,
    `annex-caching.md` and `annex-realtime.md`, each binding a tree once
    the tree meets its trigger. Inventory rows carry id, rule, section,
    mechanism, status, check, gap and source; 141 rows, 106 marks.
  - §1, §2: Microsoft.Extensions.Compliance joins the stack;
    `SharedKernel` and `Domain` admit
    `Microsoft.Extensions.Compliance.Abstractions`. An analyzer reference that adds no compile or runtime
    assembly is admitted in every project (BE-01). A real-time push
    adapter is implemented in `Api`.
  - §3, §4: the unit of work dispatches domain events to in-module
    reactions before it commits, in the same transaction; one aggregate
    per transaction is a SHOULD. A handler may dispatch another module's
    published query through the dispatcher. Translation is a switch
    expression with no discard arm: CS8524 is disabled, so an added enum
    member fails the build through CS8509, and each outcome of a type
    hierarchy has a test (BE-141).
  - §4 *Pipeline behaviors*: the idempotency and transaction behaviors
    become one unit-of-work behavior (item 4), so the idempotency record
    and its change are one commit. The fingerprint is an HMAC under a
    server key; a keyed failure result is recorded after the rollback;
    post-commit effects run after the commit (BE-28).
  - §4 *Pagination*: keyset only; offset paging is removed. A cursor is
    encoded through a codec port with ASP.NET Core Data Protection, the key
    ring persisted in the database or in Blob storage, never in a cache
    (BE-119). `ORDER BY` and the keyset predicate are generated from the
    descriptor (BE-23). A bounded collection declares its bound (BE-19).
  - §4, §5, §13: every aggregate carries an application-managed version as
    its concurrency token, replacing the row version. A command may return
    the new version (BE-11). `If-Match` maps to the expected version and
    `If-None-Match: *` to a create-only flag; a precondition failure is
    `412`, a missing `If-Match` `428`. A write returns the new `etag` in
    its body; a delete returns `204`.
  - §6: the object check goes through a resource-authorization port in
    `Platform.Application`; a query applies the record scope in its read
    (BE-117).
  - §7: a class is a `DataClassification`; `SharedKernel` defines one
    attribute per class (BE-54). Logging is through source-generated
    `LoggerMessage` methods with constant templates, every parameter but
    the first `ILogger`, `LogLevel` and `Exception` carrying its class
    (BE-55, BE-57); redaction is `EnableRedaction()` with the erasing
    redactor for `Confidential` and `Restricted`.
  - §9: every endpoint is under the `GlobalLimiter` or a named policy the
    registrar applies from the operation declaration; `[DisableRateLimiting]`
    is forbidden; `OnRejected` sets `429` and `Retry-After` (BE-70).
  - §10: Serilog is a provider, registered with
    `builder.Logging.AddSerilog(logger, dispose: true)`; `UseSerilog()` and
    the static `Log.Logger` are forbidden. The correlation id is the
    current `Activity`'s trace id; outbox rows carry the trace context; an
    outside caller's trace is linked, never adopted (BE-139); exemplars are
    trace-based. New *Audit trail* through `IAuditTrail` (BE-118). A
    processor drops `exception.message` for exception types the solution
    does not declare.
  - §13: problem types are declared URIs that generate `x-problem-types`;
    `x-data-class` is on every property and parameter schema (BE-83); the
    registrar sets `Cache-Control: no-store` or `private`; CORS admits the
    wire contract's headers; uploads are quarantined until scanned (BE-120);
    framework endpoints are a declared list (BE-81).
  - §11, §15: a test key is exempt only where `tests/test-keys.txt` lists it
    (BE-97). §15's items are marked BE-129 to BE-136. Time reaches the
    domain as an argument from `TimeProvider`.
  - New required tests: BE-115 to BE-118, BE-120, BE-122 to BE-128, BE-137
    to BE-139 and BE-141; in annexes, BE-121, BE-123 and BE-140.
- **What each project does to upgrade:**
  1. Do the core law's upgrade list (`rancho` 3.0.0), and add every new
     `⚙` row - BE-119, BE-129 to BE-136 - to `ENFORCEMENT-STATUS.md` (core
     §1).
  2. Rewrite each warning suppression that enforces a rule of the law so
     its reason is a `LAW-EXCEPTIONS.md` id (§1, core §11). List each test
     key by file and key in `tests/test-keys.txt` (BE-97).
  3. Reference analyzers with `IncludeAssets="analyzers"` and
     `PrivateAssets="all"` (BE-01); use ArchUnitNET 0.13.5 or later for
     `arch` checks.
  4. Disable CS8524 in the solution's build configuration; write
     translation as switch expressions with no discard arm, and add a test
     per outcome of a type hierarchy (§4, BE-141).
  5. Add `Microsoft.Extensions.Compliance.Abstractions` to `SharedKernel`
     and `Domain`; define one `DataClassificationAttribute` per class in
     `SharedKernel`; redeclare classes with them, a projected property
     naming its source's `Contracts` twin (§7, BE-54).
  6. Convert logging to `[LoggerMessage]` methods with constant templates
     and classified parameters; reference
     `Microsoft.Extensions.Telemetry.Abstractions` in each project that
     declares one; make CA2254 and CA1848 errors; call `EnableRedaction()`
     with the erasing redactor; remove the destructuring policy (§7,
     BE-55, BE-57).
  7. Register Serilog with `builder.Logging.AddSerilog(logger, dispose:
     true)`; remove `UseSerilog()` and any `Log.Logger` assignment (§10).
  8. Merge the idempotency and transaction behaviors into one unit-of-work
     behavior; make the fingerprint an HMAC classed `Internal`; record keyed
     failure results after the rollback (§4, BE-28).
  9. Dispatch domain events to in-module reactions inside the unit of work,
     before commit; move other effects to post-commit (§3, §4).
  10. Replace the row version with an application-managed version mapped as
      the concurrency token, with its migration (§5).
  11. Map `If-Match` and `If-None-Match: *` in endpoints; return `ETag` and
      the new `etag` in the body; `204` for a delete; `428` and `412` as the
      wire contract states (§13, BE-115).
  12. Remove offset paging; add the cursor codec port over Data Protection,
      with the key ring persisted by `PersistKeysToDbContext` or Blob
      storage with Key Vault; generate ordering from the descriptor (§4,
      BE-119, BE-23).
  13. Declare every problem type URI with its status, title and user-facing
      flag; override defaults through `CustomizeProblemDetails`; write
      `traceId` from `Activity.Current.TraceId` (§13, §10).
  14. Regenerate and commit the OpenAPI document, now carrying
      `x-data-class` on every property and parameter schema and
      `x-problem-types` (BE-83); the client regenerates from it.
  15. Set `Sdk.SetDefaultTextMapPropagator(new TraceContextPropagator())`;
      where an outside `traceparent` can reach the application, install a
      propagator that starts a new trace and links the caller's (BE-139);
      persist trace context in outbox rows; enable trace-based exemplars
      (§10).
  16. Add the resource-authorization port and each module's authorization
      handlers, and apply record scope in query reads (§6, BE-117).
  17. Add `IAuditTrail` and the audit tables (§10, BE-118).
  18. Put every endpoint under the `GlobalLimiter` or a registrar-applied
      policy, remove `[DisableRateLimiting]`, and set `429` and
      `Retry-After` in `OnRejected` (§9, BE-70, BE-127).
  19. Admit and expose the wire contract's headers in CORS; declare the
      framework endpoints as a list at the composition root (§13).
  20. Write the new required tests listed above for the rules that bind.
  - Only when the project accepts uploads: quarantine, scan and record them
    as §13 states (BE-120).
  - Only when the caching annex binds: invalidate from committed events
    after commit, log a failure at `Error`, and add BE-140.
  - Only when the real-time annex binds: move the push adapter to `Api`;
    close connections on authentication expiry with a bounded lifetime,
    rate-limit hub invocations with a hub filter, let the server assign
    groups (BE-121).
  - Only when the agents annex binds: project tool output into a tool DTO
    (BE-58, BE-59, BE-69); set `DisableWebSearch = true` on a `HarnessAgent`
    (BE-62); enforce budgets in Agent Framework middleware (§9).

## 1.1.0

- Amendment A-09, from a consuming project's compliance audit of 2026-10-30
  (items 1 to 4, 6, 9 and 10), with six conflicts the owners' review found
  and resolved in the same change. Record in `references/enforcement.md`.
  Carries the core law's 2.1.0 (A-08).
- **What each project does to upgrade:**
  - Reference `<Module>.Application` from `Api`, drop any transitive or
    `InternalsVisibleTo` route to it, and give each module's Infrastructure
    one `Add<Module>Module(IServiceCollection, IConfiguration)` that builds
    and registers its own clients, including its `DbContext`.
  - Pin Mediator 3.x. Make Infrastructure types `internal`, except
    registration entry points, concrete query handlers, which the generated
    registration names, and the types their constructors take.
  - Move an external authority's client into one Infrastructure adapter
    behind a port; nothing else references its types.
  - Move command and query message ids, and Response DTO ids, to the
    module's `Contracts` ids; a message references no `Domain` type.
  - Rewrite query handlers to read through Infrastructure read types or
    parameterized SQL into the DTO, never a `Domain` type; test each SQL
    handler against real SQL Server, and remove any raw-string SQL call.
  - Give each command that does not persist a client-supplied id, and move
    duplicating external effects, such as email, behind the outbox.
- §2: `Api` references `<Module>.Application` and names only Infrastructure
  registration entry points (BE-105, BE-106). Infrastructure types are
  `internal` and `InternalsVisibleTo` names test assemblies only (BE-34,
  BE-107). The composition root is defined as `Api` plus the registration
  entry points it calls, and BE-52, BE-95 and BE-98 are scoped to it. The package column admits `Microsoft.Extensions.*.Abstractions`,
  options and its configuration extensions, and the SDK an adapter wraps, and
  BE-01 now checks packages.
- §3: messages and Response DTOs reference no `Domain` type and carry
  `Contracts` ids (BE-109). BE-08 narrows to `Domain` and the messages.
- §4: every concrete query handler is in Infrastructure by convention, and
  references no `Domain` type (BE-108). It reads through Infrastructure read
  types or parameterized SQL, which applies the module's query filters itself
  and is tested against a real database (BE-112); raw-string SQL APIs are
  banned (BE-113). A command declares whether it persists; one that does not
  opens no unit of work, uses neither a key nor the outbox, and is idempotent
  naturally or by a client-supplied id, which an agent's invocation carries
  (BE-110, BE-111, BE-31). An effect a repeat would duplicate goes through the
  outbox. A conflict at commit becomes a `Result` conflict in the transaction
  behavior.
- §5: the adapter that receives a concurrency or unique-key exception
  translates it. §7: a class is declared on the domain type and its
  `Contracts` twin, which must agree, and a property whose type has no class
  declares one (BE-54); encryption of `Restricted` columns may live in the
  driver, and is transparent to every read path; the class is emitted on
  request properties and parameters (BE-83).
- §6: an external authority is reached through one Infrastructure adapter,
  which declares its SDK and translates its codes and failures (BE-102).
  §8: tool parameters are wire values from `Contracts`.
- §13: an endpoint may reference the message it dispatches and its DTOs' own
  types (BE-82). §15 admits a declared fault-isolation boundary and
  `IServiceScopeFactory` in a hosted service. BE-43's check skips
  compiler-generated types.
- Infrastructure types the project maps or binds - entities, read types,
  options classes - are an adapter kind (BE-34). The registration entry point
  sits at the project root (BE-35). An agent's client-supplied id has a
  required test (BE-114).
- Rows go from 104 to 114, and marks from 90 to 95 (the summary had counted
  91). The role carries 15 core rules, with ROOT-20.

## 1.0.0

- Split out of rancho 1.7.0, where this stack law was
  `skills/law/references/backend.md`. Released with rancho 2.0.0, whose core
  law it carries; amendment A-07 in the core inventory records why.
- Opens with the sections every stack law carries under core §2: its role, a
  core map from each core section to the section here that carries it, and
  its gaps. Then the stack law itself, §1–§15, numbered as before. The rules
  are unchanged: references to the root law now read core, and §7 says the
  class reaches every client, where it said the frontend.
- §16 Verification holds the `dotnet build` and `dotnet test` gate the core
  law's §10 used to state.
- The repository map the core law's §2 used to draw for `backend/` is now this
  stack law's **Layout**, relative to the tree root.
- `references/enforcement.md` holds this stack law's rows, the core rules its
  role carries under **Core rules carried**, and the records that concern this
  stack law alone.
