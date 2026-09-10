# Scan epic files and emit one record per epic.
#
#   dir <US> name <US> status <US> progress <US> github <US> created
#
# Call as: awk -f lib/read-epics.awk .claude/epics/*/epic.md
# See lib/read-tasks.awk for why this is a single pass.

BEGIN { US = sprintf("%c", 31) }

function flush() {
  if (file == "") return
  if (name == "") name = dirname
  if (progress == "") progress = "0%"
  if (status == "") status = "planning"
  printf "%s" US "%s" US "%s" US "%s" US "%s" US "%s\n", \
         dirname, name, tolower(status), progress, github, created
  file = ""; name = ""; status = ""; progress = ""; github = ""; created = ""
}

FNR == 1 {
  flush()
  file = FILENAME
  n = split(FILENAME, parts, "/")
  dirname = (n >= 2) ? parts[n-1] : ""
  infm = 0; seen = 0
}

/^---[ \t]*$/ { seen++; infm = (seen == 1); next }

infm && seen == 1 {
  if (match($0, /^[A-Za-z_]+:/)) {
    key = substr($0, 1, RLENGTH - 1)
    val = substr($0, RLENGTH + 1)
    gsub(/^[ \t]+|[ \t\r]+$/, "", val)
    if (key == "name")          name = val
    else if (key == "status")   status = val
    else if (key == "progress") progress = val
    else if (key == "github")   github = val
    else if (key == "created")  created = val
  }
}

END { flush() }
