---
name: law
description: The .NET law - modules as services, the codebase as the reference, the checks that enforce the structure, and the decisions they can't show - consistency across aggregates and modules, sagas, persistence, access, egress, and the tests each new operation needs. Read in full, after the core law, before writing, reviewing, or placing code in a tree whose CLAUDE.md names `Stack law: /rancho-dotnet:law`.
---

# .NET Law

A tree bound here is a modular monolith: each module is a bounded context, treated as a separate service. Its checks enforce most of how the code is built. This law states what they can't: how to use the codebase as the reference, and the decisions neither a check nor an example shows.

## Role

`server`. This tree publishes the contract law, `references/contract.md` in `/rancho:law`, and meets it through the platform the checks protect.

## 1. The codebase is the reference

- The tree's `CLAUDE.md` has a `## Reference` section. For each kind of code, such as a command, a query, an endpoint, a reaction, a saga or an integration test, it names the implementation to follow. Before writing code of a listed kind, read its reference and match it: structure, naming, and tests.
- Reference code is held to this law. Where the two disagree, the law wins: say so, and fix the reference in the same change or report it.
- For a kind with no reference yet, build it from this law. It is reviewed as the new reference, and its path is added to the list in the same change.
- Code not on the list is precedent, not reference. Follow it where the law and the references are silent and a second occurrence confirms it. Otherwise ask.
- If two rules leave no compliant answer, stop and ask.

### The checks

The tree carries the checks of the `rancho-dotnet` template: the reference table by project kind (`build/ProjectKinds.targets`), banned APIs (`build/BannedSymbols*.txt`), analyzers as errors (`.editorconfig`), and the architecture, host and integration tests.

- A failing `dotnet build` or `dotnet test` is the law speaking: fix the code, never the check. Don't relax a check, skip a test, or turn off `TreatWarningsAsErrors` or `Nullable` to get past one.
- A suppression names the diagnostic and gives its reason in a comment beside it. A check you believe is wrong is reported, not edited.
- A rule a check can catch gets its check in the change that introduces the first code it governs, proven to fail on a violation.

## 2. Stack

.NET 10 · EF Core on SQL Server · Mediator 3.x (source-generated) · FluentValidation · Microsoft.Extensions.Compliance · Serilog behind `ILogger` · OpenTelemetry · Azure Key Vault · xUnit v3 · NSubstitute · Shouldly · Testcontainers · ArchUnitNET

- Create solutions, projects, and references with the `dotnet` CLI (`dotnet new`, `dotnet sln add`, `dotnet reference add`, `dotnet package add`), never by writing the file. Edit a project file by hand only for what the CLI can't express, such as its `<ProjectKind>`.
- A module has at most one each of `Domain`, `Application`, `Infrastructure` and `Contracts`, never a project per feature; omit the ones it doesn't need. Features are folders in `Application`, over the module's one model.

## 3. Where logic lives

Placement follows dependencies, never the name. `Service`, `Manager`, `Provider` and `Client` are not signals.

- A rule spanning aggregates, with no dependency, is a domain service in `Domain`.
- Use-case logic with no external dependency (a strategy, a policy, a decision table, a mapper) lives in `Application`, and is unit tested without doubles.
- Anything that crosses a process boundary (an HTTP client, a queue, a blob store, an identity provider) is an adapter in `Infrastructure`, behind a port declared in `Application`, or in `Domain` when the domain's own vocabulary can express it.
- Logic placed in `Infrastructure` that needs nothing from there is invisible to the checks. Move it to `Application`.

## 4. Outcomes and exceptions

- An expected outcome is a value: a domain rejection is an outcome enum in domain vocabulary, and a handler's answer is a `Result`. A broken invariant throws.
- Only an adapter catches an exception, and only the one its framework or SDK raises for an expected outcome (a concurrency conflict, a unique-key violation, an unreachable remote service), where it is raised. It translates it into an outcome. Nothing above it catches one, and nothing catches a domain exception to answer `4xx`: add the missing guard instead.

## 5. Consistency

### Aggregates and transactions

- A command changes exactly one aggregate, in one transaction. If two aggregates must always change together, they are one aggregate: redraw the boundary, never widen the transaction. Everything else that follows from a change is a reaction, in its own transaction.
- Nothing calls an external system inside a transaction. A command whose effect is an external write declares `Transaction: None`, stages no database change, and is idempotent naturally or by a client-supplied id.
- An effect a repeat would duplicate, such as an email or a payment capture, is never performed inline. A persisting command records the intent, and the effect is performed from the outbox, with a key derived from the message wherever the external system accepts one.

### Reactions and sagas

