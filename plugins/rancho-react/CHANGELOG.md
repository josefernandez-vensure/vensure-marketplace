# Changelog

## 2.0.0

- **Breaking.** Released together with `rancho` 3.0.0, whose core law it
  carries, and `rancho-dotnet` 2.0.0. The rationale and history of each
  change are in this plugin's `RECORDS.md`, in its 3.0 stage sections
  (stage 0 to stage 8).
- **What changed:**
  - Real-time moves to `references/annex-realtime.md`, binding a tree once
    it opens its first real-time channel. 43 rows, 32 marks.
  - §1: a lint suppression names the rules it suppresses and a reason
    (FE-38); where it suppresses a law check, the reason is a register id,
    as for `@ts-expect-error`.
  - §2: `main.tsx` and `routeTree.gen.ts` move into `src/app/`. A feature's
    folder set is closed and gains `lib/` and `types/`; a store two
    features use lives in `shared/stores/` (FE-11). The identity module in
    `shared/lib/` exports the provider `app/providers.tsx` renders.
  - §3: generated types carry a class brand per `x-data-class`, and an
    unclassified property or parameter gets the `Restricted` brand (FE-22).
    Every argument to the generated client is branded. The boundary lints
    refuse an unbranded value; a literal type, `null` or `undefined`
    passes (FE-16, FE-23, FE-33). Client-only state takes its class from a
    client-state registry in `shared/lib/`; a brand assertion or
    `.brand()` outside generated code and the registry is forbidden
    (FE-34).
  - §4: query keys come from one key factory per server resource (FE-09).
    A mutation of a versioned resource sends the `etag` it read as
    `If-Match`, and a `412` refetches and tells the user (FE-36). One
    client-wide retry rule: no `4xx` but `408` and `429`, honouring
    `Retry-After` (FE-37).
  - §6: an edit form starts from server data through React Hook Form's
    `values` or `defaultValues`, carries the `etag` as a form value, and
    uses neither `keepValues` nor `keepDirtyValues` (FE-41).
  - §9: the interceptor sends `traceparent` and `tracestate` (FE-35).
    Sign-out clears the query cache, every store and everything the
    application wrote to browser storage (FE-29).
  - §10: a server error maps by `type` and `status`; `title` and field
    errors render only for a user-facing type from `x-problem-types`; a
    shown error offers its `traceId`.
  - §11: the browser's URLs and storage are inside the authenticated
    boundary only under a referrer policy of
    `strict-origin-when-cross-origin` or stricter. A persisted store names
    what it persists, such as Zustand `persist`'s `partialize` (FE-23).
  - New required tests: FE-35 to FE-37, FE-39 to FE-43.
- **What each project does to upgrade:**
  1. Do the core law's upgrade list (`rancho` 3.0.0), add FE-38 to
     `ENFORCEMENT-STATUS.md`, and re-review any built check whose row's
     Check changed (core §1, §10).
  2. Move `routeTree.gen.ts` to `src/app/routeTree.gen.ts` and set the
     generator's `generatedRouteTree` to it; move the entry module to
     `src/app/main.tsx` and point `index.html` at it (§2).
  3. Bring each feature to the closed folder set, move shared stores to
     `shared/stores/`, and place the identity module, the problem-details
     parser and the client-state registry in `shared/lib/` (§2, §3, §9).
  4. Regenerate the client from the server's new OpenAPI document, with brands per
     class and `Restricted` for an unclassified schema, adding a
     post-generation step where the generator does not see `x-data-class`
     (FE-22). Parse every generated-client argument, literals included,
     through its generated schema (§3).
  5. Create the client-state registry; validate route and search params
     through its branded schemas; remove brand assertions and `.brand()`
     calls elsewhere (§3, FE-34).
  6. Replace query-key array literals with key factories (FE-09).
  7. Configure the query client's retry rule (FE-37); send `traceparent`
     and `tracestate` from the interceptor (FE-35); send `If-Match` from
     `etag` and handle `412` (FE-36).
  8. Read user-facing problem types from `x-problem-types` in the parser
     and offer `traceId` on a shown error (§10).
  9. Rewrite edit forms as §6 states (FE-41).
  10. Set a referrer policy no weaker than `strict-origin-when-cross-origin`
      (in a host shell, declare it to the shell's owners); make sign-out
      clear browser storage; give every `persist` a `partialize` (§11,
      FE-29, FE-23).
  11. Enable `@eslint-community/eslint-plugin-eslint-comments`
      `no-unlimited-disable` and `require-description`; give each law-check
      suppression its `LAW-EXCEPTIONS.md` id; ignore generated files; use
      `boundaries/dependencies` (FE-38, FE-01, FE-03).
  12. Write the new required tests listed above.
  - Only when the real-time annex binds: keep the real-time client packages
    in the lint configuration for FE-18, scoped to
    `src/features/*/components/**`.

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
