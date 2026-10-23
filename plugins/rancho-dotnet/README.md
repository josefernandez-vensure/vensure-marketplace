# rancho-dotnet — .NET Stack Law

The stack law for a .NET server tree. It carries the core engineering law in
`rancho` (`/rancho:law`) in .NET: DDD modules and project references, EF Core
persistence, authorization, data classification, AI agents, quotas,
observability, configuration, real-time, and the API layer.

It is a stack law, not a law of its own. `/rancho:law` governs, and this plugin
needs `rancho` installed beside it.

## Layout

```
rancho-dotnet/
├── .claude-plugin/plugin.json
└── skills/law/
    ├── SKILL.md                 the stack law: role server, core map, §1–§16
    └── references/
        └── enforcement.md       every ⚙ here, and the core rules this role carries
```

Section numbers are those of `backend.md` in rancho 1.7.0, which this file was,
plus §16 Verification. Records written before 2.0.0 cite them unchanged.

## Using it in a project

Install it at project scope, beside `rancho`:

```bash
/plugin install rancho-dotnet@vensure-marketplace
```

Project scope enables it in the project's settings, which are committed:

```json
// .claude/settings.json
{
  "enabledPlugins": {
    "rancho-dotnet@vensure-marketplace": true
  }
}
```

Then bind the tree that holds the .NET code:

```
// <tree>/CLAUDE.md
Stack law: /rancho-dotnet:law
```

A tree is the directory at whose root that `CLAUDE.md` sits. Core §2 states the
binding rules: one stack law per tree, and an unbound tree stops work.

## Changing it

Under core §11: change the rule here first, then bring the codebase into
compliance. A rule every server stack law would need belongs in the core law,
not here. A new ⚙ gets a `BE-` row in `references/enforcement.md`; the prefix
belongs to this plugin.
