# Law redesign: core triage

Working file for the law redesign (`LAW-REDESIGN.md`), deleted in phase 6 like the .NET and React triage. It records phase 4: the core law, `plugins/rancho/skills/law/SKILL.md`, rewritten with the contract law, `references/contract.md`, merged into it. Together they were 4,275 words at rancho 4.1.0 (core 2,909, contract 1,366). The core law at 5.0.0 is 1,479 (`wc -w`, frontmatter included); the budget is 1,500.

Each sentence was put through the admission test. Where a sentence stayed, it is because no check enforces it in every stack, the reference code can't show it, and it prevents a security or data defect or a repeated mistake.

## Kept

| Section | What stays |
|---|---|
| §1 Reading the law | The one reading rule, word for word from the plan. A stack law may add rules and never weaken one. When a tree may be bound to none. Defaults are departed from only with a reason in the PR. The law wins over contradicting code. Stop and ask when two rules, or a rule and `DECISIONS.md`, leave no answer, and never word around a rule. Suppressions carry their reason, and a comment arguing a rule away is a defect. A disagreement with the law is an issue on this repository. |
| §1 Verification | Every touched tree's verification commands pass, from its stack law, or its `CLAUDE.md` for a tree bound to none. No test that proves only what the build proves. |
| §2 Access | Deny by default; one authorization and validation for every caller; the policy and the object check; queries filtered where read; an agent holds a subset of its user's permissions and carries the actor claim; no secrets in source, configuration or logs; the upload rule. |
| §3 Data classes and egress | The four classes, their tests and examples; unclassified is `Restricted`; the authenticated boundary; the egress table; a `Restricted` exception at a response, third-party payload or export; telemetry keeps only our own exception messages; a model's context is egress both ways; agents refer to sensitive records by id. |
| §4 Observability and audit | The trace id on every unit of work, across every boundary; one id for logs, metrics and traces, joined by exemplars; agent runs reconstructable; telemetry never optional per environment; the audit trail. |
| §5 The HTTP contract | The committed OpenAPI document is the contract, carrying `x-data-class`; the server's law produces it and the client generates from it. Then only what the document can't show: `no-store` and `private`; keyset paging with an encrypted, authenticated cursor, and rejection above the maximum page size; the `Idempotency-Key` header, declared in the document, with replays answered before preconditions; the version, `ETag`, `etag`, `If-Match`, `428`, `412` and `If-None-Match: *`; problems decided by `type` and `status` and listed in `x-problem-types`, with no leaked internals; trace adoption only through a trusted edge, and `traceId` on every problem; CORS for the headers the contract uses. |

## Dropped, and why

