# rancho-dotnet — .NET Law

The law for a .NET modular monolith: each module a bounded context, treated as a
separate service. It is short because the checks carry the rest. The
`rancho-dotnet` template (`templates/rancho-dotnet/` in this repository) builds
them into the tree: the reference table by project kind, banned APIs, analyzers
as errors, and the architecture, host and integration tests. The law states what
those checks and the tree's reference code can't show.

`/rancho:law` governs, and this plugin needs `rancho` installed beside it.

## Layout

```
rancho-dotnet/
├── .claude-plugin/plugin.json
└── skills/law/
    └── SKILL.md                 the .NET law
```

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

Then bind the tree that holds the .NET code, and list its reference code:

```
// <tree>/CLAUDE.md
Stack law: /rancho-dotnet:law

## Reference

- Command: src/Modules/<Module>/<Module>.Application/<Feature>/
- ...
```

The template's own `CLAUDE.md` is a complete example. A tree that adopts the
law adopts the template's checks with it.

## Changing it

Fix the code first. Then ask whether a check, a better reference, or a
structural change prevents a repeat; ship that in the template. Only if none
does, add one sentence to the law, within its 2,500 words, bump the version and
add one changelog line. The pull request records why.
