#!/bin/bash
# UserPromptSubmit — refuse to continue a session whose issue is finished.
#
# Exit 2 blocks the prompt, erases it, and shows stderr to the user. This is the
# only hook event that can stop a session from continuing; Stop does the
# opposite, and no hook can run /clear on the user's behalf.

DIR="${0%/*}"; [ "$DIR" = "$0" ] && DIR="."
. "$DIR/lib/session-state.sh"

payload=$(cat)
sid=$(printf '%s' "$payload" | vaca_json_field session_id)
lock=$(vaca_session_file "$sid" lock)

[ -f "$lock" ] || exit 0

prompt=$(printf '%s' "$payload" | vaca_json_field prompt)

# Slash commands always pass. /clear is the way out, and a gate that could
# swallow it would be a gate with no exit.
case "$prompt" in
  /*) exit 0 ;;
esac

# The escape hatch. Uppercase so it cannot be typed by accident, and it lifts
# the lock rather than waiving one message — a stale lock should be fixable
# once, not worked around forever.
case "$payload" in
  *"VACA OVERRIDE"*)
    rm -f "$lock" 2>/dev/null
    echo "Session lock lifted with VACA OVERRIDE. This session already finished"
    echo "an issue, so its context describes work that is done. Say so plainly"
    echo "if the user asks it to start another issue here: /clear is correct."
    exit 0
    ;;
esac

label=$(head -n 1 "$lock" 2>/dev/null)

{
  echo ""
  echo "  ────────────────────────────────────────────────────────────"
  echo "   THIS SESSION IS FINISHED — run /clear before continuing"
  echo "  ────────────────────────────────────────────────────────────"
  echo ""
  if [ -n "$label" ]; then
    echo "   Completed here:  $label"
  fi
  echo "   Every work stream this session launched has finished."
  echo "   One session carries one issue. That is the rule here, and"
  echo "   this is it being enforced."
  echo ""
  echo "   Your message was NOT sent. Do this instead:"
  echo ""
  echo "     1.  /clear"
  echo "     2.  say:  continue the <feature> epic"
  echo ""
  echo "   Nothing is lost by clearing. VACA keeps its state in"
  echo "   .claude/, and the next session is handed the current"
  echo "   picture before you type anything."
  echo ""
  echo "   If this lock is wrong, resend your message with"
  echo "   VACA OVERRIDE in the text and it will be lifted."
  echo ""
} >&2

exit 2
