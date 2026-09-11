# Execute — Start Building with Parallel Agents

This phase covers analyzing GitHub issues for parallel work streams and launching agents to execute them.

Read `conventions.md` first — it defines the task, progress, and work stream frontmatter schemas, the git and worktree conventions, and the `.claude/` paths this phase reads and writes.

---

## Session Discipline

**One session carries one issue.** A session analyses that issue, launches its
streams, sees them finish, closes the work out, and ends. It does not go on to
a second issue.

This is enforced, not encouraged. A `PreToolUse` hook counts every `vaca-stream`
launch and a `SubagentStop` hook counts every finish; when the counts balance,
the session is locked and every further user prompt is refused with an
instruction to `/clear`. Nothing you do in this phase can unlock it, and nothing
needs to: the lock arms *after* the streams end, so the close-out below still
runs in the session that has the context for it.

What this requires of you:

- **Never launch streams for a second issue in a session that has already run
  one.** The hook will not stop you at launch time, and the result is an issue
  whose work happened in a session with no room left to close it out.
- **Close out in the same turn the last stream finishes** — see *Closing Out a
  Session* below. The user cannot send you another message after that point.
- **End by telling the user to `/clear`**, and what to say next.

Cross-issue parallelism is still available; it is just one session per issue
rather than one session for several. Two issues at once means two sessions
against the same epic worktree.

---

## Issue Analysis

**Trigger**: User wants to understand how to parallelize work on an issue before starting.

### Preflight
- Find the local task file: check `.claude/epics/*/<N>.md` first, then search for `github:.*issues/<N>` in frontmatter.
- If not found: "❌ No local task for issue #<N>. Run a sync first."

### Process

Get issue details: `gh issue view <N> --json title,body,labels`

Read the local task file fully. Identify independent work streams by asking:
- Which files will be created/modified?
- Which changes can happen simultaneously without conflict?
- What are the dependencies between changes?

**Common stream patterns:**
- Database Layer: schema, migrations, models
- Service Layer: business logic, data access
- API Layer: endpoints, validation, middleware
- UI Layer: components, pages, styles
- Test Layer: unit tests, integration tests

Create `.claude/epics/<epic_name>/<N>-analysis.md`:

```markdown
---
issue: <N>
title: <title>
analyzed: <run: date -u +"%Y-%m-%dT%H:%M:%SZ">
estimated_hours: <total>
parallelization_factor: <1.0-5.0>
---

# Parallel Work Analysis: Issue #<N>

## Overview

## Parallel Streams

### Stream A: <Name>
**Scope**: 
**Files**: 
**Can Start**: immediately
**Estimated Hours**: 
**Dependencies**: none

### Stream B: <Name>
**Scope**: 
**Files**: 
**Can Start**: after Stream A
**Dependencies**: Stream A

## Coordination Points
### Shared Files
### Sequential Requirements

## Conflict Risk Assessment

## Parallelization Strategy

## Expected Timeline
- With parallel execution: <max_stream_hours>h wall time
- Without: <sum_all_hours>h
- Efficiency gain: <pct>%
```

**Output**: "✅ Analysis complete for issue #<N> — N parallel streams identified. Ready to start? Say: start issue <N>"

---

## Starting an Issue

**Trigger**: User wants to begin work on a specific GitHub issue.

### Preflight
1. Verify issue exists and is open: `gh issue view <N> --json state,title,labels,body`
2. Find local task file (as above).
3. Check for analysis file: `.claude/epics/*/<N>-analysis.md` — if missing, run analysis first (or do both in sequence: analyze then start).
4. Verify epic worktree exists: `git worktree list | grep "epic-<name>"` — if not: "❌ No worktree. Sync the epic first."

### Process

**Step 1 — Read the analysis**, identify which streams can start immediately vs. which have dependencies.

**Step 2 — Create progress tracking:**
```bash
mkdir -p .claude/epics/<epic>/updates/<N>
current_date=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
```

Create `.claude/epics/<epic>/updates/<N>/stream-<X>.md` for each stream, to the
work stream schema in `conventions.md` — `status: in_progress`, `completion: 0%`,
`checkpoint: 0/0`, `last_commit: (none yet)`, and an empty `## Checkpoints`
section.

The agent fills the checkpoint list in before it writes any code, and ticks it
off one commit at a time. Do not write the checkpoints for it: the agent is the
one that can see the shape of the work, and a list written here would be a guess
it then has to argue with.

These files live at the **project root**, not in the worktree — `conventions.md`
says why.

**Step 3 — Launch parallel agents** for each stream that can start immediately:

```yaml
Task:
  description: "Issue #<N> Stream <X>"
  subagent_type: "vaca-stream"
  prompt: |
    Issue #<N>, Stream <X>: <stream_name>

    Worktree (code):  ../epic-<name>/
    Project root (state): <absolute path>
    Epic: <epic>

    Your scope — the only files you may modify:
      <file_patterns>

    Work to complete:
      <stream_description>

    Your stream file: .claude/epics/<epic>/updates/<N>/stream-<X>.md

    Siblings running now: <other streams, or "none">
    Shared files owned by another stream: <files, or "none">
```

The `vaca-stream` agent carries the standing rules — read the law first, stay in
scope, checkpoint list before code, commit then update the stream file, never
`--force`. Do not restate them in the prompt. One copy, or they drift, and the
copy that drifts is the one the agent actually reads.

