---
name: law
description: The binding engineering law for this codebase - module boundaries, API and error contracts, validation placement, data classification and egress, AI-agent permissions, and observability. Read before writing, reviewing, or placing any backend (.NET/DDD) or frontend (React) code; before designing an endpoint, integration event, or agent tool; and whenever deciding whether a change is compliant.
---

# Engineering Law

## 0. Prime Directive

This document is the **law**. The codebase is the **idiom**.

- The law defines what MUST be true. It is not advisory.
- The codebase defines how the law is expressed here: naming, layout, ordering, phrasing. Before writing code, read the nearest existing implementation of the same kind and match it.
- **When the codebase contradicts the law, the law wins.** Report the contradiction as a defect. Existing code is never permission to repeat a violation.
- When the law is silent, follow the codebase. When both are silent, ask.

Never generalize a rule from a single example. Confirm against a second occurrence first.

## 1. Rule Language

- **MUST / MUST NOT** - absolute. A violation fails review.
- **SHOULD / SHOULD NOT** - default. Deviation requires a one-line justification in the PR.
- **MAY** - permitted, no justification needed.

Silence is not permission. If a decision is uncovered, match the nearest precedent in the codebase.

Rules marked ⚙ are machine-checked. Violating one is a build failure, not a review comment.

A rule no build check can reach carries no mark. It is carried instead by a named required test, listed in the Testing section of the file that owns it - equally binding, differently caught. `references/enforcement.md` maps every mark and every required test to its mechanism.

## 2. Repository Map

```
backend/
  src/
    Modules/<Module>/
      <Module>.Domain/          BCL only. No framework references.
      <Module>.Application/     Commands, queries, handlers, behaviors.
      <Module>.Infrastructure/  EF Core, adapters, DbContext.
      <Module>.Contracts/       Public surface. The only project other modules may reference.
    Shared/SharedKernel/        Base types only. No business rules.
    Api/                        Composition root, endpoints, auth wiring.
  tests/
frontend/
  src/
    app/                        Composition root: routes, router, providers.
    features/<feature>/         One user-facing capability.
    shared/
    generated/                  OpenAPI output. Never hand-edited.
```

Backend rules: `references/backend.md`. Frontend rules: `references/frontend.md`. Both are files in this skill's own directory, alongside this one. Read the relevant one **in full** before writing code in that tree - this document alone is not sufficient to write compliant code in either tree. `references/enforcement.md` is the inventory of every machine-checked rule; consult it by rule id rather than reading it whole.

## 3. Module Boundary Law

This section governs backend modules. Frontend boundaries are a different decomposition with their own rules, in `references/frontend.md` §2.

- A module MUST NOT reference another module's `Domain`, `Application`, or `Infrastructure`. Only `<Module>.Contracts`. ⚙
- Cross-module **writes** MUST be asynchronous: publish an integration event, committed with the state change through the outbox. Calling another module's write path inline is forbidden.
- Cross-module **reads** MAY be synchronous through the other module's published query contract.
- Domain events MUST NOT cross a module boundary. Integration events always do. ⚙
- An event with both audiences is two events, not one shared type.
- One database schema per module. Cross-schema joins and cross-module foreign keys are forbidden. References across modules are by id only. ⚙

## 4. API Contract

- REST by default. RPC-style endpoints (`POST /<resource>/<verb>`) are permitted when the operation is not a resource mutation. GraphQL is forbidden.
- Every endpoint MUST accept a Request DTO and return a Response DTO. Domain types MUST NOT appear in any signature reachable from HTTP. ⚙
- Request DTOs MUST map to a Command or Query. Endpoints map, dispatch, and translate. They contain no business logic.
- Query results MUST be projected directly to Response DTOs. Loading an aggregate to build a read model is forbidden. ⚙
- The OpenAPI document is generated and committed. ⚙
- Frontend API types are generated from that document, never hand-written. How the frontend consumes them - and that contract drift surfaces as a compile error rather than a runtime surprise - is `references/frontend.md` §3.

