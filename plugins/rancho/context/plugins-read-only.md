# The installed plugins are read-only

Never edit, create, move, or delete anything under `~/.claude/plugins/`, even
when asked to change the law, VACA, or a skill. An edit there changes every
project on this machine, and the next plugin update discards it. Make the
change in the `vensure-marketplace` repository; if that isn't the one open, say
so and stop. A hook refuses these writes: never route around it.
