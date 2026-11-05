# Records - core law

What the core law and its inventory no longer carry, because a coding agent does not need it to apply a rule: the reasoning behind wording that would otherwise look arbitrary, and the history of how the law came to read as it does. Nothing here is a rule. Every rule is stated in `skills/law/SKILL.md` or in `AUTHORING.md`.

## Rationale moved from SKILL.md in 3.0 stage 1

Moved word for word, grouped by the section of `skills/law/SKILL.md` it came from.

### §4 API Contract - Idempotency

- Callers retry. Clients time out and resubmit, jobs re-run, and agents retry by default - a duplicate request is the normal case, not the edge case.

### §8 Data Boundaries

- After ROOT-18: A boundary rule the client cannot evaluate is not a rule.
- After "Agents reference sensitive records **by id**": This is what makes agents usable over regulated personal data at all.

### §9 Observability

- After "Every request, message, job, and agent run MUST be attributable end to end.": A log line that cannot be tied to the work that produced it is noise with a timestamp.
- After the correlation id crossing every boundary: A boundary that drops it breaks the trace at the exact point where a trace earns its keep.
- After "Logs, metrics, and traces MUST share that one identity.": Three signals that cannot be joined are three tools, not one system.
- After an agent run being reconstructable from telemetry alone: An agent whose actions cannot be replayed after the fact is not auditable, and an unauditable agent is not deployable.

## Records

Moved word for word from `skills/law/references/enforcement.md` in 3.0 stage 1. Paths in them are as written there, so `../SKILL.md` is the core law, `skills/law/SKILL.md`. Sections of the core law that 3.0 stage 1 moved to `AUTHORING.md` keep their numbers: a record citing `../SKILL.md` §11, the inventory half of §10, §1's rule that a rule states a class rather than its members, or §2's list of what every stack law carries, means that section in `AUTHORING.md`. The last sentence of that §1 paragraph, and §2's statement that a stack law may add rules and never weaken one, are stated in both. A record kept "in that stack law's inventory", or in `rancho-dotnet`'s or `rancho-react`'s inventory, is now in that plugin's `RECORDS.md`.

A record states a rule as it then read, why it resisted checking or had to change, and what replaced it. Records are kept as the reasoning behind wording that would otherwise look arbitrary, and as the record `../SKILL.md` §11 requires - the law changes first. A record that concerns the core law, or more than one stack law, is kept here. One that concerns a single stack law is kept in that stack law's inventory.

Records written before 2.0.0 call the core law the root, and name `backend.md` and `frontend.md`. Those two files are now the `rancho-dotnet` and `rancho-react` stack laws, each its plugin's `skills/law/SKILL.md`, with their section numbers unchanged.

### The badge problem

**Resolved.** Six rules - BE-29, BE-49, BE-65, BE-66, BE-72, FE-17 - were provable only by a behavioural test written per instance. Nothing failed when that test was never written, which is precisely the failure ⚙ exists to prevent, so the badge was false on all six as §1 defines it.

The fix taken was to strip the mark and keep the obligation: each rule stays MUST, and each is now named in a Testing section - backend §14 under **Idempotency**, **Cache**, **Agents**, and **Budgets**, frontend §13 under **Data boundaries**. `../SKILL.md` §1 states the convention, so an unmarked rule of this kind reads as deliberate rather than forgotten. The alternative - a second marker meaning "enforced by a mandatory test" - was rejected: it would have put a second symbol on every page to describe six rules, and it would have left ⚙ meaning two different things.

One bullet had to be split to do this. BE-28 (a unique index on the attempt identity, a `contract` check) and BE-29 (what happens when two attempts race, only a test can say) shared a sentence and therefore a mark. They are now two rules, and only the first carries one.

Sections written after this decision apply it from the start: BE-89, BE-92, BE-99, BE-101, BE-103, BE-104, FE-27, and FE-29 were never marked, because no build check can reach them. They arrived as required tests. That is the convention working rather than being repaired.

