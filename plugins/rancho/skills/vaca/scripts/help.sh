#!/bin/bash
# VACA — Vensure Agentic Code Assistant
# Reference card. VACA is driven in natural language, not slash commands.

echo "📚 VACA — Vensure Agentic Code Assistant"
echo "========================================"
echo ""
echo "Spec-driven delivery: PRD → Epic → GitHub Issues → Parallel Agents → Shipped Code."
echo "Talk to VACA in plain language. The phrases below are examples, not syntax."
echo ""

echo "1. PLAN — capture requirements"
echo "   \"create a PRD for <feature>\"            write a PRD through guided brainstorming"
echo "   \"turn the <feature> PRD into an epic\"    convert a PRD to a technical epic"
echo "   \"edit the <feature> PRD\"                 revise an existing PRD or epic"
echo ""

echo "2. STRUCTURE — break it down"
echo "   \"break down the <feature> epic\"          decompose an epic into numbered tasks"
echo ""

echo "3. SYNC — push to GitHub"
echo "   \"sync the <feature> epic\"                epic + tasks become GitHub issues"
echo "   \"sync issue <N>\"                         post local progress as a comment"
echo "   \"close issue <N>\"                        close the issue and update the epic"
echo "   \"found a bug in issue <N>\"               create a linked bug issue"
echo "   \"merge the <feature> epic\"               merge, clean up, archive"
echo ""

echo "4. EXECUTE — start building"
echo "   \"analyze issue <N>\"                      identify parallel work streams"
echo "   \"start working on issue <N>\"             launch streams for one issue"
echo "   \"start the <feature> epic\"               take the next ready issue"
echo "   \"continue the <feature> epic\"            same thing, after a /clear"
echo ""

echo "5. TRACK — know where things stand"
echo "   \"project status\"        \"standup\"        \"what is next\""
echo "   \"what is blocked\"       \"what is in progress\""
echo "   \"list epics\"            \"show the <feature> epic\""
echo "   \"list PRDs\"             \"PRD status\""
echo "   \"search for <query>\"    \"validate\""
echo ""

echo "Tracking runs as scripts, not reasoning. Available directly:"
echo "  status.sh  standup.sh  next.sh  blocked.sh  in-progress.sh  validate.sh"
echo "  epic-list.sh  epic-show.sh <name>  epic-status.sh <name>"
echo "  prd-list.sh  prd-status.sh  search.sh <query>  help.sh"
echo ""

echo "ONE SESSION, ONE ISSUE"
echo "  A session takes one issue, runs its streams in parallel, closes it out,"
echo "  and ends. When the last stream finishes, the session is locked and every"
echo "  further prompt is refused until you run /clear."
echo ""
echo "  After clearing, say:  \"continue the <feature> epic\""
echo "  Nothing is lost — the next session is handed the state before you type."
echo "  Stuck lock? Resend your message with VACA OVERRIDE in the text."
echo ""

echo "Layout — all state lives in files, under the project root:"
echo "  .claude/prds/<feature>.md              product requirements"
echo "  .claude/epics/<feature>/epic.md        technical epic"
echo "  .claude/epics/<feature>/<N>.md         task, named for its GitHub issue"
echo "  .claude/epics/<feature>/updates/<N>/   per-agent progress"
echo "  .claude/epics/archived/<feature>/      completed epics"
echo ""

if [ ! -d ".claude" ]; then
  echo "⚠️  No VACA project here yet. The first PRD creates one."
  echo ""
fi

echo "Code written under VACA is governed by the engineering law: /rancho:law"

exit 0
