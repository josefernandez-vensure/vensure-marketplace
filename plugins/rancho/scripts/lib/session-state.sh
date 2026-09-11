#!/bin/bash
# Shared state for the one-issue-per-session gate.
#
# Four hooks use this file. PreToolUse counts a stream launch, SubagentStop
# counts the finish and arms the lock once the counts balance, UserPromptSubmit
# refuses to continue while the lock is armed, and SessionStart clears it.
#
# Hook input is JSON on stdin. jq is not a dependency of this plugin and is not
# present on every machine that installs it, so the fields read here — all flat
# strings — are read with grep. Anything more structured than that belongs in
# jq, not in a longer regex.

# One file per session lives here. CLAUDE_PLUGIN_DATA is the plugin's own
# persistent directory; the temp fallback keeps the hooks working under
# `claude --plugin-dir`, where it may not be set.
vaca_state_dir() {
  printf '%s/vaca-sessions' "${CLAUDE_PLUGIN_DATA:-${TMPDIR:-/tmp}}"
}

# vaca_session_file <session-id> <suffix>
vaca_session_file() {
  printf '%s/%s.%s' "$(vaca_state_dir)" "${1:-unknown}" "$2"
}

# First value of a flat JSON string field, read from stdin.
# "prompt" does not match "prompt_id": the pattern carries the key's own
# closing quote.
vaca_json_field() {
  tr '\n' ' ' \
    | grep -o "\"$1\"[[:space:]]*:[[:space:]]*\"[^\"]*\"" \
    | head -n 1 \
    | sed 's/^"[^"]*"[[:space:]]*:[[:space:]]*"//; s/"$//'
}
