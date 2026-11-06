# Review of the engineering law, 2026-11-03

> **Status, as of 3.0 stage 0.** This is the input to the 3.0 work, kept as a record. Section 1 lists PR #9's defects. All of them were fixed before PR #9 merged as 2.1.0, along with two further rounds of findings from independent checks, so section 1 is closed. Sections 2 to 7 are open. Each 3.0 stage PR names the ids it closes. Two findings were corrected after the report was written: P-7 and N-9, both about Mediator versions. Their corrected wording is below.

This review covers the core law, the .NET law, the React law, and their three inventories. It reads branch `law-group1-2.1.0`, so it includes PR #9.

**How it was done:**

- Five reviewers worked in parallel, one scope each:
  - the core law;
  - .NET §1–§5;
  - .NET §6–§16;
  - the React law;
  - size and enforcement.
- Each reviewer was told to quote the law and back every claim about outside practice with a source it fetched, and to mark anything else unverified.
- I then rechecked the findings that carry the most weight. The **Checked** column records that work:
  - **✓ source:** I confirmed it against the primary source.
  - **✓ text:** I confirmed it by reading the law. These are provable contradictions.
  - **reviewer:** the reviewer's citation, not rechecked by me.

## Verdict

The owners are right. The law does not protect as much as it claims, in three ways.

1. **It contradicts itself.** The reviewers found about 23 distinct combinations of rules that leave no compliant answer, after removing duplicates. Nine of them are in PR #9, which I wrote to remove such combinations: it fixed the ones the audit named and created new ones.
2. **It claims more enforcement than it has.**
   - Core §1, and the prime directive loaded into every session, say that breaking a ⚙ rule "is a build failure".
   - The plugins ship no check code, and the inventories admit that "`build` means the check is buildable, not built".
   - Of 26 inventory checks spot-checked, about 11 can be built as described. About 10 are weaker than their rule, 3 can't be built at all as described, and 3 are stricter than their rule or conflict with another rule.
3. **It is too big, and too much of it is argument.**
   - A .NET work stream loads about 20k tokens of law before reading any code, and each parallel stream pays that again.
   - 26–40% of the rule files is rationale, not rules.
   - About 45% of the inventories is history.
   - About 1,900 words govern features most projects don't have yet (agents, caching, real-time).

Several rules also break their own sources or the documented behavior of the tools they name. Examples are below: SignalR authorization, rate limiting, Mediator dispatch, EF Core raw SQL, and RFC 9110 conditional requests.

The review also found gaps that matter more than the wording. The law has **no object-level authorization**, which is OWASP API1:2023, the top API risk. It has **no defined audit trail**. It also misses **SignalR's cached principal** and **response caching**.

---

## 1. Defects in PR #9 (fix before merging)

