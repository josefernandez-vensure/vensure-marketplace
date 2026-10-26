# rancho-react — React Stack Law

The stack law for a React client tree. It carries the core engineering law in
`rancho` (`/rancho:law`) in React: structure and feature boundaries, generated
API types, server and client state, forms, components and styling, routing,
authentication, errors, data boundaries in the browser, real-time, and testing.

It is a stack law, not a law of its own. `/rancho:law` governs, and this plugin
needs `rancho` installed beside it. It assumes nothing about the server's stack:
it reads only the OpenAPI document the server publishes.

## Layout

```
rancho-react/
├── .claude-plugin/plugin.json
└── skills/law/
    ├── SKILL.md                 the stack law: role client, core map, §1–§14
    └── references/
        └── enforcement.md       every ⚙ here, and the core rules this role carries
```

Section numbers are those of `frontend.md` in rancho 1.7.0, which this file was.
Records written before 2.0.0 cite them unchanged.

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

Then bind the tree that holds the React code:

```
// <tree>/CLAUDE.md
Stack law: /rancho-react:law
```

A tree is the directory at whose root that `CLAUDE.md` sits. Core §2 states the
binding rules: one stack law per tree, and an unbound tree stops work.

## Changing it

Under core §11: change the rule here first, then bring the codebase into
compliance. A rule every client stack law would need belongs in the core law,
not here. A new ⚙ gets an `FE-` row in `references/enforcement.md`; the prefix
belongs to this plugin.
