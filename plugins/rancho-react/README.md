# rancho-react — React Law

The law for a React micro-frontend that the PrismOne shell mounts. It is short
because the checks carry the rest. The `rancho-react` template
(`templates/rancho-react/` in this repository) builds them into the tree, on the
organization's stack (Prismatic, VensureOne IAM, React Router, single-spa): its
lint rules, including type-aware rules that read each value's data class, a
check script, and reference code with the tests the law requires. The law
states what those checks and the reference code can't show.

`/rancho:law` governs, and this plugin needs `rancho` installed beside it. The
law reads only the OpenAPI document the server publishes.

## Layout

```
rancho-react/
├── .claude-plugin/plugin.json
└── skills/law/
    └── SKILL.md                 the React law
```

## Using it in a project

Install it at project scope, beside `rancho`:

```bash
/plugin install rancho-react@vensure-marketplace
```

Project scope enables it in the project's settings, which are committed:

```json
// .claude/settings.json
{
  "enabledPlugins": {
    "rancho-react@vensure-marketplace": true
  }
}
```

Then bind the tree that holds the React code, and list its reference code:

```
// <tree>/CLAUDE.md
Stack law: /rancho-react:law

## Reference

- Query and key factory: src/features/<feature>/api/
- ...
```

The template's own `CLAUDE.md` is a complete example. A tree that adopts the
law adopts the template's checks with it.

## Changing it

Fix the code first. Then ask whether a check, a better reference, or a
structural change prevents a repeat; ship that in the template. Only if none
does, add one sentence to the law, within its 2,500 words, bump the version and
add one changelog line. The pull request records why.
