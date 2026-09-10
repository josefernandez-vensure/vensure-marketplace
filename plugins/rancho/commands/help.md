---
description: Show the VACA reference card — the five phases, the natural-language phrases that drive each one, and where VACA keeps its state.
disable-model-invocation: true
---

Run the reference card script and show its output to the user:

```bash
bash "${CLAUDE_PLUGIN_ROOT}/skills/vaca/scripts/help.sh"
```

If `${CLAUDE_PLUGIN_ROOT}` reaches you unexpanded and is not set in the shell,
resolve it as the directory holding this plugin's `.claude-plugin/plugin.json`
— the same `skills/vaca/scripts/help.sh` beneath it. Never fall back to
reproducing the card from memory or by reading the script: run it, so the
output cannot drift from what the script actually prints.

Print the output verbatim. Do not summarise it, reformat it, or add commentary
around it — it is a reference card, and the user asked to read it.

If the user asked about something specific after `/rancho:help`, answer that
from the card once it is displayed.
