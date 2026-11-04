# Authoring the law

What a law maintainer needs and a coding agent does not: how a rule is written, what every stack law carries, how the inventories are kept, and how the law changes. It binds as the core law does; it was moved, word for word, out of `skills/law/SKILL.md` in 3.0 stage 1, under the core law's section numbers. The last sentence of §1 below, and the paragraph of §2 on what a stack law may add, are also stated in the core law, which a coding agent reads. In it, "this document" and "here" mean the core law, "this skill" is `skills/law/`, and §N is §N of the core law.

## §1 Rule Language

A rule states the class of thing it governs, never a list of that class's members. Members MAY be named, as illustrations written as such - "such as", "for example", "or any other" - and never as the boundary. This binds hardest on a `⚙` rule: a machine-checked rule is read as exhaustive, so one that enumerates grants every member it forgot to name, and its check then certifies that reading. What a mechanism actually reaches is recorded in an enforcement inventory (§10), where a shortfall is a known gap rather than a permission.

## §2 Stack Laws

Every stack law MUST carry:

- **Role** and **Core map**, at the top: for each section of this document, the section of the stack law that carries its mechanism. A section the stack law cannot yet carry is listed as a gap. A gap is a defect in the stack law, never an exemption from the core.
- **Stack**: the technologies it governs.
- **Testing**: the required tests that carry its unmarked rules (§1).
- **Verification**: the commands that MUST pass before a change in its tree is complete (§10).
- An enforcement inventory, `references/enforcement.md` in its own skill, listing every `⚙` it carries and every core rule its role binds.

A stack law MAY add rules. It MUST NOT weaken one stated here. A stack law that contradicts this document is the defect, exactly as code that contradicts it is.

A stack law MUST NOT depend on another stack law. The two trees of a project share only what this document states - the API contract, the data classes, and the correlation id - so either can be replaced without touching the other.

## §10 Verification

Every `⚙` rule and the mechanism that enforces it is inventoried. The core law's rules are in `references/enforcement.md`, in this skill's own directory; each stack law's are in its own. A `⚙` with no entry is unaccounted for and MUST be added. An entry whose check is not yet built is a known gap in enforcement - it does not make the rule optional.

A core rule binds every stack law of the role it names, and each of them MUST carry the rule's check in its own inventory. A stack law that cannot is not compliant with this document, and the rule does not become optional in its trees.

## §11 Changing the Law

This document is not edited to accommodate code that violates it. To change a rule: change it here first, then bring the codebase into compliance in the same change. Until the law says otherwise, a rule the codebase no longer satisfies is a defect in the codebase.

A stack law is changed the same way, in its own plugin. A rule that one stack law needs and every stack law of its role would need too belongs here, not in the stack law that found it.
