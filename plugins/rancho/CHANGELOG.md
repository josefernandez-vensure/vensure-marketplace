# Changelog

## 1.2.0

- One issue per session, enforced rather than encouraged. Four hooks:
  `PreToolUse` counts each `vaca-stream` launch, `SubagentStop` counts each
  finish and arms a lock when the counts balance, `UserPromptSubmit` refuses
  every prompt while the lock is armed, and `SessionStart` releases it.
  `UserPromptSubmit` is the only event that can stop a session continuing —
  blocking `Stop` makes Claude carry on instead — and no hook can run `/clear`,
  so the gate refuses work until the user does. Arming only once the counts
  balance is what keeps parallel streams working: the first of three to finish
  must not lock the session the other two are still running in. Escape hatches:
  any prompt beginning with `/`, and `VACA OVERRIDE`.
- `vaca-stream` agent. Streams launched as `general-purpose` were
  indistinguishable from task-creation and sync batches, so the end of an
  issue's work could not be identified from the harness at all. The agent also
  holds the standing rules that `execute.md` was duplicating into every launch
  prompt — law first, scope discipline, commit format, never `--force` — which
  is where the agents actually read them.
- Work stream files are checkpointed. An agent writes its checkpoint list before
  its first line of code, then commits and records one checkpoint at a time.
  `last_commit` names the newest commit the file accounts for, so its claims can
  be checked against `git log` rather than believed. Schema in `conventions.md`.
- `SessionStart` hands every session the project's state — in progress, next,
  blocked — before the user types. Clearing is only cheap if the next session
  starts oriented, and VACA already keeps everything in files.
- Resuming is defined. "continue the `<feature>` epic" reconciles in-progress
  streams before acting: no agent survives a `/clear`, so an in-progress stream
  in a fresh session either finished without recording or stopped partway. Those
  two were previously indistinguishable, which made a stranded stream something
  no later session would ever pick up.
- "Start the epic" takes the next ready issue instead of launching every ready
  issue at once. Cross-issue parallelism is now more sessions rather than one
  longer one.
- `conventions.md` states that code is committed in the worktree while all
  `.claude/` state is written at the project root. State written into the
  worktree would have been invisible to every report and every later session
  until the epic merged.

## 1.1.1

- `sync.md` split by operation. It was 308 lines against ~105 for the other
  phases, because it carried five distinct operations. It is now a router
  holding the shared repository safety check, pointing at `sync-epic.md`,
  `sync-issue.md`, `sync-close.md`, `sync-merge.md` and `sync-bug.md`. Reading
  the router plus one operation costs 339–428 lines where the whole file cost
  567.
- Every phase reference now names `conventions.md`. Previously only `SKILL.md`
  did, so a phase read on its own had no pointer to the frontmatter schemas,
  path standards or datetime rule. `track.md` states the exception: its scripts
  parse frontmatter themselves.

## 1.1.0

- `/rancho:help` command. `help.sh` is a reference card for a person, but the
  only way to reach it was to ask the model to run it. The command gives it a
  front door that survives version bumps, unlike the versioned install path.
- Removed `init.sh`. It created three directories VACA never uses and missed
  one it does, carried a dead copy block, wrote a boilerplate `CLAUDE.md` that
  competed with the `law` skill, and exited before doing anything on Windows.
  The phase references already create directories on demand.
- Epic sync now creates the labels it applies. `gh issue create --label` fails
  outright on a label that does not exist, and nothing created `feature`, `bug`
  or `epic:<name>` — so sync was already broken on a fresh repository.
- ASD-STE100 output style scoped to `on-skill-invoke:vaca` rather than
  applying to every response.

## 1.0.0

- Engineering law shipped as the `law` skill: root law in `SKILL.md`, backend,
  frontend, and enforcement inventory as on-demand references.
- `SessionStart` hook injects the prime directive and points at the skill.
- VACA shipped as the `vaca` skill: five phases, 13 tracking scripts, and four
  awk readers that parse frontmatter in exactly one place per entity type.
- Scaffolded the remaining plugin component directories.
