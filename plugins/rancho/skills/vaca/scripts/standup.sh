#!/bin/bash
# Daily standup: what moved in the last day, what is active, what is ready next.
# Four awk passes plus one find.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📅 Daily Standup - $(date '+%Y-%m-%d')"
echo "================================"
echo ""

if [ ! -d ".claude" ]; then
  echo "Not initialized. Run: bash \"\$CLAUDE_SKILL_DIR/scripts/init.sh\""
  exit 0
fi

echo "📝 Today's Activity:"
echo "===================="
echo ""

# Classify recently-touched files in one awk pass over find's output.
# The original counted with the regex "/[0-9]*.md", which matches almost any
# path ending in .md and so reported PRDs and epics as tasks.
find .claude -name "*.md" -mtime -1 2>/dev/null | awk '
  /\/prds\// { prd++; next }
  /\/updates\// { upd++; next }
  /\/epic\.md$/ { epic++; next }
  /\/[0-9]+\.md$/ { task++; next }
  END {
    if (prd)  printf "  • Modified %d PRD(s)\n", prd
    if (epic) printf "  • Updated %d epic(s)\n", epic
    if (task) printf "  • Worked on %d task(s)\n", task
    if (upd)  printf "  • Posted %d progress update(s)\n", upd
    if (!prd && !epic && !task && !upd) print "  No activity recorded today"
  }'

echo ""
echo "🔄 Currently In Progress:"
active=0
while IFS=$'\037' read -r epic issue completion last_sync; do
  [ -n "$issue" ] || continue
  echo "  • Issue #$issue ($epic) - $completion complete"
  active=$((active + 1))
done < <(awk -f "$DIR/lib/read-progress.awk" .claude/epics/*/updates/*/progress.md 2>/dev/null)
[ $active -eq 0 ] && echo "  (nothing active)"

# Task states once, reused for both sections below.
declare -A STATUS
rows=""
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  STATUS["$epic/$num"]="$status"
  rows+="$epic"$'\037'"$num"$'\037'"$status"$'\037'"$deps"$'\037'"$name"$'\n'
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

echo ""
echo "⏭️ Next Available Tasks:"
count=0
while IFS=$'\037' read -r epic num status deps name; do
  [ -n "$num" ] || continue
  [ "$status" = "open" ] || continue
  blocked=0
  for dep in $deps; do
    ds="${STATUS["$epic/$dep"]}"
    if [ -z "$ds" ] || { [ "$ds" != "closed" ] && [ "$ds" != "completed" ]; }; then
      blocked=1; break
    fi
  done
  [ $blocked -eq 1 ] && continue
  echo "  • #$num - $name"
  count=$((count + 1))
  [ $count -ge 3 ] && break
done <<< "$rows"
[ $count -eq 0 ] && echo "  (nothing unblocked)"

total=0; open=0; closed=0
while IFS=$'\037' read -r epic num status deps name; do
  [ -n "$num" ] || continue
  total=$((total + 1))
  case "$status" in
    closed|completed) closed=$((closed + 1)) ;;
    open)             open=$((open + 1)) ;;
  esac
done <<< "$rows"

echo ""
echo "📊 Quick Stats:"
echo "  Tasks: $open open, $closed closed, $total total"

exit 0
