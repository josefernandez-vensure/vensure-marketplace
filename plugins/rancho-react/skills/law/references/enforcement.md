# Enforcement Inventory - React

Every rule marked ⚙ in the React stack law, every core rule its role carries, and the mechanism that enforces each.

`../SKILL.md` is the stack law. The core law is `/rancho:law`; its own inventory defines ids and status, and holds the records that concern the core law or more than one stack law. Core §1 defines ⚙ as **"machine-checked. Violating one is a build failure, not a review comment."** This file is the audit of that claim for this stack law: every rule still carrying a mark can be made to fail the build. A ⚙ with no entry here is unaccounted for and MUST be added.

## How to read this

**Prefix:** `FE`. Ids are permanent: `FE-21` stays `FE-21` when sections renumber. The Section column is maintained; the id never changes.

**Mechanism** - what does the checking:

| Code | Mechanism | Fails at |
|---|---|---|
| `proj` | Project or build configuration - `tsconfig`, banned packages | Compile |
| `eslint` | ESLint rule, off-the-shelf or custom | Lint |
| `ci` | Repository-level script - path checks, generated-artifact freshness, diff checks | CI |
| `test` | A behavioural test that a human must write **per instance** | Test run |

**Status:**

| Status | Meaning |
|---|---|
| `build` | Enforceable as worded. A violation can be made to fail the build. |
| `required-test` | No build check can reach it. The ⚙ is stripped and the obligation is named in a Testing section, which is where it is now caught. See [The badge problem](#the-badge-problem). |
| `dropped` | The ⚙ was removed. The rule still stands - it is enforced by review, not by the build. |
| `reword` | Not decidable as worded. Resolved in [Rewordings applied](#rewordings-applied). None remain. |

**Effort** is for the check, not the rule: `S` under an hour, `M` a day, `L` more than a day.

## Summary

The stack law carries **26 ⚙ marks**. Two sit on a section heading, §3 and §14, and cover everything beneath them. Its rules are **32 rows**: 28 `build`, 3 `required-test`, and 1 `dropped`. Its role, `client`, carries **1 core rule**, below.

**Every marked rule can be made to fail the build.** No rule claims an enforcement it cannot deliver. Nothing is enforced yet, though - `build` means the check is buildable, not built.

## Core rules carried

Every core rule whose role is `client` or `both` and whose status is `build`. The section is the core law's; the rule is worded as this stack law expresses it.

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| ROOT-18 | A field's class travels with its contract, into the OpenAPI document and the generated types | §8 | `ci` | build | S | Canonical: FE-22. The server half, emitting the class into the document, is the server stack law's |

## Stack law - `../SKILL.md`

| Id | Rule | Section | Mechanism | Status | Effort | Check |
|---|---|---|---|---|---|---|
| FE-01 | `strict` on; no `any`; no type-checker suppression but a justified `@ts-expect-error` | §1 | `proj` + `eslint` | build | S | `tsconfig`; `no-explicit-any`; `ban-ts-comment` with every directive banned and `ts-expect-error` set to `allow-with-description` |
| FE-02 | `@ts-expect-error` requires a reason and an issue reference | §1 | `eslint` | build | S | `ban-ts-comment` with `descriptionFormat` |
| FE-03 | A feature MUST NOT import from another feature | §2 | `eslint` | build | S | `eslint-plugin-boundaries` |
| FE-04 | Dependency direction `shared` -> `features` -> `app`; no layer imports upward | §2 | `eslint` | build | S | `eslint-plugin-boundaries`, one `rules` entry per layer |
| FE-28 | Every feature is declared in `src/features/REGISTRY.md` | §2 | `ci` | build | S | Added by A-01, scoped by A-02. Listing of `src/features/` compared against the registry. No backend input |
| FE-05 | API types generated into `src/generated/` | §3 | `ci` | build | S | Regenerate and diff, with ROOT-09 |
| FE-06 | `src/generated/` MUST NOT be hand-edited | §3 | `ci` | build | S | Diff check against a fresh generation |
| FE-07 | A generated client response MUST NOT be cast, asserted, or re-declared | §3 | `eslint` | build | M | R-02 applied: `no-restricted-syntax` on casts of generated types |
| FE-08 | All server data through TanStack Query; no `useEffect` + `fetch` | §4 | `eslint` | build | M | Path-scoped `no-restricted-globals` on `fetch`; custom rule for the `useEffect` pattern |
| FE-09 | Query keys come from a per-feature factory | §4 | `eslint` | build | M | Custom rule: `queryKey` must be a call expression, never an array literal |
| FE-10 | Every list query paginated | §4 | — | dropped | — | R-13 applied: enforced server-side by BE-19. Client half is now FE-21 |
| FE-11 | Stores are feature-scoped | §5 | `eslint` | build | S | Path-scoped restriction on the `zustand` import |
| FE-12 | RHF + Zod only; no other form library, and no validation through the UI library's validator | §6 | `eslint` | build | S | Amended by A-06. `no-restricted-imports` on other form libraries and on the UI library's validator, both listed in the project's lint configuration. Binding a UI-library input through `Controller`, and its error text through the input's own error property, are reached by review |
| FE-13 | Components MUST NOT call `fetch` or the generated client directly | §7 | `eslint` | build | S | Path-scoped `no-restricted-imports` |
| FE-14 | Stylesheets enter only through the global stylesheet and its one layout partial; the partial never targets a UI-library component; no static object literal in `style` | §7 | `eslint` + `ci` | build | S | R-14 applied, amended by A-06. `eslint`: a stylesheet import in any module but the global stylesheet's entry. `ci`: a selector in the layout partial matching the UI library's class prefixes, listed in the project's lint configuration |
| FE-15 | Exactly one `ProblemDetails` parser, in `shared/lib/` | §10 | `eslint` | build | S | Only that path may reference the type |
| FE-16 | Classified values MUST NOT appear in any part of a URL | §11 | `eslint` | build | M | Mark restored by P-03. Rule reads the class off the generated type |
| FE-17 | Analytics, telemetry, and error reporting MUST scrub classified values | §11 | `test` | required-test | M | frontend §13 **Data boundaries**. Mark stripped |
| FE-21 | No loop that exhausts `fetchNextPage` to assemble a whole collection | §4 | `eslint` | build | M | Added by R-13. `no-restricted-syntax`: `fetchNextPage` inside a loop or self-recursive call |
| FE-22 | Generated types carry the data class of every property | §3 | `ci` | build | S | Added by P-03. Generation output asserted to contain the class |
| FE-23 | Classified values MUST NOT be written to browser storage of any kind | §11 | `eslint` | build | M | Enabled by P-03. Same rule family as FE-16. The identity library's session store is excepted in the rule itself (A-05), so rule and check still cover the same ground: the library's own writes are not application source. Any application write to one of that store's configured keys is flagged, inside the FE-24 module or out |
| FE-24 | Token acquisition lives in exactly one place in `shared/lib/`; nothing else calls the identity library or touches its session store | §9 | `eslint` | build | S | Amended by A-05. The identity library's packages and its session store's keys are listed in the project's lint configuration, as FE-30's claim names are. Flags an import of a listed package, or a storage read or write of a listed key, outside that one module |
| FE-25 | A token MUST NOT be retained by the application beyond the request - in browser storage of any kind, a store, or a module-level cache | §9 | `eslint` | build | M | Same rule family as FE-23, and the same list of storage APIs: the two rules previously named different ones. Amended by A-05: also flags a token value assigned to a module-scope binding. The library's own session store is not application source. The shortest-lived-store clause is configuration, reached by review, not by this check |
| FE-26 | The token is attached by exactly one interceptor | §9 | `eslint` | build | M | No literal `Authorization` header outside that module |
| FE-27 | `401` retries once silently then goes interactive; `403` never refreshes | §9 | `test` | required-test | M | frontend §13 **Authentication**. Amended by A-05: in a host shell, the interactive step is the shell's sign-in page |
| FE-29 | Sign-out ends the identity-provider session, not only local caches | §9 | `test` | required-test | M | Added by A-03. frontend §13 **Authentication** |
| FE-30 | A credential nested inside a token MUST NOT be extracted, stored, forwarded, or logged | §9 | `eslint` | build | S | Added by A-03. Same rule family as FE-25: reads of the configured nested-credential claim names off a decoded token |
| FE-31 | No wrapper, adapter or re-export of a UI-library component; `shared/` imports neither the UI library nor its component library | §7 | `eslint` | build | S | Added by A-06. `boundaries` / path-scoped `no-restricted-imports` refusing the listed UI packages under `src/shared/**`; `no-restricted-syntax` on an `export … from` of a listed UI package. A wrapper, adapter or restyle written inside a feature, without a re-export, is reached by review |
| FE-32 | A UI-class package is a dependency only if the UI library depends on it; application code imports no UI-class package but the UI library and its component library | §1 | `ci` + `eslint` | build | S | Added by A-06. `ci`: the manifest's UI-class dependencies compared against the UI library's published dependency closure. `eslint`: `no-restricted-imports` on the UI-class packages in that closure, minus the UI library and its component library. The closure also holds packages the application uses in its own right, such as its runtime; those are outside the rule and outside the check. The UI-class packages are listed in the project's lint configuration |
| FE-18 | Components MUST NOT subscribe to hubs directly | §12 | `eslint` | build | S | Path-scoped restriction on the real-time client packages, listed in the project's lint configuration as FE-24's are. Amended by A-07 |
| FE-19 | MSW for all mocking; no module mock of the generated client | §13 | `eslint` | build | S | `no-restricted-syntax` on any `vi` module-mocking call with a generated-path argument |
| FE-20 | `pnpm lint && pnpm typecheck && pnpm test` gate every change | §14 | `ci` | build | S | CI pipeline |

## Rewordings applied

Three of the fifteen rewordings concern this stack law alone, and are kept here. R-01 to R-03 are in the core inventory, R-04 to R-12 in `rancho-dotnet`'s. Each entry records the rule, why the original wording resisted checking, and what replaced it. Records written before 2.0.0 name this file's stack law `frontend.md`, and call the core law the root; P-03, A-03, A-04 and A-05 are in the core inventory.

**R-13 · FE-10 · "Every list query MUST be paginated."**
The client cannot verify this; it consumes whatever the generated client exposes. *Applied:* the frontend ⚙ is dropped, the law now says outright that enforcement is server-side (BE-19), and the genuinely client-side half became a marked rule of its own - *no loop that exhausts `fetchNextPage` to assemble a whole collection*, FE-21.

**R-14 · FE-14 · "Inline `style` only for genuinely dynamic values."**
"Genuinely dynamic" is judgment. *Applied:* split. `ci`/`eslint` enforces the checkable half - no stylesheet import outside the global stylesheet and its layout partial (A-06) - and a static object literal passed to `style` becomes a lint error. Anything computed passes, which is the intent.

**R-15 · FE-16 · "Classified values MUST NOT appear in URLs or route/search params."**
The frontend has no classification metadata today, so nothing can decide which values are classified: the mark was fiction. *Applied:* the ⚙ is removed and the law states the condition on which it returns - classification carried into the OpenAPI document and the generated types, P-03 in the core inventory. The rule itself is untouched and still MUST. This is the one rewording that made a rule *less* enforced rather than more, because the alternative was leaving a mark that no check could ever honour.

## Amendments

A rewording keeps a rule and fixes how it is stated. An amendment changes what the law requires, under core §11 - the law first, the codebase after. A-06 concerns this stack law alone and is kept here; every other amendment is in the core inventory.

**A-06 · frontend §1, §2, §6, §7, FE-12, FE-14 · The law named a component kit and a styling system instead of a shape.**
Frontend §1 named Shadcn/ui and Tailwind, §2 gave `shared/ui/` to Shadcn primitives, §6 routed every form through Shadcn's `<Form>`, and §7 made Tailwind the only styling. The organization's applications are required to build on its own design system: a component library, itself built on a commercial component suite, published as a package with its own stylesheet, theme and icon set. An application on it could not comply with FE-12 or FE-14 whatever its own code did, because the `<Form>` and `shared/ui/` they name do not exist, and `../SKILL.md` §11 forbids working around the rule in one repository. What the rules protected still held: one form library with one schema per form, styling the codebase cannot drift from, and no stylesheet arriving from anywhere. Only their wording assumed a kit. Copying a kit into `shared/ui/` as editable source was also the opposite of what a design system needs. A local copy is a fork that drifts from the design system, and a wrapper layer in `shared/` is where that fork would grow back.
*Applied:* §1 names the UI library and the component library it is built on as a class, recorded in `DECISIONS.md`, as A-03 and A-05 did for the identity provider and library. §1 also gains FE-32: a UI-class package - one whose purpose is presentation, such as a component, styling, icon, animation or class-name package - exists only if the UI library depends on it, and application code imports no UI-class package but the UI library and its component library. The rule and its check are scoped to that class: the library's dependency closure also holds packages the application uses in its own right, such as its runtime, and a ban on the whole closure would forbid them. §2 removes `shared/ui/`. §7 replaces the editable-primitives rule with FE-31: features and `app/` use the library directly, `shared/` imports neither library, and a wrapper, adapter or re-export is forbidden. §7's styling rule and FE-14 become the library's stylesheet entry, its component library's theme and its icon stylesheet, imported once by the global stylesheet, plus one layout partial for layout the library has no component for. That partial uses only the library's own tokens and mixins, and never targets a library component; this is the pattern the organization's own application template uses. §6 and FE-12 keep React Hook Form + Zod, bind a library input through `Controller`, and forbid the library's own validator, so every form still has exactly one schema. Two clauses no build check reaches are recorded as reached by review in their rows: the `Controller` binding under FE-12, and a wrapper written inside a feature under FE-31. The packages each check reads are listed in the project's lint configuration, as FE-24's are, so no rule names a product (A-04). Nothing is weakened for an application whose UI library ships as editable source: it records that library in `DECISIONS.md` and uses it directly, and FE-31 forbids the wrapper layer `shared/ui/` used to invite. Frontend rows go from 30 to 32.

## Build order

Sequenced by value per unit of effort, not by document order.

**First - the cheap ones that fail loudly.** FE-01 through FE-04, FE-11 through FE-13, FE-15, FE-18, FE-19 are almost all off-the-shelf ESLint configuration. **Enforced in about a day**, none requiring a decision.

**Second - the classification types.** FE-22 asserts the generated types carry the class the server stack law emits into the OpenAPI document (ROOT-18). That is what makes FE-16 and FE-23 enforceable on the client at all, so build it before either.