| # | Finding | Checked |
|---|---|---|
| P-1 | Core §6 says the commit step's result value is "the only result value an Application handler does not produce". That's false: validation, authorization and idempotency failures from the pipeline are result values too. | ✓ text |
| P-2 | Core §8 lets a class be declared only on a domain type, on its published twin, or on a projected property. That leaves nowhere to classify a page size, a cursor, a sort key or the page envelope. Unclassified data is `Restricted`, so the emitter fails and the client's URL rule refuses the cursor. | ✓ text |
| P-3 | BE-106 requires `Add<Module>Module(IServiceCollection, IConfiguration)`. BE-95 forbids `IConfiguration` outside `Api`. The same undefined "composition root" breaks §15 (resolving a factory registration from `IServiceProvider`) and §5 (`Api` must register a decorator it may not name). | ✓ text |
| P-4 | An external authority's SDK can go nowhere. `Api` may declare "never a package an Infrastructure project declares" and may name no SDK type, while BE-102 forbids the authority's types outside `Api`. | ✓ text |
| P-5 | A command whose effect is an external write may not use an idempotency key (BE-110). An agent tool call must carry one (BE-31). An agent-reachable upload therefore has no legal shape, and neither does a non-idempotent external call such as email or payment. | ✓ text |
| P-6 | BE-109 says a message references no `Domain` type. §8 says "Tool parameters MUST be typed domain values". | ✓ text |
| P-7 | The A-09 branch "Where the generator resolves a handler through its interface, the handler is `internal`" doesn't match the named library. Every version of Mediator names the concrete handler in its generated registration: 2.x registers and resolves the handler by its concrete type, and 3.x registers it under its `I…Handler<,>` interface with `typeof` the concrete class. Either way the handler must be public. | ✓ source: [2.1.5 generated code](https://ignatandrei.github.io/RSCG_Examples/v2/docs/Mediator); v3.0.2 template `Mediator.sbn-cs`, line 68 |
| P-8 | The field sets A-10 introduced can't be checked as the inventory claims. A field name doesn't say where a value came from, and following a value to its destination is beyond ESLint. Branded generated types checked at known destinations (navigation, storage, telemetry calls) can be built. | reviewer (openapi-typescript `transformProperty`) |
| P-9 | Requiring `SqlQuery` into a DTO has costs PR #9 didn't price: it skips global query filters such as soft-delete or tenant filters; ROOT-04 and BE-37 can no longer see cross-schema joins in reads; Always Encrypted supports equality only, so no sorting, `LIKE` or keyset paging on such columns; BE-14's NoTracking check becomes stricter than its rule. EF calls raw SQL "a last resort". | reviewer ([EF efficient querying](https://learn.microsoft.com/ef/core/performance/efficient-querying#using-sql-queries)) |
| P-10 | BE-105's check ("type references resolve to those sets") leaves out BCL and ASP.NET Core types, so every `Api` type fails. | ✓ text |
| P-11 | A-10 gave sign-out to the host shell but left with each app three things it can't own in a shared session: refresh (several apps refreshing one shared token race), clearing the session store, and requesting the narrowest scopes. | ✓ text |

My recommendation: fix these in PR #9 before it merges. Most are a sentence each. P-7 and P-9 need a decision:

- **P-7:** drop the dead branch, and record that handlers are public because the generator names them.
- **P-9:** allow LINQ over read types mapped in Infrastructure (keyless types or views) as well as parameterized `SqlQuery`, and forbid `SqlQueryRaw` with input.

## 2. Combinations with no compliant answer (already in the law)

### Core law

| # | Finding | Checked |
|---|---|---|
| C-1 | Core §9 says logs, metrics and traces share one identity. .NET §10 forbids unbounded metric dimensions. A per-request id can't be both. The standard answer is OpenTelemetry **exemplars**, which link a metric point to a trace by `trace_id`. | reviewer ([OTel exemplars](https://opentelemetry.io/docs/specs/otel/metrics/data-model/#exemplars)) |
| C-2 | Core §7 says an agent holds a "strict narrowing… never equal". .NET's formula, user ∩ manifest ∩ …, equals the user's own permissions whenever the manifest covers them. "The calling user" is also undefined for an autonomous run. | ✓ text |
| C-3 | Every tree must be bound to exactly one stack law, and only two roles exist. Infrastructure-as-code, CI scripts and end-to-end test suites have no possible binding. | ✓ text |
| C-4 | Core §4 says every endpoint returns a Response DTO. .NET allows `204` with no content, and core §8 has a file-export endpoint. | ✓ text |

### .NET law

