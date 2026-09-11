# Sync — Push to GitHub & Track Progress

This phase covers pushing local epics and tasks to GitHub as issues, syncing
progress as comments, closing issues when work is done, merging a finished
epic, and filing bugs against work already shipped.

Read `conventions.md` first if you have not already — the frontmatter schemas,
path standards, GitHub operation rules, and datetime rule all apply here.

---

## Repository Safety Check

**Always run this before any GitHub write operation:**

```bash
remote_url=$(git remote get-url origin 2>/dev/null || echo "")
REPO=$(echo "$remote_url" | sed 's|.*github.com[:/]||' | sed 's|\.git$||')
```

---

## Pick the operation

Read the one file for the operation at hand. Do not read them all.

| The user wants | Read |
|---|---|
| Push a local epic and its tasks to GitHub as issues | `sync-epic.md` |
| Post local development progress to an issue as a comment | `sync-issue.md` |
| Mark a task complete and close its issue | `sync-close.md` |
| Merge a finished epic back to main and archive it | `sync-merge.md` |
| File a bug found while testing an issue, linked to it | `sync-bug.md` |

Triggers, for reference:

```
"sync the <feature> epic"       -> sync-epic.md
"push tasks to github"          -> sync-epic.md
"sync issue <N>"                -> sync-issue.md
"post progress on <N>"          -> sync-issue.md
"close issue <N>"               -> sync-close.md
"merge the <feature> epic"      -> sync-merge.md
"found a bug in issue <N>"      -> sync-bug.md
"testing issue <N> revealed X"  -> sync-bug.md
```

Every one of these is a **write** to GitHub. Run the Repository Safety Check
above first, and never proceed past a failed `gh` command — report it and stop
rather than leaving the local files and the remote issues disagreeing.
