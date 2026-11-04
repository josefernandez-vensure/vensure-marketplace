"""Rule ledger: which normative sentences of an earlier law are no longer stated.

Run from the repository root:  python -I tools/law-ledger.py <git-ref>

It collects every normative sentence - one with MUST, SHOULD, MAY, "forbidden"
or "defect" - and every table row from the law files at <git-ref>
(each law plugin's skills/law/SKILL.md and skills/law/references/annex-*.md).
It then looks for each one, verbatim after normalising marks and whitespace, in
the law files of the working tree, plus plugins/rancho/AUTHORING.md.

A restructure that changes no rule should report nothing, except sentences it
removed on purpose as duplicates of one that survives. A stage that changes
rules on purpose uses the list as its ledger: every line must be an intended
change. It proves that text moved, not that the move kept its meaning.
"""

import re
import subprocess
import sys
from pathlib import Path

PLUGINS = ("rancho", "rancho-dotnet", "rancho-react")
GEAR = "⚙"
ID = r"(?:ROOT|BE|FE)-\d+"
MARK = GEAR + r"\s*" + ID + r"(?:\s*,\s*" + ID + r")*"
NORMATIVE = re.compile(r"\b(MUST|SHOULD|MAY)\b|[Ff]orbidden|\bdefect")
SENTENCE_END = re.compile(r"(?<=[.!?])\s+(?=[A-Z`*(\"])")


def norm(text):
    text = text.replace("`" + GEAR + "`", GEAR)
    text = re.sub(MARK, "", text)
    text = re.sub(r"\s*\(" + ID + r"(?:\s*,\s*" + ID + r")*\)", "", text)
    text = text.replace(GEAR, "")
    text = re.sub(r"^\s*(?:[-*]|\d+\.)\s+", "", text)
    return re.sub(r"\s+", " ", text).strip()


def sentences(line):
    """Split a line into sentences. A mark ends the sentence it marks."""
    out = []
    for part in re.split("(" + MARK + ")", line):
        if re.fullmatch(MARK, part):
            if out:
                out[-1] += " " + part
            continue
        out.extend(SENTENCE_END.split(part))
    return out


def items(text):
    """Normative sentences and table rows, with their line numbers."""
    out = []
    fenced = False
    for n, line in enumerate(text.splitlines(), 1):
        if line.strip().startswith("```"):
            fenced = not fenced
            continue
        if fenced or not line.strip() or line.startswith("#"):
            continue
        if line.lstrip().startswith("|"):
            if not re.match(r"\s*\|[\s:|-]+\|\s*$", line):
                out.append((n, norm(line)))
            continue
        for sentence in sentences(line):
            if NORMATIVE.search(sentence):
                s = norm(sentence)
                if s:
                    out.append((n, s))
    return out


def git_show(ref, path):
    r = subprocess.run(["git", "show", f"{ref}:{path}"], capture_output=True, text=True, encoding="utf-8")
    return r.stdout if r.returncode == 0 else None


def law_paths_at(ref, plugin):
    base = f"plugins/{plugin}/skills/law"
    r = subprocess.run(["git", "ls-tree", "-r", "--name-only", ref, base], capture_output=True, text=True)
    return [p for p in r.stdout.splitlines()
            if p.endswith("/SKILL.md") or re.search(r"/references/annex-[\w-]+\.md$", p)]


def main():
    if len(sys.argv) != 2:
        print(__doc__)
        return 2
    ref = sys.argv[1]
    current = []
    for plugin in PLUGINS:
        base = Path("plugins") / plugin / "skills" / "law"
        for f in [base / "SKILL.md"] + sorted((base / "references").glob("annex-*.md")):
            current.append(f.read_text(encoding="utf-8"))
    authoring = Path("plugins/rancho/AUTHORING.md")
    if authoring.exists():
        current.append(authoring.read_text(encoding="utf-8"))
    haystack = norm(" ".join(norm(line) for text in current for line in text.splitlines()))

    missing = 0
    for plugin in PLUGINS:
        for path in law_paths_at(ref, plugin):
            text = git_show(ref, path)
            for n, s in items(text or ""):
                if s not in haystack:
                    missing += 1
                    print(f"{path}:{n}: {s[:220]}")
    print(f"\n{missing} normative sentence(s) or table row(s) of {ref} are not stated verbatim now.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
