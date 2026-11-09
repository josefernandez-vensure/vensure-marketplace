# Law 4.0 ledger: the core law becomes an index

Accepted on 2026-11-09, with the three recommended decisions below. The work lands as one PR, with one commit per stage of *Order of work*, each passing `check-law.py`.

## The problem

`plugins/rancho/skills/law/SKILL.md` says it states "what MUST be true in every tree, whatever it is built with". About half of it does that: §0, §1, §2, §10, §11, and parts of §7, §8 and §9. The other half is written in the vocabulary of one server architecture: modules, outbox, Command/Query, pipeline behavior, handler, unit of work, middleware, decorator, hub. It also leaks .NET names, such as `ProblemDetails` and "a parameter of a logging method". As a result:

- A client tree reads a server architecture it can't follow.
- A tree bound to `none` (IaC, CI) inherits HTTP rules that mean nothing to it.
- A second server stack (Node, Go) would get a .NET-shaped core to satisfy.

This drifted because there has only ever been one server stack, so no second implementation pushed back on a sentence.

## The shape

`rancho`'s law becomes two files. Each stack law stays complete for its own tree.

| File | Content | Who reads it |
|---|---|---|
| `rancho/skills/law/SKILL.md` | **The core:** how to use the law, the index of laws, principles that bind every tree, verification, exceptions | Every tree |
| `rancho/skills/law/references/contract.md` | **The contract law:** what a server and a client must agree on across HTTP | Every tree whose stack law's role is `server` or `client` |
| `rancho-dotnet`, `rancho-react` | **The stack laws:** everything inside their own tree, including the server architecture that leaves the core | Their own trees |

Why the contract law stays in `rancho` and doesn't become its own plugin:
- Its rules keep their `ROOT` ids and stay in the core inventory, so nothing is renumbered.
- `AUTHORING.md` §2 forbids one stack law depending on another. Both stacks must depend on one owner of the contract, and `rancho` is that owner already.

### The new core, section by section

1. **How to use the law.** Today's §0 (prime directive) and §1 (rule language, ⚙, `ENFORCEMENT-STATUS.md`, required tests), unchanged.
2. **Binding and reading order.** Today's §2 binding rules, unchanged. The reading order gains one step: the core, then the contract law if the stack law has a role, then the stack law in full, then each annex whose trigger the work meets.
3. **Index.** One table: each law, its role, and when to read it. It points and never summarizes, because a summary is a restatement.

   | Law | Role | Read when |
   |---|---|---|
   | `/rancho:law` `references/contract.md` | server and client | the tree's stack law has a role |
   | `/rancho-dotnet:law` | server | the tree binds it |
   | `/rancho-dotnet:law` annexes | server | each annex's trigger |
   | `/rancho-react:law` | client | the tree binds it |
   | `/rancho-react:law` annex | client | its trigger |

4. **Principles that bind every tree.** Outcomes only, each a line or two, with no mechanism vocabulary. Draft wording:
   - **Deny by default.** An operation, a record, or a field nobody granted is refused. Authorized-by-omission is a defect. (ROOT-14, review)
   - **One check for every caller.** Every caller of an operation passes the same authorization and the same authoritative validation, whether it comes over a transport or in process (an agent, a job, a test). Validation on the user's device is UX, never a guarantee.
   - **Two-part authorization.** Permission to perform an operation, and permission for each record it names. A read returns only the records the principal may see, filtered where the records are read.
   - **Agents never hold more permission than the user they act for.** For an autonomous run, the limit is the declared service principal. An agent always carries the claim that identifies it.
   - **No secrets in source, configuration files, or logs.**
   - **Uploaded content is untrusted** until its type is proven from its content, it is stored under a name the server chose where it cannot execute, and a malware scan passes.
   - **Data classes.** Today's §8 class table, its test, its illustrations, and the authenticated boundary, unchanged. **Unclassified is `Restricted`.** ⚙ ROOT-16. A value's class is declared once, where the value is defined. Everything that carries the value inherits that class and never restates it.
   - **Egress.** Today's egress table as outcomes, with the Owner column removed (owners are a stack law's): an HTTP or real-time response, a model's context, telemetry, a cache entry, a third-party payload, an export. Also kept: the no-exception sentence, the telemetry rule for exception messages, and the agent rules (model context is egress in both directions; disclosure is answered by notification and rotation; reference by id).
   - **Attributable work.** Every request, message, job and agent run carries one correlation id across every boundary it crosses. ⚙ ROOT-19. Logs, metrics and traces share that identity. An agent run can be rebuilt from telemetry alone. Whether a signal is emitted never varies by environment.
   - **Audit trail.** Today's §9 *Audit trail*, unchanged.
