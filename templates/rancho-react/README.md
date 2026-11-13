# Rancho React template

A React micro-frontend for the PrismOne shell whose lint and checks enforce the `rancho-react` law. Rules a check can enforce live here as code, not as prose in the law.

The stack is the organization's:

- **UI library:** `@prismhr/components-react` (Prismatic), the React port of the Prismatic component library. Syncfusion's grid is the one other presentation package, hosted by Prismatic's grid components.
- **Identity:** `@vensure-devops-qa/vensureone-iam-react`, through one module, `src/shared/lib/identity.tsx`. The PrismOne shell signs the user in and owns the session; this application never refreshes or stores a token.
- **Host shell:** single-spa. `src/app/main.tsx` exports the `bootstrap`, `mount` and `unmount` lifecycles.
- **Routing:** `react-router` 8, the router the UI library is built on. `react-router-dom` 7 is installed only because the identity library imports it; the application never does.
- TanStack Query, Zustand, React Hook Form with Zod, Vitest with React Testing Library and MSW, and an API client generated from the server's OpenAPI document by `@hey-api/openapi-ts`.

## Setup

The organization's packages come from GitHub Packages. Set `NODE_AUTH_TOKEN` to a token that can read them (`read:packages`); `.npmrc` reads it from the environment and holds no token itself.

```bash
npm install
npm run verify
```

## What enforces what

| Rule | Where |
|---|---|
| TypeScript strict: no `any`, no suppression but a `@ts-expect-error` naming its issue | `tsconfig.json`, `eslint.config.js`, `scripts/check.mjs` (`typescript`) |
| A lint suppression names its rules and says why | `eslint.config.js` (`eslint-comments`) |
| Dependencies flow `shared` → `features` → `app`; a feature imports no other feature | `eslint.config.js` (`boundaries/dependencies`) |
| Every feature is declared in `src/features/REGISTRY.md` | `scripts/check.mjs` (`registry`) |
| `src/generated/` is what the generator writes from `openapi/v1.json`, never edited | `scripts/check.mjs` (`generated`) |
| Server data goes through TanStack Query: no `fetch` outside the interceptor, and no effect that loads data | `eslint.config.js` (`no-restricted-globals`, `no-restricted-properties`, `rancho/no-fetch-in-effect`) |
| A component reaches the server only through its feature's `api/` | `eslint.config.js` (`no-restricted-imports` on `src/features/*/components/**`) |
| A query key comes from its resource's key factory | `rancho/query-key-from-factory` |
| A list is never paged through to its end | `no-restricted-syntax`, `rancho/no-paging-to-the-end` |
| A store lives in a feature's `stores/` or `shared/stores/` | `no-restricted-imports` |
| Forms are React Hook Form with Zod: no other form library, no UI-library validator | `no-restricted-imports` |
| `shared/` uses no UI library, and nothing re-exports a UI-library component | `no-restricted-imports`, `rancho/no-ui-reexport` |
| No presentation package but the UI library (and Syncfusion's grid) | `no-restricted-imports`, `scripts/check.mjs` (`ui-dependencies`) |
| Stylesheets enter only through `src/app/styles.ts`; the global stylesheet imports only the UI library's styles and the layout partial, which never restyles a UI-library component; no static style object | `no-restricted-imports`, `rancho/no-static-style`, `scripts/check.mjs` (`styles`) |
| Routing is `react-router` | `no-restricted-imports` |
| Only the identity module uses the identity library or touches its session store, and only its interceptor attaches the token | `no-restricted-imports`, `no-restricted-syntax` |
| Tests mock the network with MSW, never the generated client | `no-restricted-syntax` |
| Every value from the server carries its data class: `npm run generate` writes `src/generated/classes.gen.ts`, a schema per type and parameter whose output is classified from `x-data-class`, unclassified as `restricted` | `scripts/generate-classes.mjs`, `scripts/check.mjs` (`generated`) |
| Only public and internal data reach a URL (links, navigation, search params, history), browser storage (Web Storage, cookies, IndexedDB, the Cache API, a persisted store's `partialize`) or telemetry (the console); unparsed input is unclassified, so it never does | `rancho/classified-at-boundary` |
| A value takes its class only from a generated schema or the client-state registry (`src/shared/lib/client-state.ts`) | `rancho/no-class-assertion` |
| A generated response is parsed, never asserted to another type | `rancho/no-response-assertion` |
| Only `src/shared/lib/problem-details.ts` reads a problem-details body | `rancho/one-problem-parser` |
| No token leaves the identity module, and nothing reads a credential nested inside one | `rancho/identity-exports-no-token`, `no-restricted-syntax` |

`python -I templates/check-rancho-react.py`, from the repository root, seeds one violation per check into a copy and proves each is caught.

## Status

Phase 3 of `LAW-REDESIGN.md`. Done: increment 1 (the structural checks) and increment 2 (data classes on every value from the server, and the type-aware rules that read them). Next: reference code carrying the required behaviour tests (increment 3).
