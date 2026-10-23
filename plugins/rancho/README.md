# rancho — Vensure Dev Assistant

A Claude Code plugin carrying the core engineering law and the
development-process tooling built around it. It is the same in every project.
The technology-specific half of the law is in the stack-law plugins beside it -
`rancho-dotnet`, `rancho-react` - which each project enables for itself.

## Layout

```
rancho/
├── .claude-plugin/plugin.json   manifest — the only file that goes in here
├── skills/                      skills as <name>/SKILL.md    [law, vaca]
│   ├── law/
│   │   ├── SKILL.md             the core law
│   │   └── references/          enforcement.md: the core inventory and records
│   └── vaca/
│       ├── SKILL.md             Vensure Agentic Code Assistant
│       ├── references/          one file per phase, plus conventions.md
│       └── scripts/             tracking reports, lib/ holds the awk readers
├── agents/                      subagent definitions, flat .md [vaca-stream]
├── commands/                    skills as flat .md files            [help]
├── output-styles/               output style definitions, .md [asd-ste100]
├── workflows/                   workflow scripts, .js              [empty]
├── monitors/                    background monitors, monitors.json [empty]
├── themes/                      color themes, .json                [empty]
├── scripts/                     hook scripts, lib/ holds their shared state
├── bin/                         executables added to PATH          [empty]
├── hooks/hooks.json             event handlers             [four events]
├── context/prime-directive.md   payload the SessionStart hook prints
├── .mcp.json                    MCP servers                        [empty]
├── .lsp.json                    LSP servers                        [empty]
└── settings.json                default settings when enabled      [empty]
```

Empty directories are held by `.gitkeep`. The three JSON stubs are valid-but-empty
so the loader parses them cleanly; delete one rather than leaving malformed JSON.

**Nothing but `plugin.json` goes inside `.claude-plugin/`.** Every component
directory sits at the plugin root.

## What goes where

### skills/ — instructions loaded into context

`skills/<name>/SKILL.md`, frontmatter `description` plus a Markdown body.
Invoked as `/rancho:<name>`. The `description` is the only part always
in context; it decides whether the skill triggers, so write it as trigger
conditions, not as a summary. Bulk content goes in sibling files that the body
tells Claude to read.

This is the only way a plugin ships instructions — a `CLAUDE.md` at the plugin
root is **not** loaded as project context.

### agents/ — subagents

Flat `.md` files with frontmatter (`name`, `description`, `tools`, `model`).
They appear in `/context` under Custom Agents and are @-mentionable.

Note: project and user `.claude/agents/` definitions **override** same-named
plugin agents.

### commands/ — flat-file skills

The older shape. Use `skills/` for anything new; `commands/` exists mainly for
migration from `.claude/commands/`.

### output-styles/ — response shaping

```yaml
---
name: terse
description: What this style does
when: always            # or on-skill-invoke:<skill-name>
---
```

`when` defaults to `always`, which applies the style whenever the plugin is
enabled. Prefer `on-skill-invoke:<skill-name>` unless you really do mean to
change every response. Plugin-shipped styles are read-only; users copy them into
`~/.claude/output-styles/` to edit.

### hooks/hooks.json — event handlers

Same shape as the `hooks` object in `settings.json`. Hook input arrives as JSON
on stdin (use `jq` to pick fields). Reference bundled scripts with
`${CLAUDE_PLUGIN_ROOT}`.

Stdout is added to Claude's context for `SessionStart`, `UserPromptSubmit`,
`UserPromptExpansion`, and `PostModelSwitch` — for every other event it is not.
The `SessionStart` hook here depends on that.

### .mcp.json — MCP servers

```json
{
  "mcpServers": {
    "example": {
      "command": "${CLAUDE_PLUGIN_ROOT}/servers/db-server",
      "args": ["--config", "${CLAUDE_PLUGIN_ROOT}/config.json"],
      "env": { "DB_PATH": "${CLAUDE_PLUGIN_DATA}/data" }
    }
  }
}
```

Servers start automatically when the plugin is enabled. `${CLAUDE_PLUGIN_ROOT}`,
`${CLAUDE_PLUGIN_DATA}`, and `${CLAUDE_PROJECT_DIR}` substitute in `command`,
`args`, `env`, and — for HTTP/SSE/WebSocket servers — `url`, `headers`, and
`headersHelper`.

### .lsp.json — language servers

```json
{
  "csharp": {
    "command": "csharp-ls",
    "extensionToLanguage": { ".cs": "csharp" }
  }
}
```

