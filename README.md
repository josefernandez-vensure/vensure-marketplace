# vensure-marketplace

Vensure's internal Claude Code plugin marketplace.

## Install

```bash
/plugin marketplace add josefernandez-vensure/vensure-marketplace
/plugin install rancho@vensure-marketplace
```

The repository is private, so `git` has to be able to authenticate as the user
running the command — an SSH key or a `gh auth login` session.

## Plugins

| Plugin | What it is |
|---|---|
| [`rancho`](plugins/rancho) | Vensure Dev Assistant — VACA (Vensure Agentic Code Assistant) and the engineering law governing .NET and React service work. |

## Layout

```
vensure-marketplace/
├── .claude-plugin/
│   └── marketplace.json      the catalog — one entry per plugin
├── plugins/
│   └── rancho/
│       ├── .claude-plugin/
│       │   └── plugin.json   each plugin has its own manifest
│       ├── skills/
│       │   ├── law/
│       │   └── vaca/
│       └── ...
└── README.md
```

Plugins are independent. Each is installed and enabled on its own, owns its own
version, and shares nothing with its neighbors at runtime — the marketplace is a
catalog, not a container.

## Adding a plugin

1. Create `plugins/<name>/` with a `.claude-plugin/plugin.json` manifest.
2. Add an entry to `.claude-plugin/marketplace.json`:

   ```json
   {
     "name": "<name>",
     "source": "./plugins/<name>",
     "description": "One line on what it does",
     "version": "1.0.0",
     "author": { "name": "Vensure Engineering" }
   }
   ```

   `source` paths resolve relative to this repository root and must start with
   `./`. Never use `../` — anything outside the marketplace root is rejected.

3. Validate both levels, then commit:

   ```bash
   claude plugin validate ./plugins/<name>
   ```

`metadata.pluginRoot` is set to `./plugins`, so entries may instead use a bare
`"source": "<name>"`. That form needs Claude Code v2.1.239 or later; the explicit
`./plugins/<name>` path works on every version, which is why the entries here
spell it out.

## Renaming or removing a plugin

Users have the old name pinned in their settings, so record the change in the
`renames` map in `marketplace.json` — former name to current name, or `null` if
the plugin is gone:

```json
"renames": { "old-name": "new-name", "retired-plugin": null }
```

Skipping this leaves installed copies orphaned rather than migrated.

## Versioning

A plugin's `version` pins it: users receive an update only when that string
changes. Bump it in **both** `plugins/<name>/.claude-plugin/plugin.json` and the
marketplace entry, or installs will disagree about what is current.

## Local development

Test a plugin without installing it:

```bash
claude --plugin-dir ./plugins/rancho
```

`/reload-plugins` picks up edits without a restart. A `--plugin-dir` plugin
shadows an installed one of the same name for that session, so you can iterate on
a plugin you already have installed.
