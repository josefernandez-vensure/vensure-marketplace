# Engineering Law is in force in this repository

This repository is governed by an engineering law. It is not advisory.

- The law defines what MUST be true. The codebase defines only how the law is
  expressed here: naming, layout, ordering, phrasing.
- **When the codebase contradicts the law, the law wins.** Report the
  contradiction as a defect. Existing code is never permission to repeat a
  violation.
- When the law is silent, follow the codebase's nearest precedent, confirmed
  by a second occurrence: never generalize a rule from a single example. With
  fewer than two, or when both are silent, ask.
- **When two rules, or a rule and a recorded decision, leave no compliant
  answer, stop and ask.** A comment arguing that a rule does not apply is a
  defect, never an exemption; the one exemption is an entry in
  `LAW-EXCEPTIONS.md` (core §0, §7).
- MUST / MUST NOT is absolute. SHOULD / SHOULD NOT is the default and a
  deviation requires a one-line justification. MAY needs no justification.
  Silence is not permission. What a rule marked with the gear glyph means is
  stated in core §1.

**First, find the law bound to the tree you are working in.** The nearest
`CLAUDE.md` at or above the files that holds a `Stack law:` line names it, as
`Stack law: /<plugin>:law`. If that law says, before its first section,
`This law stands alone.`, read it in full and nothing else: not the core law,
the contract law, or any annex or inventory. The rules above are the core
law's and give way to that law's own. The rest of this section does not apply
to that tree.

**Otherwise, before writing, placing, or reviewing any code in this repository, read the
full core law via the `/rancho:law` skill** - and then, in full, the contract
law beside it (`references/contract.md`) wherever the bound stack law declares
a role, and the stack law bound to the tree you are working in. The nearest `CLAUDE.md` at or above the
files that holds a `Stack law:` line names it, as `Stack law: /<plugin>:law`
(core §2). A tree bound as
`Stack law: none - <reason>` is not a defect: only the core law binds it, so
read only the core law there. Otherwise, read each annex of that
stack law whose trigger the tree already meets, or the work would meet:
annexes are the `references/annex-*.md` files beside the stack law's
`SKILL.md`, and each names its trigger at its top and in the stack law. Consult an enforcement
inventory (`references/enforcement.md`) only by rule id, never whole.

A tree with no binding, or one naming a stack law this session does not have,
is a defect: stop and say so, and never choose a stack law by guessing from
the files. Do not rely on this summary; it states that the law binds you, not
what it requires.

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
