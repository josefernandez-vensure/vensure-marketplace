# Enforcement Inventory - Core

Every rule marked ⚙ in the core law, and the role whose stack laws carry it.

`../SKILL.md` §1 defines ⚙ as **"machine-checked. Violating one is a build failure, not a review comment."** This file is the audit of that claim for the core law. A check is written in a technology, so a core rule's check lives in a stack law: this file names the role each rule binds, and every stack law of that role lists the rule, with its mechanism and check, under **Core rules carried** in its own inventory (core §10, in `AUTHORING.md` at the plugin root). A ⚙ with no entry here is unaccounted for and MUST be added.

The records of how the law came to read as it does - rewordings, amendments, prerequisites, and the history of each row - are in `RECORDS.md` at the plugin root.

## How to read an inventory

This legend is shared: every inventory, core and stack, is read by it.

**Ids are permanent.** `BE-31` stays `BE-31` when sections renumber. The Section column is maintained; the id never changes. An id is never deleted and never reused, in any law: a rule that leaves keeps its row, with the status that says why. Tests reference the id in their name: `BE_31_QueryHandlers_DoNotReferenceAggregates`.

**Each law has its own prefix.** `ROOT` is the core law's. A stack law declares its own prefix at the top of its inventory - `BE` is `rancho-dotnet`'s, `FE` is `rancho-react`'s - and a new stack law takes a new one.

**Columns.** A core row is `| Id | Rule | Section | Binds | Status | Source |`. A stack row, and a row under a stack inventory's **Core rules carried**, is `| Id | Rule | Section | Mechanism | Status | Check | Gap | Source |`.

| Column | Meaning |
|---|---|
| Id | The rule's permanent id. |
| Rule | The rule in short. The law's own sentence governs; the row only names it. |
| Section | The section of the law that states the rule (§N). |
| Binds | Core rows only: the role (`../SKILL.md` §2) whose stack laws MUST carry the rule - `server`, `client`, or `both`. |
| Mechanism | Stack rows only: the tool the check is written in. The codes belong to the stack laws; each stack inventory defines the codes for the tools its technology checks with. |
| Status | One of the four below. |
| Check | Stack rows only: what the check does. `Canonical: X` means row X's check satisfies this row too; `Covered by X` means X's check reaches this rule; `Merged into X` means the rule's sentence now survives only as X. |
| Gap | Stack rows only: what the check knowingly does not reach, or the part review reaches instead; `—` when nothing. A gap is a known gap in enforcement - it does not make the rule optional. |
| Source | Reserved for where the rule's wording comes from; `—` until it is recorded. |

**Status:**

| Status | Meaning |
|---|---|
| `build` | Enforceable as worded. A violation can be made to fail the build, and the law marks the rule ⚙ with this id. |
| `required-test` | No build check can reach it. The rule carries no ⚙; the obligation is named, with this id, in its law's Testing section, which is where it is caught. |
| `dropped` | The ⚙ was removed. The rule still stands - it is enforced by review, not by the build. |
| `retired` | Merged into another row: the rule's sentence was a duplicate of one that survives, and the Check cell names it (`Merged into X`). The id is kept and never reused. |

## Summary

The core law carries **15 ⚙ marks**, one on each `build` row below. Its inventory has **17 rows**: 15 `build`, 0 `required-test`, 2 `dropped`, and 0 `retired`. The two `dropped` rows record core rules whose mark was dropped and which review enforces.

**Every one of the 15 marked rules can be made to fail the build** in each stack law that carries it. Totals for each stack law are in that stack law's inventory.

Twelve root rules are **duplicated** by a stack-law rule. One check satisfies each pair; the stack law's inventory names the canonical id.

## Root - `../SKILL.md`

| Id | Rule | Section | Binds | Status | Source |
|---|---|---|---|---|---|
| ROOT-01 | No module references another module's internals; only its published contracts | §3 | server | build | — |
| ROOT-02 | Domain events MUST NOT cross a module boundary | §3 | server | build | — |
| ROOT-03 | "An event with both audiences is two events" | §3 | server | dropped | — |
| ROOT-04 | One schema per module; no cross-schema joins, no cross-module FKs | §3 | server | build | — |
| ROOT-07 | Endpoints take a Request DTO, return a Response DTO; no domain type reachable from HTTP | §4 | server | build | — |
| ROOT-08 | Query results projected directly to Response DTOs | §4 | server | build | — |
| ROOT-09 | OpenAPI document generated and committed | §4 | server | build | — |
| ROOT-11 | Every collection endpoint paginated | §4 | server | build | — |
| ROOT-12 | One page shape across the API | §4 | server | build | — |
| ROOT-13 | Every state-changing endpoint declares an idempotency mechanism | §4 | server | build | — |
| ROOT-14 | Deny by default, everywhere | §7 | both | dropped | — |
| ROOT-15 | Acting principal explicit; no ambient HTTP context in application or domain | §7 | server | build | — |
| ROOT-16 | Unclassified is `Restricted` | §8 | server | build | — |
| ROOT-17 | Integration events MUST NOT carry `Restricted` payloads | §8 | server | build | — |
| ROOT-18 | A field's class travels with its contract: the OpenAPI document carries the class of every request and response property and every parameter, and the client enforces it at its own boundaries | §8 | both | build | — |
| ROOT-19 | Every unit of work carries a correlation id | §9 | server | build | — |
| ROOT-20 | A class declared on a domain type and on its published contract type is proven by a check to agree | §8 | server | build | — |
