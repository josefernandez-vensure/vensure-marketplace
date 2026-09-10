#!/bin/bash
# Epics grouped by status, with task counts.
# Two awk passes total.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📚 Project Epics"
echo "================"
echo ""

if [ ! -d ".claude/epics" ]; then
  echo "📁 No epics yet. Ask: 'turn the <feature> PRD into an epic'"
  exit 0
fi

# Task counts per epic, one pass.
declare -A TASKS
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  TASKS["$epic"]=$(( ${TASKS["$epic"]:-0} + 1 ))
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

planning=""; in_progress=""; completed=""
total=0
while IFS=$'\037' read -r dir name status progress github created; do
  [ -n "$dir" ] || continue
  total=$((total + 1))
  t="${TASKS["$dir"]:-0}"

  if [ -n "$github" ]; then
    issue="${github##*/}"
    entry="   📋 $name (#$issue) - $progress complete ($t tasks)"
  else
    entry="   📋 $name - $progress complete ($t tasks)"
  fi

  case "$status" in
    in-progress|in_progress|active|started)        in_progress+="$entry"$'\n' ;;
    completed|complete|done|closed|finished)       completed+="$entry"$'\n' ;;
    *)                                             planning+="$entry"$'\n' ;;
  esac
done < <(awk -f "$DIR/lib/read-epics.awk" .claude/epics/*/epic.md 2>/dev/null)

if [ $total -eq 0 ]; then
  echo "📁 No epics yet. Ask: 'turn the <feature> PRD into an epic'"
  exit 0
fi

echo "📝 Planning:"
[ -n "$planning" ] && printf '%s' "$planning" || echo "   (none)"
echo ""
echo "🚀 In Progress:"
[ -n "$in_progress" ] && printf '%s' "$in_progress" || echo "   (none)"
echo ""
echo "✅ Completed:"
[ -n "$completed" ] && printf '%s' "$completed" || echo "   (none)"

task_total=0
for n in "${TASKS[@]}"; do task_total=$((task_total + n)); done

echo ""
echo "📊 Summary"
echo "   Total epics: $total"
echo "   Total tasks: $task_total"

exit 0
