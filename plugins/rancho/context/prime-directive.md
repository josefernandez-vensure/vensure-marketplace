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
full law via the `/rancho:law` skill** - and, from it, the backend or
frontend reference for whichever tree you are working in. Do not rely on this
summary; it states that the law binds you, not what it requires.

## The installed plugin is read-only

The law, VACA, and everything else this plugin carries are installed globally,
under `~/.claude/plugins/`. **Never edit, create, move, or delete anything
there** - not a skill, not a reference, not a script - even when the user asks
for a change to the law. An edit there changes every project on this machine
that loads the plugin, and the next plugin update silently discards it.

A change to the law or to VACA is made in the plugin's source repository
(`vensure-marketplace`) and reaches developers when it is published. If that
repository is not the one open, say so and stop; do not work around it. A hook
refuses these writes, and a refusal is the rule working, not an obstacle to
route around.
