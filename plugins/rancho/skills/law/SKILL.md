---
name: law
description: The core engineering law, the same in every project - how to read the law, access, data classes and egress, observability and audit, and the HTTP contract between a server and its clients. Read before writing, reviewing, or placing any code; then read the stack law the tree's CLAUDE.md names.
---

# Engineering Law

## 1. Reading the law

Read `/rancho:law`, then the law named by the nearest `CLAUDE.md` at or above your files that holds a `Stack law:` line. A tree bound as `Stack law: none - <reason>` reads only the core. No binding, or a law the session doesn't have: stop and say so.

- A stack law may add rules, never weaken one here.
- Bind a tree to none only when no stack law governs any of its code, such as infrastructure-as-code or CI scripts.
- Every sentence binds. Depart from a default only with the reason in the PR.
- Where code contradicts the law, the law wins. Say so, and never copy the violation.
- If two rules, or a rule and a decision in the project's `DECISIONS.md`, leave no compliant answer, stop and ask. Never word around a rule, or place code where its check doesn't look.
- A suppression carries a comment beside it giving its reason. A comment arguing that a rule doesn't apply is a defect, not an exemption.
- A disagreement with the law is an issue on the `vensure-marketplace` repository, which ships it. Until the law changes, it binds.

### Verification

Before a change is done, the verification commands of every tree it touches pass: those its stack law names, or, for a tree bound to none, those its `CLAUDE.md` names.

Never write a test that proves only what a successful build proves, such as that a type exists or a reference resolves.

## 2. Access

- Deny by default. An operation, a record or a field nobody granted is refused.
- Every caller of an operation, over a transport or in process (an agent, a job, a test), passes the same authorization and the same validation. Validation on the user's device is UX, never a guarantee.
- Authorization has two parts: the operation's policy, and a check that the principal may act on each record the caller names, unless the policy grants every record. A query returns only what the principal may see, filtered where the records are read, never after.
- An AI agent holds a subset of the calling user's permissions, or of its declared service principal for an autonomous run, and always carries the actor claim naming it.
- Secrets never appear in source, configuration files or logs.
- An uploaded file is untrusted until the server has proven its type from its content, stored it under a name it chose where it can't execute, and seen a malware scan pass.

## 3. Data classes and egress

A class belongs to the data, not to the endpoint that returns it: it is declared once, where the value is defined, and everything that carries the value takes it, the contract (§5) included.

| Class | The test | Examples, not a list |
|---|---|---|
| `Public` | Published outside the authenticated boundary, deliberately | A published listing |
| `Internal` | Not secret, but not for publication: how the organization is arranged and operates | Org structure, site locations, operational metrics |
| `Confidential` | Attributable to a person or a counterparty, and disclosing something they would expect to control | Contact details, pay, performance, hours worked |
| `Restricted` | Regulated by statute or contract, or enough on its own to impersonate, defraud or discriminate against its subject | Government identifiers, date of birth, bank and payment details, health, protected-class attributes |

Unclassified is `Restricted`. `Internal` and above stay inside the authenticated boundary: the organization's systems and an authenticated user's signed-in session, including the URLs and storage of the browser it runs in.

### Egress boundaries

Each boundary has one owner, named by its tree's stack law, and no other path across it.

| Boundary | Rule |
|---|---|
| A response to an HTTP or real-time client | `Confidential` and above only where the operation's policy grants that field |
| A model's context | `Restricted` never crosses |
| A log, metric, trace or error report | `Confidential` and above are never written. Log the id, never the value |
| A cache entry | `Restricted` is never cached; `Confidential` only under a key scoped to the principal |
| A payload to a third party | Only the fields on that integration's declared, reviewed allowlist |
| An export, report or file handed to a user | The policy of the equivalent read, and the egress is audited |

- `Restricted` crosses a response, a third-party payload or an export only by an exception declared at that boundary: it names the field, carries a comment giving its reason, and is approved in review.
- Telemetry keeps an exception's message only where our code wrote it, and our messages hold no classified value. A message from code we don't control, such as a driver's, is dropped or scrubbed before export.
- A model's context is egress in both directions: prompts, tool arguments and output, retrieved documents, and retained history.
- An agent refers to a sensitive record by id, resolved by an authorized read in the user's own session, never by widening what the model sees.

## 4. Observability and audit

- Every unit of work carries a correlation id, down to each delivery of a message and each step of an agent run. It is the W3C Trace Context trace id, and it crosses every boundary the work does, user to agent and server to client included (§5).
- Logs, metrics and traces share that one id. Join metrics to traces with exemplars, never with a metric dimension.
- An agent run can be reconstructed from telemetry alone: which user, which operations, which rows, what it cost.
- Telemetry is never optional per environment. Sampling and log levels may differ; whether a signal is emitted may not.

### Audit trail

Record in an audit trail every failed authorization, every executed command, and every read of `Restricted` data, and by default of `Confidential`: the principal and any acting agent, the operation, the ids of the records it named or returned, the time and the outcome. The trail holds ids, never values. It is separate from telemetry, never sampled, append-only, and kept as long as the project's `DECISIONS.md` declares. Where a law says something is audited, this is the record.

## 5. The HTTP contract

A server and its clients agree on one contract: the server's OpenAPI document, generated and committed, with every property and parameter carrying its class as `x-data-class`. The server's stack law states how it is produced. A client generates its types from it, never by hand. Both sides keep these rules, which the document can't show:

- **Caching.** A response carrying `Confidential` or `Restricted` data is sent `Cache-Control: no-store`, and one whose representation depends on the principal `private` or stricter.
- **Paging.** Paging is by keyset, and its opaque cursor, which the server encrypts and authenticates, is the only position a request sends. A page size above the server's maximum is rejected, never truncated.
- **Idempotency.** A key travels as the `Idempotency-Key` header, which the document declares on the operation. A replay returns the original response without executing again, and is answered before any precondition, such as `If-Match`, is evaluated.
- **Concurrency.** A resource whose writes can conflict has a version the application manages, changed by any change to it or inside it: its strong `ETag`, and `etag` in its Response DTO, which a client sends back unchanged as `If-Match`. A write to an existing resource without `If-Match`, or with `*`, is `428`; a stale one is `412`. A create that names its own id sends `If-None-Match: *`.
- **Problems.** A client decides by a problem's `type` and `status`, never its `title` or `detail`. Every type is the server's own, listed in the document's top-level `x-problem-types` with its `status`, `title` and `userFacing`. No payload carries an exception message, a stack trace, SQL or an internal type name.
- **Trace context.** Requests carry `traceparent`, and `tracestate` when present. A server adopts a caller's trace only through an edge that only callers inside its trust boundary can reach, decided by the edge before authentication, never by credentials. For any other caller, such as a browser, it starts a new trace linked to the caller's. Every problem carries the server's trace id as `traceId`.
- **Cross-origin.** A server whose clients are served from another origin admits, through CORS, every request header this contract uses, and exposes every response header a client reads, such as `ETag` and `Retry-After`.
