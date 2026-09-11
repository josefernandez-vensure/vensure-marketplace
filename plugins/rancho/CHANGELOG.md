# Changelog

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