### Collections

- Every endpoint returning a collection MUST be paginated. An unbounded list response is a defect - with agents in the system it is also an exfiltration path and a cost incident. ⚙
- One page shape across the whole API. A per-feature paging envelope is a defect. ⚙
- The maximum page size is enforced by the server. A request above it MUST be rejected, never silently truncated.

### Idempotency

- Every state-changing endpoint MUST declare how it is idempotent: naturally, by client-supplied id, or by idempotency key. Undeclared is a defect. ⚙
- Where a key is used it travels as the `Idempotency-Key` header, and a replay MUST return the original response rather than executing again.
- Callers retry. Clients time out and resubmit, jobs re-run, and agents retry by default - a duplicate request is the normal case, not the edge case.

### Limits

- Every endpoint MUST be covered by a rate limit policy. A rejection is `429` with `Retry-After`, shaped as `ProblemDetails`.
- Transport limits do not bound an in-process caller. Agent runs carry their own budgets; see `references/backend.md` §9.

## 5. Validation

- Authoritative validation is a pipeline behavior on the Command/Query, not the endpoint. Callers that bypass HTTP - agents, jobs, tests - MUST receive identical validation.
- Request DTO annotations MAY duplicate it for early failure and OpenAPI documentation. They MUST NOT be the only validation.
- A handler MUST be able to assume its message is structurally valid, and MUST NOT re-validate shape.
- Frontend validation is UX. Nothing server-side may treat it as a guarantee.

## 6. Error Contract

Three translation boundaries. Each is owned by exactly one layer. No layer may skip one.

| Boundary | Owner | Rule |
|---|---|---|
| Domain outcome to `Result` | Application handler | The domain speaks in domain vocabulary and MUST NOT reference `Result`. |
| `Result` to `ProblemDetails` | API endpoint | Every failure response is RFC 9457 `ProblemDetails`. |
| Exception to 500 | Global middleware | Exceptions signal bugs, never business outcomes. |

- Expected business outcomes MUST be return values. Exceptions MUST NOT be control flow.
- An exception escaping a handler means an invariant was breached, i.e. an upstream guard failed. It is a defect: log with full context, return 500. Never catch a domain exception to convert it into a 4xx - add the missing guard instead.
- Error payloads MUST NOT leak exception messages, stack traces, SQL, or internal type names.

## 7. Security Baseline

- Deny by default, everywhere. Authorized-by-omission is a defect, never a default. The principle is not itself checkable; the rules that implement it are, and each carries its own mark.
- Authorization attaches to the **message** (Command/Query), not the transport. HTTP and in-process callers get identical checks.
- The acting principal MUST be explicit in the dispatch context. Reading ambient HTTP context from application or domain code is forbidden. ⚙
- AI agents MUST hold a strict narrowing of the calling user's permissions - never equal, never wider. Full rules in `references/backend.md` §8.
- Secrets MUST NOT appear in source, config files, or logs.

## 8. Data Boundaries

Classification is a property of the **data**, not of the endpoint that happens to return it. A field's class travels with it through every layer, DTO, log line, cache entry, and model prompt.

| Class | The test | Rule |
|---|---|---|
| `Public` | Already published outside the authenticated boundary, deliberately | No restriction. |
| `Internal` | Not secret, but not for publication: how the organization is arranged and how it operates | Stays inside the authenticated boundary. |
| `Confidential` | Attributable to a person or a counterparty, and disclosing something they would expect to control | Crosses a boundary only where the operation's policy grants that field. |
| `Restricted` | Regulated by statute or contract, or sufficient on its own to impersonate, defraud, or discriminate against its subject | MUST NOT cross any egress boundary below without a named, reviewed exception declared at that boundary. |

