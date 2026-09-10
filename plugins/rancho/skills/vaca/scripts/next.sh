#!/bin/bash
# Open tasks whose dependencies are all closed.
# One awk pass over every task file; the rest is bash builtins, no spawns.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📋 Next Available Tasks"
echo "======================="
echo ""

if [ ! -d ".claude/epics" ]; then
  echo "No epics yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

# Read every task once. Archived epics sit one level deeper, so this glob skips them.
declare -A STATUS
rows=""
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  STATUS["$epic/$num"]="$status"
  rows+="$epic"$'\037'"$num"$'\037'"$status"$'\037'"$parallel"$'\037'"$deps"$'\037'"$name"$'\n'
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

found=0
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  [ "$status" = "open" ] || continue

  # A dependency blocks only while it is still open.
  blocked=0
  for dep in $deps; do
    dep_status="${STATUS["$epic/$dep"]}"
    if [ -z "$dep_status" ] || [ "$dep_status" = "open" ] || [ "$dep_status" = "in-progress" ]; then
      blocked=1; break
    fi
  done
  [ $blocked -eq 1 ] && continue

  echo "✅ Ready: #$num - $name"
  echo "   Epic: $epic"
  [ "$parallel" = "true" ] && echo "   🔄 Can run in parallel"
  echo ""
  found=$((found + 1))
done <<< "$rows"

if [ $found -eq 0 ]; then
  echo "No available tasks found."
  echo ""
  echo "💡 Suggestions:"
  echo "  • Check blocked tasks: ask 'what is blocked'"
  echo "  • View all epics: ask 'list epics'"
fi

echo ""
echo "📊 Summary: $found tasks ready to start"

exit 0
