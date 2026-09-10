# rancho — Vensure Dev Assistant

A Claude Code plugin carrying the engineering law and the development-process
tooling built around it.

## Layout

```
rancho/
├── .claude-plugin/plugin.json   manifest — the only file that goes in here
├── skills/                      skills as <name>/SKILL.md    [law, vaca]
│   ├── law/
│   │   ├── SKILL.md             the root law
│   │   └── references/          backend.md, frontend.md, enforcement.md
│   └── vaca/
│       └── SKILL.md             Vensure Agentic Code Assistant  [placeholder]
├── agents/                      subagent definitions, flat .md      [empty]
├── commands/                    skills as flat .md files           [empty]
├── output-styles/               output style definitions, .md      [empty]
├── workflows/                   workflow scripts, .js              [empty]
├── monitors/                    background monitors, monitors.json [empty]
├── themes/                      color themes, .json                [empty]
├── scripts/                     hook + utility scripts             [empty]
├── bin/                         executables added to PATH          [empty]
├── hooks/hooks.json             event handlers            [SessionStart]
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

## The law

`skills/law/` holds the engineering law. Only the frontmatter `description` is
always in context; the body loads when the skill triggers, and
`references/backend.md`, `references/frontend.md`, and
`references/enforcement.md` load when the law instructs Claude to read them.

Skills are model-invoked, which is a weaker guarantee than the law's own claim
that it "is not advisory". The `SessionStart` hook closes that gap: it states
that the law binds and that the skill must be read, and deliberately restates no
rules — so there is exactly one copy of every rule to keep current.

Consuming repositories should keep thin `CLAUDE.md` stubs, which restore the
subtree-triggered load that nested `CLAUDE.md` files give for free:

```
backend/CLAUDE.md   ->  "Backend law: references/backend.md in the rancho:law skill."
frontend/CLAUDE.md  ->  "Frontend law: references/frontend.md in the rancho:law skill."
```

`references/enforcement.md` is kept whole. Its rollout sections ("Prerequisites",
"Build order") are specific to the first adopting repository, but they are
cross-linked to the inventory by rule id and by an internal anchor, so splitting
the file breaks those references. Trim them when a second repository needs a
different inventory.

Changing a rule: change the law first, then bring the codebase into compliance.
Per §11, the document is never edited to accommodate code that violates it.
