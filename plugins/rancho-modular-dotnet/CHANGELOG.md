# Changelog

## 0.1.0

- First draft, not yet trialled on a real change.
- States `This law stands alone.` at its top. With a `rancho` that recognizes standalone laws, a tree bound to it reads this law alone, not the core law.
- A standalone law of about 2,500 words. It replaces the rule-by-rule approach of `rancho-dotnet` with structure: the reference table and the placement table, plus the decisions that differ from a sensible default.
- The project's own code is the reference (§0). The tree's `CLAUDE.md` lists the reference implementation for each kind of code.
- Modules are treated as services:
  - one command changes one aggregate in one transaction;
  - every reaction runs from the outbox, as the module's service principal;
  - processes across modules are sagas with compensating commands.
- Caching, AI model calls and real-time push are not covered yet (§12).
