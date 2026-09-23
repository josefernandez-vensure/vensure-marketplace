# Conventions — File Formats, Paths & Rules

Read this before doing any file operations across all phases.

---

## Directory Structure

```
.claude/
├── prds/
│   └── <feature-name>.md          # Product requirement documents
├── epics/
│   ├── <feature-name>/
│   │   ├── epic.md                # Technical epic
│   │   ├── <N>.md                 # Task files (named by GitHub issue number after sync)
│   │   ├── <N>-analysis.md        # Parallel work stream analysis
│   │   ├── github-mapping.md      # Issue number → URL mapping
│   │   ├── execution-status.md    # Active agents tracker
│   │   └── updates/
│   │       └── <issue_N>/
│   │           ├── stream-A.md    # Per-agent progress
│   │           ├── progress.md    # Overall issue progress
│   │           └── execution.md  # Execution state
│   └── archived/
│       └── <feature-name>/        # Completed epics
└── context/                       # Project context docs (separate system)
```

---

## Frontmatter Schemas

### PRD (.claude/prds/<name>.md)
```yaml
---
name: <feature-name>        # kebab-case, matches filename
description: <one-liner>    # used in lists and summaries
status: backlog | active | completed
created: <ISO 8601>         # date -u +"%Y-%m-%dT%H:%M:%SZ"
---
```

### Epic (.claude/epics/<name>/epic.md)
```yaml
---
name: <feature-name>
status: backlog | in-progress | completed
created: <ISO 8601>
updated: <ISO 8601>
progress: 0%                # recalculated when tasks close
prd: .claude/prds/<name>.md
github: https://github.com/<owner>/<repo>/issues/<N>  # set on sync
---
```

### Task (.claude/epics/<name>/<N>.md)
```yaml
---
name: <Task Title>
status: open | in-progress | closed
created: <ISO 8601>
updated: <ISO 8601>
github: https://github.com/<owner>/<repo>/issues/<N>  # set on sync
depends_on: []              # issue numbers this must wait for
parallel: true              # can run concurrently with non-conflicting tasks
conflicts_with: []          # issue numbers that touch the same files
---
```

### Progress (.claude/epics/<name>/updates/<N>/progress.md)
```yaml
---
issue: <N>
started: <ISO 8601>
last_sync: <ISO 8601>
completion: 0%
---
```

### Work stream (.claude/epics/<name>/updates/<N>/stream-<X>.md)
```yaml
---
issue: <N>
stream: <X>                 # A, B, C ...
name: <stream name>
agent: vaca-stream
status: in_progress | blocked | completed
started: <ISO 8601>
updated: <ISO 8601>         # moves with every checkpoint, never behind one
completion: 0%
checkpoint: <done>/<total>  # checkpoints ticked / planned
last_commit: <sha>          # newest commit this state reflects
---
```

The body carries `## Scope`, `## Checkpoints`, `## Notes`, and `## Blockers`.
Checkpoints are a written-up-front list of independently committable units,
ticked as `- [x] 1. <unit> — <sha> — <ISO 8601>`.

**The stream file is updated immediately after each checkpoint commits, not at
the end of the stream.** Reports, standups, and the session that picks the work
up after a `/clear` all read these files and nothing else, so a file that runs
ahead of the commits is a lie and a file that lags behind them is a stranded
stream nobody can tell from an abandoned one. `last_commit` is what makes the
claim checkable against `git log` instead of merely believed.

---

## Datetime Rule

Always get real current datetime from the system — never use placeholder text:
```bash
date -u +"%Y-%m-%dT%H:%M:%SZ"
```

---

## Frontmatter Update Pattern

When updating a single frontmatter field in an existing file:
```bash
sed -i.bak "/^<field>:/c\\<field>: <value>" <file>
rm <file>.bak
```

When stripping frontmatter to get body content for GitHub:
```bash
awk 'BEGIN{n=0} /^---\r?$/{n++; if(n<=2) next} n>=2' <file> > /tmp/body.md
[ -s /tmp/body.md ] || { echo "❌ Body is empty - refusing to post"; exit 1; }
```

**Use this, not `sed '1,/^---$/d; 1,/^---$/d'`.** That idiom silently empties the
file whenever the body carries no further `---` line, which is the normal case for
an epic or a task. The second range opens at line 1, finds that the first range has
already consumed the closing delimiter, and deletes to end of file; it appears to
work only on documents that happen to contain a horizontal rule. The `awk` form
keys on the first two delimiters, tolerates CRLF, and cannot run past them.

The emptiness check is not optional. An empty body is not visibly wrong until the
issue is already posted, and a posted issue is a write nobody can take back.

---

## GitHub Operations

### Repository Safety Check (run before any write operation)
```bash
remote_url=$(git remote get-url origin 2>/dev/null || echo "")
REPO=$(echo "$remote_url" | sed 's|.*github.com[:/]||' | sed 's|\.git$||')
```

### Authentication
Don't pre-check authentication. Run the `gh` command and handle failure:
```bash
gh <command> || echo "❌ GitHub CLI failed. Run: gh auth login"
```

### Getting Issue Numbers
```bash
# From a task file's github field:
grep 'github:' <file> | grep -oE '[0-9]+$'
```

---

## Git / Worktree Conventions

**Code is written in the worktree. State is written at the project root.**
Every `.claude/` path in these references resolves against the project root,
including the progress and stream files that agents working inside a worktree
write. The tracking scripts, the status reports, and the next session after a
`/clear` all read `.claude/` from the project root; state written into the
worktree copy stays invisible to all of them until the epic merges, which is
after the point where anyone needed it.

- One branch per epic: `epic/<name>`
- Worktrees live at `../epic-<name>/` (sibling to project root)
- Always start branches from an up-to-date main:
  ```bash
  git checkout main && git pull origin main
  git worktree add ../epic-<name> -b epic/<name>
  ```
- Commit format inside epics: `Issue #<N>: <description>`
- Never use `--force` in any git operation

---

## Naming Conventions

- Feature names: kebab-case, lowercase, letters/numbers/hyphens, starts with a letter
- Task files before sync: `001.md`, `002.md`, ... (sequential)
- Task files after sync: renamed to GitHub issue number (e.g., `1234.md`)
- Labels applied on sync: `epic`, `epic:<name>`, `feature` (for epics); `task`, `epic:<name>` (for tasks)

---

## Epic Progress Calculation

```bash
total=$(ls .claude/epics/<name>/[0-9]*.md 2>/dev/null | wc -l)
closed=$(grep -l '^status: closed' .claude/epics/<name>/[0-9]*.md 2>/dev/null | wc -l)
progress=$((closed * 100 / total))
```

Update epic frontmatter when any task closes.