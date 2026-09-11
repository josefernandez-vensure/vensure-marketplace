#!/bin/bash
# PreToolUse:Task — record that this session launched a VACA work stream.
#
# Counting launches here, rather than trusting the model to report them, is what
# makes the end of an issue a fact the harness knows. Exits 0 on every path: a
# stream launch must never depend on this bookkeeping succeeding.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
. "$DIR/lib/session-state.sh"

payload=$(cat)

# Only VACA streams count. The same Task tool launches task-file batches and
# sync batches, and those are bookkeeping, not work.
case "$payload" in
  *vaca-stream*) ;;
  *) exit 0 ;;
esac

sid=$(printf '%s' "$payload" | vaca_json_field session_id)
mkdir -p "$(vaca_state_dir)" 2>/dev/null || exit 0
printf 'open\n' >> "$(vaca_session_file "$sid" streams)" 2>/dev/null

exit 0
