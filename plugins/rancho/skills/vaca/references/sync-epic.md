# Sync — Epic to GitHub Issues

> Part of the **Sync** phase. Read `conventions.md` first for the frontmatter
> schemas, path standards, and datetime rule — they apply here unchanged.
> Run the Repository Safety Check in `sync.md` before any GitHub write.

---

**Trigger**: User wants to push a local epic and its tasks to GitHub as issues.

### Preflight
- Verify `.claude/epics/<name>/epic.md` exists.
- Verify numbered task files exist — if none: "❌ No tasks to sync. Decompose the epic first."

### Process

**Step 0 — Ensure the labels exist:**

`gh issue create --label X` fails outright if `X` does not exist on the
repository, so create them first. `--force` makes this idempotent, so it is
safe to run on every sync:

```bash
gh label create "epic"        --repo "$REPO" --color "0E8A16" --description "Epic containing multiple related tasks" --force
gh label create "task"        --repo "$REPO" --color "1D76DB" --description "Individual task within an epic" --force
gh label create "feature"     --repo "$REPO" --color "A2EEEF" --description "New capability" --force
gh label create "bug"         --repo "$REPO" --color "D73A4A" --description "Something is broken" --force
gh label create "epic:<name>" --repo "$REPO" --color "5319E7" --description "Belongs to epic <name>" --force
```

If label creation fails the account lacks permission on the repository. Report
that and stop — do not proceed to create issues, because every one of them will
fail on the label.

**Step 1 — Create epic issue:**

Strip frontmatter from epic.md, then:
```bash
awk 'BEGIN{n=0} /^---\r?$/{n++; if(n<=2) next} n>=2' .claude/epics/<name>/epic.md > /tmp/epic-body.md
[ -s /tmp/epic-body.md ] || { echo "❌ Epic body is empty - refusing to post"; exit 1; }
epic_url=$(gh issue create \
  --repo "$REPO" \
  --title "Epic: <name>" \
  --body-file /tmp/epic-body.md \
  --label "epic,epic:<name>,feature")
epic_number=$(echo "$epic_url" | grep -oE '[0-9]+$')
```

Two things here are easy to get wrong and both post a broken issue before anything
looks wrong:

- **Strip with `awk`, never `sed '1,/^---$/d; 1,/^---$/d'`.** See `conventions.md`
  — that idiom empties any file whose body carries no second `---`, which is the
  normal case for an epic. Keep the emptiness check; a posted issue cannot be
  un-posted.
- **`gh issue create` has no `--json` flag.** It prints the issue URL, so take the
  number from the URL.

**Step 2 — Create task sub-issues:**

Check if `gh-sub-issue` extension is available:
```bash
if gh extension list | grep -q "yahsan2/gh-sub-issue"; then
  use_subissues=true
fi
```

For <5 tasks: create sequentially.
For ≥5 tasks: use parallel Task agents (3-4 tasks per batch).

Per task:
```bash
awk 'BEGIN{n=0} /^---\r?$/{n++; if(n<=2) next} n>=2' <task_file> > /tmp/task-body.md
[ -s /tmp/task-body.md ] || { echo "❌ Task body is empty - refusing to post"; exit 1; }
task_url=$(gh issue create \
  --repo "$REPO" \
  --title "<task_name>" \
  --body-file /tmp/task-body.md \
  --label "task,epic:<name>")
task_number=$(echo "$task_url" | grep -oE '[0-9]+$')

# Then, if the extension is available, link it under the epic:
gh sub-issue add "$epic_number" "$task_number" --repo "$REPO"
```

**`gh sub-issue` does not take the flags you would expect.** Create the issue with
`gh issue create` and link it afterwards:

- `add` takes **positional** arguments — `gh sub-issue add <parent> <child>`. There
  is no `--parent` flag on `add`.
- `gh sub-issue create` accepts `--body` but **not** `--body-file`, which is why a
  task body of any real size goes through `gh issue create` first.

Link failures are silent if you discard stderr. Verify with
`gh sub-issue list "$epic_number" --repo "$REPO"` before moving on.

**Step 3 — Rename task files and update references:**

After all issues are created, rename `001.md` → `<issue_number>.md` and update all `depends_on`/`conflicts_with` arrays to use real issue numbers (not sequential numbers).

```bash
# Build old→new mapping, then for each task file:
sed -i.bak "s/\b001\b/<new_num_1>/g" <file>  # repeat for each mapping
mv 001.md <new_num>.md
```

**Step 4 — Update frontmatter:**
```bash
current_date=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
# Update github: and updated: fields in epic.md and each task file
github_url="https://github.com/$REPO/issues/<number>"
sed -i.bak "/^github:/c\\github: $github_url" <file>
sed -i.bak "/^updated:/c\\updated: $current_date" <file>
rm <file>.bak
```

**Step 5 — Create worktree for the epic:**
```bash
git checkout main && git pull origin main
git worktree add ../epic-<name> -b epic/<name>
```

**Step 6 — Create github-mapping.md:**
```markdown
# GitHub Issue Mapping
Epic: #<N> - https://github.com/<repo>/issues/<N>
Tasks:
- #<N>: <title> - https://github.com/<repo>/issues/<N>
Synced: <datetime>
```

**Output:**
```
✅ Synced epic <name> to GitHub
  Epic: #<N>
  Tasks: N sub-issues
  Worktree: ../epic-<name>
  Next: "start working on issue <N>" or "start the <name> epic"
```

---
