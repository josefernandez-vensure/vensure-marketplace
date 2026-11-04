# Changelog

## 1.1.0

- Amendment A-10, from a consuming project's compliance audit of 2026-10-30
  (items 5 and 8). Record in `references/enforcement.md`. Carries the core
  law's 2.1.0 (A-08).
- §3: generated types carry the class of every property and parameter as a
  type the checker reads, such as a brand per class (FE-22). Type-aware lints
  at the URL, storage and telemetry calls read it (FE-16, FE-23), and the
  telemetry lint (FE-33) supports FE-17's required test. User input takes its
  brand only by parsing through a schema the generator emits; asserting a
  brand outside generated code is forbidden (FE-34). A value with no brand
  passes, a gap recorded in FE-16. **Each consumer regenerates against a server that emits
  request classes** (`rancho-dotnet` 1.1.0, BE-83).
- §9: an application in a host shell clears its own caches and stores and
  delegates sign-out to the shell's sign-out, which MUST end the provider
  session. A standalone application calls the end-session endpoint itself.
  §13's sign-out test follows (FE-29). In a host shell, the refresh after a
  `401`, the session store and the grant request are the shell's too
  (FE-27).
- Rows go from 32 to 34.

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
