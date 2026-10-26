# Changelog

## 1.0.0

- Split out of rancho 1.7.0, where this stack law was
  `skills/law/references/frontend.md`. Released with rancho 2.0.0, whose core
  law it carries; amendment A-07 in the core inventory records why.
- Opens with the sections every stack law carries under core §2: its role, a
  core map from each core section to the section here that carries it, and
  its gaps. Then the stack law itself, §1–§14, numbered as before and changed
  only as below. References to the root law now read core.
- `references/enforcement.md` holds this stack law's rows, the core rules its
  role carries under **Core rules carried**, and the records that concern this
  stack law alone.
- §12 no longer assumes the server's stack. Real-time messages over SignalR,
  or any other channel the server offers, update the TanStack Query cache, and
  `FE-18` reads the real-time client packages from the project's lint
  configuration, beside the identity and UI-library lists. **Each consumer adds
  that list.**
- §2, §3 and §6 say server where they said backend. The rules are unchanged.
- Gap, listed in the core map: how a client carries the correlation id core §9
  requires across the server-to-client boundary is not yet stated.
