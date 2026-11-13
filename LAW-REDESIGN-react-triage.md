# Phase 3 triage: the React inventory, row by row

Working file for the law redesign (`LAW-REDESIGN.md`), deleted in phase 6 like the .NET triage. It covers the 43 rows of `rancho-react`'s `enforcement.md` at rancho 4.1.0 (31 `build`, 11 `required-test`, 1 `dropped`) and its two core rows. The checks land in `templates/rancho-react/`.

## The organization's stack, and what it changes

The template is built on what the organization's React code already uses. The Angular skeleton (`Vensure-Devops-QA/prismone-emptyproj-prismatic-spa`) has React counterparts for each of its libraries:

| Concern | Package | Effect on the law |
|---|---|---|
| UI library | `@prismhr/components-react` 1.4 (Prismatic), the React port of `@prismhr/components-ng-shared`. Its stylesheet (`styles.css`) carries the component library's CSS and Font Awesome; its Vite plugin serves its assets. Syncfusion is used only for the data grid, which Prismatic's grid components host. | FE-14, FE-31 and FE-32 name it. Its `Validators` stand-in is the "UI library's validator" FE-12 refuses. |
| Identity | `@vensure-devops-qa/vensureone-iam-react` 1.1, the React port of the IAM Angular library. Its session lives under one `localStorage` key that every mounted micro-frontend shares; `createFetchWithAuth` attaches the token; `signOut` clears the store and ends the provider session. | FE-24 to FE-30 name it. In the shell, `enableTokenRefreshInterval: false`: the application never refreshes a token (§9's host-shell rule). |
| Host shell | single-spa under the PrismOne root config, with its own login micro-frontend. The application exports `bootstrap`, `mount` and `unmount` from `app/`. | §9's host-shell branch is the only one the template takes. |
| Router | `react-router` 8: Prismatic peer-depends on it, the IAM library's route components are React Router's, and `prismhr/mocks-react` uses it. | **Decided:** the law moves from TanStack Router to React Router (data router). The generated route tree and its generator config go; search params are parsed with Zod where they are read. |
| Package manager | npm, as every organization repository uses | `npm run verify` replaces `pnpm lint && pnpm typecheck && pnpm test` (FE-20) |

Found while building: the IAM library imports `react-router-dom` (any version from 6), which React Router 8 no longer publishes, so the template installs `react-router-dom` 7 only to satisfy that import and lints against using it. The IAM library should move to `react-router`; until it does, its `AuthGuard` and callback pages can't run under the application's router, and the template uses only its provider, hooks and fetch wrapper.

## Increment 1: structural checks (done)

The template: a single-spa micro-frontend with one feature (`products`, a paged list over the Catalog API from `templates/rancho-dotnet/openapi/v1.json`), its key factory, a store, the identity module, the route tree, a component test against MSW, and the generated client. Lint, the type check, the check script and the tests pass on it, and `check-rancho-react.py` catches 40 of 40 seeds, in under two minutes.

| Mechanism | Rows |
|---|---|
| `tsconfig.json` and `scripts/check.mjs` (`typescript`, over `tsc --showConfig`) | FE-01, configuration half |
| `eslint.config.js`, off-the-shelf rules | FE-01 and FE-02 (`no-explicit-any`, `ban-ts-comment` with an issue reference); FE-38 (`eslint-comments`); FE-03 and FE-04 (`boundaries/dependencies`); FE-08, global half (`no-restricted-globals`, `no-restricted-properties`); FE-11, FE-12, FE-13 and FE-31, import half (`no-restricted-imports`, scoped by path); FE-14, import half; FE-19, FE-21 loop half, FE-24 and FE-26 (`no-restricted-syntax`); FE-32, import half |
| `eslint/rancho-plugin.js` | FE-08, effect half (`no-fetch-in-effect`); FE-09 (`query-key-from-factory`); FE-14, style half (`no-static-style`); FE-21, recursion half (`no-paging-to-the-end`); FE-31, re-export half (`no-ui-reexport`, by name too, which the old row left to a type-aware rule) |
| `scripts/check.mjs` | FE-05 and FE-06 (`generated`: regenerates into a scratch folder and compares, so no `git status` is needed); FE-14, stylesheet half (`styles`); FE-28 (`registry`); FE-32, manifest half (`ui-dependencies`, against Prismatic's dependency closure in the lockfile) |

New, from the decisions above: `react-router-dom` is refused everywhere (routing is `react-router`), and the identity module is the only one that may import the identity library or `jwt-decode`, name its session-store keys, or clear `localStorage` or `sessionStorage`.

What differs from the old rows:

- **FE-02:** a `@ts-expect-error` names an issue (`#123`, `ABC-123` or a URL). The register entry id goes with core §7's register in phase 4.
- **FE-14:** the global stylesheet imports Prismatic's `styles.css` and the layout partial. The partial may not select a `phr-` element or class, or Syncfusion's `e-` classes.
- **FE-24:** the session store is the IAM library's `prismone-iam-storage` key (and its `prismone-cache-storage` and `prismone-iam-original-uri`); a key built at runtime is still not reached.
- **FE-32:** the closure is Prismatic's runtime dependencies plus Syncfusion's grid. Presentation packages are recognised by a list and a name pattern; one that matches neither is reached by review.

## Increment 2: the class brands (next)

FE-22 (the generator emits a class brand per property and parameter, `Restricted` where `x-data-class` is missing), and the type-aware rules that read it: FE-16 (URL), FE-23 (browser storage), FE-33 (telemetry), FE-34 (no brand asserted outside generated code and the client-state registry), FE-07 (no cast of a response), FE-15 (one problem-details parser), FE-25 (no retained token) and FE-30 (no nested credential).

## Increment 3: reference code with the required tests (after)

FE-17, FE-27 (in the shell: no refresh of its own, and a `403` never refreshes), FE-29 (sign-out delegates to the IAM library), FE-35 to FE-37, FE-39 to FE-43: a form, an edit form of a versioned resource, the problem-details parser, the query client's retry rule, the interceptor's `traceparent`, an axe check per route, and the root route's error element.

## Deferred and dropped

- **FE-18** (real-time) stays with the annex: "stop and ask" until a project opens a real-time channel.
- **FE-10** stays dropped.
- The TanStack Router rules (§2's generator configuration, §8's typed route tree) go with the router decision.