`command` and `extensionToLanguage` are required. Optional: `args`, `transport`,
`env`, `initializationOptions`, `settings`, `workspaceFolder`, `startupTimeout`,
`shutdownTimeout`, `restartOnCrash`, `maxRestarts`, `diagnostics`.

Users must have the server binary installed. A server that fails to start shows
in `/plugin` under Errors; an invalidly configured entry is silently skipped
instead, so use `claude --debug` when one goes missing without explanation.

Check the official marketplace first — TypeScript, Python, and Rust already have
prebuilt LSP plugins.

### monitors/monitors.json — background watchers

```json
[
  { "name": "build", "command": "tail -F ./logs/build.log", "description": "Build log" }
]
```

`name`, `command`, and `description` are required; `when` is `always` or
`on-skill-invoke:<skill-name>`. Each stdout line reaches Claude as a
notification. Interactive CLI sessions only, and unsandboxed at hook trust level.

### settings.json — defaults applied when enabled

Only `agent` and `subagentStatusLine` are honored; unknown keys are ignored.
`{"agent": "name"}` makes one of this plugin's agents the main thread — its
system prompt, tool restrictions, and model. That is a large behavioral change,
so treat it as a deliberate choice rather than a convenience.

## Two things that will bite

**Manifest path fields are not uniform.** `skills` *adds* to the default
`skills/` scan. `commands`, `agents`, `workflows`, and `outputStyles` *replace*
their defaults. This manifest declares no component paths at all, so everything
loads from its default location — keep it that way unless you specifically want
to override.

**`themes/` and `monitors/` are inert until declared.** They load only when the
manifest names them:

```json
"experimental": { "themes": "./themes/", "monitors": "./monitors/monitors.json" }
```

Both are experimental. The directories are placeholders; wire them up when they
have content, not before — pointing `experimental.monitors` at a file that does
not exist is a load error.

Also worth knowing: **`bin/` cannot be included** in a plugin distributed through
claude.ai organization settings. If this ships to the org that way, executables
have to reach machines by some other route.

## Configuration

`userConfig` in the manifest prompts users for values, available as
`${user_config.KEY}` inside MCP, LSP, skill, and agent content. Mark secrets
`sensitive: true` so they land in secure storage. Shell commands — hooks and
monitors — cannot use `${user_config.*}`; they read `CLAUDE_PLUGIN_OPTION_<KEY>`
from the environment instead.

## Develop and test

```bash
claude --plugin-dir ./plugins/rancho     # load for one session
claude plugin validate ./plugins/rancho  # check structure
```

`/reload-plugins` picks up edits without restarting. Verify components
individually: skills via `/rancho:<name>`, agents in `/context`, hooks
by triggering their event and reading the debug log, MCP and LSP servers in
`/plugin` under Errors.

## The session gate

VACA runs one issue per session. Four hooks enforce that, and `scripts/` holds
them plus the state they share:

| Event | Matcher | Script | What it does |
|---|---|---|---|
| `PreToolUse` | `Task` | `stream-open.sh` | Records a launch, if the payload names `vaca-stream` |
| `SubagentStop` | `.*vaca-stream.*` | `stream-close.sh` | Records the finish, and arms the lock once the counts balance |
| `UserPromptSubmit` | — | `session-gate.sh` | Exits 2 while the lock is armed: the prompt is blocked and erased, and stderr is shown to the user |
| `SessionStart` | — | `session-reset.sh` | Releases the lock, then injects the project's current VACA state |

Why those four:

- **`UserPromptSubmit` is the only event that can stop a session continuing.**
  `Stop` does the opposite — blocking it makes Claude carry on rather than stop.
- **No hook can run `/clear`.** Slash commands are client-side. The gate can
  refuse to work until the user clears; it cannot clear for them.
- **Counting launches and finishes** — rather than asking the model to report
  them — is what makes "this issue is done" a fact the harness knows. It is also
  why streams must launch as `subagent_type: "vaca-stream"`: the matcher is how
  a work stream is told apart from a bookkeeping subagent.
- **Arming only when the counts balance** is what keeps parallel streams
  working. The first of three to finish must not lock the session the other two
  are still running in.

State lives in `${CLAUDE_PLUGIN_DATA}/vaca-sessions/<session-id>.streams` and
`.lock`, falls back to the temp directory when that variable is unset, and ages
out after seven days. Nothing is written into the project tree.

Two escape hatches, both deliberate: any prompt beginning with `/` passes, or
`/clear` itself could be swallowed by the gate; and a prompt containing
`VACA OVERRIDE` lifts the lock outright.

## The plugin guard

