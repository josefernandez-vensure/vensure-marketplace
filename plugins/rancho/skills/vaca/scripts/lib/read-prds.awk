# Scan PRD files and emit one record per PRD.
#
#   slug <US> name <US> status <US> description <US> created
#
# Call as: awk -f lib/read-prds.awk .claude/prds/*.md
# See lib/read-tasks.awk for why this is a single pass.

BEGIN { US = sprintf("%c", 31) }

function flush() {
  if (file == "") return
  if (name == "") name = slug
  if (status == "") status = "backlog"
  if (description == "") description = "No description"
  printf "%s" US "%s" US "%s" US "%s" US "%s\n", \
         slug, name, tolower(status), description, created
  file = ""; name = ""; status = ""; description = ""; created = ""
}

FNR == 1 {
  flush()
  file = FILENAME
  n = split(FILENAME, parts, "/")
  slug = parts[n]; sub(/\.md$/, "", slug)
  infm = 0; seen = 0
}

/^---[ \t]*$/ { seen++; infm = (seen == 1); next }

infm && seen == 1 {
  if (match($0, /^[A-Za-z_]+:/)) {
    key = substr($0, 1, RLENGTH - 1)
    val = substr($0, RLENGTH + 1)
    gsub(/^[ \t]+|[ \t\r]+$/, "", val)
    if (key == "name")             name = val
    else if (key == "status")      status = val
    else if (key == "description") description = val
    else if (key == "created")     created = val
  }
}

END { flush() }
