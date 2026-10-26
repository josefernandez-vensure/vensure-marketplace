# Engineering Law is in force in this repository

This repository is governed by an engineering law. It is not advisory.

- The law defines what MUST be true. The codebase defines only how the law is
  expressed here: naming, layout, ordering, phrasing.
- **When the codebase contradicts the law, the law wins.** Report the
  contradiction as a defect. Existing code is never permission to repeat a
  violation.
- When the law is silent, follow the codebase. When both are silent, ask.
- MUST / MUST NOT is absolute. SHOULD / SHOULD NOT is the default and a
  deviation requires a one-line justification. MAY needs no justification.
  Silence is not permission.
- Rules marked with the gear glyph are machine-checked: violating one is a
  build failure, not a review comment.

**Before writing, placing, or reviewing any code in this repository, read the
full core law via the `/rancho:law` skill** - and then, in full, the stack law
bound to the tree you are working in. The nearest `CLAUDE.md` at or above the
files names it, as `Stack law: /<plugin>:law`. A tree with no binding, or one
naming a stack law this session does not have, is a defect: stop and say so,
and never choose a stack law by guessing from the files. Do not rely on this
summary; it states that the law binds you, not what it requires.

## The installed plugins are read-only

The core law, the stack laws, VACA, and everything else these plugins carry
are installed globally, under `~/.claude/plugins/`. **Never edit, create,
move, or delete anything there** - not a skill, not a reference, not a script -
even when the user asks for a change to the law. An edit there changes every
project on this machine that loads the plugin, and the next plugin update
silently discards it.

A change to the core law, a stack law, or VACA is made in the plugins' source
repository (`vensure-marketplace`) and reaches developers when it is
published. If that repository is not the one open, say so and stop; do not
work around it. A hook refuses these writes, and a refusal is the rule
working, not an obstacle to route around.
