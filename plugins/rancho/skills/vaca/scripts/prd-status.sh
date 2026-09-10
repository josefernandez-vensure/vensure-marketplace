#!/bin/bash
# PRD distribution and recent activity.
# One awk pass; bars are built with bash string ops, so no `seq` spawns.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "📄 PRD Status Report"
echo "===================="
echo ""

if [ ! -d ".claude/prds" ]; then
  echo "No PRDs yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

backlog=0; in_progress=0; implemented=0; total=0
recent=""
while IFS=$'\037' read -r slug name status description created; do
  [ -n "$slug" ] || continue
  total=$((total + 1))
  case "$status" in
    in-progress|active)         in_progress=$((in_progress + 1)) ;;
    implemented|completed|done) implemented=$((implemented + 1)) ;;
    *)                          backlog=$((backlog + 1)) ;;
  esac
  # created is ISO 8601, so lexical sort is chronological.
  recent+="$created|$name"$'\n'
done < <(awk -f "$DIR/lib/read-prds.awk" .claude/prds/*.md 2>/dev/null)

if [ $total -eq 0 ]; then
  echo "No PRDs yet. Ask: 'create a PRD for <feature>'"
  exit 0
fi

bar() {
  local filled=$(( $1 * 20 / total )) out="" i
  for ((i = 0; i < filled; i++)); do out+="█"; done
  printf '%s' "$out"
}

echo "📊 Distribution:"
echo "================"
echo ""
printf "  Backlog:     %-3d [%s]\n" "$backlog"     "$(bar $backlog)"
printf "  In Progress: %-3d [%s]\n" "$in_progress" "$(bar $in_progress)"
printf "  Implemented: %-3d [%s]\n" "$implemented" "$(bar $implemented)"
echo ""
echo "  Total PRDs: $total"

echo ""
echo "📅 Most recently created PRDs:"
printf '%s' "$recent" | sort -r | head -5 | while IFS='|' read -r _created name; do
  [ -n "$name" ] && echo "  • $name"
done

echo ""
echo "💡 Next Actions:"
[ $backlog -gt 0 ]     && echo "  • Parse backlog PRDs to epics: ask 'turn the <name> PRD into an epic'"
[ $in_progress -gt 0 ] && echo "  • Check progress on active PRDs: ask 'status of the <name> epic'"

exit 0
