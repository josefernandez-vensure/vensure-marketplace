# Closure of the 2026-11-03 review

How every finding of `RECORDS-3.0-review.md` was closed in the 3.0 stages. Audited at the start of stage 8 (commit 5b50a51); file:line references are to that commit. After the audit, stage 8 closed the one open id: BE-90's gap is recorded in the .NET inventory (`plugins/rancho-dotnet/RECORDS.md`, `## 3.0 stage 8`). Still open by deliberate deferral, recorded there too: the duplicate merges of the review's §6 are linked by `Covered by` rather than merged, and the .NET stream loads about 19.9k tokens against the ~16k target.


Branch `law-3.0-s8-release` at 5b50a51. Read-only. Paths are relative to the repository root.

Abbreviations: `core-R` = `plugins/rancho/RECORDS.md`, `net-R` = `plugins/rancho-dotnet/RECORDS.md`, `fe-R` = `plugins/rancho-react/RECORDS.md`; `core-L` = `plugins/rancho/skills/law/SKILL.md`, `net-L` = `plugins/rancho-dotnet/skills/law/SKILL.md`, `fe-L` = `plugins/rancho-react/skills/law/SKILL.md`; `net-E`, `fe-E` = each stack's `skills/law/references/enforcement.md`.

## Method

- Ids taken: every row id of §1-§3 (P-, C-, N-, R-, S-), every id in §7's verdict table (A-), and every id §4 and §5 name as the subject of a finding (A-21, BE-, FE-). A rule id cited only as context inside a P-/N- row (such as BE-106 in P-3) is covered by that row.
- **The review contains no CORE-, NET-A-, NET-B- or FE-R- ids** (`grep` count 0). Those ids appear only in the three RECORDS.md files; they are the reviewers' own finding ids. They are mapped in a separate table at the end, by inference from the review item each RECORDS line closes alongside them.
- Pass 1: a RECORDS.md line naming the id in a "Closes" clause, or (for §1) the review's own status line 3, which says §1 was fixed before PR #9 merged as 2.1.0. Otherwise "open".
- Pass 2: every pass-1 "open" id checked against the current law text.
- Stage 2 (PR #12, commit 06ef7fa) and stage 1 (PR #11, commit 7f6547d) wrote no "## 3.0 stage N" section in any RECORDS.md. Their changes are cited by commit and law text.

## Counts

| Result | Count |
|---|---|
| Closed - a RECORDS line (or the review's §1 status) closes it | 66 |
| Resolved in text - no RECORDS line names it, the current law resolves it | 14 |
| Still open | 1 (BE-90) |
| **Total ids** | **81** |

Non-id items still open (§6 size and structure): see the last section.

## §1 PR #9 defects (closed before 2.1.0, review line 3)

| Id | Status | Record of the fix | Current text |
|---|---|---|---|
| P-1 | Closed, PR #9 | core-R:81 ("A draft that called the commit outcome the only result value ... was corrected before release") | core-L:118 |
| P-2 | Closed, PR #9 | core-R:81 ("A draft that admitted only the projected case left a cursor and the page envelope unclassifiable ... corrected") | core-L:157 |
| P-3 | Closed, PR #9 | net-R:258 (*The composition root*: `Api` plus the registration entry points; BE-52, BE-95, BE-98 scoped to it) | net-L:110 |
| P-4 | Closed, PR #9 | net-R:259 (the authority's SDK in one Infrastructure adapter, BE-102) | net-L:360 |
| P-5 | Closed, PR #9 | net-R:264 (a command declares whether it persists; BE-31 carries a key only for a command that declares one) | net-L:249, annex-agents.md:15 |
| P-6 | Closed, PR #9 | net-R:264 (tool parameters are wire values from `Contracts`) | annex-agents.md:62 |
| P-7 | Closed, PR #9 | net-R:258 (Mediator 3.x pinned; the draft generator branch removed; handlers public) | net-L:65, net-L:285 |
| P-8 | Closed, PR #9 | fe-R:83 (the per-boundary field-name sets replaced by a class brand per type) | fe-L:108 |
| P-9 | Closed, PR #9 | net-R:260 (read types or parameterized SQL; raw-string SQL banned; NoTracking assertion withdrawn) | net-L:187-188 |
| P-10 | Closed, PR #9 | net-R:318 (BE-105 narrowed from an allowlist), net-R:267 | net-L:108 |
| P-11 | Closed, PR #9 | fe-R:83 (refresh, session store and grant given to the shell) | fe-L:167, :172, :174 |

## §2 Combinations with no compliant answer

| Id | Pass 1 | Pass 2 / evidence |
|---|---|---|
| C-1 | Closed, stage 3: core-R:119, net-R:377, fe-R:136, :138, :139 | core-L:187 (exemplars) |
| C-2 | Closed, stage 5: core-R:145, :149; net-R:440 | core-L:130 |
| C-3 | open | **Resolved in text**, stage 2 (06ef7fa): core-L:36 binds a tree no stack law governs with `Stack law: none - <reason>`, naming IaC and CI scripts, with its own verification commands (core-L:202). Note: end-to-end test suites are not named; a Playwright suite is code in a technology `rancho-react`'s Stack names (fe-L:39), so a standalone suite binds `rancho-react`, whose rules are mostly conditional on features it would not have. No contradiction found, but the case is not stated. |
| C-4 | open (not named) | **Resolved in text**: core-L:66 ("a Response DTO, no body, or a stream it declares"), net-L:493. core-R:115 closes CORE-14, which is this finding. |
| N-1 | Closed, stage 5: net-R:430 | net-L:226 (one unit-of-work-and-idempotency behavior, item 4) |
| N-2 | open (not named) | **Resolved in text**, PR #9: net-R:258 (BE-53's check narrowed to module `Domain`/`Application`), restated stage 7 net-R:606; net-L:357, net-E:136. The authorization behavior in `Platform.Application` is outside BE-53's scope. |
| N-3 | Closed, stage 5: net-R:431 | net-L:106 |
| N-4 | open (not named) | **Resolved in text**, stage 5: net-R:429 closes NET-A-8 and A-19 with this change. net-L:168 (events dispatched to in-module reactions before commit, same transaction), net-L:127 (a reaction changes state inside the command's unit of work and calls no handler), net-L:226. The at-most-once concern goes with in-transaction dispatch. |
| N-5 | Closed, stage 5: net-R:432 | net-L:191 |
| N-6 | open (not named) | **Resolved in text**, PR #9: net-R:260 (a strongly-typed id's constructor and value member exempt from BE-08), stage 7 net-R:557 (compiler-generated members too). net-L:162, net-L:323. |
| N-7 | Closed, stage 5: net-R:433 | annex-realtime.md:10 |
| N-8 | open (not named) | **Resolved in text**: PR #9 net-R:265 (BE-82 admits the endpoint abstraction, `Task`, `CancellationToken`, HTTP result types); stage 5 net-R:437 (registrar applies authorization and rate-limit metadata from the operation declaration). net-L:490 ("an endpoint declares neither itself"), net-L:491. |
| N-9 | open (not named) | **Resolved in text**, PR #9: net-R:258 records Mediator 3.x pinned in §1. net-L:65; the decorator is annex-caching.md:18. |
| N-10 | Closed, stage 5: core-R:147 | core-L:191; net-L:452 now consistent |
| N-11 | Closed, stage 5: net-R:434 | net-L:145, net-L:527 (BE-132) |
| R-1 | Closed, stage 6: fe-R:154, :155 | fe-L:69, :130-131 |
| R-2 | Closed, stage 6: fe-R:156 | fe-L:115-117 |
| R-3 | Closed, stage 6: fe-R:157 | fe-L:108, :156 |
| R-4 | Closed, stage 6: fe-R:154, :158 | fe-L:56, :97, :166 |

## §3 Security gaps

| Id | Closed by | Current text |
|---|---|---|
| S-1 | Stage 4: core-R:135, net-R:406 | core-L:128; net-L:353-354; BE-117 net-L:511 |
| S-2 | Stage 4: net-R:410 | annex-realtime.md:12-14; BE-121 |
| S-3 | Stage 4: core-R:139, net-R:407 | core-L:196-198; net-L:466-470; BE-118 |
| S-4 | Stage 4: core-R:137 (review fix :140; registrar net-R:416) | core-L:166; net-L:497 |
| S-5 | Stage 4: core-R:138 (net-R:415) | core-L:168, :175; net-L:459 |
| S-6 | Stage 4: core-R:136, net-R:409 | core-L:133-139; net-L:498; BE-120 |
| S-7 | Stage 4: net-R:408 | net-L:215 (BE-119) |
| S-8 | Stage 4: fe-R:149 | fe-L:124; FE-37 |

## §7 Amendment verdicts

| Id | Pass 1 | Pass 2 / evidence |
|---|---|---|
| A-11 | open (not named) | **Resolved in text**, stage 2 (06ef7fa): core-L:27 (mark is a claim about the rule; `ENFORCEMENT-STATUS.md`; reviewer verifies until built); prime-directive.md:19-20 defers to core §1 and no longer says "build failure". |
| A-12 | open (not named; net-R:435 "NET-A-12" is a false match) | **Resolved in text**, stage 2: core-L:16-17. Reconciliation: suppressions name the register entry (core-L:215, net-L:67, fe-L:43, :45); a limit a rule's own text sets is part of the rule (core-L:214); `data-testid` comment states which element lacks a role (fe-L:201). |
| A-13 | open (not named) | **Resolved in text**, stage 2: core-L:206-216 (`LAW-EXCEPTIONS.md`, `EXC-n`, conflict and boundary entries, amendment request and expiry); boundary exception names its entry (core-L:150, :173); UI-library clause in `rancho-react` (fe-L:146). Stage 6 core-R:182 adds the form for an unmarked rule. |
| A-15 | Closed, stage 3: core-R:116, net-R:374, :381 | core-L:74; net-L:207 |
| A-17 | Closed, stage 5: net-R:438 (net-R:408 is NET-A-17, a false match) | net-L:211 |
| A-18 | Closed, stage 3: net-R:373, :376, :378, :380; fe-R:133, :139 | net-L:182, :296 |
| A-19 | Closed, stage 5: net-R:429 | net-L:244-245 |
| A-22 | Closed, stage 5: net-R:437 | rationale in net-R:207; rule net-L:488 |
| A-23 | Closed, stage 5: net-R:436, :437; core-R:146 | net-L:420, :422, :490 |
| A-24 | Closed, stage 6: fe-R:159 | fe-L:114, :141 |
| A-25 | Closed, stage 6: fe-R:160 | fe-L:145 |
| A-26 | Closed, stage 6: fe-R:154 | fe-L:59-66, :82 |
| A-27 | Closed, stages 3 and 6: fe-R:134, :135, :164 | fe-L:182-183; core-L:96 |
| A-28 | Closed, stage 4: core-R:138 | core-L:168, :175 |
| A-30 | open (not named) | **Resolved in text**, stage 1 (7f6547d): net-L:10-16 and the annexes `annex-agents.md`, `annex-caching.md`, `annex-realtime.md` (.NET) and `annex-realtime.md` (React); net-R:336, :346, :353, :357-360 record tests moved to annexes. Core's short agent sentences stay in core (core-L:87, :130, :186, :190), and net-L:347's illustrative operation declaration still shows `Human: HumanRequirement.Required` (illustrative; the only rules on it are in annex-agents.md:48, :57). |
| A-31 | open (not named) | **Resolved in text**, stage 2: as the verdict says, a SHOULD in VACA's planning, not the law: `plugins/rancho/skills/vaca/references/plan.md:100`. |
| A-32 | Closed, stage 4: core-R:135 | core-L:128 |
| A-33 | Closed, stage 4: core-R:136 | core-L:133-139 |
| A-34 | Closed, stage 4: core-R:139 | core-L:196-198 |
| A-35 | Closed, stage 3: net-R:373, :376, :378, :380; fe-R:133, :139 | core-L:97 |
| A-36 | Closed, stage 3: core-R:119, net-R:377, fe-R:136, :138, :139 | core-L:187; fe-L:171 |
| A-37 | Closed, stage 6: fe-R:166 | fe-L:210 (FE-42) |
| A-38 | open (not named) | **Resolved in text**, PR #9 (BE-107, net-R:256, :258, row history net-R:320); widened stage 7 net-R:558 (the mocking library's proxy assembly). net-L:285. |

## Ids named in §4 and §5

| Id | Finding | Pass 1 | Pass 2 / evidence |
|---|---|---|---|
| BE-70 | §4 rate limiting (no default policy, check, `[DisableRateLimiting]`, `Retry-After`, `503`) | Closed, stage 5: net-R:436, :452; core-R:146 | net-L:420, :423; net-E:164 |
| BE-96 | §4 options validation sweep | Closed, stage 5: net-R:439 (E14), stage 7 net-R:607 | net-E:177 |
| A-21 | §4 "source generator registers the validator" is wrong | open (not named) | **Resolved in text**: net-E:177 counts "a registered `[OptionsValidator]` class", so the generated validator counts only once registered. (The hand-off itself withdrew A-21.) |
| BE-19 | §4 "`Page<T>` only" fails single-item queries | Closed, stage 3: net-R:374, :391; stage 7 net-R:572 | net-E:88 (non-collection responses skipped) |
| FE-09 | §4/§5 flags the canonical key factory | Closed, stage 6: fe-R:156, :175 | fe-E:51 |
| FE-07 | §5 `no-restricted-syntax` has no type information | Closed, stage 6: fe-R:163, :177; stage 7 fe-R:239 | fe-E:49 |
| BE-12 | §5 banned-API lists are per project | Closed, stage 5: net-R:439; stage 7 net-R:572 | net-E:78 (`arch` `NotCallAny`) |
| BE-63 | §5 ArchUnitNET cannot see string literals | Closed, stage 7: net-R:571, :574 | net-E:156 |
| FE-16 | §5 needs taint tracking | Closed, stage 7: fe-R:237, :248-249 (gap recorded; unbranded refused) | fe-E:58 |
| FE-23 | §5 needs taint tracking | Closed, stage 7: fe-R:237, :248-249 | fe-E:62 |
| FE-25 | §5 needs taint tracking | Closed, stages 6/7: fe-R:162, :237 (gap recorded) | fe-E:64 |
| BE-31 | §5 "by construction" accepts `Guid.Empty` | Closed, stage 7: net-R:572 (gap stated) | net-E:105: the invoker derives the key; determinism recorded as a gap |
| BE-90 | §5 "by construction" accepts `Guid.Empty` | Stage 7 net-R:573 lists it "confirmed as written" | **Still open.** net-E:169 still reads "By construction: the context cannot be built without one", Gap "—". The value is now the `Activity` trace id (net-L:440); a required parameter still admits a default, all-zero id. The check is narrower than the rule and no gap is recorded, which core-L:204 requires. Fix: record the gap, or have the check refuse a default id. |
| BE-06 | §5 the repository port lives in Domain | Closed, stage 7: net-R:572 | net-E:68 |
| BE-23 | §5 checks the declaration, not the SQL | Closed, stage 5: net-R:438, :454; stage 7 :572 | net-L:211; net-E:92 |
| BE-82 | §5 needs call-graph analysis | Closed, PR #9 net-R:265; stage 7 net-R:572 | net-E:187 |
| BE-50 | §5 stricter than its rule | Closed, stage 5: net-R:439; stage 7 net-R:590 | net-E:133 |
| BE-53 | §5 stricter than its rule (N-2) | Closed, PR #9 net-R:258; stage 7 net-R:606 | net-E:136 |
| BE-13 | §5 contradicts BE-41 | Closed, stage 7: net-R:572 (gap stated); the "no longer covered" note left the row in stage 1 (net-R:342) | net-E:79 |
| BE-41 | §5 contradicts BE-13 | Closed, stage 7: net-R:572 | net-E:121 |

## §5 items without ids

- **The ⚙ claim** - A-11 above.
- **Unmarked MUSTs with no required test.** `429` with `Retry-After`: BE-127 (net-R:565). Every behavior emits a span: BE-128 (net-R:566). React loading/error/empty: FE-39; accessibility: FE-42 (fe-R:166, which also lists the React MUSTs reached by review). §15: BE-129 to BE-136, commented-out code reached by review (net-R:568). "Recoverable via REST" (annex-realtime.md:9): no test and no mark; **resolved in text** by core-L:29, which since stage 2 says a rule no test can prove is reached by review. The .NET law, unlike the React law (fe-R:166), never lists its review-reached MUSTs.

## Reviewer ids (in RECORDS.md only, not in the review)

Mapping is inferred from the review item closed on the same RECORDS line.

| Id | RECORDS line | Review item |
|---|---|---|
| CORE-7 | core-R:135 | S-1 / A-32 |
| CORE-12 | core-R:137 | S-4 |
| CORE-13 | core-R:138 | S-5 / A-28 |
| CORE-14 | core-R:115; net-R:381 | C-4 |
| CORE-15 | core-R:114 | §4 RPC endpoints |
| CORE-16 | core-R:116; net-R:374, :381 | A-15 |
| CORE-17 | core-R:118; net-R:379; fe-R:137 | wire contract (A-35, A-27, data class) |
| CORE-18 | core-R:117; net-R:375, :380 | §4 idempotency status codes |
| CORE-19 | core-R:119; net-R:377; fe-R:136, :138, :139 | C-1 / A-36 |
| NET-A-8 | net-R:429 | N-4 / A-19 |
| NET-A-10 | net-R:373, :376, :378, :380; fe-R:133, :139 | A-18 / A-35 (§4 conditional requests) |
| NET-A-12 | net-R:435 | §4 value objects |
| NET-A-17 | net-R:408 | S-7 |
| NET-B-6 | net-R:410 | S-2 |
| NET-B-7 | net-R:437 | A-22 / N-8 (registrar metadata) |
| NET-B-8 | net-R:436; core-R:146 | §4 rate limiting / A-23 |
| NET-B-11 | net-R:424-427 | §4 data classification |
| NET-B-13 | net-R:427, :440; core-R:145 | §4 agent budgets / C-2 |
| FE-R-11 | fe-R:149 | S-8 |
| FE-R-12 | fe-R:134, :135 | A-27 |
| FE-R-13 | fe-R:160, :166 | FE-31 / unmarked React MUSTs |

All 21 are closed by the line shown.

## §6 Size and structure (no ids): still open in part

`python -I tools/check-law.py` (read-only) reports, at 1.4 tokens per word:

| Measure | Review (2.1.0) | Target | Now |
|---|---|---|---|
| Load per .NET stream (core + stack SKILL.md) | ~20k | ~11-12k | **~18.7k** (core 4,309 words, .NET 9,035) |
| Load per React stream | ~9-10k | - | ~11.6k |

Stage 1's commit message reported .NET ~18.6k to ~15.3k; stages 2-7 added text back. The checker reports "The law's bookkeeping is consistent."

| §6 cut | State |
|---|---|
| 1. History out of the inventories | Done, stage 1 (row history in each RECORDS.md) |
| 2. Rationale out | Done, stages 1, 5, 6 ("Rationale moved" sections) |
| 3. Annexes (A-30) | Done, stage 1 |
| 4. Delete duplicates; merge the five query-side bullets, BE-13/BE-41, BE-79/BE-42, BE-38/BE-43 | **Open in part.** Two .NET restatements removed (net-R:209-215). The pairs are not merged: all six rows remain (net-E:79, :121, :75, :122, :118, :123). The query-side bullets are still separate (net-L:183-190). Stage 7 counts four .NET rules restating another (BE-05, BE-13, BE-14, BE-32) and eleven core rules restated (net-R:608). |
| 5. Maintainer-only text out | Done, stage 1 (`AUTHORING.md`) |
| 6. Rows reference the rule id; one shared legend | Shared legend done, stage 1. Rows still carry a short Rule column. |
