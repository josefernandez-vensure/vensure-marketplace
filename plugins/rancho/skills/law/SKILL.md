---
name: law
description: The binding core engineering law, the same in every project - how to use the law, which law governs each tree and when to read it, and the principles every tree keeps - access, data classification and egress, AI-agent permissions, observability and audit - with verification and exceptions. Read before writing, reviewing, or placing any code, and whenever deciding whether a change is compliant. Then read the contract law beside it where the tree's stack law declares a role, and the stack law bound to the tree you are working in.
---

# Engineering Law

## 0. Prime Directive

This document is the **law**. The codebase is the **idiom**.

- The law defines what MUST be true. It is not advisory.
- The codebase defines how the law is expressed here: naming, layout, ordering, phrasing. Before writing code, read the nearest existing implementation of the same kind and match it.
- **When the codebase contradicts the law, the law wins.** Report the contradiction as a defect. Existing code is never permission to repeat a violation.
- When the law is silent, follow the codebase's nearest precedent, confirmed by a second occurrence: never generalize a rule from a single example. With fewer than two, or when both are silent, ask.
- **When two rules of the law, or a rule and a decision recorded in the project's `DECISIONS.md`, leave no compliant answer, stop and ask.** Do not resolve the conflict by wording around it, by placing code where a rule's check does not look, or by an exemption this document does not provide.
- A code comment arguing that a rule of the law does not apply is a defect, not an exemption. Comments explain behaviour. A rule id in a comment is a cross-reference, never a justification; the one exemption is an entry in the exceptions register (§7).

## 1. Rule Language

- **MUST / MUST NOT** - absolute. A violation fails review.
- **SHOULD / SHOULD NOT** - default. Deviation requires a one-line justification in the PR.
- **MAY** - permitted, no justification needed.

Silence is not permission: an uncovered decision follows §0.

A rule marked `⚙` can be made to fail the build. The mark is a claim about the rule, not about any one project: these plugins ship no checks, and each project builds its own. Each project's maintainers MUST keep an `ENFORCEMENT-STATUS.md` at its repository root, listing every marked rule its trees bind, the check that enforces it, whether that check is built, and whether it has been reviewed against the rule's wording. A marked rule missing from it counts as unbuilt. When the plugins add a marked rule, the project adds it to the file at upgrade. Until a project has built and reviewed a rule's check, the rule is verified by the reviewer who approves each change in that project, and a green build says nothing about it.

A rule no build check can reach carries no mark. Where a test can prove it, it is carried by a named required test, listed in the Testing section of the law that owns it - equally binding, differently caught. Otherwise it is reached by review, and is no less binding. The enforcement inventories map every mark and every required test to its mechanism (`AUTHORING.md` §6).

## 2. The Laws, and Which One Binds a Tree

This document is the **core law**: what MUST be true in every tree, whatever it is built with. The **contract law**, `references/contract.md` in this skill, states what a server and a client MUST agree on across HTTP. A **stack law** states how one technology satisfies both, and everything internal to its own tree. Each stack law is its own plugin, carrying a `law` skill, and a project enables the stack laws it builds with.

| Law | Role | Read it |
|---|---|---|
| The contract law, `references/contract.md` | `server` and `client` | In every tree whose stack law declares a role |
| `/rancho-dotnet:law` | `server` | In every tree bound to it, with each annex whose trigger the work meets |
| `/rancho-react:law` | `client` | In every tree bound to it, with each annex whose trigger the work meets |

A role says what a stack law does with the contract law:

| Role | What it does with the contract law |
|---|---|
| `server` | Publishes it: owns its endpoints, its persistence, and every egress boundary in §4. |
| `client` | Consumes it: generates its types from the published document, and owns what reaches the user's device. |

Binding:

