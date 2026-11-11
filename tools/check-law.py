"""Consistency check for the engineering law.

Run from the repository root:  python -I tools/check-law.py

It reads each law plugin's skills/law/SKILL.md, its annexes
(skills/law/references/annex-*.md), the core's contract law
(plugins/rancho/skills/law/references/contract.md) and each inventory
(skills/law/references/enforcement.md), and fails on bookkeeping that does not
agree: marks without ids, ids without rows, rows nothing marks, required tests
the Testing section does not name, broken canonical pointers, core rules a
stack law does not carry, and summary counts that differ from what it counts.

It proves the law's bookkeeping, not its meaning. Whether a rule and its check
cover the same ground is still a reviewer's question.

Conventions it relies on:
- A mark is the gear glyph followed by the ids of the rows it marks, such as
  "... is a defect. <gear> BE-34" or "<gear> BE-01, BE-105". The glyph written
  as prose sits in a code span and is not a mark.
- A required test names the ids it carries in its law's Testing section.
- An inventory row is a table line whose first cell is an id.
"""

import re
import sys
from pathlib import Path

GEAR = "⚙"
ID = r"(?:ROOT|BE|FE)-\d+"
STATUSES = {"build", "required-test", "dropped", "retired", "reword"}
LAWS = {"rancho": "ROOT", "rancho-dotnet": "BE", "rancho-react": "FE"}

errors = []


def fail(where, msg):
    errors.append(f"{where}: {msg}")


def strip_code(text):
    """Remove fenced blocks and inline code spans, keeping line structure."""
    text = re.sub(r"```.*?```", lambda m: "\n" * m.group(0).count("\n"), text, flags=re.S)
    return re.sub(r"`[^`\n]*`", "", text)


def law_files(plugin):
    base = Path("plugins") / plugin / "skills" / "law"
    files = [base / "SKILL.md"] + sorted((base / "references").glob("annex-*.md"))
    if plugin == "rancho":
        files.append(base / "references" / "contract.md")
    return base, files


def read_marks(path):
    """Return a list of (line number, [ids]) for every mark in a file."""
    marks = []
    for n, line in enumerate(strip_code(path.read_text(encoding="utf-8")).splitlines(), 1):
        for m in re.finditer(GEAR, line):
            ids = re.match(r"\s*(%s(?:\s*,\s*%s)*)" % (ID, ID), line[m.end():])
            if not ids:
                fail(f"{path}:{n}", "mark with no id after it")
                marks.append((n, []))
            else:
                marks.append((n, re.findall(ID, ids.group(1))))
    return marks


def testing_ids(path):
    """Ids named in a file's Testing section(s)."""
    found, inside, level = set(), False, 0
    for line in path.read_text(encoding="utf-8").splitlines():
        h = re.match(r"(#+)\s+(.*)", line)
        if h:
            if inside and len(h.group(1)) <= level:
                inside = False
            if re.search(r"\bTesting\b", h.group(2)):
                inside, level = True, len(h.group(1))
            continue
        if inside:
            found.update(re.findall(ID, line))
    return found


def read_inventory(path):
    """Return {id: {"status", "cells", "line", "section"}} and the raw text."""
    rows, section = {}, ""
    text = path.read_text(encoding="utf-8")
    for n, line in enumerate(text.splitlines(), 1):
        if line.startswith("#"):
            section = line.lstrip("# ").strip()
            continue
        m = re.match(r"\|\s*(%s)\s*\|" % ID, line)
        if not m:
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        status = next((c for c in cells if c in STATUSES), None)
        if status is None:
            fail(f"{path}:{n}", f"row {m.group(1)} has no recognised status")
        key = (m.group(1), section if section.startswith("Core rules carried") else "")
        if key in rows:
            fail(f"{path}:{n}", f"row {m.group(1)} appears twice")
        rows[key] = {"status": status, "cells": cells, "line": n, "section": section}
    return rows, text


def summary_numbers(text):
    s = {}
    m = re.search(r"carries \*\*(\d+) %s marks\*\*" % GEAR, text)
    if m:
        s["marks"] = int(m.group(1))
    m = re.search(r"\*\*(\d+) rows\*\*:([^.]*)", text)
    if m:
        s["rows"] = int(m.group(1))
        for n, status in re.findall(r"(\d+) `([\w-]+)`", m.group(2)):
            s[status] = int(n)
    m = re.search(r"carries \*\*(\d+) core rules?\*\*", text)
    if m:
        s["core"] = int(m.group(1))
    m = re.search(r"\|\s*`\.\./SKILL\.md`\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|\s*(\d+)\s*\|", text)
    if m:
        s["rows"], s["build"], s["required-test"], s["dropped"] = map(int, m.groups())
    return s


def role_of(skill_text):
    m = re.search(r"^## Role\s*\n+`(\w+)`", skill_text, re.M)
    return m.group(1) if m else None


def words(path):
    return len(path.read_text(encoding="utf-8").split())


