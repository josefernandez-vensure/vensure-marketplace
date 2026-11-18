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
├── agents/law-reviewer.md       read-only review against the rules no check can see
├── hooks/hooks.json             Stop: build and architecture tests before a turn ends
├── scripts/verify-checks.sh     the Stop hook
└── skills/law/
    └── SKILL.md                 the .NET law
```

Before a turn ends, the Stop hook builds each tree holding `build/ProjectKinds.targets`
that has uncommitted code changes, and runs its architecture tests. A failure goes
back to Claude to fix, once per turn; a turn that changed no code costs nothing. The
integration tests need Docker and minutes, so they stay with `dotnet test` and CI.

The `law-reviewer` agent reviews a change against the judgment rules of the law
(transactions and sagas, migrations safe for the previous release, egress, the
tests each operation needs) and ignores what the build already enforces. Run it
once a change builds and its tests pass.

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

The template ships as a NuGet template package with the same version as this
plugin, so a service records which checks it started from. From this
repository's root:

```bash
dotnet pack templates/pack -o templates/pack/dist
dotnet new install templates/pack/dist/Vensure.Rancho.Templates.<version>.nupkg
dotnet new rancho-dotnet -n MyService
```

A service made from an earlier version keeps its copy of the checks. Each
changelog entry says which checks changed, so the service can take them over.

## Changing it

Fix the code first. Then ask whether a check, a better reference, or a
structural change prevents a repeat; ship that in the template. Only if none
does, add one sentence to the law, within its 2,500 words, bump the version and
add one changelog line. The pull request records why.