- Every tree that holds code MUST be bound to exactly one stack law, or to none as below. The binding is one line in the `CLAUDE.md` at the root of that tree: `Stack law: /<plugin>:law`. A repository that is a single tree binds it in its root `CLAUDE.md`.
- A stack law governs code written with a technology its Stack section names. A tree whose code no stack law governs - such as infrastructure-as-code or CI scripts - is bound instead with `Stack law: none - <reason>`, and only this document binds it. Its `CLAUDE.md` MUST name its verification commands. A tree MUST NOT be bound to none when a stack law governs any of its code.
- The tree a file belongs to is the nearest directory at or above it whose `CLAUDE.md` holds a `Stack law:` line.
- Before writing, placing, or reviewing code in a tree bound to a stack law, read this document; then, where that stack law declares a role, the contract law; then that stack law **in full**; and then each annex of that stack law whose trigger the tree meets, or the work would meet, as the stack law states.
- A tree with no binding, a binding to a stack law that is not available in the session, or two bindings for one tree is a defect in the repository. Stop and say so. A stack law MUST NOT be chosen by guessing from file extensions or folder names.

What every stack law must carry is in `AUTHORING.md` at the plugin root. A stack law MAY add rules. It MUST NOT weaken one stated here or in the contract law. A stack law that contradicts this document is the defect, exactly as code that contradicts it is.

## 3. Access

- Deny by default, everywhere. An operation, a record, or a field nobody granted is refused. Authorized-by-omission is a defect, never a default.
- Every caller of an operation passes the same authorization and the same authoritative validation, whether it arrives over a transport or in process - an agent, a job, a test. Validation on the user's device is UX: nothing on the server treats it as a guarantee.
- Authorization has two parts, and both are required. The operation's policy decides whether the principal may perform it at all. An object check decides whether it may perform it on the records it names: an operation that acts on a record the caller identifies MUST check that the principal may act on that record, unless its policy grants every record. A query MUST return only the records the principal may see, filtered where the records are read, never after.
- AI agents MUST hold a subset of the calling user's permissions - or, for an autonomous run, of the service principal declared for its manifest - never wider, and always carry the actor claim that identifies the agent. Full rules in the server stack law.
- Secrets MUST NOT appear in source, config files, or logs.
- A file a user uploads is untrusted until the server has proven its type from its content, stored it under a name the server chose where it cannot execute, and seen a malware scan pass. The server stack law states the rule in full.

## 4. Data Boundaries

Classification is a property of the **data**, not of the endpoint that happens to return it. A field's class travels with it through every layer, DTO, log line, cache entry, and model prompt.

| Class | The test | Rule |
|---|---|---|
| `Public` | Already published outside the authenticated boundary, deliberately | No restriction. |
| `Internal` | Not secret, but not for publication: how the organization is arranged and how it operates | Stays inside the authenticated boundary. |
| `Confidential` | Attributable to a person or a counterparty, and disclosing something they would expect to control | Crosses a boundary only where the operation's policy grants that field. |
| `Restricted` | Regulated by statute or contract, or sufficient on its own to impersonate, defraud, or discriminate against its subject | MUST NOT cross any egress boundary below without a named, reviewed exception declared at that boundary that names its entry in the exceptions register (§7). |

The test decides the class. The following illustrate it and do not enumerate it: `Public` - a published listing, marketing copy. `Internal` - org structure, site locations, operational metrics. `Confidential` - contact details, pay, performance, hours worked. `Restricted` - government identifiers, date of birth, bank and payment details, health and medical, protected-class attributes.

The authenticated boundary is the organization's systems and an authenticated user's signed-in session, including the URLs and storage of the browser it runs in; the client stack law states what keeps that browser inside it.

- **Unclassified is `Restricted`.** Deny by default applies to data, not only to operations. ⚙ ROOT-16
- A value's class is declared where the value is defined, and every type that carries the value inherits its class and MUST NOT redeclare it. Where a stack law declares a class in two places, such as a domain type and its published twin, a check proves the two agree. Between a server and a client, the class travels with the contract (contract §6). How a class is declared and proven is the stack law's.

### Egress boundaries

Each boundary has exactly one owner, named by the stack law of the tree it is in. There is no other path across it.

| Boundary | Rule |
|---|---|
| A response to an HTTP or real-time client | `Confidential` and above require the operation's policy to grant that field. |
| Anything placed in a model's context | `Restricted` MUST NOT cross. |
| Log, metric, trace, or error report | `Confidential` and above MUST NOT be written. Log the id, never the value. |
| Cache entry | `Restricted` MUST NOT be cached. `Confidential` only under a principal-scoped key. |
| Payload to a third-party integration | A per-integration allowlist of fields, declared and reviewed. |
| Export, report, or file handed to a user | The same policy as the equivalent read, and the egress is audited. |