| # | Finding | Checked |
|---|---|---|
| N-1 | **Pipeline order.** Idempotency (step 4) wraps Transaction (step 5), but the idempotency record "is written through this unit of work". By the time step 4 sees the result, the commit is done, and only step 4 may touch the store (BE-30). The rule that two concurrent attempts resolve "as one execution and one replay" also contradicts step 5's conflict mapping, and the IETF draft answers that case with `409`. | ✓ text; reviewer ([IETF idempotency draft -08](https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/)) |
| N-2 | **The authorization behavior can't compile.** It lives in `Platform.Application` and calls `IAuthorizationService.AuthorizeAsync(ClaimsPrincipal, …)`. BE-53 bans `System.Security.Claims` everywhere outside `Api`. | ✓ text |
| N-3 | **Contracts can't publish a page.** "nothing else from `Platform.Application` may appear in a contract's public surface", yet `Page<T>` lives there, and published collection queries must be paged. | ✓ text |
| N-4 | **Domain-event reactions have no write path.** Events are dispatched after commit, the transaction behavior wraps commands only, and "Handlers MUST NOT call other handlers". After-commit, in-memory dispatch is also at most once; Microsoft warns about this unless the events are stored in the original transaction. | reviewer ([MS domain events](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)) |
| N-5 | **Cross-module reads.** Core §3 permits synchronous reads through another module's published query. The only way to make one is to dispatch it, which "Handlers MUST NOT call other handlers" forbids. | ✓ text |
| N-6 | **Strongly-typed id converters.** EF value converters need a public `Guid` constructor and accessor on the id. BE-08 forbids a bare `Guid` on any public member of `Domain`, and BE-107 now closes the `InternalsVisibleTo` way around it. | reviewer ([EF value conversions](https://learn.microsoft.com/ef/core/modeling/value-conversions)) |
| N-7 | **Hubs.** A hub "MUST NOT reference any type other than DTOs and the dispatcher", yet it must construct the message it dispatches. This is the BE-82 defect A-09 fixed, left in BE-74. | ✓ text |
| N-8 | **Endpoint references.** BE-82's list leaves out `[Authorize]` and rate-limit attributes, policy constants, `Task`, `CancellationToken` and the endpoint base type, all of which §6 and §9 require. | ✓ text |
| N-9 | **The read-side cache decorator depends on the Mediator version, and the law named none.** It is "a decorator over `IQueryHandler<,>`". Mediator 2.x resolves handlers by their concrete type, so a decorator there is never called. Mediator 3.x resolves through the interface, so a decorator can work. **Corrected after verification:** the first draft of this review said the decorator could never work. PR #9 now pins Mediator 3.x in §1. | ✓ source: v3.0.2 template, lines 68 and 184 |
| N-10 | **Debug logging.** "`Debug` … MUST NOT be enabled in production" contradicts "emission MUST NOT [differ per environment]". Read together, they forbid `Debug` everywhere. | ✓ text |
| N-11 | **The domain example can't run.** It calls `_clock.UtcNow` inside an aggregate, but EF injects only its own services into entity constructors, so a materialized aggregate has no clock. Time has to be a method argument. | reviewer ([EF constructors](https://learn.microsoft.com/ef/core/modeling/constructors#injecting-services)) |

### React law

| # | Finding | Checked |
|---|---|---|
| R-1 | A global store is allowed "when two or more features need it", but stores MUST be in `features/<feature>/stores/` and no feature may import another. | ✓ text |
| R-2 | Query keys come from per-feature factories, and mutations must invalidate what they change. A mutation in feature A that changes data shown by feature B can't reach B's factory. | ✓ text |
| R-3 | Data that starts on the client, such as a tab, a sort order or saved grid state, has no class. Unclassified is `Restricted`, so none of it may go in a URL or in storage. TanStack Router keeps application state in search params, and §8 requires typed search params. | ✓ text |
| R-4 | `app/` may not be imported from outside it, but the configuration the law mandates writes `routeTree.gen.ts` outside `app/`, and that file imports from `app/routes/`. `providers.tsx` also imports the identity library, which FE-24 confines to one `shared/lib` module. | ✓ text |

## 3. Security gaps

| # | Gap | Source | Checked |
|---|---|---|---|
| S-1 | **No object-level authorization.** A policy is checked per message and nothing per record. Recommendation: accept A-32. Commands check the loaded record through a port; query SQL filters by the principal's scope. | [OWASP API1:2023](https://api-security.owasp.org/editions/2023/en/0xa1-broken-object-level-authorization) | reviewer |
| S-2 | **SignalR caches the principal for the life of the connection, and doesn't revalidate it.** A revoked role keeps working until the client reconnects, which defeats §6's revocation window. Hub calls also bypass the HTTP rate limiter. Require `CloseOnAuthenticationExpiration` or a bounded connection lifetime, plus a per-call limit in a hub filter. | [SignalR authn/authz](https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz?view=aspnetcore-10.0) | ✓ source |
| S-3 | **No defined audit trail.** "Audited" appears three times and is never defined, and sampled telemetry can't serve as the trail. Recommendation: accept A-34, with an unsampled, append-only record written in the command's own unit of work. | [OWASP ASVS 5.0 V16](https://github.com/OWASP/ASVS/blob/v5.0.0/5.0/en/0x25-V16-Security-Logging-and-Error-Handling.md) | reviewer |
| S-4 | **Browser and proxy caches.** Only the server-side cache is governed. Responses carrying `Confidential` or `Restricted` data need `Cache-Control: no-store`. | [ASVS 5.0 V14](https://github.com/OWASP/ASVS/blob/v5.0.0/5.0/en/0x23-V14-Data-Protection.md) | reviewer |
| S-5 | **Exception messages and auto-instrumentation.** Framework exception messages carry values, and OpenTelemetry's automatic HTTP and database spans don't pass through the "Logging behavior", the one owner core §8 gives that boundary. Recommendation: accept A-28, and make the telemetry pipeline that boundary's owner. | [OTel exception spans](https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-spans/) | reviewer |
| S-6 | **Uploads have no legal shape**, and file-upload security is not covered. Recommendation: accept A-33, plus a declared class and egress exception for file content. | [OWASP File Upload](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html) | reviewer |
| S-7 | **Cursors** carry sort-key values, which can be `Confidential`, in URLs, and nothing requires them to be encrypted. Use Data Protection. | text | ✓ text |
| S-8 | **Client retries.** TanStack Query retries failed queries 3 times by default, including `403`, which the law says must never be retried, and doesn't honor `Retry-After`. The law needs one client-wide retry rule and a test for it. | [TanStack important defaults](https://tanstack.com/query/latest/docs/framework/react/guides/important-defaults) | reviewer |

## 4. Wrong for the named tools, or stricter than the sources

- **Rate limiting (.NET §9)** — checked against the [source](https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0):
  - ASP.NET Core has a `GlobalLimiter` and named policies, but no "default policy".
  - BE-70's check looks for metadata the global limiter never adds.
  - `[DisableRateLimiting]` overrides even the global limiter, and nothing bans it.
  - `ConcurrencyLimiter` can't provide `Retry-After`.
  - The default rejection status is `503`, not `429`.
- **Data classification** — reviewer, [Microsoft.Extensions.Compliance](https://learn.microsoft.com/dotnet/core/extensions/data-classification):
  - Microsoft ships classification attributes and redaction for logging. The law reinvents both, and its BCL-only `SharedKernel` row forbids Microsoft's attribute.
  - CA2254 covers only the Microsoft.Extensions.Logging API, not Serilog's.
- **Agent budgets** — reviewer, [Agent Framework middleware](https://learn.microsoft.com/agent-framework/concepts/agents/middleware/):
  - Model spend and elapsed time accrue in the chat loop, not in message dispatch, so the budgets belong in Agent Framework middleware.
  - Agent Framework is not in §1 Stack.
- **Options validation (BE-96)** — reviewer: the check sweeps framework options classes too. A-21's claim that the source generator registers the validator is wrong; you register it yourself.
- **Idempotency status codes** — reviewer: the IETF draft uses `409` for a request still in flight, `422` for a different payload under the same key, and `400` for a missing key. The law says "conflict" for all of them.
- **Conditional requests** — reviewer, [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110):
  - A failed `If-Match` is `412`, not a `409` conflict.
  - A-35, one wire format for concurrency, needs A-18 first, because BE-11 limits what a command may return.
  - A SQL Server `rowversion` covers one row, so a change to a child row doesn't protect the aggregate root. Use an application-managed version on the root.
- **Pagination** — reviewer:
  - OWASP API4 asks for a server-enforced bound on records returned, not for paging, and Microsoft's API guidelines make paging a SHOULD. Recommendation: accept A-15.
  - BE-19's "`Page<T>` only" fails every query that returns a single item.
- **Aggregates (A-19)** — reviewer: Vernon calls one aggregate per transaction a "rule of thumb" and names reasons to break it. Recommendation: accept A-19.
- **Value objects** — reviewer: "MUST be mapped as owned or complex types" forbids the value converters EF documents for single-valued value objects.
- **RPC endpoints** — reviewer: Google AIP-136 uses `POST` for methods with side effects. The law's rule pushes domain transitions into status `PATCH`es, the `SetStatus` shape .NET §3 calls a defect.
- **React** — reviewer:
  - **FE-09** flags the canonical query-key factory pattern: a property, and array spreads inside the factory.
  - **A-24 (edit forms):** accept, widened to React Hook Form's `values` option.
  - **A-27 (rendering error text):** accept, modified: `type` is the identifier, and `title`/`detail` are shown only for types the server declares user-facing.
  - **A-36 (correlation):** modify. The W3C standard defines no response header (`traceresponse` was removed from Trace Context Level 2), so the client should send `traceparent` instead.

## 5. Enforcement honesty

- **The ⚙ claim.** Core §1 and the prime directive say "build failure"; the inventories say "buildable, not built". Recommendation: accept A-11. The core inventory already has the honest wording, "can be made to fail the build", and each project records what it has actually built.
- **Checks that can't be built as described:**
  - FE-07: `no-restricted-syntax` has no type information.
  - BE-12: banned-API lists apply per project, and query handlers share Infrastructure with code that must call `SaveChangesAsync`.
  - BE-63: ArchUnitNET can't see string literals.
- **Checks much weaker than their rule:**
  - FE-16, FE-23 and FE-25: these need taint tracking.
  - BE-31, BE-90: "by construction" accepts `Guid.Empty`.
  - BE-06: the repository port lives in Domain.
  - BE-23: checks the declaration, not the SQL that runs.
  - BE-82: needs call-graph analysis.
- **Checks stricter than their rule:**
  - BE-50 also bans `[Authorize(Policy = "X")]`.
  - BE-53 bans the claims namespace in the authorization behavior itself (N-2).
  - FE-09 flags the canonical pattern.
- **Rows that contradict each other:** BE-13 says "covered by BE-01", BE-41 says "no longer covered by BE-01".
- **Unmarked MUST rules with no required test,** against core §1. Examples: "`429` with `Retry-After`", "every behavior emits a span", "recoverable via REST", the React loading/error/empty states, and accessibility. §15 has no inventory rows at all, and much of it can't be expressed in BannedApiAnalyzers: `dynamic`, `#region`, `async void`, commented-out code.

## 6. Size and structure

| Measure | Now |
|---|---|
| Law loaded per .NET stream (core plus .NET, both read in full) | ~20k tokens |
| Law loaded per React stream | ~9–10k tokens |
| Rule text that is rationale | core ~40%, .NET 26–30%, React ~30% |
| Inventory text that is history | core 81%, .NET 28%, React 38% |
| Words governing features most projects lack (agents, caching, real-time) | ~1,900 |
| Rules stated twice (core vs stack, or within a stack law) | 30+ pairs |

The biggest cuts that drop no protection:

1. **Move the history** — inventory records, rewordings, and in-row "Added/Amended by" notes, about 6,800 words — into the CHANGELOGs or decision records.
2. **Move rationale paragraphs into the records,** about 2,000–2,500 words. A rule keeps one clause of why at most.
3. **Accept A-30:** agents, caching and real-time become annexes that bind when the feature first appears, about 1,900 words. The core §8 egress table stays.
4. **Delete the duplicates.** Stack laws point at the core id instead of restating it. In .NET, the five query-side bullets become one, and BE-13/BE-41, BE-79/BE-42 and BE-38/BE-43 merge.
5. **Move maintainer-only text out of what coding agents load:** the stack-law contract in core §2, §10's inventory duties, §11, the core maps, and the class-not-list authoring rule.
6. **Inventory rows reference the rule id** instead of restating the rule, with one shared legend across the three.

The estimated per-stream load drops from about 20k to about 11–12k tokens for .NET.

## 7. The audit's other amendments (Groups 2–5)

| Item | Verdict |
|---|---|
| A-11 · an honest ⚙ | **Accept** |
| A-12 · stop when rules collide | **Accept, reconciled.** The legitimate comments — suppressions, `@ts-expect-error`, `data-testid` — must go through the register, or they become "defects" |
| A-13 · exceptions register | **Accept, modified.** Each entry cites an upstream amendment request and an expiry. Fold in the other exception forms (core §8's boundary exception, suppressions, SHOULD justifications) instead of declaring them invalid. The UI-library clause moves to `rancho-react` |
| A-15 · bounded collections | **Accept.** Declare the bound in OpenAPI as `maxItems` |
| A-17 · ordering | **Accept** |
| A-18 · return the new version | **Accept,** with an application-managed version on the root |
| A-19 · aggregates per transaction | **Accept** |
| A-22 · why endpoints are types | **Accept** |
| A-23 · write rate-limit policy | **Accept,** applied by the registrar from the operation declaration, with `DisableRateLimiting` banned |
| A-24 · edit forms | **Accept, widened** |
| A-25 · UI-library wrappers | **Accept** |
| A-26 · feature folders | **Accept** |
| A-27 · rendering error text | **Modify** (§4) |
| A-28 · exceptions in telemetry | **Accept,** with telemetry as its own egress owner |
| A-30 · trigger-scoped annexes | **Accept** |
| A-31 · consumer first | **SHOULD, in VACA's planning,** not the law. "Next planned unit of work" can't be decided by a check |
| A-32 · object-level authorization | **Accept,** into core §7 |
| A-33 · file uploads | **Accept, extended** |
| A-34 · audit trail | **Accept, modified** |
| A-35 · `ETag`/`If-Match` | **Accept,** with A-18. `412` on a failed `If-Match` and `428` when it's missing |
| A-36 · client correlation | **Modify:** `traceparent` instead, and adopt W3C trace ids as the correlation id |
| A-37 · automated accessibility | **Accept** |
| A-38 · `InternalsVisibleTo` | Already carried by PR #9 (BE-107) |

## 8. Recommended path

1. **Fix PR #9's eleven defects (§1) and merge it.** Consuming projects are blocked on what it resolves.
2. **3.0, how the law reads:** an honest ⚙ (A-11), the collision rule (A-12) and the exceptions register (A-13). These change how every other rule is read and enforced, so they come first.
3. **3.0, security:** object-level authorization, the audit trail, SignalR revalidation, `no-store`, telemetry egress, uploads (S-1 to S-8).
4. **3.0, the remaining contradictions** (§2) and the tool corrections (§4).
5. **3.0, the restructure** (§6): history out, rationale out, annexes, duplicates removed.
6. **Re-derive every inventory check against the real tool,** and mark each realistic, weaker (an inventoried gap) or removed.

Steps 2–5 are one breaking release, because cutting text and fixing contradictions touch the same lines. Doing them as separate amendments would mean rewriting the same paragraphs several times.