Promotion out of this category is possible and has happened once: BE-23's determinism was test-only until R-07 required paginated queries to declare their ordering as inspectable metadata rather than burying it in a LINQ chain. The same move may be available for some of the six - if it is, it is a change to the law under §10, not a change to a test.

### Rewordings applied

All fifteen are now in the law. Each entry records the rule, why the original wording resisted checking, and what replaced it. The three that touch a root rule are kept here. R-04 to R-12 are in `rancho-dotnet`'s inventory, and R-13 to R-15 in `rancho-react`'s.

**R-01 · ROOT-03 · "An event with both audiences is two events."**
A design judgment about intent. No mechanical test distinguishes one event serving two audiences from one event that legitimately has one. *Applied:* drop the ⚙ and keep the sentence as guidance. The enforceable half - domain events do not leave the module - is already ROOT-02/BE-07.

**R-02 · ROOT-10, FE-07 · "No hand-written interface describing a server payload."**
"Describing a server payload" is intent. A local interface that happens to match a DTO is indistinguishable from any other type. *Applied:* reword to the checkable form - *every call to the generated client MUST use the generated types; a type assertion or cast on a generated client response is forbidden.* That is an ESLint rule over a known import path.

**R-03 · ROOT-14 · "Deny by default, everywhere."**
A principle spanning authorization, rate limiting, classification, and configuration. Not one check. *Applied:* remove the ⚙ from the principle and let it stand as the stated intent of BE-50, BE-51, BE-70, BE-73, and ROOT-16, each of which is separately checkable.

### Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under `../SKILL.md` §11 - the law first, the codebase after.

**A-01 · ROOT-05 · "Frontend folders mirror backend modules one-to-one."**
The rule asserted a bijection the domain does not have. Backend modules are consistency boundaries; frontend features are user capabilities. One bounded context is usually several capabilities, and one screen usually reads several contexts - so the check failed in both directions: a backend module with no UI demanded an empty folder, and a UI-only capability could not have one at all. Worse, the frontend tree had `modules/`, `shared/` and `generated/` and no composition layer, while FE-03 forbade cross-module imports and FE-04 forbade `shared/` importing modules. A screen spanning two modules therefore had no legal home anywhere in the tree, which makes a suppression comment the only way to ship it. *Applied:* `frontend.md` §2 is rewritten around a one-way dependency graph, `shared` -> `features` -> `app`, with cross-feature composition in `app/routes/` - the structure Bulletproof React and Feature-Sliced Design converge on, and the one TanStack Router's generated route tree already assumes. The mirroring check becomes a registry check (FE-28): a feature declares the backend module it consumes and reuses its name, or declares itself UI-only with a reason. Vocabulary drift stays caught; the false failures go. Net effect on the inventory at the time: the mirroring row became a registry row and the frontend gained one. A-02 then moved it out of the root table altogether.

**A-02 · ROOT-05, ROOT-06, ROOT-10 · Frontend rules stated in the root law.**
The root law carried three rules that bind only the frontend tree: how its folders are named, that its features may not import each other, and that a generated client response may not be cast. All three were already stated in full in `frontend.md`, so the root copy governed nothing the subordinate file did not already govern - it only made the root read as though one decomposition covered both trees. A-01 made this worse by writing the registry rule into §3, a section whose every other bullet is about .NET project references, EF schemas, and the outbox. *Applied:* the three rules leave the root law and the root table; §3 opens by scoping itself to backend modules and pointing at `frontend.md` §2; §4 keeps the boundary fact - frontend types are generated from the OpenAPI document - and delegates what the frontend does with them. The line drawn: **the root states obligations that span the boundary or bind both trees; each subordinate file states what is internal to its own.** FE-28's check was rescoped to match, reading only `src/features/` and the registry, with backend name reuse demoted from a cross-tree ⚙ to a SHOULD. Root drops from 19 rows to 16.

