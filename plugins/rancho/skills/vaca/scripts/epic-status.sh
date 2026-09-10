#!/bin/bash
# Task breakdown and progress bar for one epic.
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
  echo "❌ Please specify an epic name"
  echo "Usage: epic-status.sh <epic-name>"
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

IFS=$'\037' read -r _dir _name status progress github created < <(awk -f "$DIR/lib/read-epics.awk" "$epic_file")

echo "📚 Epic Status: $epic_name"
echo "================================"
echo ""

# Task states, resolving dependencies so "blocked" means actually blocked.
declare -A STATUS
rows=""
while IFS=$'\037' read -r epic num st parallel deps name; do
  [ -n "$num" ] || continue
  STATUS["$num"]="$st"
  rows+="$num"$'\037'"$st"$'\037'"$deps"$'\n'
done < <(awk -f "$DIR/lib/read-tasks.awk" ".claude/epics/$epic_name"/[0-9]*.md 2>/dev/null)

total=0; open=0; closed=0; blocked=0; in_prog=0
while IFS=$'\037' read -r num st deps; do
  [ -n "$num" ] || continue
  total=$((total + 1))
  case "$st" in
    closed|completed) closed=$((closed + 1)); continue ;;
    in-progress)      in_prog=$((in_prog + 1)); continue ;;
  esac
  is_blocked=0
  for dep in $deps; do
    ds="${STATUS["$dep"]}"
    if [ -z "$ds" ] || { [ "$ds" != "closed" ] && [ "$ds" != "completed" ]; }; then
      is_blocked=1; break
    fi
  done
  if [ $is_blocked -eq 1 ]; then blocked=$((blocked + 1)); else open=$((open + 1)); fi
done <<< "$rows"

if [ $total -gt 0 ]; then
  percent=$((closed * 100 / total))
  filled=$((percent * 20 / 100))
  bar=""
  for ((i = 0; i < filled; i++)); do bar+="█"; done
  for ((i = filled; i < 20; i++)); do bar+="░"; done
  echo "Progress: [$bar] $percent%"
else
  echo "Progress: No tasks created"
fi

echo ""
echo "📊 Breakdown:"
echo "  Total tasks: $total"
echo "  ✅ Completed: $closed"
[ $in_prog -gt 0 ] && echo "  🚧 In progress: $in_prog"
echo "  🔄 Available: $open"
echo "  ⏸️ Blocked: $blocked"

if [ -n "$github" ]; then
  echo ""
  echo "🔗 GitHub: $github"
fi

exit 0