The test decides the class. The following illustrate it and do not enumerate it: `Public` - a published listing, marketing copy. `Internal` - org structure, site locations, operational metrics. `Confidential` - contact details, pay, performance, hours worked. `Restricted` - government identifiers, date of birth, bank and payment details, health and medical, protected-class attributes. A field this codebase holds that no example names is still classified, by the test.

- **Unclassified is `Restricted`.** Deny by default applies to data, not only to operations. ⚙
- A class MUST be declared once, on the type that carries the value - never repeated per DTO, per log call, or per endpoint. Two declarations of the same fact will drift. Mechanism in `references/backend.md` §7.
- A field's class MUST travel with its contract: the OpenAPI document carries the class of every property, and the generated frontend types carry it onward. ⚙ A boundary rule the client cannot evaluate is not a rule.

### Egress boundaries

Each boundary has exactly one owner. There is no other path across it.

| Boundary | Owner | Rule |
|---|---|---|
| Response DTO to an HTTP or SignalR client | API endpoint / hub | `Confidential` and above require the operation's policy to grant that field. |
| Anything placed in a model's context | Agent tool projection | The model vendor is an external processor. `Restricted` MUST NOT cross. |
| Log, metric, trace, or error report | Logging behavior | `Confidential` and above MUST NOT be written. Log the id, never the value. |
| Cache entry | Cache decorator | `Restricted` MUST NOT be cached. `Confidential` only under a principal-scoped key. |
| Payload to a third-party integration | Infrastructure adapter | A per-integration allowlist of fields, declared and reviewed. |
| Export, report, or file handed to a user | Export endpoint | The same policy as the equivalent read, and the egress is audited. |

- A model's context is egress in **both** directions: tool arguments, tool output, system and user prompts, retrieved documents, and retained conversation history. All of it leaves the boundary.
- Egress to a model is not reversible. Once a value is sent it is disclosed; the response is notification and rotation, not deletion.
- Agents reference sensitive records **by id**. The id is resolved to values by an authorized read in the user's own session, not by widening what the model may see. This is what makes agents usable over regulated personal data at all.
- Integration events MUST NOT carry `Restricted` payloads. They are persisted in the outbox, replayed, and read by operators - reference by id. ⚙

## 9. Observability

Every request, message, job, and agent run MUST be attributable end to end. A log line that cannot be tied to the work that produced it is noise with a timestamp.

- Every unit of work MUST carry a correlation id: an HTTP request, a dispatched message, an outbox delivery, a background job, an agent run and each of its steps. ⚙
- The correlation id MUST cross every boundary the work crosses - module to module through the outbox, user to agent, backend to frontend. A boundary that drops it breaks the trace at the exact point where a trace earns its keep.
- Logs, metrics, and traces MUST share that one identity. Three signals that cannot be joined are three tools, not one system.
- An agent run MUST be reconstructable from telemetry alone: which user, which operations, which rows, what it cost. An agent whose actions cannot be replayed after the fact is not auditable, and an unauditable agent is not deployable.
- Telemetry MUST NOT be optional per environment. Sampling rates MAY differ; emission MUST NOT.
- What may be emitted is governed by §8. Classification applies to every log line, metric label, and trace attribute, not only to responses.

Mechanism in `references/backend.md` §10.

## 10. Verification

Backend: `dotnet build` (warnings are errors) and `dotnet test` (includes architecture tests).
Frontend: `pnpm lint`, `pnpm typecheck`, `pnpm test`.

All MUST pass before a change is complete. Never report work as done on unverified code.

Every ⚙ rule and the mechanism that enforces it is inventoried in `references/enforcement.md`. A ⚙ with no entry there is unaccounted for and MUST be added. An entry whose check is not yet built is a known gap in enforcement - it does not make the rule optional.

## 11. Changing the Law

This document is not edited to accommodate code that violates it. To change a rule: change it here first, then bring the codebase into compliance in the same change. Until the law says otherwise, a rule the codebase no longer satisfies is a defect in the codebase.
