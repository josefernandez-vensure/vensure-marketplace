#!/bin/bash
# Issues with an open progress file, plus epics marked in-progress.
# Three awk passes total.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "🔄 In Progress Work"
echo "==================="
echo ""

if [ ! -d ".claude/epics" ]; then
  echo "No epics yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

# Task names, one pass, so each issue can be labelled.
declare -A NAME
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  NAME["$epic/$num"]="$name"
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

found=0
while IFS=$'\037' read -r epic issue completion last_sync; do
  [ -n "$issue" ] || continue
  task_name="${NAME["$epic/$issue"]:-Unknown task}"
  echo "📝 Issue #$issue - $task_name"
  echo "   Epic: $epic"
  echo "   Progress: $completion complete"
  [ -n "$last_sync" ] && echo "   Last update: $last_sync"
  echo ""
  found=$((found + 1))
done < <(awk -f "$DIR/lib/read-progress.awk" .claude/epics/*/updates/*/progress.md 2>/dev/null)

echo "📚 Active Epics:"
active=0
while IFS=$'\037' read -r dir name status progress github created; do
  [ -n "$dir" ] || continue
  case "$status" in
    in-progress|in_progress|active|started)
      echo "   • $name - $progress complete"
      active=$((active + 1)) ;;
  esac
done < <(awk -f "$DIR/lib/read-epics.awk" .claude/epics/*/epic.md 2>/dev/null)
[ $active -eq 0 ] && echo "   (none)"

echo ""
if [ $found -eq 0 ]; then
  echo "No active work items found."
  echo ""
  echo "💡 Start work with: ask 'what is next'"
else
  echo "📊 Total active items: $found"
fi

exit 0
