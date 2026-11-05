# Enforcement Inventory - React

Every rule marked ⚙ in the React stack law, every core rule its role carries, and the mechanism that enforces each.

`../SKILL.md` is the stack law, and `annex-realtime.md`, beside this file, holds its §12. The core law is `/rancho:law`. Core §1 defines ⚙ as **"can be made to fail the build."** This file names, for this stack law, the check that can make each marked rule do so. A ⚙ with no entry here is unaccounted for and MUST be added. This stack law's records and row history are in `../../../RECORDS.md`.

## How to read this

**Prefix:** `FE`. Ids, columns and statuses: `/rancho:law`'s inventory, *How to read an inventory*.

**Mechanism** - what does the checking:

| Code | Mechanism | Fails at |
|---|---|---|
| `proj` | Project or build configuration - `tsconfig`, banned packages | Compile |
| `eslint` | ESLint rule, off-the-shelf or custom | Lint |
| `ci` | Repository-level script - path checks, generated-artifact freshness, diff checks | CI |
| `test` | A behavioural test that a human must write **per instance** | Test run |

In ESLint flat config, a rule configured in a later block replaces its options from earlier blocks for the files both match, so each path-scoped `no-restricted-imports` or `no-restricted-syntax` block repeats every restriction that applies to its paths.

## Summary

The stack law carries **32 ⚙ marks**, each naming the ids of the rows it marks. Its rules are **43 rows**: 31 `build`, 11 `required-test`, 1 `dropped`, and 0 `retired`. Its role, `client`, carries **1 core rule**, below.

**Every marked rule can be made to fail the build.** No rule claims an enforcement it cannot deliver. Whether a project has built and reviewed a check is that project's record, in its `ENFORCEMENT-STATUS.md` (core §1).

## Core rules carried

Every core rule whose role is `client` or `both` and whose status is `build`. The section is the core law's; the rule is worded as this stack law expresses it.

| Id | Rule | Section | Mechanism | Status | Check | Gap | Source |
|---|---|---|---|---|---|---|---|
| ROOT-18 | A field's class travels with its contract, and the client enforces it at its own boundaries | §8 | `ci` + `eslint` | build | Canonical: FE-22 for the class in the types; FE-16, FE-23 and FE-33 for its enforcement at the URL, storage and telemetry. The server half, emitting the class into the document, is the server stack law's | — | — |

## Stack law - `../SKILL.md` and its annex