A boundary whose own rule forbids `Restricted` outright - a model's context, a cache entry, a log line - admits no exception.

- An exception's type and stack trace MAY be recorded in telemetry. Its message MAY be recorded only where our code wrote it, and a message our code writes MUST NOT contain a classified value. A message from code we do not control - a framework, a driver, a vendor SDK - is dropped or scrubbed before it is exported.
- A model's context is egress in **both** directions: tool arguments, tool output, system and user prompts, retrieved documents, and retained conversation history.
- Once a value is sent to a model, the response is notification and rotation, not deletion.
- Agents reference sensitive records **by id**. The id is resolved to values by an authorized read in the user's own session, not by widening what the model may see.

## 5. Observability

Every request, message, job, and agent run MUST be attributable end to end.

- Every unit of work MUST carry a correlation id: a request, a message and each delivery of it, a background job, an agent run and each of its steps. ⚙ ROOT-19
- The correlation id is the W3C Trace Context trace id. It MUST cross every boundary the work crosses - service to service, module to module, user to agent, and server to client (contract §7 *Trace context*).
- Logs, metrics, and traces MUST share that one identity. Metrics are joined to traces through exemplars, never through a metric dimension.
- An agent run MUST be reconstructable from telemetry alone: which user, which operations, which rows, what it cost.
- Telemetry MUST NOT be optional per environment. Sampling rates and log levels MAY differ; whether each signal is emitted MUST NOT.
- What may be emitted is governed by §4. The mechanism is each stack law's.

### Audit trail

Every failed authorization, every executed command, and every read of `Restricted` data MUST be recorded in an audit trail: the principal, and the agent when one acts; the operation; the ids of the records it named or returned; the time; and the outcome. A read of `Confidential` data SHOULD be. The trail holds ids, never values (§4). It is separate from telemetry: never sampled, append-only, and kept for as long as the project's `DECISIONS.md` declares. Where the law says that something is audited, this is the record it means.

## 6. Verification

Every stack law names its verification commands. Before a change is complete, the verification commands of every tree the change touches MUST pass: its stack law's, or for a tree bound to none, those its `CLAUDE.md` names (§2). Never report work as done on unverified code.

A test MUST NOT prove only what a successful build already proves - that a file, folder, project or type exists, that a reference resolves, or that the code compiles. The verification commands prove those, and such a test is a defect. A rule whose inventory row says it is covered by another row's check needs no check beyond what its own row names.

A check MUST NOT be stricter than its rule. Narrow the check, or ask for the rule to be widened (§7). A check narrower than its rule is a known gap, recorded where the check is listed - in the plugin's enforcement inventory for a check the law describes, in the project's `ENFORCEMENT-STATUS.md` for one the project builds; it never makes the rest of the rule optional.

## 7. Changing the Law, and Exceptions

The law changes only in its plugins; how, and what a stack law must carry, is in `AUTHORING.md` at the plugin root. A project records its exceptions to the law in one register:

- Each repository keeps one exceptions register, `LAW-EXCEPTIONS.md` at its root. An entry MUST be recorded there before the code that relies on it is merged. Every entry has a permanent id, `EXC-<n>`, never reused.
- An agent that finds a case for an entry proposes the entry and stops (§0). A maintainer of the project adds it.
- A **conflict** entry records a case the law leaves no compliant answer for, or a rule the project's maintainers decide must yield until the law changes. It carries the rule id - or, for a rule with none, its section and the sentence it departs from - the paths it covers, the justification, the upstream amendment request - to the law, or to the dependency whose gap forces the conflict - and an expiry. A conflict entry past its expiry is a defect.
- A **boundary** entry records a `Restricted` field an egress boundary admits by exception (§4). It carries the boundary, the fields, the justification, and a review date. It needs no amendment request.
- No other form of exemption from a rule of the law is valid. A limit a rule's own text sets - a marker, a folder, a documented reason it names - is part of the rule, not an exemption. A check MAY read the register, and only the register, to exempt a path.
- Where code suppresses a check to apply an entry, the suppression names the entry's id; each stack law states its suppression forms. A suppression of a warning that enforces no rule of the law is the stack law's alone.
- A SHOULD deviation is not an exemption: it needs only its PR justification (§1).
