#!/bin/bash
# Stop — before a turn ends, build every rancho-dotnet tree that has uncommitted code changes and run its
# architecture tests. The checks are the law; this makes sure they ran.
#
# A tree is a directory holding build/ProjectKinds.targets: the session's directory or one above it, or one up
# to three levels below the project directory. A tree with no uncommitted change to code or to a check is
# skipped, so a turn that only talks costs nothing.
#
# Exit 2 blocks the stop and hands stderr to Claude, which fixes the code and tries again. When Claude is
# already continuing because of this hook (stop_hook_active), it exits 0, so a failure it can't fix is
# reported once rather than looping. The integration tests need Docker and minutes; they stay with
# `dotnet test` before a change is done.

payload=$(cat)

if printf '%s' "$payload" | grep -q '"stop_hook_active"[[:space:]]*:[[:space:]]*true'; then
  exit 0
fi

command -v dotnet >/dev/null 2>&1 || exit 0

start="${CLAUDE_PROJECT_DIR:-$PWD}"

trees=()
dir="$PWD"
while [ -n "$dir" ] && [ "$dir" != "/" ]; do
  if [ -f "$dir/build/ProjectKinds.targets" ]; then
    trees+=("$dir")
    break
  fi
  parent="${dir%/*}"
  [ "$parent" = "$dir" ] && break
  dir="$parent"
done
while IFS= read -r targets; do
  trees+=("${targets%/build/ProjectKinds.targets}")
done < <(find "$start" -maxdepth 4 -path '*/build/ProjectKinds.targets' \
  -not -path '*/node_modules/*' -not -path '*/bin/*' -not -path '*/obj/*' 2>/dev/null)

failed=0
seen=""
for tree in "${trees[@]}"; do
  # One spelling per directory: the walk up and the search down can name the same tree differently.
  tree=$(cd "$tree" 2>/dev/null && pwd -P) || continue
  case "|$seen|" in *"|$tree|"*) continue ;; esac
  seen="$seen|$tree"

  changes=$(git -C "$tree" status --porcelain --untracked-files=all -- . 2>/dev/null \
    | grep -E '\.(cs|csproj|props|targets|slnx|sln|editorconfig|txt)$|global\.json$')
  [ -z "$changes" ] && continue

  solution=$(ls "$tree"/*.slnx "$tree"/*.sln 2>/dev/null | head -n 1)
  [ -z "$solution" ] && continue

  if ! build=$(cd "$tree" && dotnet build "$solution" -nologo -v q 2>&1); then
    {
      echo "dotnet build fails in $tree. A failing check is the law speaking: fix the code, not the check."
      printf '%s\n' "$build" | grep -E ' error |error [A-Z]+[0-9]+' | sort -u | head -n 40
    } >&2
    failed=1
    continue
  fi

  if [ -d "$tree/tests/Architecture.Tests" ]; then
    if ! tests=$(cd "$tree" && dotnet test --project tests/Architecture.Tests --no-build 2>&1); then
      {
        echo "The architecture tests fail in $tree. A failing check is the law speaking: fix the code, not the check."
        printf '%s\n' "$tests" | grep -E '^failed |^  [A-Z].*\.$|^    - ' | head -n 60
      } >&2
      failed=1
    fi
  fi
done

[ "$failed" -eq 1 ] && exit 2
exit 0
