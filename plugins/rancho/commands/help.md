---
description: Show the VACA reference card — the five phases, the natural-language phrases that drive each one, and where VACA keeps its state.
disable-model-invocation: true
---

Run the reference card script and show its output to the user:

```bash
bash "${CLAUDE_PLUGIN_ROOT}/skills/vaca/scripts/help.sh"
```

Print the output verbatim. Do not summarise it, reformat it, or add commentary
around it — it is a reference card, and the user asked to read it.

If the user asked about something specific after `/rancho:help`, answer that
from the card once it is displayed.
