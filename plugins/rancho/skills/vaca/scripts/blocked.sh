#!/bin/bash
# Open tasks still waiting on an unclosed dependency.
# One awk pass over every task file; the rest is bash builtins, no spawns.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "🚫 Blocked Tasks"
echo "================"
echo ""

if [ ! -d ".claude/epics" ]; then
  echo "No epics yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

declare -A STATUS
declare -A NAME
rows=""
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  STATUS["$epic/$num"]="$status"
  NAME["$epic/$num"]="$name"
  rows+="$epic"$'\037'"$num"$'\037'"$status"$'\037'"$parallel"$'\037'"$deps"$'\037'"$name"$'\n'
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

found=0
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  [ "$status" = "open" ] || continue
  [ -n "$deps" ] || continue

  waiting=""
  for dep in $deps; do
    dep_status="${STATUS["$epic/$dep"]}"
    if [ -z "$dep_status" ]; then
      waiting="$waiting #$dep(missing)"
    elif [ "$dep_status" != "closed" ] && [ "$dep_status" != "completed" ]; then
      waiting="$waiting #$dep"
    fi
  done
  [ -n "$waiting" ] || continue

  echo "⏸️ Task #$num - $name"
  echo "   Epic: $epic"
  echo "   Waiting for:$waiting"
  echo ""
  found=$((found + 1))
done <<< "$rows"

if [ $found -eq 0 ]; then
  echo "No blocked tasks found!"
  echo ""
  echo "💡 All tasks with dependencies have them satisfied."
else
  echo "📊 Total blocked: $found tasks"
fi

exit 0
