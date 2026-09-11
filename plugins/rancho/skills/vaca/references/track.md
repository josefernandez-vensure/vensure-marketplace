# Track — Know Where Things Stand

Tracking operations use bash scripts directly for speed and consistency. The LLM is not needed for these — just run the script and present the output.

The scripts parse frontmatter themselves, so this phase does not require `conventions.md`. Read it only if you need to interpret a field by hand or explain what a report means.

---

## Script-First Rule

All tracking operations have a corresponding bash script. Run the script; do not reconstruct the output manually.

Scripts live in `${CLAUDE_SKILL_DIR}/scripts/` and read `.claude/` relative to the **project root** (where `.claude/` lives). The working directory is the project, not the skill, so always invoke by full path:

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/<script>.sh" [args]
```

Each report is a single `awk` pass over the task files rather than a loop of `grep`/`sed` calls per file. That keeps a report at a fixed handful of processes no matter how many tasks exist — which matters on Windows, where process creation is slow enough that per-file pipelines made these reports unusable. Do not hand-roll an equivalent.
---

## Project Status

**Trigger**: "what's our status", "project status", "overview"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/status.sh"
```

Shows: active epics, open issues count, recent activity.

---

## Standup Report

**Trigger**: "standup", "daily standup", "what did we do", "morning update"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/standup.sh"
```

Shows: what was completed yesterday, what's in progress today, any blockers.

---

## List Epics

**Trigger**: "list epics", "show epics", "what epics do we have"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/epic-list.sh"
```

---

## Show Epic Details

**Trigger**: "show the <name> epic", "epic details for <name>"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/epic-show.sh" "<name>"
```

---

## Epic Status

**Trigger**: "status of the <name> epic", "how far along is <name>"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/epic-status.sh" "<name>"
```

Shows: task completion breakdown, active agents, blocking issues.

---

## List PRDs

**Trigger**: "list PRDs", "what PRDs do we have", "show backlog"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/prd-list.sh"
```

---

## PRD Status

**Trigger**: "PRD status", "which PRDs are parsed", "what's in backlog"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/prd-status.sh"
```

---

## Search

**Trigger**: "search for <query>", "find issues about <topic>", "look for <term>"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/search.sh" "<query>"
```

Searches local task files, PRDs, and epics for the query term.

---

## What's In Progress

**Trigger**: "what's in progress", "what are we working on", "active work"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/in-progress.sh"
```

---

## What's Next

**Trigger**: "what should I work on next", "what's next", "next priority"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/next.sh"
```

Shows highest-priority open tasks with no blocking dependencies.

---

## What's Blocked

**Trigger**: "what's blocked", "any blockers", "what can't we move on"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/blocked.sh"
```

---

## Validate Project State

**Trigger**: "validate", "check project state", "is everything consistent"

```bash
bash "${CLAUDE_SKILL_DIR}/scripts/validate.sh"
```

Checks: frontmatter consistency, orphaned files, missing GitHub links, dependency integrity.

---

## When Scripts Fail

If a script fails or the output needs interpretation (e.g., an error in the output, or the user asks "what does this mean"), then step in to explain. But always run the script first — don't guess at what status/standup output would look like.

If `.claude/` does not exist, the project has no VACA state yet. Nothing needs installing — the Plan phase creates `.claude/prds/` on the first PRD, and the later phases create the rest as they go. Point the user at `"create a PRD for <feature>"`.