**A-03 · §6 Authorization, frontend §9 · The law named an identity provider instead of a shape.**
Backend §6 said "Entra ID claims are mapped to policies", and frontend §9 opened with "Entra ID through MSAL". The organization's applications authenticate through a different, federating provider, and their tokens carry no roles: permissions come from a remote authorization service through a vendor package. The law's intent held - deny by default, named policies, no raw claims below `Api`, a browser that decides nothing - but by naming one provider it was silent on the three failure modes a remote authority has and a token claim does not. The authority can be unreachable, and nothing said an outage denies (BE-51 covers a forgotten attribute, not an unreachable authority). Its answers can be cached past a revocation, and nothing bounded how long. It answers for the user, and nothing said that answer is still intersected with an agent's manifest under §8. On the frontend, sign-out cleared local caches but could leave the provider session alive, so the next person at a shared workstation signed in silently as the previous one. *Applied:* the provider names leave both files, and provider choice is recorded in each application's `DECISIONS.md`. Backend §6 gains the rule that a permission source is a composition-root concern with the authority's codes confined to `Api` (BE-102), a subsection on external permission authorities - fail closed (BE-99), a bounded and declared cache lifetime that an outage never extends (BE-100, BE-99), intersection for agent principals (BE-103) - and the rule that a transport filter is never the only enforcement point, applying `../SKILL.md` §7 where the wiring happens (BE-101). Frontend §9 gains ending the provider session on sign-out (FE-29) and the rule that a credential nested inside a token is never extracted (FE-30). The cache lifetime MUST be bounded and declared and SHOULD be configurable: a vendor client that caches for a fixed period complies by stating that period, so the rule does not fail every consumer of a package that does not expose it, and a lifetime nobody can state is still a defect. The four runtime rules arrived as required tests, named in backend §14 and frontend §13. Backend rows go from 98 to 103, frontend from 28 to 30.

**A-04 · BE-39, BE-80, BE-95, BE-97, BE-98, FE-01, FE-16, FE-19, FE-23, FE-25, §15 · A ⚙ rule named members of its class instead of the class.**
A machine-checked rule is read as exhaustive: the check is taken to be the boundary, so whatever passes it is permitted. A rule that enumerates therefore grants every member it forgot to name. BE-98 named `IsProduction()` and `IsDevelopment()`; `IsStaging()`, `IsEnvironment("Development")` and a comparison on `EnvironmentName` are the same question, and a check built to the text let them through. BE-97 named four kinds of secret in three places; a consuming application's reference implementation commits a `nuget.config` whose `<packageSourceCredentials>` holds placeholders, one local substitution away from a committed package-feed token - a kind and a place the rule did not name. A sweep for the same shape found it again in rules nobody had tripped over yet. BE-95 banned `IConfiguration` but not the environment variables behind it. BE-39 banned the InMemory provider and left SQLite. BE-80 named `MapGet` and `MapPost`. FE-01 omitted `@ts-nocheck`. FE-25 and FE-23 listed different storage APIs for the same concern, and FE-25's list lacked IndexedDB. FE-16 omitted the fragment, FE-19 `vi.doMock`, and §15's clock item `DateTimeOffset.UtcNow` and `DateTime.Today`. *Applied:* each rule now states its class, with members kept only as marked illustrations. `../SKILL.md` §1 makes that the rule for writing rules, and §10 adds the converse case: a check stricter than its rule is the same contradiction, resolved by widening the rule or narrowing the check in the same change. BE-98's check moves from predicates to types, which closes the `EnvironmentName` gap a predicate ban leaves, and telemetry that needs the environment's name gets it from the composition root. BE-97 gains the requirement that a committed credential slot holds a reference, which is what makes the check decidable - a sweep cannot tell a substituted value from the placeholder it replaced. BE-97's row records its two genuinely different checks, `contract` for the committed set and `ci` for history, and the `contract` mechanism admits committed-file inspection. No rule id is added or retired and the row counts stand.

