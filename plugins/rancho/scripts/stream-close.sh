#!/bin/bash
# SubagentStop:vaca-stream — one work stream finished.
#
# When every stream this session launched has finished, the issue is done and
# the session has served its purpose: arm the lock. Until then, siblings are
# still running and the session is still needed.
#
# Exits 0 on every path — a subagent is never blocked from finishing.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
. "$DIR/lib/session-state.sh"

payload=$(cat)
sid=$(printf '%s' "$payload" | vaca_json_field session_id)

mkdir -p "$(vaca_state_dir)" 2>/dev/null || exit 0
streams=$(vaca_session_file "$sid" streams)
printf 'close\n' >> "$streams" 2>/dev/null

# One pass for both counts.
counts=$(awk '/^open$/ { o++ } /^close$/ { c++ } END { printf "%d %d", o + 0, c + 0 }' "$streams" 2>/dev/null)
opened=${counts%% *}
closed=${counts##* }

# No recorded launch means the count is unreliable — say nothing rather than
# lock a session that may still be working.
[ "${opened:-0}" -gt 0 ] || exit 0
[ "${closed:-0}" -ge "${opened:-0}" ] || exit 0

# Name the work, so the refusal can be specific about what finished. The
# streams have just written their progress files, which makes the most recently
# touched updates/<issue>/ directory the issue this session carried.
label=""
proj="${CLAUDE_PROJECT_DIR:-$(printf '%s' "$payload" | vaca_json_field cwd)}"
newest=$(ls -1dt "$proj"/.claude/epics/*/updates/*/ 2>/dev/null | head -n 1)
if [ -n "$newest" ]; then
  issue=$(basename "$newest")
  epic=$(basename "$(dirname "$(dirname "$newest")")")
  label="$epic, issue #$issue"
fi

printf '%s\n' "$label" > "$(vaca_session_file "$sid" lock)" 2>/dev/null

exit 0
