# Answer · A command that runs without a transaction

To the consuming project's team that sent "Hand-off · A command that runs without a transaction" (issue #222, AD-1; task #230). From the owners of the engineering law (`vensure-marketplace`).

**Releases:**

- First answered in `rancho-dotnet` 1.1.0, as part of amendment A-4 of the 2026-10-30 audit hand-off. Release: <link to be added>
- Revised in `rancho-dotnet` 2.0.0, with `rancho` 3.0.0. Release: <link to be added>

**File names:** `backend.md` is now `rancho-dotnet`'s `skills/law/SKILL.md`, with its section numbers unchanged. Its inventory is `skills/law/references/enforcement.md`.

## The answer

**Option 1, amended.** A command MAY declare that it runs without a transaction, and it MUST NOT write to the database. The stack law states how this is enforced. It goes beyond option 1 in four ways:

- It names how such a command is idempotent.
- It allows the command to load an aggregate.
- It routes an effect a repeat would duplicate through the outbox instead.
- It fixes the upload's shape.

Options 2 and 3 were not taken. "Transaction - commands only" no longer exists as you quoted it: the transaction and idempotency behaviors are now one behavior.

## The governing text

All from `rancho-dotnet` `skills/law/SKILL.md` unless named otherwise.

**§4 *Pipeline behaviors*, item 4** (it replaces the former items 4 and 5):

> **Unit of work and idempotency** - commands only. One behavior. For a command that persists aggregate state, it opens the unit of work, then, for a command that declares a key, resolves the attempt identity [...] For a command that does not persist, it opens nothing, and after the handler returns it fails the dispatch if the module's context holds a staged change.

**§4 *Transactions and consistency*:**

> Handlers MUST NOT call external systems inside a transaction.

> A command declares, in its operation declaration (§6), whether it persists aggregate state. Only a command that does opens the unit of work. A command whose effect is an external write runs without a transaction, and MUST NOT use a key-based idempotency mechanism or the outbox. ⚙ BE-110 It MUST NOT stage a database change; the unit-of-work behavior refuses one (§4 *Pipeline behaviors*, item 4; its test, §14). It is idempotent naturally or by a client-supplied id, such as a blob name derived from the aggregate id. It MAY load an aggregate to decide whether the write may proceed. Recording the result of the write is a separate command that persists.

> An external effect that a repeat would duplicate - such as sending an email or taking a payment - is not such a command. A command that persists records the intent and writes an outbox row, and the outbox delivery performs the effect, passing an idempotency key derived from the row wherever the external system accepts one.

**§4 *Idempotency*:**

> Every command MUST declare its idempotency mechanism in its operation declaration (§6): naturally idempotent, client-supplied id, or idempotency key. An undeclared command MUST fail to dispatch, exactly as an undeclared policy does. ⚙ BE-26

