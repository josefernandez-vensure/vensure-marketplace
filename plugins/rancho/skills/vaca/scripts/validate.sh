#!/bin/bash
# Consistency checks over the .claude tree.
# Three awk passes plus two finds.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."

echo "🔍 Validating VACA project state"
echo "================================"
echo ""

errors=0
warnings=0

echo "📁 Directory Structure:"
if [ -d ".claude" ]; then
  echo "  ✅ .claude directory exists"
else
  echo "  ❌ .claude directory missing"
  errors=$((errors + 1))
fi
[ -d ".claude/prds" ]  && echo "  ✅ PRDs directory exists"  || echo "  ⚠️ PRDs directory missing"
[ -d ".claude/epics" ] && echo "  ✅ Epics directory exists" || echo "  ⚠️ Epics directory missing"

if [ $errors -gt 0 ]; then
  echo ""
  echo "Not initialized. Run: bash \"\$CLAUDE_SKILL_DIR/scripts/init.sh\""
  exit 1
fi

echo ""
echo "🗂️ Data Integrity:"

# Every epic directory needs an epic.md.
for epic_dir in .claude/epics/*/; do
  [ -d "$epic_dir" ] || continue
  case "$epic_dir" in */archived/*) continue ;; esac
  if [ ! -f "$epic_dir/epic.md" ]; then
    echo "  ⚠️ Missing epic.md in ${epic_dir#.claude/epics/}"
    warnings=$((warnings + 1))
  fi
done

# Task files outside an epic directory.
orphaned=$(find .claude -name "[0-9]*.md" -not -path ".claude/epics/*/*" 2>/dev/null | wc -l)
if [ "$orphaned" -gt 0 ]; then
  echo "  ⚠️ Found $orphaned orphaned task file(s) outside any epic"
  warnings=$((warnings + 1))
fi
[ $warnings -eq 0 ] && echo "  ✅ Epic directories well-formed"

echo ""
echo "🔗 Reference Check:"

declare -A EXISTS
rows=""
while IFS=$'\037' read -r epic num status parallel deps name; do
  [ -n "$num" ] || continue
  EXISTS["$epic/$num"]=1
  rows+="$epic"$'\037'"$num"$'\037'"$deps"$'\n'
done < <(awk -f "$DIR/lib/read-tasks.awk" .claude/epics/*/[0-9]*.md 2>/dev/null)

ref_errors=0
while IFS=$'\037' read -r epic num deps; do
  [ -n "$num" ] || continue
  for dep in $deps; do
    if [ -z "${EXISTS["$epic/$dep"]}" ]; then
      echo "  ⚠️ Task #$num ($epic) depends on missing task: $dep"
      warnings=$((warnings + 1))
      ref_errors=$((ref_errors + 1))
    fi
  done
  # A task cannot depend on itself.
  for dep in $deps; do
    if [ "$dep" = "$num" ]; then
      echo "  ⚠️ Task #$num ($epic) depends on itself"
      warnings=$((warnings + 1))
      ref_errors=$((ref_errors + 1))
    fi
  done
done <<< "$rows"
[ $ref_errors -eq 0 ] && echo "  ✅ All dependency references valid"

echo ""
echo "📝 Frontmatter Validation:"
invalid=0
while IFS= read -r file; do
  [ -n "$file" ] || continue
  # First line must open a frontmatter block.
  IFS= read -r first < "$file" || first=""
  case "$first" in
    ---*) ;;
    *) echo "  ⚠️ Missing frontmatter: ${file#.claude/}"; invalid=$((invalid + 1)) ;;
  esac
done < <(find .claude \( -path "*/epics/*" -o -path "*/prds/*" \) -name "*.md" 2>/dev/null)
[ $invalid -eq 0 ] && echo "  ✅ All files have frontmatter"

echo ""
echo "📊 Validation Summary:"
echo "  Errors: $errors"
echo "  Warnings: $warnings"
echo "  Invalid files: $invalid"

if [ $errors -eq 0 ] && [ $warnings -eq 0 ] && [ $invalid -eq 0 ]; then
  echo ""
  echo "✅ State is consistent."
else
  echo ""
  echo "💡 Fix the items above, or ask VACA to reconcile the epic state."
fi

exit 0
