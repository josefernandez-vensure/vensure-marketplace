# Scan task files and emit one record per task.
#
#   epic <US> num <US> status <US> parallel <US> deps <US> name
#
# deps is space-separated issue numbers, or empty.
# One awk process reads every task file, so a report costs one spawn instead of
# three per field per file. That matters: process creation is ~0.5s on Windows
# under EDR, which made the per-file grep|head|sed loops unusable at scale.
#
# Call as: awk -f lib/read-tasks.awk .claude/epics/*/[0-9]*.md
# The glob excludes .claude/epics/archived/<name>/<n>.md by depth, so archived
# epics never reach a live report.

BEGIN { US = sprintf("%c", 31) }

function flush(   d) {
  if (file == "") return
  d = deps
  gsub(/[\[\]",]/, " ", d)
  gsub(/^[ 	]+|[ 	]+$/, "", d)
  gsub(/[ 	]+/, " ", d)
  if (status == "") status = "open"
  printf "%s" US "%s" US "%s" US "%s" US "%s" US "%s\n", epic, num, status, parallel, d, name
  file = ""; status = ""; parallel = ""; deps = ""; name = ""
}

FNR == 1 {
  flush()
  file = FILENAME
  n = split(FILENAME, parts, "/")
  num = parts[n]; sub(/\.md$/, "", num)
  epic = (n >= 2) ? parts[n-1] : ""
  infm = 0; seen = 0
}

# Track the frontmatter block: it is the first --- ... --- pair.
/^---[ 	]*$/ {
  seen++
  infm = (seen == 1)
  next
}

infm && seen == 1 {
  line = $0
  if (match(line, /^[A-Za-z_]+:/)) {
    key = substr(line, 1, RLENGTH - 1)
    val = substr(line, RLENGTH + 1)
    gsub(/^[ 	]+|[ \t\r]+$/, "", val)
    if (key == "status")        status = val
    else if (key == "parallel") parallel = val
    else if (key == "depends_on") deps = val
    else if (key == "name")     name = val
  }
}

END { flush() }