5. **Verification.** Today's §10, unchanged.
6. **Changing the law, and exceptions.** Today's §11, unchanged. Its example ".NET suppression or a TypeScript `@ts-expect-error`" becomes "a stack law's suppression".

### The contract law

The contract law is today's §4, plus the client-facing parts of §6, §8 and §9:
- REST by default, RPC form, no GraphQL.
- The OpenAPI document is generated and committed. ⚙ ROOT-09
- Client types are generated from that document.
- *Collections*: ⚙ ROOT-11, ROOT-12, and the server-enforced page size.
- *Idempotency*: ⚙ ROOT-13, the `Idempotency-Key` behavior, and its status codes.
- *Limits*: every operation under a rate limit; `429`, with `Retry-After` where the limiter can estimate it.
- *Wire contract*: `x-data-class`, the page, the stream, problem details, concurrency, trace context, and cross-origin.
- **Errors on the wire**, from §6:
  - Every failure response is RFC 9457 problem details (the RFC alone, without `ProblemDetails`).
  - An unexpected failure is a `500` problem.
  - A payload never leaks exception messages, stack traces, SQL or internal type names.
- **The class travels with the contract.** ⚙ ROOT-18. A response carrying `Confidential` or `Restricted` data is sent `Cache-Control: no-store`.
- **Trace context across the wire**, from §9:
  - The trace id is the W3C trace-id.
  - A caller's `traceparent` is adopted only through an edge reachable from inside the trust boundary.
  - A browser's trace is linked, never adopted.
  - `traceId` is returned on every problem.

The contract states only the shape of the wire, never how either side builds it. "Every endpoint MUST accept a Request DTO" becomes: "an operation's request and response bodies are schemas in the document; its response is one of those schemas, no body, or a stream it declares".

## Where every rule goes

The destinations are:
- **Core:** the principles above.
- **Contract:** `references/contract.md`.
- **.NET:** `rancho-dotnet`, in the section named. Most of it is already there, so the move retires the core sentence more often than it adds a new one.