The plugin is installed globally. An edit under `~/.claude/plugins/cache/` or
`~/.claude/plugins/marketplaces/` changes every project on the machine that
loads it, and the next update silently discards the edit. Claude has tried to
make such an edit when asked to change the law, so one more hook refuses it:

| Event | Matcher | Script | What it does |
|---|---|---|---|
| `PreToolUse` | `Edit\|Write\|MultiEdit\|NotebookEdit\|Bash\|PowerShell` | `plugin-guard.sh` | Exits 2 on a write into a protected root: the tool call is blocked and stderr tells Claude why |

- **File tools are checked exactly.** Only the target path is read, never the
  content, so documenting the protected roots — this section — is not a write
  into them.
- **Shell commands are checked on a best-effort basis.** A command is refused
  when it names a protected root and also carries a write verb (`rm`, `sed -i`,
  `Set-Content`, `git checkout`, …) or an output redirect. Redirects to
  `/dev/null` and `$null` do not count, so reading the installed plugin stays
  allowed. A shell command cannot be parsed with `grep`, so a determined
  enough command gets through; the prime directive states the rule for that
  case.
- **`CLAUDE_CONFIG_DIR` is honoured.** A relocated config directory is protected
  as well as the default one.
- **Plugin data is not protected.** `~/.claude/plugins/data/` is where
  `${CLAUDE_PLUGIN_DATA}` points, and the session gate writes there.

`claude --plugin-dir` loads the plugin from this repository, not from the
cache, so the guard never gets in the way of developing it.

These scripts read their JSON input with `grep`, not `jq`. `jq` is not a
dependency of this plugin and is not present on every machine that installs it,
and every field they need is a flat string. A requirement more structured than
that is a reason to take the dependency, not to write a longer regex.

## The law

`skills/law/` holds the core engineering law: what must be true in every tree,
whatever it is built with. How one technology satisfies it is a **stack law**,
shipped as a plugin of its own - `rancho-dotnet` (role `server`) and
`rancho-react` (role `client`) today. Core §2 states the contract every stack
law meets: a role, a core map from each core section to the section carrying
it, a gap list, Stack, Testing, and Verification sections, and its own
enforcement inventory.

Only the frontmatter `description` is always in context; the body loads when
the skill triggers, and `references/enforcement.md` loads when the law
instructs Claude to read it. A stack law loads the same way, from its own
plugin, and only in a project that enables that plugin.

Skills are model-invoked, which is a weaker guarantee than the law's own claim
that it "is not advisory". The `SessionStart` hook closes that gap: it states
that the law binds and that the skill must be read, and deliberately restates no
rules — so there is exactly one copy of every rule to keep current.

### Binding a project

A project chooses its stack laws; VACA and the core law do not change. Two
steps, both committed to the project:

1. Enable the stack-law plugins it builds with, in `.claude/settings.json`:

   ```json
   {
     "enabledPlugins": {
       "rancho-dotnet@vensure-marketplace": true,
       "rancho-react@vensure-marketplace": true
     }
   }
   ```

2. Bind each tree with one line in the `CLAUDE.md` at its root. A nested
   `CLAUDE.md` loads when Claude reads files beneath it, so the binding arrives
   with the tree:

   ```
   backend/CLAUDE.md   ->  Stack law: /rancho-dotnet:law
   frontend/CLAUDE.md  ->  Stack law: /rancho-react:law
   ```

   A single-tree repository binds its root `CLAUDE.md`. A repository with two
   servers on different stacks binds each tree to its own.

An unbound tree, or one bound to a stack law the session does not have, is a
defect the law tells Claude to stop on and report, never to guess past.

### Adding a stack law

A new technology is a new plugin, `plugins/rancho-<stack>/`, holding
`skills/law/SKILL.md` and `skills/law/references/enforcement.md`. It declares a
role, maps every core section, lists its gaps, carries every core rule its
role binds under **Core rules carried**, and takes an id prefix of its own.
`rancho-dotnet` and `rancho-react` are the worked examples. It depends on the
core law and never on another stack law, so either tree of a project can change
stack without touching the other.

### The enforcement inventories

The core inventory, `references/enforcement.md`, lists the `ROOT` rows and the
role each binds, and holds every record that concerns the core law or more than
one stack law. Each stack law's inventory holds its own rows, the core rows its
role carries with their mechanism and check, and the records that concern it
alone. Ids are permanent across the split.

Changing a rule: change the law first, then bring the codebase into compliance.
Per §11, the document is never edited to accommodate code that violates it. A
stack-law rule is changed in its own plugin; a rule every stack law of a role
would need is changed here.