**§6, the illustrative operation declaration** (a command's: it declares `Kind: OperationKind.Command`):

> `Transaction: TransactionRequirement.Required,         // None: an external write (§4)`

**§13, the upload bullet:**

> An upload endpoint's Request DTO carries the file as a stream with its metadata, and its size limit is enforced at the request boundary - the endpoint's request size limit - before the body is buffered. It dispatches a command that does not persist to store the file in quarantine, under a name the server derives from the uploader's principal and the command's client-supplied id - an id the validation behavior has parsed - with a keyed hash, such as an HMAC under a server key, so a retry finds the same file and no client can name another's; never from the client's file name (§4). A persisting command, run under the uploader's principal recorded with the quarantined file, records it once the scan passes (core §7 *File uploads*).

**§10 *Audit trail*:**

> Every other record is written through the port outside any unit of work: [...] a command that does not persist (§4), through a context or connection of the port's own, never the module's [...]

**§14 *Testing*:**

> **Transactions**: a command that runs without a transaction MUST have a test proving that a staged database change fails the dispatch and is never committed (BE-111).

**The inventory row for BE-110** (`references/enforcement.md`), check column:

> `contract`: the operation registry sweep (as BE-25) finds the declaration on every command and refuses `Required` on a query. It also refuses a non-persisting command that declares an idempotency key. `arch`: the handlers of the commands that sweep finds not to persist, fed to ArchUnitNET as a type list, do not depend on the outbox writer port

Its Gap column: "A non-persisting handler that reaches the outbox through another Application type it calls is not seen [...]; reached by review".

Also binding on an upload: core `rancho` §7 *File uploads* (policy, size and type allowlist, content-based type validation, server-derived name outside any served path, quarantine until a malware scan passes, a declared class) and §14 **Uploads** (BE-120).

## How your `None` path compares

### What you built that the law now requires

- **The declaration.** An operation declares a transaction requirement, `Required` or `None`. This is the field the law's §6 illustration uses (`Transaction: TransactionRequirement.Required`, with `None` for an external write). The rule's own words are "declares ... whether it persists aggregate state".
- **No unit of work for `None`.** Nothing is begun, committed or rolled back.
- **The staged-change guard.** You check the module's context after the handler and fail if a change is staged. That is item 4's "fails the dispatch if the module's context holds a staged change". The law does not say how the dispatch fails. Your throw, giving a `500` and a logged defect, fits core §6: an exception signals a bug. Your test is the required test BE-111.
- **The key refusal.** You refuse `None` together with an idempotency key at startup. That is BE-110's `contract` half.
- **The outbox refusal.** You refuse a `None` handler that references the outbox writer. That is BE-110's `arch` half.
- **Loading an aggregate.** *Send the content* loads its aggregate to check the upload may proceed. That is permitted ("MAY load an aggregate").
- **The separate recording command.** A separate, ordinary command records "content received". That is required ("Recording the result of the write is a separate command that persists").

### What you have that the law does not ask for, or that conflicts with it

- **Your registry refuses `None` on a query. The law refuses `Required` on a query.** You must change this. Precisely, the law says two things about a query here, and no more:
  - The rule binds commands only: "A command declares ... whether it persists aggregate state". No rule requires a query to declare the field.
  - BE-110's check "finds the declaration on every command and refuses `Required` on a query". Item 4 is "commands only", so a query never opens a unit of work. The 3.0 records list your check, which failed a query declaring `Transaction: None`, as "stricter than its rule" (`rancho-dotnet` `RECORDS.md`, 3.0 stage 7).

  So BE-110's check fails a query that carries `Required`, and accepts one that states `None`. The refusal of `Required` is stated in the inventory's check, not in the rule's own text.
- **Whether a query may omit the field is not settled by the law.** The law does not say whether the field may be absent on a query, or what an absent or defaulted value means. The §6 illustration does not settle it: it is a command's declaration. The stage 7 record reads it as giving the field to every operation, but no rule says so. Your default is `Required`, so under BE-110's check a query that omits the field carries `Required` and is refused. With that default, every query must state `None`. Stating `None` on every query satisfies the law under any reading. If you want queries to omit the field, ask us (core §0) rather than choosing.
- **You filed the outbox check under BE-33.** In the law it is BE-110's `arch` half. Record it, and the key refusal, under BE-110 in your `ENFORCEMENT-STATUS.md` (core §1), with BE-110's gap: a handler reaching the outbox through another type it calls is reached by review.
- **"The pipeline order is unchanged. The transaction behavior still runs fifth."** That no longer matches the law. The idempotency and transaction behaviors are now one behavior, item 4. The idempotency record and the change it proves are then one commit. You must merge them. For a `None` command the merged behavior opens nothing, resolves no attempt identity, and runs the staged-change guard.

### What the law requires that your description does not mention

- **Idempotency for the non-persisting command.** It must declare client-supplied id or naturally idempotent (BE-26, BE-110). For *send the content*, §13 says how:
  - the command carries a client-supplied id;
  - the validation behavior parses it;
  - the quarantined blob's name is derived from the uploader's principal and that id with a keyed hash, such as an HMAC under a server key, never from the client's file name.

  A retry then finds the same blob rather than adding a second one, and no client can name another's blob. §4's example, "a blob name derived from the aggregate id", is an illustration; for an upload, §13 states the name.
- **Audit.** A command that does not persist is audited through `IAuditTrail`, on the port's own context or connection, never the module's. A record staged in the module's context would trip your staged-change guard. A failed audit write fails the operation. The required test is §14 **Audit** (BE-118): "every command that does not persist, a test proving its record is written".
- **The object check.** The command loads an aggregate the caller identifies, so it checks the principal may act on that record. The check runs after the load and before the handler acts, through the resource-authorization port (.NET §6; core §7). The required test is §14 **Authorization** (BE-117).
- **The upload's own rules** (§13; core §7 *File uploads*; §14 **Uploads**, BE-120):
  - the 100 MB limit is the endpoint's request size limit, enforced before the body is buffered;
  - the file is stored in quarantine, under the keyed-hash name above;
  - its type is validated from its content;
  - the accepting operation declares the file's class;
  - "content received" is recorded only once the malware scan passes, by a persisting command run under the uploader's principal recorded with the quarantined file.
- **Agents, if the agents annex binds your tree** (`references/annex-agents.md`). An agent invoking this command carries the same client-supplied id on every attempt (BE-31), and the re-invocation test BE-114 applies.

## Answer table

| Item | Reported | Answer |
| --- | --- | --- |
| Transaction-less commands (backend.md §4, *Transaction* vs "MUST NOT call external systems inside a transaction") | Issue #222 (AD-1), task #230 | **Option 1, amended** in `rancho-dotnet` 1.1.0 and revised in 2.0.0 (§4 *Pipeline behaviors* item 4, §4 *Transactions and consistency*, BE-110, BE-111, §13 upload bullet). A command declares whether it persists. One that does not opens no unit of work, stages nothing (refused after the handler), uses no idempotency key and no outbox, and is idempotent naturally or by a client-supplied id. **Changes for you:** refuse `Required` on a query, not `None` (with your `Required` default, every query states `None`; whether a query may omit the field is not settled by the law); merge the idempotency and transaction behaviors into item 4; declare a client-supplied id and derive the quarantined blob's name from the uploader's principal and that id with a keyed hash; audit the command through `IAuditTrail`'s own context; record the outbox and key checks under BE-110. Release: <link to be added> |
