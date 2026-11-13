#!/usr/bin/env bash
# SessionStart: puts the prime directive in context, but only in a repository
# that binds a law - its root CLAUDE.md, or any CLAUDE.md git tracks, holds a
# `Stack law:` line. The read-only rule for the installed plugins prints in
# every repository, since the plugin guard can't parse every shell command.

root="${CLAUDE_PROJECT_DIR:-$PWD}"
plugin="${CLAUDE_PLUGIN_ROOT:-$(cd "$(dirname "$0")/.." && pwd)}"
binding='^[[:space:]]*Stack law:'

bound=0
if grep -qs "$binding" "$root/CLAUDE.md"; then
  bound=1
else
  while IFS= read -r file; do
    case "$file" in
      CLAUDE.md | */CLAUDE.md)
        if grep -qs "$binding" "$root/$file"; then
          bound=1
          break
        fi
        ;;
    esac
  done < <(git -C "$root" ls-files -- '*CLAUDE.md' 2>/dev/null)
fi

if [ "$bound" = 1 ]; then
  cat "$plugin/context/prime-directive.md"
  echo ""
fi
cat "$plugin/context/plugins-read-only.md"
exit 0
