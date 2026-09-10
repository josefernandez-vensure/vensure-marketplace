#!/bin/bash
# Full detail for one epic: metadata, task list, statistics, next actions.
# Two awk passes total.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
epic_name="${1:-}"

list_epics() {
  echo "Available epics:"
  local found=0
  while IFS=$'\037' read -r dir name status progress github created; do
    [ -n "$dir" ] || continue
    echo "  • $dir"
    found=1
  done < <(awk -f "$DIR/lib/read-epics.awk" .claude/epics/*/epic.md 2>/dev/null)
  [ $found -eq 0 ] && echo "  (none)"
}

if [ -z "$epic_name" ]; then
  echo "❌ Please provide an epic name"
  echo "Usage: epic-show.sh <epic-name>"
  echo ""
  list_epics
  exit 1
fi

epic_file=".claude/epics/$epic_name/epic.md"
if [ ! -f "$epic_file" ]; then
  echo "❌ Epic not found: $epic_name"
  echo ""
  list_epics
  exit 1
fi

IFS=$'\037' read -r _dir name status progress github created \
  < <(awk -f "$DIR/lib/read-epics.awk" "$epic_file")

echo "📚 Epic: $epic_name"
echo "================================"
echo ""
echo "📊 Metadata:"
echo "  Status: $status"
echo "  Progress: $progress"
[ -n "$github" ] && echo "  GitHub: $github"
echo "  Created: ${created:-unknown}"
echo ""

echo "📝 Tasks:"
task_count=0; open_count=0; closed_count=0
while IFS=$'\037' read -r epic num st parallel deps tname; do
  [ -n "$num" ] || continue
  task_count=$((task_count + 1))
  case "$st" in
    closed|completed)
      echo "  ✅ #$num - $tname"
      closed_count=$((closed_count + 1)) ;;
    *)
      # The original emitted "(parallel)" with a bare `echo -n` after the line
      # had already ended, so it landed at the head of the next task.
      suffix=""
      [ "$parallel" = "true" ] && suffix=" (parallel)"
      [ -n "$deps" ] && suffix="$suffix (depends on: ${deps// /, })"
      echo "  ⬜ #$num - $tname$suffix"
      open_count=$((open_count + 1)) ;;
  esac
done < <(awk -f "$DIR/lib/read-tasks.awk" ".claude/epics/$epic_name"/[0-9]*.md 2>/dev/null)

if [ $task_count -eq 0 ]; then
  echo "  No tasks created yet"
fi

echo ""
echo "📈 Statistics:"
echo "  Total tasks: $task_count"
echo "  Open: $open_count"
echo "  Closed: $closed_count"
[ $task_count -gt 0 ] && echo "  Completion: $((closed_count * 100 / task_count))%"

echo ""
echo "💡 Actions:"
[ $task_count -eq 0 ] && echo "  • Decompose into tasks: ask 'break down the $epic_name epic into tasks'"
[ -z "$github" ] && [ $task_count -gt 0 ] && echo "  • Sync to GitHub: ask 'sync the $epic_name epic'"
[ -n "$github" ] && [ "$status" != "completed" ] && echo "  • Start work: ask 'start the $epic_name epic'"

exit 0
