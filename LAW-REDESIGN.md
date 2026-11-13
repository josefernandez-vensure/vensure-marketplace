# Law redesign: target design and migration plan

Status: draft, 2026-11-11. Scope: the laws only. The VACA workflow, its hooks and its scripts don't change.

## Why

| Measure | Today (rancho 4.1.0) | Target |
|---|---|---|
| Words an agent reads before writing .NET code | ~14,100 (core + contract + .NET law), plus annexes | ≤ 4,000 |
| Words an agent reads before writing React code | ~8,500 | ≤ 4,000 |
| Files touched by a one-sentence law change | 17 (commit e80ef9c) | 1, plus a changelog line |
| Kinds of law document | core, contract, stack, standalone, annex, inventory, AUTHORING | core, stack |
| Share of the repo that is history (RECORDS, ledgers, changelogs) | 58% (~89k words) | git history and PRs |
| Rules a build check could enforce, kept as prose for every project to rebuild | ~95 .NET, ~33 React | 0: shipped as checks |

The cause is the design, not how it was carried out. The law is written as a legal code: complete, free of contradictions, auditable, and with an amendment process. Keeping a code like that consistent costs more with every rule, because each new rule has to be checked against all the others. That is what stages 3.0 to 4.1 spent their time on. A coding agent needs three things instead: the build failing when it breaks a structural rule, an example to copy, and a short list of the decisions that neither of those can show.

## Principles

1. **Enforce in code, not prose.** If a check can catch a rule, ship the check and take the sentence out of the law. The failing build tells the agent.
2. **Show, don't state.** Each consuming tree's `CLAUDE.md` lists reference implementations (the `## Reference` section of `rancho-modular-dotnet` §0). The idiom lives there.
3. **Derive, don't restate.** A rule that a structural constraint already makes impossible to break (the reference table, type placement, accessibility) isn't written down.
4. **Rules come from evidence.** A sentence enters a law only if it passes the admission test below.
5. **One copy, and only one.** Where the reading rule lives elsewhere (the prime directive, VACA), it is a pointer, never a restatement.
6. **History lives in git.** The reason for a rule goes in the PR that adds it, not in a file the plugin ships.

### Admission test

A sentence goes into a law only if all three hold:

- No check can enforce it, or the check would cost more than the mistake.
- The reference code can't show it.
- It prevents a security or data defect, or Claude has got it wrong in a real change, more than once.

### Budgets

- Core law: ≤ 1,500 words.
- Each stack law: ≤ 2,500 words.
- Go over budget and you remove something.

## Target shape

```
plugins/
  rancho/
    skills/law/SKILL.md        core: how to read the law, access, data classes and egress,
                               observability and audit, the wire contract. ≤ 1,500 words.
    skills/vaca/...            unchanged
    context/prime-directive.md 5 lines, printed only in a repo that binds a law
  rancho-dotnet/
    skills/law/SKILL.md        the .NET law (rancho-modular-dotnet becomes this). ≤ 2,500 words.
  rancho-react/
    skills/law/SKILL.md        the React law, rewritten. ≤ 2,500 words.
```

Outside the marketplace, the checks ship as code:

- **.NET:** a `dotnet new` template, or a package, carrying the ArchUnitNET tests, the `BannedSymbols.txt`, the analyzer set and `Directory.Build.props`.
- **React:** a shared ESLint config and the CI scripts.

### The one reading rule

Stated once, in core §1:

> Read `/rancho:law`, then the law named by the nearest `CLAUDE.md` at or above your files that holds a `Stack law:` line. A tree bound as `Stack law: none - <reason>` reads only the core. No binding, or a law the session doesn't have: stop and say so.

The model drops roles, standalone laws, annex triggers, and reading inventories by rule id. Every tree reads exactly two files.

### What happens to each part of today's law

| Today | Target |
|---|---|
| Core §0 Prime Directive, §1 Rule language, the ⚙ mark | 5–8 lines in core §1. Drop ⚙ and rule IDs: a check is named for what it checks. |
| Core §2: routing table, roles, the standalone law | The one reading rule above |
| Core §3 Access, §4 Data boundaries, §5 Observability and audit | Keep, tightened. This is the part with real value. |
| Contract law (`references/contract.md`) | Merged into core as one short section. The server law owns how it is produced. The client generates its types from the committed OpenAPI document. |
| Core §6 Verification | Each stack law's verification commands. Keep the "no test that only proves the build" rule. |
| Core §7: exceptions register, `LAW-EXCEPTIONS.md`, `ENFORCEMENT-STATUS.md` | Drop the requirement. A suppression carries a comment with its reason. A disagreement with the law is an issue on this repo. |
| `rancho-dotnet` SKILL.md (9.9k words) | Retire it. `rancho-modular-dotnet` (2.6k) takes the name. Each rule moves to a check, to the reference code, or into the law if it passes the admission test. Otherwise it is dropped. |
| .NET annexes (agents, caching, real-time) | "Not yet covered: stop and ask" (as modular-dotnet §12 does today). A short section is written when the first real project needs it, using the annex as source material. |
| `rancho-react` SKILL.md (4.2k words) | Rewrite it to ≤ 2.5k with the same test, and move the ~33 ESLint/CI rows into the shared config |
| `enforcement.md` ×3 (15.8k words) | Delete. The checks are the inventory. |
| `RECORDS.md` ×3, `RECORDS-*` ×5 (80k words) | Archive under tag `law-archive-4.1`, then remove from `main` |
| `AUTHORING.md`, `tools/check-law.py`, `tools/law-ledger.py` | Delete. A 10-line "Changing a law" section in the README replaces them. |
| CHANGELOGs | One line per change, from now on |
| SessionStart prime directive (prints in every repo) | 5 lines, printed only when the repo root `CLAUDE.md` holds `Stack law:` |
| Reading-rule paragraphs in `vaca/SKILL.md` (lines 84–86), `vaca-stream.md` and `help.sh` | One pointer line each. The workflow is unchanged. |
| `plugin-guard.sh` | Keep |

