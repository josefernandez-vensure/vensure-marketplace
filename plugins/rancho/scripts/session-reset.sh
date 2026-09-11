#!/bin/bash
# SessionStart — release this session id's lock, then hand the fresh session the
# state it needs.
#
# The two halves are one idea: clearing is only cheap if the new session starts
# oriented. VACA keeps its state in files, so re-reading it costs one awk pass,
# and the session that just cleared knows what is in flight before the user
# types anything.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
. "$DIR/lib/session-state.sh"

payload=$(cat)
sid=$(printf '%s' "$payload" | vaca_json_field session_id)

rm -f "$(vaca_session_file "$sid" lock)" "$(vaca_session_file "$sid" streams)" 2>/dev/null

# Sessions that end without clearing leave their counters behind. Nothing reads
# them, so age them out rather than growing the directory forever.
find "$(vaca_state_dir)" -type f -mtime +7 -delete 2>/dev/null

proj="${CLAUDE_PROJECT_DIR:-$(printf '%s' "$payload" | vaca_json_field cwd)}"
[ -d "$proj/.claude/epics" ] || exit 0
cd "$proj" || exit 0

scripts="${CLAUDE_PLUGIN_ROOT:-$(cd "$DIR/.." && pwd)}/skills/vaca/scripts"

echo "# VACA — where this project stands"
echo ""
echo "This project runs **one issue per session**. When every work stream of an"
echo "issue has finished, close it out in the same session — post progress,"
echo "close the issue, update the epic — then tell the user to /clear and stop."
echo "Prompts after that point are refused by a hook until they do."
echo ""
echo "Do not start a second issue in a session that has already finished one."
echo ""

bash "$scripts/in-progress.sh" 2>/dev/null
bash "$scripts/next.sh" 2>/dev/null

exit 0