def main():
    data = {}
    for plugin, prefix in LAWS.items():
        base, files = law_files(plugin)
        inv_path = base / "references" / "enforcement.md"
        skill_text = (base / "SKILL.md").read_text(encoding="utf-8")
        rows, inv_text = read_inventory(inv_path)
        marks = [(f, n, ids) for f in files for n, ids in read_marks(f)]
        tested = set().union(*(testing_ids(f) for f in files))
        data[plugin] = dict(prefix=prefix, base=base, files=files, rows=rows,
                            inv_text=inv_text, marks=marks, tested=tested,
                            role=role_of(skill_text), skill_text=skill_text)

        own = {k[0]: v for k, v in rows.items() if not k[1] and k[0].startswith(prefix + "-")}
        marked = {}
        for f, n, ids in marks:
            for i in ids:
                marked.setdefault(i, []).append(f"{f.name}:{n}")
                if not i.startswith(prefix + "-"):
                    fail(f"{f}:{n}", f"marks {i}, which is not this law's id")
                elif i not in own:
                    fail(f"{f}:{n}", f"marks {i}, which has no inventory row")
                elif own[i]["status"] != "build":
                    fail(f"{f}:{n}", f"marks {i}, whose status is {own[i]['status']}")
        for i, r in own.items():
            where = f"{inv_path}:{r['line']}"
            if r["status"] == "build" and i not in marked:
                fail(where, f"{i} is `build` but no mark carries it")
            if r["status"] == "required-test" and i not in tested:
                fail(where, f"{i} is `required-test` but no Testing section names it")
        for i in tested:
            if i.startswith(prefix + "-") and i not in own:
                fail(str(base / "SKILL.md"), f"Testing names {i}, which has no row")

        # canonical and covered-by pointers
        for (i, _), r in rows.items():
            for kind, target in re.findall(r"(Canonical|Covered by|Merged into):?\s+(%s)" % ID, " ".join(r["cells"])):
                tgt = own.get(target)
                if tgt is None and target.startswith(prefix + "-"):
                    fail(f"{inv_path}:{r['line']}", f"{i} points to {target}, which has no row")
                elif kind == "Canonical" and tgt is not None:
                    if any(c.startswith(("Covered by", "Merged into")) for c in tgt["cells"][1:]):
                        fail(f"{inv_path}:{r['line']}", f"{i}'s canonical row {target} is only covered by another")

        # summary counts
        s = summary_numbers(inv_text)
        n_marks = sum(1 for _, _, ids in marks if ids)
        counted = {"marks": n_marks, "rows": len(own),
                   "build": sum(r["status"] == "build" for r in own.values()),
                   "required-test": sum(r["status"] == "required-test" for r in own.values()),
                   "dropped": sum(r["status"] == "dropped" for r in own.values()),
                   "retired": sum(r["status"] == "retired" for r in own.values())}
        if prefix != "ROOT":
            counted["core"] = sum(1 for k in rows if k[1])
        for k, v in s.items():
            if k in counted and counted[k] != v:
                fail(str(inv_path), f"summary says {v} {k}, counted {counted[k]}")
        missing = [k for k in ("marks", "rows", "build") if k not in s]
        if missing:
            fail(str(inv_path), f"summary states no count for: {', '.join(missing)}")

        # annex triggers
        named = set(re.findall(r"annex-[\w-]+\.md", skill_text))
        present = {f.name for f in files[1:] if f.name.startswith("annex-")}
        for a in present - named:
            fail(str(base / "SKILL.md"), f"names no trigger for {a}")
        for a in named - present:
            fail(str(base / "SKILL.md"), f"names {a}, which does not exist")

    # core rules carried by each stack law of the role they bind
    core_rows = {k[0]: v for k, v in data["rancho"]["rows"].items() if not k[1]}
    for i, r in core_rows.items():
        if r["status"] != "build":
            continue
        binds = next((c for c in r["cells"] if c in {"server", "client", "both"}), None)
        for plugin in ("rancho-dotnet", "rancho-react"):
            d = data[plugin]
            if binds in (d["role"], "both"):
                if not any(k[0] == i and k[1] for k in d["rows"]):
                    fail(f"plugins/{plugin}", f"does not carry core rule {i} (binds {binds})")

    # size report
    print("Size (words, ~tokens at 1.4 tokens per word):")
    load = {}
    for plugin, d in data.items():
        for f in d["files"] + [d["base"] / "references" / "enforcement.md"]:
            w = words(f)
            print(f"  {f}: {w} words, ~{round(w * 1.4 / 1000, 1)}k tokens")
        load[plugin] = words(d["base"] / "SKILL.md")
    contract = words(data["rancho"]["base"] / "references" / "contract.md")
    print(f"  load per stream bound to none (core SKILL.md): ~{round(load['rancho'] * 1.4 / 1000, 1)}k tokens")
    for plugin in ("rancho-dotnet", "rancho-react"):
        w = load["rancho"] + contract + load[plugin]
        print(f"  load per {plugin} stream (core + contract + stack SKILL.md, no annexes): ~{round(w * 1.4 / 1000, 1)}k tokens")
    standalone = Path("plugins/rancho-modular-dotnet/skills/law/SKILL.md")
    if standalone.exists():
        w = words(standalone) + contract
        print(f"  load per rancho-modular-dotnet stream (standalone law + contract): ~{round(w * 1.4 / 1000, 1)}k tokens")

    if errors:
        print(f"\n{len(errors)} problem(s):")
        for e in errors:
            print("  " + e)
        return 1
    print("\nThe law's bookkeeping is consistent.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
