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

## Increment 2: data classes (done)

`npm run generate` now also runs `scripts/generate-classes.mjs`, which writes `src/generated/classes.gen.ts` from the document: for every schema and every operation's parameters, a Zod schema whose output carries each primitive's class as a `__dataClass` brand. The reference parses every response through its schema, parses user input through the operation's parameter schema before it goes into the URL, and keeps a persisted layout preference through the client-state registry. `check-rancho-react.py` catches 56 of 56 seeds (16 new); a link built from public and internal values passes.

| Mechanism | Rows |
|---|---|
| `scripts/generate-classes.mjs`, and `scripts/check.mjs` (`generated`) over its output | FE-22, ROOT-16 and ROOT-18 (the class travels with the contract; unclassified is `restricted`) |
| `rancho/classified-at-boundary` | FE-16 (`to` and `href`, `navigate`, `setSearchParams`, `redirect`, `generatePath`, `createSearchParams`, `history.pushState`/`replaceState`, `location.assign`/`replace`/`href`); FE-23 (`Storage.setItem`, `document.cookie`, `cookieStore.set`, IndexedDB `put`/`add`, `Cache.put`, and a zustand `persist`, which must name `partialize`); FE-33 (the console). Each walks the value's type through objects, arrays, unions, template literals and `JSON.stringify` or `String`, and refuses anything confidential, restricted or unclassified |
| `rancho/no-class-assertion` | FE-34 (off in `src/shared/lib/client-state.ts`) |
| `rancho/no-response-assertion` | FE-07 |
| `rancho/one-problem-parser` | FE-15 |
| `rancho/identity-exports-no-token`, and `no-restricted-syntax` on nested-credential claims | FE-25, FE-30 |

What differs from the old rows:

- **FE-22:** the brand is a phantom `__dataClass` property, put on by the generated schema's transform, rather than a Zod `.brand()`. The SDK's own types stay unbranded, so a value straight from the client is unclassified and refused at a boundary until it is parsed. Arguments to the generated client aren't branded: its request URLs are outside FE-16.
- **FE-25:** stronger than the old row. Rather than tracking a token brand through the application, the identity module may export nothing that is, holds or returns a token, so no other module can hold one. Only the module itself is left to review.
- **FE-30:** the template reads no claims, so the rule is `no-restricted-syntax` over the configured claim names, everywhere.
- **FE-33:** the template has no telemetry SDK, so the boundary is the console. A project adds its SDK's calls to the rule's list.
- The server document leaves `HttpValidationProblemDetails.errors` unclassified, so its messages are `restricted` and may not be logged or stored. The .NET template should classify them.

## Increment 3: reference code with the required tests (done)

The template gains the API client's interceptors (a `traceparent` per call, a `401` raising one sign-in request, every failure an `ApiError` with its status and `Retry-After`), the query client's retry rule, a rename form over the versioned product, a keyed stock receipt that is retried, a product route whose params are parsed, sign-out, and how a failure is shown. Its 21 tests pass; `check-rancho-react.py` breaks each behaviour and proves its test fails, and catches 72 of 72 seeds in all (16 new).

Found by the seeds: the first `429` test passed with `Retry-After` ignored, because the default backoff was longer than the header's one second, so it now also proves `Retry-After: 0` retries at once. And the first edit form took the product through React Hook Form's `values`, so a refetch while the user typed replaced their edits and their etag; it now mounts once the product loads, takes it as its default values, and resets only after its own write or a `412`. A test proves a refetch behind the user changes nothing.

| Test | Rows |
|---|---|
| `src/shared/lib/api-client.test.ts` | FE-35 (`traceparent` on every call); FE-27, in the shell: a `401` asks the shell to sign the user in once and is never retried, a `403` is a permission error and asks nothing |
| `src/shared/lib/query-client.test.ts` | FE-37 |
| `src/features/products/components/RenameProductForm.test.tsx` | FE-41 (and a refetch behind the user changes neither their edits nor the etag they send); FE-36 |
| `src/features/products/api/product.test.tsx` | FE-40 |
| `src/features/products/components/ProductList.test.tsx` | FE-39 |
| `src/shared/lib/sign-out.test.ts` | FE-29, in the shell |
| `src/shared/lib/ProblemMessage.test.tsx`, `tests/routes.test.tsx` | FE-43; FE-42 (axe over every route, and a test that fails when a route isn't visited) |

What differs from the old rows:

- **FE-27:** in the shell the application never refreshes. A `401` raises one `unauthorized` event, which the identity module answers with the IAM library's `signIn` (the shell's sign-in page). The test proves the API client half; the identity half is one line.
- **FE-29:** sign-out clears the query cache and every key the application wrote, which all carry its storage prefix (`shared/lib/app-storage.ts`), then calls the IAM library's `signOut`, which clears the shared session store and ends the provider session. Nothing checks that every write uses the prefix; review carries it.
- **FE-43:** the server document has no `x-problem-types` yet (noticed in .NET slice 2d), so `userFacingProblemTypes` is empty and every problem is shown generically, with its trace id. The generator reads `x-problem-types` (`{ type, userFacing }` entries) once the server emits it.
- **FE-17** waits for the first project with a telemetry or error-reporting SDK: the template has none to scrub. The console is the telemetry boundary `classified-at-boundary` checks.
- **FE-40:** the server document declares no `Idempotency-Key` header on `catalog.receive-stock`, so the mutation sets it through the client's `headers` option. The .NET template's document should declare it.

## Deferred and dropped

- **FE-18** (real-time) stays with the annex: "stop and ask" until a project opens a real-time channel.
- **FE-10** stays dropped.
- The TanStack Router rules (§2's generator configuration, §8's typed route tree) go with the router decision.