### How a rule changes after this

1. Claude gets something wrong in a real change.
2. Fix the code first. Then ask whether a check, a better reference, or a structural change prevents a repeat (see Principles 1–3).
3. Only if none does: add one sentence to one law, within budget. Bump that plugin's version and add one changelog line.
4. The PR description records why.

## Migration

Ship it as one breaking release, so consuming projects upgrade once: `rancho` 5.0.0 and `rancho-dotnet` 4.0.0 (formerly modular), `rancho-react` 3.0.0. Each phase below is its own PR into a release branch.

| Phase | Work | Done when |
|---|---|---|
| 0. Freeze | Tag `law-archive-4.1`. No more amendments to the current law. | Tag pushed |
| 1. .NET checks (done: `templates/rancho-dotnet/`; triage in `LAW-REDESIGN-dotnet-triage.md`) | Turn each of the ~95 build-checkable rows in `rancho-dotnet`'s `enforcement.md` (`arch`, `contract`, `roslyn`, `proj`, `ci`) into a test or config in the template, or drop it. Turn the 32 `required-test` rows into the short list of behaviour tests in the law (modular-dotnet §11 already has this form). | The template builds, its arch tests pass on itself, and they fail on a seeded violation |
| 2. .NET law (done: `plugins/rancho-dotnet/skills/law/SKILL.md`, 1,931 words; triage's last section) | Port into modular-dotnet only the rules from `rancho-dotnet` that pass the admission test and aren't caught by phase 1. Drop the "stands alone" line and its own data-class and trace-id sections; core carries them. | ≤ 2,500 words; `rancho-dotnet` 3.x SKILL.md retired |
| 3. React (done: `templates/rancho-react/` and `plugins/rancho-react/skills/law/SKILL.md`, 1,901 words; triage in `LAW-REDESIGN-react-triage.md`) | The same as phases 1 and 2: the ESLint config package, then the law rewritten | ≤ 2,500 words |
| 4. Core | Merge the contract into core; state the one reading rule; delete standalone, roles, annexes and inventories from the model | ≤ 1,500 words |
| 5. Pointers | Make the prime directive conditional and short; the three VACA reading-rule passages become one pointer each | `grep "stands alone"` and `grep "contract law"` return nothing outside the archive tag |
| 6. Clean-up | Delete RECORDS, ledgers, `enforcement.md`, `AUTHORING.md` and `tools/`; trim the READMEs | The repo holds the laws, VACA, and the READMEs |
| 7. Prove it | Re-run 3–5 completed tasks from a consuming project with the new law (and the old one if possible). Compare: the build and tests pass, review findings, tokens loaded. | No new class of defect; load ≤ 4k words |
| 8. Upgrade note | One page for consuming projects: add `## Reference` to each tree's `CLAUDE.md`, adopt the template's checks, and fold `LAW-EXCEPTIONS.md` entries into suppression comments or issues. `ENFORCEMENT-STATUS.md` is no longer required. | Note in the release |

Phase 1 is the largest, and the only one that builds new code. Everything after it is mostly deleting.

## Open decisions

1. **One .NET law or two?** Recommended: one. `rancho-modular-dotnet` replaces `rancho-dotnet`, since both describe the same modular monolith. Keep both only if some project is not a modular monolith.
2. **Ship the checks as a template or as a package?** Recommended: a template first, because it needs no package feed; move to a package once two projects use it. Where should the template live? This marketplace only ships plugins, so a separate repo is probably right.
3. **Is the governance layer required by anyone outside the team?** That means the exception IDs, audit hand-offs and amendment groups. If compliance or audit needs them, keep them in a human-facing document outside the plugins, never in what an agent loads.
4. **Keep MUST/SHOULD/MAY?** Recommended: plain imperatives ("Always", "Never", "Prefer"), with "should" kept only where a default can be departed from with a reason.
