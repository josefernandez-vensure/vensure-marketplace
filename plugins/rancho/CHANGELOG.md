# Changelog

## 5.0.1

- **§5 *Idempotency* says which precondition a replay precedes**: the comparison of the version `If-Match` names with the resource's. A check of the header's own shape (`428` for none or `*`, `412` for a tag that can never match) may come first, as validation does: a replay repeats the original request, so it passes that check whenever the original did. The template already works this way. No upgrade step.

## 5.0.0

- **Breaking.** The core law is rewritten to 1,500 words or fewer: one reading rule (core then the tree's bound law), access, data classes and egress, observability and audit, and the HTTP contract, merged in from `references/contract.md`, which is gone. Roles, standalone laws, annexes, the `⚙` mark, rule ids, `LAW-EXCEPTIONS.md` and `ENFORCEMENT-STATUS.md` are gone: a suppression carries its reason, and a disagreement with the law is an issue on this repository. Released with `rancho-dotnet` 4.0.0 and `rancho-react` 3.0.0.
- The prime directive is five lines, printed only in a repository whose `CLAUDE.md` binds a law; the read-only rule for the installed plugins prints everywhere. VACA, `vaca-stream` and VACA's help point at core §1 instead of restating the reading rule.
- The 4.x records, ledgers, enforcement inventories, `AUTHORING.md` and `tools/` are removed; they are under the `law-archive-4.1` tag. The checks in `templates/` are the inventory, and each CHANGELOG starts at this release.
- **Upgrade:** follow `UPGRADE-5.0.md` at the repository root.

Earlier versions are under the `law-archive-4.1` tag.