| Today (4.1.0) | Why it went |
|---|---|
| Core §0 Prime Directive, §1 Rule language | Folded into §1's bullets. MUST/SHOULD/MAY gave way to plain imperatives (open decision 4); "by default" marks the one default left. Precedent and "a second occurrence" are each stack law's §1, beside its `## Reference`. |
| The `⚙` mark, rule ids, `ENFORCEMENT-STATUS.md`, the inventory paragraphs | The plan drops them: a check is named for what it checks, and the checks are the inventory. |
| §2's law index, roles, standalone laws, annexes, the reading order | Replaced by the one reading rule. With no roles, the contract binds every server and client through core §5. No annexes remain: each stack law's "Not yet covered" says stop and ask. `/rancho-modular-dotnet:law` no longer exists. |
| "Two bindings for one tree is a defect"; "never choose a law by guessing from file extensions" | The nearest `CLAUDE.md` holding `Stack law:` decides the tree, which leaves nothing to guess; "no binding: stop" covers the rest. |
| "What every stack law must carry is in `AUTHORING.md`" | `AUTHORING.md` goes in phase 6. The rancho README's *Adding a stack law* names the worked examples instead. |
| §6: a check never stricter than its rule; a narrower check is a recorded gap | These were rules for writing checks, recorded in the inventories, which are gone. The stack laws' "a check you believe is wrong is reported, not edited" carries the agent's part. |
| §7: the exceptions register, conflict and boundary entries, `EXC-<n>` ids, expiry | The plan replaces it with a suppression comment carrying its reason and an issue on this repository. The `Restricted` boundary exception survives as a declaration at the boundary, with a comment giving its reason, approved in review. |
| "AI agents… Full rules in the server stack law"; "the server stack law states the [upload] rule in full" | The .NET law has no such rules: model calls and uploads are "stop and ask" there. The core sentences stand on their own. |
| A value's class inherited and never redeclared; a check proves two declarations agree | Structural in .NET (`[ClassOf<T>]` can't disagree) and checked in React (`generated`). Kept only as "declared once, where the value is defined". |
| "Once a value is sent to a model, the response is notification and rotation, not deletion" | Incident procedure, not a coding decision; no change gets it wrong. |
| Contract §1: REST by default, RPC-style verbs, no GraphQL | The reference code shows REST and the RPC-style transition (`POST /ordering/orders/{id}/cancel`); no change has reached for GraphQL. |
| Contract §1: the document generated and committed (ROOT-09) | Checked; kept as the premise of §5, not as a rule. |
| Contract §2: pagination of unbounded collections and `maxItems` (ROOT-11); one page shape (ROOT-12); the page's field names; streams | Checked by the .NET template (BE-19, BE-20) and visible in the generated types. |
| Contract §3: every write declares its idempotency (ROOT-13); `400`, `422`, and `409` for a duplicate in flight | ROOT-13 is checked. The status codes are the .NET law's required tests (§9), and the client needs only the same key per intent (React §4, §11). |
| Contract §4: rate limiting and `429` | Checked over every route by the .NET template. |
| Contract §5: problem details everywhere; `500` for the unexpected | Shown by the template's global handler and the client's parser. The no-leak rule stays. |
| Contract §6: `x-data-class` on every schema (ROOT-18), and its placement on a parameter's schema | Checked on both sides: the .NET schema transformer can't emit an unclassified schema, and React's `generated` check reads every class. |
| Contract §7: the `errors` path format, `traceId`'s 32 hex digits, `etag` with quotes, the new `etag` after a write, `204` on delete | Shown by the reference code on both sides, and proved by the React edit-form tests (React §11). |
| The contract's standalone-law paragraph | There are no standalone laws. |

## The stack laws

- Both lose `## Role`. Each says, in its opening paragraph, that it publishes or consumes the HTTP contract of core §5, and neither points at `references/contract.md` any more.
- Both lose "If two rules leave no compliant answer, stop and ask", which core §1 now states once.
- The .NET law loses "Don't write a test that proves only what the build proves", which core §1 states. React keeps its own form, about the type checker.
- Neither changes version: both are unreleased (4.0.0 and 3.0.0) and ship with rancho 5.0.0.

## Gaps for the templates

Core §5 states these, and no check or reference yet shows them:

- The .NET document emits no `x-problem-types` (noticed in .NET slice 2d), so the React client shows every problem generically.
- The .NET document doesn't declare `Idempotency-Key` on a keyed operation (React triage, FE-40).
- No .NET endpoint sends `Cache-Control: no-store` for `Confidential` or `Restricted` data, or `private` for a representation that depends on the principal.
- The .NET template has no CORS policy, so nothing exposes `ETag` or `Retry-After` to a client on another origin.
- No telemetry processor drops an exception message our code didn't write (core §3).

## Left for later phases

- Phase 5 (done): the prime directive, `skills/vaca/SKILL.md`, `agents/vaca-stream.md` and `help.sh` each point at core §1.
- Phase 6: `plugins/rancho/skills/law/references/enforcement.md` still cites `contract §N`, and `AUTHORING.md`, `RECORDS*` and `tools/` describe the 4.x model. No law reads them; the rancho README says so.
