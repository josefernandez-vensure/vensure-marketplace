# Rancho — Modular .NET Law

A minimal law for .NET modular monoliths. It states the structure and the few decisions that differ from a sensible default. Everything else comes from the project's own code: the reference implementations listed in the tree's `CLAUDE.md`.

The law grows only from evidence. When Claude gets something wrong in a real change, fix the code first. Add a rule only when the mistake is something code can't show, or something Claude repeats despite the reference.

## Install

```
/plugin install rancho-modular-dotnet --marketplace josefernandez-vensure/vensure-marketplace
```

Use it with `rancho` 4.0.0 or later. Earlier versions don't recognise a standalone law, and read the core law in its trees as well.

## Binding a tree

Put this in the `CLAUDE.md` at the root of the .NET tree:

```markdown
Stack law: /rancho-modular-dotnet:law

## Reference

- Command: src/Modules/<Module>/<Module>.Application/<Feature>/
- Query: src/Modules/<Module>/<Module>.Infrastructure/Queries/<Feature>/
- Endpoint: src/Api/Endpoints/<Feature>/
- Reaction: src/Modules/<Module>/<Module>.Application/<Reaction>/
- Saga orchestrator: src/Modules/<Module>/<Module>.Domain/<Process>/
- Integration test: tests/<Module>.IntegrationTests/<Feature>/
```

Start with an empty `## Reference` section. Each entry is added when the first implementation of that kind has been built from the law and reviewed (law §0).

## What the plugin carries

- `skills/law/SKILL.md`: the law.
