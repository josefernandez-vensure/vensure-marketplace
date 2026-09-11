---
name: vaca
description: "Spec-driven project management: PRD → Epic → GitHub Issues → parallel agents → shipped code. Use this skill for anything in the software delivery lifecycle: writing a PRD ('write a PRD for X', 'let's plan X', 'scope this out'), parsing a PRD into an epic, decomposing an epic into tasks, syncing to GitHub ('sync the X epic', 'push tasks to github'), starting work on an issue ('start working on issue N', 'let's work on issue N'), analyzing parallel work streams, running standups ('standup', 'run the standup'), checking status ('what's next', 'what's blocked', 'what are we working on'), closing issues, or merging an epic. Use vaca any time the user is talking about shipping a feature, managing work, or tracking progress — even if they don't say 'vaca' or 'PRD'. Do NOT use for: debugging code, writing tests, reviewing PRs, or raw GitHub issue/PR operations with no delivery context."
---

# VACA — Vensure Agentic Code Assistant

A spec-driven development workflow: PRD → Epic → GitHub Issues → Parallel Agents → Shipped Code.

## Core Philosophy

Requirements live in files, not heads. Every feature starts as a PRD, becomes a technical epic, decomposes into GitHub issues, and gets executed by parallel agents with full traceability.

## File Conventions

Before doing anything, read `${CLAUDE_SKILL_DIR}/references/conventions.md` for path standards, frontmatter schemas, and GitHub operation rules. These apply to all phases.

## The Five Phases

### 1. Plan — Capture requirements
**When**: User wants to define a new feature, product requirement, or scope of work.
**Read**: `${CLAUDE_SKILL_DIR}/references/plan.md`
**Covers**: Writing PRDs through guided brainstorming, converting PRDs to technical epics.

### 2. Structure — Break it down
**When**: An epic exists and needs to be decomposed into concrete tasks.
**Read**: `${CLAUDE_SKILL_DIR}/references/structure.md`
**Covers**: Epic decomposition into numbered task files with dependencies and parallelization.

### 3. Sync — Push to GitHub
**When**: Local epic/tasks need to become GitHub issues, progress needs to be posted as comments, or a bug is found and needs a linked issue created.
**Read**: `${CLAUDE_SKILL_DIR}/references/sync.md`
**Covers**: Epic sync (epic + tasks → GitHub issues), issue sync (progress comments), closing issues, merging an epic, bug reporting against completed issues.
`sync.md` is a router: it carries the repository safety check and points at one of `sync-epic.md`, `sync-issue.md`, `sync-close.md`, `sync-merge.md`, `sync-bug.md`. Read the router plus the one operation, not all of them.

### 4. Execute — Start building
**When**: User wants to start working on one or more GitHub issues with parallel agents.
**Read**: `${CLAUDE_SKILL_DIR}/references/execute.md`
**Covers**: Issue analysis (parallel work stream identification), launching parallel agents, coordinating worktrees.

### 5. Track — Know where things stand
**When**: User asks for status, standup report, what's blocked, what's next, or needs to validate state.
**Read**: `${CLAUDE_SKILL_DIR}/references/track.md`
**Covers**: Status, standup, search, in-progress, next priority, blocked items, validation.

---

## Script-First Rule

For deterministic operations — anything that reads and reports without needing reasoning — always run the bash script directly rather than doing the work manually.

Scripts read `.claude/` relative to the **project root**, so run them from there. Their own location is `${CLAUDE_SKILL_DIR}/scripts/`, which is not the working directory — always invoke them by full path:

| What the user wants | Script to run |
|---|---|
| Project status | `bash "${CLAUDE_SKILL_DIR}/scripts/status.sh"` |
| Standup report | `bash "${CLAUDE_SKILL_DIR}/scripts/standup.sh"` |
| List all epics | `bash "${CLAUDE_SKILL_DIR}/scripts/epic-list.sh"` |
| Show epic details | `bash "${CLAUDE_SKILL_DIR}/scripts/epic-show.sh" <name>` |
| Epic status | `bash "${CLAUDE_SKILL_DIR}/scripts/epic-status.sh" <name>` |
| List PRDs | `bash "${CLAUDE_SKILL_DIR}/scripts/prd-list.sh"` |
| PRD status | `bash "${CLAUDE_SKILL_DIR}/scripts/prd-status.sh"` |
| Search issues/tasks | `bash "${CLAUDE_SKILL_DIR}/scripts/search.sh" <query>` |
| What's in progress | `bash "${CLAUDE_SKILL_DIR}/scripts/in-progress.sh"` |
| What's next | `bash "${CLAUDE_SKILL_DIR}/scripts/next.sh"` |
| What's blocked | `bash "${CLAUDE_SKILL_DIR}/scripts/blocked.sh"` |
| Validate project state | `bash "${CLAUDE_SKILL_DIR}/scripts/validate.sh"` |
| Reference card | `bash "${CLAUDE_SKILL_DIR}/scripts/help.sh"` |

Use the LLM for work that requires reasoning: writing PRDs, analyzing parallelism, launching agents, synthesizing updates.

**Do not replace a script with a hand-rolled equivalent.** Frontmatter is parsed in exactly one place — the readers in `scripts/lib/read-*.awk` — and every report is a single `awk` pass through one of them. That is why a report stays fast as a project grows, and why all of them agree on where frontmatter ends, what a missing field defaults to, and how to handle Windows line endings. Reconstructing the same answer with a loop of `grep`/`sed` calls is slower and reintroduces exactly those disagreements.

---

## Engineering Law

Code produced under VACA is governed by the engineering law in `/rancho:law`. VACA decides *what* gets built and in what order; the law decides what the code must look like. Where they touch:

- Agents launched in the Execute phase must read the law before writing code, and the backend or frontend reference for whichever tree they are working in.
- Task acceptance criteria may reference law sections, but must not restate rules — one copy of every rule, or they drift.
- If a task as written cannot be implemented without violating the law, that is a defect in the task. Say so and revise the task, rather than letting an agent ship the violation.

---

## Quick Reference

```
Plan a feature:     "I want to build X" or "create a PRD for X"
Parse to epic:      "turn the X PRD into an epic"
Decompose:          "break down the X epic into tasks"
Sync to GitHub:     "push the X epic to GitHub"
Start an issue:     "start working on issue 42"
Check status:       "what's our status" / "standup"
What's next:        "what should I work on next"
Merge epic:         "merge the X epic"
Report a bug:       "found a bug in issue 42" / "testing issue 42 revealed X"
```