| Today | Content | Goes to |
|---|---|---|
| §0 | Prime directive | Core 1 |
| §1 | Rule language, ⚙, `ENFORCEMENT-STATUS.md` | Core 1 |
| §2 | Binding, reading order, roles, may add / must not weaken | Core 2 and 3 |
| §3 | Module isolation | .NET §2 (ROOT-01 retires into BE-01) |
| §3 | Cross-module writes asynchronous through the outbox; reads through a query contract | .NET §4 *Transactions and consistency* |
| §3 | Domain events stay in their module | .NET §3 (ROOT-02 retires into BE-07) |
| §3 | An event with two audiences is two events | .NET §4 *Integration event contracts* (ROOT-03 stays `dropped`) |
| §3 | One schema per module | .NET §5. ROOT-04 has its own check, so BE-37 takes back the schema half it gave ROOT-04 in the unreleased change from PR #23, and ROOT-04 retires into BE-37 |
| §4 | REST, RPC form, no GraphQL | Contract |
| §4 | Request DTO / Response DTO; no domain type reachable from HTTP | Shape on the wire: Contract (unmarked). DTO and domain vocabulary: .NET §13 (ROOT-07 retires into BE-80, with BE-82) |
| §4 | Framework-mapped endpoints (probes, OpenAPI, hub) | .NET §13, which already lists them. Their rate limit and authorization follow from Contract *Limits* and Core deny-by-default |
| §4 | Request DTO maps to a Command or Query; endpoints hold no business logic | .NET §13 |
| §4 | Queries project straight to Response DTOs | .NET §4 (ROOT-08 retires into BE-108) |
| §4 | OpenAPI generated and committed; clients generate from it | Contract (ROOT-09 kept) |
| §4 | *Collections*, *Idempotency*, *Limits* (transport), *Wire contract* | Contract (ROOT-11, -12, -13 kept) |
| §4 | Agent runs carry their own budgets | .NET §9 *Agent runs* |
| §5 | Validation is a pipeline behavior; annotations may duplicate it; a handler doesn't re-validate | .NET §4 *Pipeline behaviors* |
| §5 | Every caller gets the same validation; client validation is UX | Core principle |
| §6 | The four translation boundaries table | .NET §4 and §13 |
| §6 | Pipeline steps return their own outcomes | .NET §4 |
| §6 | Expected outcomes are values; exceptions aren't control flow; adapters translate framework exceptions | .NET §3–§5 (see decision 3) |
| §6 | Failure responses are RFC 9457; unexpected failure is `500`; no leaked internals | Contract |
| §7 | Deny by default | Core principle (ROOT-14 stays `dropped`) |
| §7 | Authorization on the message; identical for every caller | Core principle (outcome). "Message" vocabulary: .NET §6 |
| §7 | Policy plus object check; filtered where read | Core principle |
| §7 | Acting principal explicit; no ambient HTTP context | .NET §2 (ROOT-15 retires into BE-01) |
| §7 | Agents hold a subset of permissions | Core principle |
| §7 | Secrets | Core principle |
| §7 | *File uploads* | Core principle (outcome). Mechanism and the class of an upload: .NET, in a new *File uploads* section under §13 |
| §8 | Classification, class table, test, illustrations, authenticated boundary | Core principle |
| §8 | Unclassified is `Restricted` | Core (ROOT-16 kept) |
| §8 | Class on the domain type and its contract twin, inheritance, logging-method parameter | Core keeps "declared once, inherited everywhere". The rest goes to .NET §7 (ROOT-20 retires into BE-54) |
| §8 | Class travels with the contract | Contract (ROOT-18 kept) |
| §8 | Egress table, rules column | Core principle |
| §8 | Egress table, owner column | .NET §7, and `rancho-react` §11 for the client's own boundaries |
| §8 | `Cache-Control: no-store` for classified responses | Contract |
| §8 | Exception messages in telemetry; model context rules; agents by id | Core principle |
| §8 | Integration events carry no `Restricted` | .NET §7 (ROOT-17 retires into BE-61) |
| §9 | Attributable end to end; one id across boundaries; shared identity; agent runs reconstructable; not optional per environment | Core principle (ROOT-19 kept) |
| §9 | W3C trace id, trust edge, `traceId` on a problem | Contract |
| §9 | Exemplars, not metric dimensions | .NET §10 |
| §9 | *Audit trail* | Core principle |
| §10 | Verification | Core 5 |
| §11 | Exceptions register | Core 6 |

## What happens to the ROOT ids

| Kept in core | Kept in contract | Retired into a stack row | Unchanged status |
|---|---|---|---|
| ROOT-16, ROOT-19 | ROOT-09, ROOT-11, ROOT-12, ROOT-13, ROOT-18 | ROOT-01 → BE-01, ROOT-02 → BE-07, ROOT-04 → BE-37, ROOT-07 → BE-80, ROOT-08 → BE-108, ROOT-15 → BE-01, ROOT-17 → BE-61, ROOT-20 → BE-54 | ROOT-03, ROOT-14 `dropped`; ROOT-05, -06, -10 already `retired` |

