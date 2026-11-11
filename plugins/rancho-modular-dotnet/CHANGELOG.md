# Changelog

## 0.2.0

- **Role `server`.** Still standalone: the core law and stack laws do not
  apply. Its trees now also read the contract law, `references/contract.md`
  in `/rancho:law`, in full, and this law publishes it. Needs `rancho`
  4.1.0.
- §7 *Wire contract* no longer restates the wire. It defers to the
  contract law and keeps only how this tree meets it: keyset paging with a
  Data Protection cursor, the aggregate version as the `ETag`, the
  operation declaration's idempotency, per-principal rate limits, the
  declared class as `x-data-class`, each module's problem types in
  `x-problem-types`, and the CORS policy in `Api`.
  - The contract's rules that the old §7 left out now bind this tree. Among
    them: `x-problem-types`, the field-error shape, the cursor's
    `internal` class, the `etag` format and its return after a write,
    `Cache-Control`, replay before `If-Match`, `409` for a duplicate in
    flight, and the CORS headers a browser client reads.
- **Upgrade:** a tree checks its API against the contract law. A React
  client bound to `rancho-react` expects every rule there.

## 0.1.1

- States `This law stands alone.` at its top. With a `rancho` that recognizes standalone laws, a tree bound to it reads this law alone, not the core law.

## 0.1.0

- First draft, not yet trialled on a real change.
- A standalone law of about 2,500 words. It replaces the rule-by-rule approach of `rancho-dotnet` with structure: the reference table and the placement table, plus the decisions that differ from a sensible default.
- The project's own code is the reference (§0). The tree's `CLAUDE.md` lists the reference implementation for each kind of code.
- Modules are treated as services:
  - one command changes one aggregate in one transaction;
  - every reaction runs from the outbox, as the module's service principal;
  - processes across modules are sagas with compensating commands.
- Caching, AI model calls and real-time push are not covered yet (§12).