**A-05 · §6 Authorization, frontend §1, §2, §9, §11 · The law named an identity library instead of a shape.**
A-03 took the provider's name out of the law and left its library in. Frontend §9 was written around MSAL: tokens acquired silently from its cache, never stored by the application, the MSAL cache cleared on sign-out. An application mounted in the organization's host shell uses none of that. The shell's own login application signs the user in and writes the access, ID and refresh tokens, with the user's profile, to one `localStorage` store that the shell and every application on the origin share, and the library offers no memory-only option. Such an application could not comply with FE-25 whatever its own code did, because the only token it can reach is in a store it does not own, and `../SKILL.md` §11 forbids working around the rule in one repository. What FE-25 protects - no second copy, and no store the application owns - still held; only its wording assumed a library. On the backend, the organization's authentication package reads routing claims off a token before its signature is checked, then validates it with an uncached remote call. Nothing limited what an unvalidated claim may decide, and backend §6's external-authority rules covered a remote permission service but not a remote validation service with the same three failure modes. *Applied:* frontend §1 names the identity library as a class, recorded in `DECISIONS.md` as A-03 did for the provider. §2 places a host shell's lifecycle entry in `app/`. §9 forbids an application in a host shell from running its own sign-in, widens FE-24 to the library's session store and FE-25 from "stored" to "retained beyond the request", names that store as the one place a token may live with the shortest-lived store the library supports, states the grant rule as a class - scopes, a service name, or any other - and has sign-out clear the store through the library. §11 excepts that store for the user's own profile and forbids the application writing to it. FE-27's interactive step is the shell's sign-in page. Backend §6 gains BE-104: a claim read before validation may only select how the token is validated, and must agree with the validated token. The subsection becomes "External authorities", so BE-99 and BE-100 cover a validating authority too. Nothing is weakened for an application that runs its own sign-in: where its library offers a memory store, FE-25 now requires it. Backend rows go from 103 to 104; frontend stays at 30.

**A-07 · §2, §3, §6, §8, §9, §10, §11 · The law named its stacks instead of a role.**
The root law was written for one pair of trees: a `backend/` in .NET and a `frontend/` in React. §2 drew their folders, §10 ran `dotnet` and `pnpm`, and the mechanism of half its sections was a path into `backend.md` or `frontend.md`, all three files shipped in one plugin with the development process that reads them. VACA inherited the coupling: its agents read "the backend or frontend reference", so a project on any other stack had no law to read, and a project could not take a different stack law without every project taking it. Nothing the root law required was about .NET or React; only where it pointed was. *Applied:* the root becomes the core law, the same in every project, and each technology becomes a stack law in a plugin of its own - `rancho-dotnet`, role `server`, and `rancho-react`, role `client`, from `backend.md` and `frontend.md` with their section numbers unchanged. A project enables the stack laws it builds with. §2 replaces the repository map with the binding - every tree names exactly one stack law in its `CLAUDE.md`, and an unbound tree stops work rather than being guessed at - the two roles, and what every stack law carries, including a core map from each core section to its mechanism and a gap list for any it cannot carry. §3 states the module boundary as internals and published contracts, not as four .NET projects; §6 names a result value and RFC 9457 problem details, not a .NET type; §8's egress table names a real-time client, not SignalR; §9 says server to client. §10 runs the verification of every stack law a change touches, and requires each stack law to carry the check of every core rule its role binds. §11 extends to stack laws, and sends a rule every stack law of a role would need to the core. The ROOT table loses its mechanism, effort and check columns, which were .NET, and gains the role each rule binds; each stack inventory carries those rows under **Core rules carried**. One stack rule is widened to match: `rancho-react` §12 named SignalR as the only channel a client could receive real-time messages over, which assumed the server's stack. It now names the class, and FE-18's packages move to the project's lint configuration. No rule is weakened and no id is added or retired. ROOT-18, previously canonical at BE-83 alone, is carried by both roles: BE-83 for the server, FE-22 for the client.