- A reaction runs as its module's service principal. The initiating user is not re-authorized: a step that must stop when the user loses access checks that explicitly.
- A process that spans modules is a saga. Use choreography by default: each module reacts to the others' events. Use an orchestrator when the process has more than three steps or its compensations must run in order. The orchestrator is an aggregate in the module that owns the process; it advances one step per transaction, sending the next participant's command message and taking the participant's integration events as its replies.
- Order the steps: those that can be compensated first, then the point of no return, then those that can only be retried. An effect that can't be undone, such as an email or a payment capture, comes after the point of no return.
- A compensation, such as `Cancel`, `Refund` or `Release`, records new state and deletes nothing. It is idempotent and retried until it succeeds.
- While a process is in flight, its aggregates show it in a pending state, such as `Reserved` or `PendingApproval`, that other operations respect. A read straight after a command may not show its reactions yet.

### Published messages

- A change to the shape or the meaning of a published integration event or command message is a new version, `<Message>V<N+1>`; only an added optional field is not. The contract snapshot catches a change of shape, not a change of meaning.
- Keep publishing and handling the old version until no consumer subscribes to it and no outbox row holds one. A consumer ignores fields it doesn't know. Producers ship before consumers.

## 6. Persistence

- A repository loads the whole aggregate, its entities and owned values included, in one operation, and saves it whole. A handler never chooses includes, projections or tracking.
- A migration must be safe to run while the previous release still serves. Split a destructive change (dropping a column or table, narrowing a type, a `NOT NULL` column with no default) across releases: add the new, write both, backfill, stop reading the old, then drop.
- A data migration is idempotent and re-runnable. A migration seeds only reference data the domain can't work without, never demo records or fixtures.
- A committed migration is never edited; correct it with a new one.

## 7. Access

- Every read applies the principal's scope, such as its tenant, in the query itself: a repository's load, a LINQ read or a SQL predicate. Never filter a result afterwards. A record outside the scope is not found.
- An external authority (one that validates tokens or answers for permissions) that is unreachable, times out or errors denies. Never accept the token unvalidated or fall back to a default permission set. A claim read before the token's signature is validated may only choose how to validate it, and must agree with the validated token.
- An options default never weakens a control: a missing limit, timeout or policy denies.
- Secrets come from Key Vault (user secrets locally) and never appear in a committed file, source included; the architecture test sweeps configuration only.
- An expensive read names a stricter rate limit than the default.

## 8. Data classes and egress

The classes and the egress boundaries are the core law's. The class is declared on the value's type, and the checks prove everything that leaves through a response, an event, the outbox or a log resolves to one. Each boundary's owner here:

| Boundary | Owner |
|---|---|
| A response to an HTTP client | the endpoint registrar and the OpenAPI document's transformers |
| A log, metric or trace | the redacting logging generator and the telemetry pipeline |
| An event or outbox message | the outbox; the checks keep `Restricted` out of its payloads |
| A payload to a third party | its `Infrastructure` adapter, with a declared allowlist of the fields it sends |
| An export, report or file handed to a user | its own endpoint, under the policy of the equivalent read, audited |

- A `Restricted` column is encrypted at rest, in the mapping or by the driver (SQL Server Always Encrypted), transparently to every read. Choose an encryption that supports every operation a query runs on the column, such as the comparison keyset paging makes on a sort key.
- A problem's `detail`, a validation message and an exception message name fields and ids, never values.
- Nothing updates or deletes an audit record.

## 9. Tests each new operation needs

The checks already run over every route and message: a principal without the policy is denied and recorded, spans nest in pipeline order, rate limits answer `429`, the OpenAPI document and the contract snapshots are current, and every domain method and outcome has a test by name. Write these yourself, one each, following the reference integration tests:

- An operation on a record: another tenant's record is never read, listed or changed.
- A versioned write: no `If-Match`, or `*`, is `428`; a stale version is `412`.
- A keyed command: a replay returns the recorded result; the same key with another payload is `422`; no key is `400`; two concurrent attempts are one execution and one replay.
- A paged query: no row is skipped or repeated across a page boundary, including inside a run of equal sort values.
- A query that reads with SQL: it projects into its response and applies the scope, against SQL Server.
- A reaction: a redelivered message changes nothing.
- A saga: a step that fails after a compensable one runs its compensations.

Don't write a test that proves only what the build proves, such as that a type exists or a reference resolves.

## 10. Not yet covered

Caching, calls to AI models (or exposing operations to one as tools), real-time push, and file uploads have no rules here yet. Before adding the first cache, model call, persistent client connection, or upload to a tree, stop and ask.

## 11. Verification

`dotnet build` and `dotnet test` pass before a change is done; the integration tests need Docker. On every pull request, CI also runs `build/check-migrations.sh` and `build/check-contracts.sh` against the base branch.