`subagent_type` must be exactly `vaca-stream`. The session hooks identify a work
stream by agent type; launched as `general-purpose` a stream is invisible to
them, and the session will never lock.

Streams with unmet dependencies are queued — launch them as their dependencies complete.

**Step 4 — Assign on GitHub:**
```bash
gh issue edit <N> --add-assignee @me --add-label "in-progress"
```

**Step 5 — Create execution status file** at `.claude/epics/<epic>/updates/<N>/execution.md`:
```markdown
## Active Streams
- Stream A: <name> — Started <time>
- Stream B: <name> — Started <time>

## Queued
- Stream C: <name> — Waiting on Stream A

## Completed
(none yet)
```

**Output:**
```
✅ Started issue #<N> — <title>

Launched N streams:
  Stream A: <name> ✓ running
  Stream B: <name> ✓ running
  Stream C: <name> ⏸ queued (waits on A)

State: .claude/epics/<epic>/updates/<N>/
This session carries issue #<N>, and ends when its streams do.
```

---

## Closing Out a Session

**Trigger**: every stream launched in this session has finished.

The session locks the moment the last stream ends — the user cannot send another
message until they `/clear`. Everything below therefore happens in that same
turn, unprompted.

**Step 1 — Reconcile the stream files against git.** For each stream, check its
`last_commit` against `git -C ../epic-<name> log --oneline`. A file that lags
behind its commits belongs to a stream that died between committing and
recording; repair it from the log before reporting anything out of it.

**Step 2 — Update** `progress.md` and `execution.md` for the issue: every stream
moved to Completed or Blocked, nothing left in Active.

**Step 3 — Post the progress comment** — Sync phase, `sync-issue.md`.

**Step 4 — Close the issue if its acceptance criteria are met** —
`sync-close.md`. If they are not, name the ones outstanding and leave it open.

**Step 5 — Report, then stop:**

```
✅ Issue #<N> complete — <what shipped>
   Streams:  A ✓   B ✓   C ✓
   Commits:  <n> on epic/<name>
   Issue:    closed          (or: open — <criterion> outstanding)

This session is finished. Run /clear, then say:
  "continue the <feature> epic"
```

Do not offer to carry on, and do not start the next issue. The next issue
belongs to the next session, and implying otherwise sets the user up to hit a
refusal they did not expect.

---

## Starting or Continuing an Epic

**Trigger**: "start the <feature> epic", "continue the <feature> epic", "what should I pick up".

One session carries one issue, so this resolves to exactly one: the next issue
that is ready. Start and continue are the same operation — which is why the
phrasing does not have to be exact.

### Preflight
- Verify `.claude/epics/<name>/epic.md` exists and has a `github:` field (i.e., it's been synced).
- Check for uncommitted changes: `git status --porcelain` — block if dirty.
- Verify epic branch exists: `git branch -a | grep "epic/<name>"`

### Process

**Step 1 — Read all task files** in `.claude/epics/<name>/`. Parse frontmatter for `status`, `depends_on`, `parallel`.

**Step 2 — Categorize tasks:**
- Ready: status=open, no unmet depends_on
- Blocked: has unmet depends_on
- In Progress: has an execution file — **reconcile before believing it** (Step 3)
- Complete: status=closed

**Step 3 — Reconcile anything in progress.** No agent survives a `/clear`, so an
in-progress stream in a fresh session is not running. It either finished without
recording the fact, or it stopped partway. For each one:

- Compare the stream file's `last_commit` with `git -C ../epic-<name> log`.
- **Commits after `last_commit`** — the file is behind. Bring it up to date from
  the log, then judge the stream on what is committed rather than on what it
  claimed.
- **No later commits, checkpoints outstanding** — the stream stopped partway.
  Relaunch it with the unticked checkpoints as its scope.
- **Every checkpoint ticked** — mark it `completed` and move on.

Say what you reconciled and why. A silently corrected record is how a record
stops being worth reading.

**Step 4 — Take one issue.** If anything survived reconciliation as unfinished,
that is the issue. Otherwise take the first ready one, in `next.sh` order. Name
what else is waiting so the user can see the queue — but do not launch it.

**Step 5 — Analyze it** if it has no analysis file, then follow *Starting an
Issue* above.

**Step 6 — Update** `.claude/epics/<name>/execution-status.md` with the issue
this session took and the issues still queued.

Running two issues at once is two sessions, in two terminals, against the same
epic worktree. It is not two issues in this one.

---

## Agent Coordination Rules

The rules an agent follows — read the law first, stay in scope, checkpoints
before code, commit then record, never `--force`, never auto-resolve a conflict
— are in the `vaca-stream` agent definition, which is what the agents actually
read. They are not restated here, so that there is one copy to keep current.

What stays with the main session:

- **Shared files belong to exactly one stream.** Types, config, `package.json`
  and their like are a scope decision, and scope is decided at launch. Streams
  cannot negotiate it between themselves at runtime.
- **Launch queued streams** as the streams they depend on finish.
- **Never relaunch a stream that is already running** — check `execution.md`
  first.
- **A blocked stream is yours to resolve.** It stopped precisely because the fix
  was outside the scope you gave it.
