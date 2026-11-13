#!/usr/bin/env bash
# A published contract never breaks. Run in CI on every pull request:
#
#   build/check-contracts.sh origin/main
#
# Compares this branch's Contracts types with the snapshots the base branch committed under contracts/,
# so a snapshot edited by hand can't hide a removed, renamed, retyped or newly required property.
set -euo pipefail

base="${1:-origin/main}"
since="$(git merge-base "$base" HEAD)"
baseline="$(mktemp -d)"
trap 'rm -rf "$baseline"' EXIT

for snapshot in $(git ls-tree --name-only "$since" contracts/ 2>/dev/null || true); do
  git show "$since:$snapshot" > "$baseline/$(basename "$snapshot")"
done

CONTRACT_BASELINE="$baseline" dotnet test --project tests/Architecture.Tests -- --filter-method "*.Published_contracts_keep_their_shape"
