# Scan per-issue progress files and emit one record each.
#
#   epic <US> issue <US> completion <US> last_sync
#
# Call as: awk -f lib/read-progress.awk .claude/epics/*/updates/*/progress.md
# See lib/read-tasks.awk for why this is a single pass.

BEGIN { US = sprintf("%c", 31) }

function flush() {
  if (file == "") return
  if (completion == "") completion = "0%"
  printf "%s" US "%s" US "%s" US "%s\n", epic, issue, completion, last_sync
  file = ""; completion = ""; last_sync = ""
}

FNR == 1 {
  flush()
  file = FILENAME
  n = split(FILENAME, parts, "/")
  # .../epics/<epic>/updates/<issue>/progress.md
  issue = (n >= 2) ? parts[n-1] : ""
  epic  = (n >= 4) ? parts[n-3] : ""
  infm = 0; seen = 0
}

/^---[ 	]*$/ { seen++; infm = (seen == 1); next }

infm && seen == 1 {
  if (match($0, /^[A-Za-z_]+:/)) {
    key = substr($0, 1, RLENGTH - 1)
    val = substr($0, RLENGTH + 1)
    gsub(/^[ 	]+|[ \t\r]+$/, "", val)
    if (key == "completion")     completion = val
    else if (key == "last_sync") last_sync = val
  }
}

END { flush() }