**A-08 · §6, §8 · The core law forbade what the framework requires, and asked one fact to be declared on every type that carries it.**
A consuming project's compliance audit of 2026-10-30 found code shaped to pass a check while breaking the rule behind it. Two of its items, 3 and 5, reach the core law. *Exceptions:* "Exceptions MUST NOT be control flow" read as forbidding what a persistence framework forces - EF Core, for one, signals a concurrency conflict or a unique-key race only by throwing - so a consuming project threw its own exception and caught it one frame up rather than catching the framework's where it is raised. The owners' review found a second gap behind it: such a conflict is often raised when the unit of work commits, after the handler has returned, so the one boundary §6 gave to the Application handler could not translate it. *Applied:* an adapter MAY catch the exceptions its framework or SDK uses to signal an expected outcome - a concurrency conflict, a unique-key violation, or an unreachable or failing remote service, which the server stack law's fail-closed authority rules also require catching - and MUST translate them where they are raised. §6 gains a fourth boundary, commit outcome to result value, owned by the one pipeline step that commits, and says that the pipeline's own steps return result values without a boundary. A draft that called the commit outcome the only result value a handler does not produce, and that limited the catch to persistence adapters, was corrected before release. *Classification:* "declared once, on the type that carries the value" combined with typed ids and "no domain type reachable from HTTP" to demand a new type per response row, and then a class declaration on each - one consuming project held ten id types for one concept and one name's class declared eight times, the drift the rule exists to prevent. On the client, an unclassified request value, such as a typed search term, is `Restricted` and so could not reach a URL at all. *Applied:* a class is declared twice per concept - on the domain type and on the published contract type, since a server stack law may keep the two from referencing each other - and a check proves they agree. Every other carrier of a classified value inherits it. A property whose type carries no class of its own - a page size, a cursor, a computed flag, a field of a shared platform type, or a value projected from a classified source - declares its class on the property, and a projected one is checked against its source. A draft that admitted only the projected case left a cursor and the page envelope unclassifiable, so `Restricted`, and was corrected before release. ROOT-18 now carries the class of request properties and parameters as well as response properties, and the client enforces it at its own boundaries by its own mechanism. One row is added, ROOT-20, because the agreement check is a machine-checked obligation every server stack law must carry; no id is retired.

A-06 and A-10 concern `rancho-react` alone, and A-09 `rancho-dotnet` alone; each is kept in its own inventory.

### Prerequisites (all adopted)

Four checks were blocked on an architectural decision rather than on the work of writing a test. All four decisions are now in the law. P-03 spans both roles and is kept here. P-01, P-02 and P-04 are in `rancho-dotnet`'s inventory.

**P-03 · Classification reaches the frontend.** *Adopted.* The browser had no way to know which fields were classified, which is why R-15 had to strip FE-16's mark. The class is now emitted into the OpenAPI document as a vendor extension (BE-83), carried into the generated types (FE-22), and readable by ESLint. That restored FE-16 and made FE-23 checkable. FE-17 stays `test`: a lint rule can catch a classified value handed to a telemetry call, but whether an SDK's scrubbing configuration is correct is a runtime fact, not a static one.

### Former status legend

The inventory's status legend before 3.0 stage 1, word for word:

| Status | Meaning |
|---|---|
| `required-test` | No build check can reach it. The ⚙ is stripped and the obligation is named in a Testing section, which is where it is now caught. See [The badge problem](#the-badge-problem). |
| `reword` | Not decidable as worded. Resolved in [Rewordings applied](#rewordings-applied). None remain. |

**Effort** is for the check, not the rule: `S` under an hour, `M` a day, `L` more than a day. The Effort column was dropped in 3.0 stage 1.

## Row history

From the note under the inventory's root table: "ROOT-03 is dropped by R-01 and ROOT-14 by R-03: each rule stands, enforced by review. ROOT-05, ROOT-06 and ROOT-10 left the root law under A-02 and are retired."

- ROOT-03: Dropped by R-01; the rule stands, enforced by review.
- ROOT-05: Left the root law under A-02; retired. It had no row in the inventory at 3.0 stage 1.
- ROOT-06: Left the root law under A-02; retired. It had no row in the inventory at 3.0 stage 1.
- ROOT-10: Left the root law under A-02; retired. It had no row in the inventory at 3.0 stage 1.
- ROOT-14: Dropped by R-03; the rule stands, enforced by review.

## 3.0 stage 3

- §4: RPC-style endpoints admit `POST /<resource>/{id}/<verb>` for a transition of one resource, and are permitted for any operation that is not a plain create, replace, update or delete. Closes CORE-15.
- §4, ROOT-07: an endpoint returns a Response DTO, no body, or a stream it declares; the inventory row's rule text follows. Closes CORE-14.
- §4 *Collections*, ROOT-11: only a collection not bounded by construction must be paginated; a bounded one declares `maxItems` and rejects a request that would exceed it. The inventory row's rule text follows. Closes CORE-16, A-15.
- §4 *Idempotency*: a missing key is `400`, the same key with a different payload `422`, and a duplicate still in progress a replay or `409`, never `500`. Closes CORE-18.
- §4 *Wire contract*: new subsection stating the formats both trees must agree on - `x-data-class`, the page shape, problem details (`type`, `errors`, `traceId`, `x-problem-types`), concurrency (`ETag`, `If-Match`, `428`, `412`), and `traceparent`. Closes CORE-17.
- §9: the correlation id is the W3C Trace Context trace id; a server adopts an inside caller's `traceparent` and otherwise starts and links a new trace; metrics join traces through exemplars. Closes C-1, A-36, CORE-19.
- Review fix: §4 *Wire contract*, Concurrency: the strong `ETag` combines the version with a representation-variant identifier and is compared by its version part; a Response DTO of a versioned resource carries `etag`; `If-Match: *` is refused with `428`; a create naming its own id carries `If-None-Match: *`; a successful write returns the target's new `ETag`, with `Content-Location` for a verb URI.
- Review fix: §4 *Wire contract*, Problem details: every problem has a declared `type`, never a default per status; `x-problem-types` is a top-level object keyed by type URI carrying `status`, `title` and `userFacing`; the validation type is user-facing; `errors` keys are request-body paths with a stated syntax; `traceId` is the 32-hex-digit trace-id, not a whole `traceparent`.
- Review fix: §4 *Wire contract*, Data class: every schema the document contains carries `x-data-class`, framework schemas included.
- Review fix: §4 *Wire contract*, Page: paging is keyset; the cursor is the only position a request may send.
- Review fix: §4 *Wire contract*: new Stream bullet defining the declared stream ROOT-07 admits - one representation of a declared media type, not a collection, under the export rule of §8.
- Review fix: §4 *Wire contract*, Trace context: requests carry `tracestate` when present beside `traceparent`.
- Review fix: §9: a server adopts the caller's `traceparent` only for a caller inside its trust boundary - another service of the same deployment, authenticated as such; for a browser or other external caller it starts and links a new trace, whose id reaches the caller as `traceId` on a problem details response.
- Second review fix: §4 *Wire contract*, Concurrency: the `ETag` is the version itself, as a strong entity tag; the representation-variant ETag and `Content-Location` are dropped as unowned and against RFC 9110 §8.7 / §13.1.1. A principal-dependent response is sent `Cache-Control: private` or stricter, `If-Match` is evaluated before the write, and a successful write returns the new `etag` in its body.
- Second review fix: §4 *Wire contract*, Problem details: `errors` keys are paths into the request - the body's JSON property paths, or a query or path parameter's name.
- Second review fix: §4 *Wire contract*, Data class: every property schema the document contains carries `x-data-class`.
- Second review fix: §9: a server adopts the caller's `traceparent` only for a request arriving through an edge that only callers inside its trust boundary can reach; trust is decided by the edge, before authentication runs, never by the caller's credentials.
- Second review fix: §9: the correlation id crosses server to client on every problem details response.

## 3.0 stage 4

- §7: authorization has two parts - the message's policy, and an object check on the records an operation names; a query returns only the records the principal may see, filtered where they are read. Closes S-1, A-32, CORE-7.
- §7 *File uploads*: new subsection - upload policy, server-enforced size and type allowlist, content-based type validation, server-generated storage name outside any served path, quarantine until a malware scan passes, and a declared class (undeclared is `Restricted`). Closes S-6, A-33.
- §8 egress table, Response DTO row: a response carrying `Confidential` or `Restricted` data is sent `Cache-Control: no-store`. Closes S-4, CORE-12.
- §8 egress table, telemetry row: the owner is the telemetry pipeline - the logging behavior and every processor and exporter, automatic instrumentation included; a new bullet admits an exception's type and stack trace, its message only where our code wrote it, and drops or scrubs a message from code we do not control. §6: "log with full context" is bounded by §8's telemetry rule. Closes S-5, A-28, CORE-13.
- §9 *Audit trail*: new subsection - every failed authorization, executed command and read of `Restricted` data is audited (principal, agent, operation, record ids, time, outcome), separate from telemetry, never sampled, append-only, retained as `DECISIONS.md` declares. Closes S-3, A-34.
- Review fix: §8 egress table, Response DTO row: `Cache-Control: no-store` applies to an HTTP response.
- Review fix: §7 *File uploads*: a file is stored under a name the server generates or derives, never one the client chose.

## 3.0 stage 5

- §7: AI agents hold a subset of the calling user's permissions - never wider - and always carry the actor claim that identifies the agent, replacing "a strict narrowing ... never equal". Closes C-2, NET-B-13.
- §4 *Limits*: a rejection is `429`, with `Retry-After` wherever the limiter can estimate it. Closes NET-B-8, A-23.
- §9: sampling rates and log levels MAY differ per environment; whether each signal is emitted MUST NOT. Closes N-10.
- Review fix: §8, the declaring-a-class bullet: a parameter of a logging method declares the class of the value it logs, and the same check proves the declaration agrees with the value's type.
- Review fix: §7: an autonomous agent run holds a subset of the permissions of the service principal declared for its manifest, never wider, in place of the calling user's.

## Rationale moved in 3.0 stage 5

Reasons, consequences and examples removed from `skills/law/SKILL.md` when the law was shortened in stage 5, verbatim, grouped by section and prefixed with the rule they came from. No rule changed; the rule text stays in the law.

### §4 API Contract

- **ROOT-11** (Collections): An unbounded list response is a defect - with agents in the system it is also an exfiltration path and a cost incident.
- **Wire contract (introduction)** (Wire contract): The two trees share only what this document states (§2), so

### §6 Error Contract

- **An exception escaping a handler**: An exception escaping a handler means an invariant was breached, i.e. an upstream guard failed.

### §7 Security Baseline

- **Deny by default, everywhere**: The principle is not itself checkable; the rules that implement it are, and each carries its own mark.

### §8 Data Boundaries

- **ROOT-20**: two declarations of one fact drift unless something holds them together.
- **Egress table, model's context row** (Egress boundaries): The model vendor is an external processor.
- **An exception's type and stack trace MAY be recorded in telemetry** (Egress boundaries): such messages routinely carry the values that caused them.
- **Egress to a model is not reversible** (Egress boundaries): Egress to a model is not reversible. Once a value is sent it is disclosed
- **ROOT-17** (Egress boundaries): They are persisted in the outbox, replayed, and read by operators

### §10 Verification

- **A check MUST NOT be stricter than its rule**: the check will be read as the rule, and the next reader of the rule is entitled to approve what the check refuses.

## 3.0 stage 6

- §11: a conflict entry carries the rule id - or, for a rule with none, its section and the sentence it departs from. An unmarked rule has no id, and a conflict entry against one had no form. Found while closing the React law's unmarked MUSTs in stage 6.
