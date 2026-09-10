# vensure-marketplace

A Claude Code plugin marketplace: engineering governance and delivery tooling
for .NET and React services.

## Requirements

The `law` skill needs nothing beyond Claude Code. `vaca` drives git and GitHub,
so it needs:

| | | |
|---|---|---|
| **git** | required | Epics run in a worktree per epic. |
| **[GitHub CLI](https://cli.github.com/)** (`gh`) | required | Creates and updates issues. Must be authenticated: `gh auth login`. |
| **`bash` and `awk`** | required | The tracking scripts. Present on macOS and Linux; on Windows use Git Bash, which ships with Git. |
| **[`gh-sub-issue`](https://github.com/yahsan2/gh-sub-issue)** | optional | Links tasks to their epic as GitHub sub-issues. Without it, tasks are created as plain issues. Install: `gh extension install yahsan2/gh-sub-issue` |

There is no setup step. VACA creates `.claude/prds/` and `.claude/epics/` in your
project as it needs them, starting with your first PRD.

Issue labels (`epic`, `task`, `feature`, `bug`, and one `epic:<name>` per epic)
are created on your GitHub repository the first time an epic syncs, so the
account running `gh` needs permission to create labels there.

## Install

```bash
/plugin marketplace add josefernandez-vensure/vensure-marketplace
```

```bash
/plugin install rancho@vensure-marketplace
```

Then run `/reload-plugins` if the install summary asks you to.

## Plugins

### rancho — Dev Assistant

Two skills, installed together but usable independently.

| Skill | Invoke | What it does |
|---|---|---|
| `law` | `/rancho:law` | The engineering law: module boundaries, API and error contracts, validation placement, data classification and egress, AI-agent permissions, observability. Backend (.NET/DDD), frontend (React), and the enforcement inventory load on demand. |
| `vaca` | `/rancho:vaca` | Vensure Agentic Code Assistant. Spec-driven delivery: PRD → epic → GitHub issues → parallel agents → shipped code. |

The two are meant to work together. VACA decides *what* gets built and in what
order; the law decides what the finished code must look like. Agents that VACA
launches read the law before writing anything.

A `SessionStart` hook states that the law is binding and points at the skill. It
deliberately restates no rules, so there is exactly one copy of each to keep
current.

`vaca` is driven in natural language — "create a PRD for X", "what's next",
"standup", "start working on issue 42". Run `/rancho:vaca` and ask for help for
the full card.

## Layout

```
vensure-marketplace/
├── .claude-plugin/
│   └── marketplace.json      the catalog — one entry per plugin
├── plugins/
│   └── rancho/
│       ├── .claude-plugin/plugin.json
│       ├── skills/law/       SKILL.md + references/
│       ├── skills/vaca/      SKILL.md + references/ + scripts/
│       ├── hooks/hooks.json
│       ├── output-styles/
│       └── ...               agents/, commands/, workflows/ etc. are
│                             scaffolded but empty
└── README.md
```

Plugins are independent. Each is installed and enabled on its own, owns its own
version, and shares nothing with its neighbours at runtime — the marketplace is
a catalog, not a container.

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
`"source": "<name>"`. That form needs Claude Code v2.1.239 or later; the
explicit `./plugins/<name>` path works on every version, which is why the
entries here spell it out.

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
shadows an installed one of the same name for that session, so you can iterate
on a plugin you already have installed.

Shell scripts and awk programs are forced to LF by `.gitattributes`. Keep it
that way — a CRLF shebang fails on Linux and macOS with `bad interpreter`, and
this repo is cloned onto machines that are not Windows.

## Licence

MIT — see [LICENSE](LICENSE).

The `vaca` skill is derived from [CCPM](https://github.com/automazeio/ccpm)
by Ran Aroussi, also MIT. That attribution and the original notice are in
[NOTICE](NOTICE), as the licence requires.

Issues and pull requests welcome.
