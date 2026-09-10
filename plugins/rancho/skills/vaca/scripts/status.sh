#!/bin/bash
# Project overview: PRD, epic and task counts.
# Two awk passes total.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📊 Project Status"
echo "================"
echo ""

if [ ! -d ".claude" ]; then
  echo "Not initialized. Run: bash \"\$CLAUDE_SKILL_DIR/scripts/init.sh\""
  exit 0
fi

echo "📄 PRDs:"
prds=(.claude/prds/*.md)
[ -e "${prds[0]}" ] && echo "  Total: ${#prds[@]}" || echo "  None yet"

echo ""
echo "📚 Epics:"
epic_total=0
active=0
while IFS=$'\037' read -r dir name status progress github created; do
  [ -n "$dir" ] || continue
  epic_total=$((epic_total + 1))
  case "$status" in
    in-progress|in_progress|active|started) active=$((active + 1)) ;;
  esac
done < <(awk -f "$DIR/lib/read-epics.awk" .claude/epics/*/epic.md 2>/dev/null)
if [ $epic_total -eq 0 ]; then
  echo "  None yet"
else
  echo "  Total: $epic_total"
  echo "  Active: $active"
fi

echo ""
echo "📝 Tasks:"
total=0; open=0; closed=0; in_prog=0
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  total=$((total + 1))
  case "$status" in
    closed|completed) closed=$((closed + 1)) ;;
    in-progress)      in_prog=$((in_prog + 1)) ;;
    *)                open=$((open + 1)) ;;
  esac
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

if [ $total -eq 0 ]; then
  echo "  None yet"
else
  echo "  Open: $open"
  [ $in_prog -gt 0 ] && echo "  In progress: $in_prog"
  echo "  Closed: $closed"
  echo "  Total: $total"
  echo ""
  echo "  Progress: $((closed * 100 / total))%"
fi

exit 0