The core inventory goes from 15 ⚙ to 7: two in the core and five in the contract. `rancho-dotnet`'s **Core rules carried** shrinks to those seven. `rancho-react`'s still carries ROOT-16 and ROOT-18. Every retired ROOT row already has a canonical BE row, except ROOT-04 (BE-37 takes it back) and ROOT-01 (BE-01's allowlist already reaches it).

## What else changes

- **`AUTHORING.md` §2:**
  - The Core map maps the core's principles and the contract's sections, not today's §3–§9.
  - "The two trees of a project share only what this document states" names the core and the contract law.
- **`context/prime-directive.md`, `agents/vaca-stream.md`, `skills/vaca/SKILL.md`:** the reading order gains the contract step.
- **Both stack laws:** each Core map is rewritten, and every `core §N` citation is renumbered (42 in `rancho-react`, 70 in `rancho-dotnet`, counted across each plugin's Markdown, records included).
- **`tools/check-law.py`:**
  - Reads ⚙ marks from `references/contract.md` into the `rancho` inventory.
  - Its load estimate splits: core only (a `none` tree), core + contract + stack law (a server or client tree).
- **Versions:**
  - `rancho` 4.0.0: breaking, because sections renumber and eight ROOT ids retire.
  - `rancho-dotnet` 3.0.0: breaking, because projects move checks named for retired ROOT ids onto the BE rows.
  - `rancho-react` 2.1.0: its ROOT ids are unchanged and only citations move. It becomes 3.0.0 if a project names core sections in its own records.

## Load, honestly

These are estimates, made before any rewrite.

| Stream | Today | After |
|---|---|---|
| A tree bound to `none` | ~6.5k | ~2–2.5k |
| `rancho-react` (core + contract + stack) | ~12.4k | ~10.5k |
| `rancho-dotnet` (core + contract + stack) | ~19.5k | ~19–20k: the server architecture moves into it rather than disappearing |

The gain is correctness of scope, not size: a tree reads only the rules that can apply to it.

## Decisions needed

1. **Where the contract law lives.**
   - **Recommended:** `rancho/skills/law/references/contract.md`. ROOT ids survive, there is one owner, and there is no stack-to-stack dependency.
   - **Alternative:** a `rancho-contract` plugin that both stacks require. It is cleaner to enable on its own, but its rules take a new prefix, and seven ids would be retired and re-issued.
2. **Where the server architecture lives.**
   - **Recommended:** `rancho-dotnet`, now. `AUTHORING.md` §11 says a rule every stack of a role would need belongs in the core. With one server stack nobody can know that, so the rule goes down now, and whatever a second server stack shares comes back up when it exists.
   - **Alternative:** a `server.md` reference in `rancho`, read by every `server`-role tree. It is ready for a second server stack, but keeps CQRS vocabulary in the core plugin.
3. **"Expected outcomes are values, not exceptions":** .NET only (recommended, since `rancho-react` has its own error rules in its §10), or a core principle for every tree.

## Order of work

One PR, one commit per stage, each passing `check-law.py`.

1. **Extract the contract.** Move §4 and the wire parts of §6, §8 and §9 into `references/contract.md`, keeping their ids. Update the reading order and `check-law.py`.
2. **Move the server architecture down.** Move §3, §5, the rest of §6, and the mechanism parts of §7, §8 and §9 into `rancho-dotnet`. Retire the eight ROOT rows.
3. **Rewrite the core as the index:** principles, index table and renumbering.
4. **Follow-through:** `AUTHORING.md`, both Core maps, the citations, `vaca`, the READMEs and the CHANGELOGs, released together.

## Outcome

As built, against the plan above:

- **Order:** the stages landed as planned, one commit each: 1, the contract law; 2, server architecture into `rancho-dotnet`; 3, the core rewritten; 4, the follow-through.
- **ROOT ids:** as planned. ROOT-07 retired into BE-80, and its Request DTO walk moved into BE-82's Check. ROOT-01's assembly assertion moved into BE-01's Check, and ROOT-04's schema check and gap into BE-37's.
- **New core numbering:** §0 Prime Directive, §1 Rule Language, §2 The Laws and Which One Binds a Tree, §3 Access, §4 Data Boundaries, §5 Observability, §6 Verification, §7 Changing the Law and Exceptions. `AUTHORING.md`'s mirrored sections are now §1, §2, §6 and §7.
- **Old-to-new citations:**

  | Old | New |
  |---|---|
  | §3 | `rancho-dotnet` §2, §3, §4, §5 |
  | §4 | the contract law, `rancho-dotnet` §4 and §13 |
  | §5 | §3 (outcome), `rancho-dotnet` §4 *Pipeline behaviors* |
  | §6 | contract §5, `rancho-dotnet` §4 and §13 |
  | §7 | §3 |
  | §8 | §4, contract §6, `rancho-dotnet` §7 |
  | §9 | §5, contract §7 *Trace context* |
  | §10 | §6 |
  | §11 | §7 |

- **Load**, from `check-law.py`:

  | Stream | Before | After |
  |---|---|---|
  | `none` | ~6.5k | ~3.8k |
  | `rancho-react` | ~12.4k | ~11.5k |
  | `rancho-dotnet` | ~19.5k | ~19.5k |

  A `none` tree loads more than the ~2–2.5k estimate, because the principles keep the class table and the egress table in full.
- **Obligation check:** after stage 3, an independent read of the old core law, sentence by sentence, against the new core, the contract law and the .NET law. Its findings and their fixes are in `plugins/rancho/RECORDS.md`, *4.0*.
