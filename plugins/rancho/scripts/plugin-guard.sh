#!/bin/bash
# PreToolUse — refuse any write into the installed plugin directories.
#
# Plugins are installed globally, under the user's Claude config directory. An
# edit there changes every project on the machine that loads the plugin, and the
# next update silently discards it. Changes belong in the plugin's source
# repository, published like any other release.
#
# Exit 2 blocks the tool call and hands stderr to Claude. Every other path exits
# 0: this hook guards one location and must never get in the way of anything
# else.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
. "$DIR/lib/session-state.sh"

payload=$(cat)

# Lowercase, backslashes to slashes, runs of slashes collapsed. A Windows path
# arrives JSON-escaped as C:\\Users\\..., so both forms must fold to one.
normalize() {
  tr '[:upper:]' '[:lower:]' | sed 's#\\\\#/#g; s#\\#/#g; s#//*#/#g'
}

# The protected roots, as they appear after normalize. The default config
# directory is matched by name, so "~/.claude/...", "$HOME/.claude/..." and an
# absolute path all hit; CLAUDE_CONFIG_DIR adds a relocated one.
protected='/\.claude/plugins/(cache|marketplaces)/'
if [ -n "$CLAUDE_CONFIG_DIR" ]; then
  cfg=$(printf '%s' "$CLAUDE_CONFIG_DIR" | normalize | sed 's#/$##; s#[.[\*^$()+?{|]#\\&#g')
  protected="($protected|$cfg/plugins/(cache|marketplaces)/)"
fi

refuse() {
  {
    echo "Blocked: this writes into the installed plugin directory ($1)."
    echo "Plugins there are installed globally. An edit changes every project on"
    echo "this machine that loads the plugin, and the next plugin update silently"
    echo "discards it. Make the change in the plugin's source repository instead,"
    echo "and tell the user this write was refused and why."
  } >&2
  exit 2
}

tool=$(printf '%s' "$payload" | vaca_json_field tool_name)

case "$tool" in
  Edit|Write|MultiEdit|NotebookEdit)
    # Only the target path is inspected. The content being written may mention
    # the plugin directory - documentation about this hook does - and that is
    # not a write into it.
    target=$(printf '%s' "$payload" | vaca_json_field file_path)
    [ -n "$target" ] || target=$(printf '%s' "$payload" | vaca_json_field notebook_path)
    if printf '%s' "$target" | normalize | grep -Eq "$protected"; then
      refuse "$target"
    fi
    ;;
  Bash|PowerShell)
    # Best effort. A shell command cannot be parsed with grep, so this refuses a
    # command that names a protected root and also carries a write verb or an
    # output redirect. Reading the installed plugin stays allowed.
    cmd=$(printf '%s' "$payload" | normalize)
    printf '%s' "$cmd" | grep -Eq "$protected" || exit 0
    stripped=$(printf '%s' "$cmd" | sed -E 's#[0-9&]?>&[0-9]##g; s#[0-9&]?>[[:space:]]*(/dev/null|\$null)##g')
    if printf '%s' "$stripped" | grep -Eq \
      '(sed|perl) +-i|(^|[^a-z0-9_-])(tee|rm|rmdir|mv|cp|touch|mkdir|chmod|truncate|ln|patch|rsync)[[:space:]]|set-content|add-content|out-file|remove-item|move-item|copy-item|new-item|rename-item|clear-content|git +(apply|checkout|restore|reset|stash|pull|merge|commit|am)|>'; then
      refuse "a $tool command naming it"
    fi
    ;;
esac

exit 0