| Id | Rule | Section | Mechanism | Status | Check | Gap | Source |
|---|---|---|---|---|---|---|---|
| FE-01 | `strict` on; no `any`; no type-checker suppression but a justified `@ts-expect-error` | §1 | `proj` + `eslint` | build | `tsconfig`; `no-explicit-any`; `ban-ts-comment` with every directive banned and `ts-expect-error` set to `allow-with-description`. Generated files are in the lint configuration's `ignores` | — | — |
| FE-02 | `@ts-expect-error` requires a reason and an issue reference, or the register entry's id where it suppresses a check of a rule of the law | §1 | `eslint` | build | `ban-ts-comment` with a `descriptionFormat` accepting an issue reference or an `EXC-<n>` id | — | — |
| FE-38 | A lint suppression names its rules and gives a reason | §1 | `eslint` | build | `@eslint-community/eslint-plugin-eslint-comments`: `no-unlimited-disable`, and `require-description` with `ignore` set to `eslint-enable`, `eslint-env`, `exported`, `global` and `globals`, both as errors. Generated files are in the lint configuration's `ignores` | — | https://eslint-community.github.io/eslint-plugin-eslint-comments/rules/require-description.html |
| FE-03 | A feature MUST NOT import from another feature | §2 | `eslint` | build | `eslint-plugin-boundaries` | — | — |
| FE-04 | Dependency direction `shared` -> `features` -> `app`; no layer imports upward | §2 | `eslint` | build | `eslint-plugin-boundaries`, one `rules` entry per layer | — | — |
| FE-28 | Every feature is declared in `src/features/REGISTRY.md` | §2 | `ci` | build | Listing of `src/features/` compared against the registry. No backend input | — | — |
| FE-05 | API types generated into `src/generated/` | §3 | `ci` | build | Regenerate and diff, with ROOT-09 | — | — |
| FE-06 | `src/generated/` MUST NOT be hand-edited | §3 | `ci` | build | Diff check against a fresh generation | — | — |
| FE-07 | A generated client response MUST NOT be cast, asserted, or re-declared | §3 | `eslint` | build | Type-aware custom rule, the same family as FE-34: an `as` or angle-bracket assertion whose operand's type is the return type of a generated client function, or derived from one, fails; `as const` is exempt | Re-declaring a response by a locally written type, by annotation rather than assertion, is reached by review | https://typescript-eslint.io/developers/custom-rules/#typed-rules |
| FE-08 | All server data through TanStack Query; no `useEffect` + `fetch` | §4 | `eslint` | build | Path-scoped `no-restricted-globals` on `fetch`; custom rule for the `useEffect` pattern | — | — |
| FE-09 | Query keys come from a key factory, one per server resource | §4 | `eslint` | build | Custom rule: an array literal passed as a key - a `queryKey` property, or the key argument of a `QueryClient` method that takes one positionally, such as `getQueryData` or `setQueryData` - fails outside the key-factory modules, which match a file-name pattern in the project's lint configuration. A factory's member - a property such as `todoKeys.all`, or a call - is accepted | One factory per server resource is reached by review | https://tkdodo.eu/blog/effective-react-query-keys |
| FE-10 | Every list query paginated | §4 | — | dropped | Enforced server-side by BE-19. Client half is now FE-21 | — | — |
| FE-11 | A store lives in a feature's `stores/` or in `shared/stores/` | §5 | `eslint` | build | Path-scoped `no-restricted-imports` with `importNames` `create` and `createStore` from `zustand` and `zustand/vanilla`, allowed only under `src/features/*/stores/**` and `src/shared/stores/**`; using a store elsewhere is not restricted | — | — |
| FE-12 | RHF + Zod only; no other form library, and no validation through the UI library's validator | §6 | `eslint` | build | `no-restricted-imports` on other form libraries and on the UI library's validator, both listed in the project's lint configuration | — | — |
| FE-13 | Components MUST NOT call `fetch` or the generated client directly | §7 | `eslint` | build | Path-scoped `no-restricted-imports` | — | — |
| FE-14 | Stylesheets enter only through the global stylesheet and its one layout partial; the partial never targets a UI-library component; no static object literal in `style` | §7 | `eslint` + `ci` | build | `eslint`: a stylesheet import in any module but the global stylesheet's entry. `ci`: a selector in the layout partial matching the UI library's class prefixes, listed in the project's lint configuration | — | — |
| FE-15 | Exactly one `ProblemDetails` parser, in `shared/lib/` | §10 | `eslint` | build | Only that path may reference the type | — | — |
| FE-16 | Classified values MUST NOT appear in any part of a URL | §11 | `eslint` | build | A type-aware rule at the calls that build a URL - navigation, links, search-param setters - reads the class brand on the argument's type (FE-22). User input takes its brand only when a generated schema parses it; a brand the application asserts is FE-34's | Known gap: a value with no brand - one rebuilt with string operations, or client-only state not parsed through the registry - is not reached; review carries it | — |
| FE-17 | Analytics, telemetry, and error reporting MUST scrub classified values | §11 | `test` | required-test | frontend §13 **Data boundaries**. FE-33's type-aware lint, reading the class brand on telemetry call arguments, supports the test and does not replace it, because whether an SDK's scrubbing is configured correctly is still a runtime fact | — | — |
| FE-21 | No loop that exhausts `fetchNextPage` to assemble a whole collection | §4 | `eslint` | build | `no-restricted-syntax`: `fetchNextPage` inside a loop or self-recursive call | — | — |
| FE-22 | Generated types carry the class of every property and parameter in a form the type checker reads, with a schema per field and parameter that brands what it parses | §3 | `ci` | build | The brand is emitted through the generator's schema transform, which sees each property's schema and each parameter's schema, where the server places a parameter's class (BE-83); the parsing schemas, such as Zod schemas whose output carries the brand, are generated beside the types. Generation output asserted to brand every property and parameter | — | — |
| FE-23 | Classified values MUST NOT be written to browser storage of any kind | §11 | `eslint` | build | Same rule family as FE-16: a type-aware rule at the storage write calls reads the class brand on the value. The identity library's session store is excepted in the rule itself, so rule and check still cover the same ground: the library's own writes are not application source. Any application write to one of that store's configured keys is flagged, inside the FE-24 module or out | — | — |
| FE-24 | Token acquisition lives in exactly one module in `shared/lib/`, which also exports the identity provider component; nothing else imports the identity library or touches its session store | §9 | `eslint` | build | The identity library's packages and its session store's keys are listed in the project's lint configuration, as FE-30's claim names are. Flags an import of a listed package, or a storage read or write of a listed key, outside that one module | — | — |
| FE-25 | A token MUST NOT be retained by the application beyond the request - in browser storage of any kind, a store, or a module-level cache | §9 | `eslint` | build | Same rule family as FE-23, and the same list of storage APIs. Also flags a token value assigned to a module-scope binding. The library's own session store is not application source | — | — |
| FE-26 | The token is attached by exactly one interceptor | §9 | `eslint` | build | No literal `Authorization` header outside that module | — | — |
| FE-27 | `401` retries once silently then goes interactive; `403` never refreshes | §9 | `test` | required-test | frontend §13 **Authentication**. In a host shell, the interactive step is the shell's sign-in page, and the refresh is the shell's too: the application never refreshes a token itself | — | — |
| FE-29 | Sign-out ends the identity-provider session, not only local caches | §9 | `test` | required-test | frontend §13 **Authentication**. In a host shell, the test proves sign-out is delegated to the shell's sign-out | — | — |
| FE-30 | A credential nested inside a token MUST NOT be extracted, stored, forwarded, or logged | §9 | `eslint` | build | Same rule family as FE-25: reads of the configured nested-credential claim names off a decoded token | — | — |
| FE-31 | `shared/` imports neither the UI library nor its component library; no module re-exports a UI-library component | §7 | `eslint` | build | `boundaries` / path-scoped `no-restricted-imports` refusing the listed UI packages under `src/shared/**`; a custom rule flagging an `export … from` a listed UI package, or an export of a binding imported from one | An export that aliases a UI-library component through a local binding, such as `export const X = Button`, is reached by review | — |
| FE-32 | A UI-class package is a dependency only if the UI library depends on it; application code imports no UI-class package but the UI library and its component library | §1 | `ci` + `eslint` | build | `ci`: the manifest's UI-class dependencies compared against the UI library's published dependency closure. `eslint`: `no-restricted-imports` on the UI-class packages in that closure, minus the UI library and its component library. The closure also holds packages the application uses in its own right, such as its runtime; those are outside the rule and outside the check. The UI-class packages are listed in the project's lint configuration | — | — |
| FE-33 | Client telemetry calls refuse a value whose class the boundary does not permit | §3 | `eslint` | build | Same rule family as FE-16, at the calls into analytics, telemetry and error reporting, which are listed in the project's lint configuration. Supports FE-17, whose required test it does not replace | Same known gap as FE-16: an unbranded value passes | — |
| FE-34 | No type assertion to a class brand outside generated code and the client-state registry | §3 | `eslint` | build | Type-aware rule: an `as` or angle-bracket assertion whose target type is, or contains, a class brand fails outside `src/generated/` and the client-state registry module. Without it, a brand asserted by the application passes FE-16, FE-23 and FE-33 | That a registry schema accepts only a closed set is reached by review | — |
| FE-18 | Components MUST NOT subscribe to hubs directly | §12 | `eslint` | build | Path-scoped restriction on the real-time client packages, listed in the project's lint configuration as FE-24's are | — | — |
| FE-19 | MSW for all mocking; no module mock of the generated client | §13 | `eslint` | build | `no-restricted-syntax` on any `vi` module-mocking call with a generated-path argument | — | — |
| FE-35 | Every call to the server carries `traceparent` | §9 | `test` | required-test | frontend §13 **Wire contract** | — | https://www.w3.org/TR/trace-context/ |
| FE-36 | A `412` on a mutation refetches the resource and does not retry the write | §4 | `test` | required-test | frontend §13 **Wire contract** | — | https://www.rfc-editor.org/rfc/rfc9110#section-13.1.1 |
| FE-37 | The query client never retries a `4xx` except `408` and `429`, and honours `Retry-After` | §4 | `test` | required-test | frontend §13 **Retries** | — | https://tanstack.com/query/latest/docs/framework/react/guides/important-defaults |
| FE-39 | Loading, error, and empty states are each handled | §4 | `test` | required-test | frontend §13 **Query states** | — | — |
| FE-40 | A retried mutation reuses one idempotency key per intent | §4 | `test` | required-test | frontend §13 **Idempotency** | — | https://datatracker.ietf.org/doc/draft-ietf-httpapi-idempotency-key-header/ |
| FE-41 | Forms show validation and server field errors; an edit form sends the `etag` of the values it was last reset from | §6 | `test` | required-test | frontend §13 **Forms** | — | https://react-hook-form.com/docs/useform#values |
| FE-42 | Every route passes an automated axe check | §7 | `test` | required-test | frontend §13 **Accessibility** | — | https://playwright.dev/docs/accessibility-testing |
| FE-43 | A render error shows the route's error component; a type not marked user-facing renders no `title` or `detail` | §10 | `test` | required-test | frontend §13 **Errors** | — | https://www.rfc-editor.org/rfc/rfc9457#name-title |
| FE-20 | `pnpm lint && pnpm typecheck && pnpm test` gate every change | §14 | `ci` | build | CI pipeline | — | — |
