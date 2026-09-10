#!/bin/bash
# PRDs grouped by status.
# One awk pass; the original looped over every PRD three times, three greps deep.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📋 PRD List"
echo "==========="
echo ""

if [ ! -d ".claude/prds" ]; then
  echo "📁 No PRDs yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

backlog=""; active=""; done_=""
n_backlog=0; n_active=0; n_done=0; total=0

while IFS=$'\037' read -r slug name status description created; do
  [ -n "$slug" ] || continue
  total=$((total + 1))
  entry="   📋 $name - $description"
  case "$status" in
    in-progress|active)
      active+="$entry"$'\n';        n_active=$((n_active + 1)) ;;
    implemented|completed|done)
      done_+="$entry"$'\n';         n_done=$((n_done + 1)) ;;
    *)
      backlog+="$entry"$'\n';       n_backlog=$((n_backlog + 1)) ;;
  esac
done < <(awk -f "$DIR/lib/read-prds.awk" .claude/prds/*.md 2>/dev/null)

if [ $total -eq 0 ]; then
  echo "📁 No PRDs yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

echo "🔍 Backlog PRDs:"
[ -n "$backlog" ] && printf '%s' "$backlog" || echo "   (none)"

echo ""
echo "🔄 In-Progress PRDs:"
[ -n "$active" ] && printf '%s' "$active" || echo "   (none)"

echo ""
echo "✅ Implemented PRDs:"
[ -n "$done_" ] && printf '%s' "$done_" || echo "   (none)"

echo ""
echo "📊 PRD Summary"
echo "   Total PRDs: $total"
echo "   Backlog: $n_backlog"
echo "   In-Progress: $n_active"
echo "   Implemented: $n_done"

exit 0
