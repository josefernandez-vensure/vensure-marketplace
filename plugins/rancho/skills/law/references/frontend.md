# Frontend Law - React

`../SKILL.md` governs. This file adds frontend rules. ⚙ = machine-checked.

## 1. Stack

React · Vite · TypeScript (strict) · Shadcn/ui · Tailwind · TanStack Query · TanStack Router · Zustand · React Hook Form + Zod · MSAL (`@azure/msal-browser`, `@azure/msal-react`) · Vitest + React Testing Library · Playwright · MSW · ESLint 9 flat config, type-aware · Prettier

TypeScript `strict` MUST stay on. `any` is forbidden; use `unknown` and narrow. `@ts-ignore` is forbidden; `@ts-expect-error` is permitted only with a reason comment and an issue reference. ⚙

## 2. Structure

```
src/
  app/                  composition root. Nothing outside it may import from it.
    routes/             the route tree. A route file composes features.
    router.tsx
    providers.tsx       query client, router, MSAL, theme
  features/<feature>/   one user-facing capability. Only the folders it needs.
    api/                query options, mutations, use of the generated client
    components/
    hooks/
    stores/
    schemas/
  shared/               used by two or more features
    ui/                 Shadcn primitives
    lib/
  generated/            OpenAPI output. Never hand-edited.
  routeTree.gen.ts      TanStack Router output. Never hand-edited.
```

Dependencies flow one way: `shared` -> `features` -> `app`.

- A feature MUST NOT import from another feature. ⚙
- `shared/` MUST NOT import from `features/` or `app/`, and a feature MUST NOT import from `app/`. ⚙
- `app/` MAY import from anything.
- Cross-feature composition happens in `app/routes/`. A screen that needs two features is a route that renders both, never a feature that reaches into another. Without a layer permitted to know about two features, "a feature MUST NOT import from another feature" has no legal answer for the screens this domain is made of, and the rule gets suppressed instead of followed.
- Promote to `shared/` on the second consumer, never in anticipation of one.
- A feature contains only the folders it needs. An empty folder created for symmetry is a defect.

### Feature names

- Every feature MUST be declared in `src/features/REGISTRY.md`: the folder name and one line saying what capability it covers. ⚙ An undeclared feature folder is a defect. The check reads `src/features/` and the registry and nothing else - it does not reach into the backend tree.
- Where a feature covers the same concept as a backend module, it SHOULD reuse that module's name. Shared vocabulary is worth having. A build check that crosses into the other tree to enforce it is not, and a naming convention is not worth coupling two trees over.
- The two trees share vocabulary, not shape. A backend module MAY have no feature. A feature MAY read several modules' contracts, and a UI-only feature - a dashboard, onboarding, global search - MAY map to none. A backend module is a consistency boundary; a feature is a user capability. They are not the same decomposition and MUST NOT be forced into one.

### Routing

Route files live in `app/routes/`. Configure the generator once, in `vite.config.ts`, and do not work around it:

```ts
tanstackRouter({
  routesDirectory: './src/app/routes',
  generatedRouteTree: './src/routeTree.gen.ts',
})
```

## 3. API Types ⚙

- All request and response types are generated from the backend OpenAPI document into `src/generated/`.
- `src/generated/` MUST NOT be hand-edited.
- A response from the generated client MUST NOT be cast, type-asserted, or re-declared by a locally written type. Contract drift must be a compile error, not a runtime surprise.
- Generated types MUST carry the data class of every property, as emitted into the OpenAPI document (`../SKILL.md` §8). ⚙ Without it, every data-boundary rule on this side is a hope rather than a check.
- Regenerate whenever the backend contract changes, and commit the output.

## 4. Server State - TanStack Query

- All server data MUST go through TanStack Query. `useEffect` plus `fetch` for data loading is forbidden. ⚙
- Server data MUST NOT be copied into Zustand or component state. Derive from the query result; a second copy is a second source of truth.
- Query keys MUST come from a per-feature factory. Inline array literals as keys are forbidden - they make invalidation unauditable. ⚙
- Mutations MUST declare their invalidations explicitly. A mutation that leaves stale data on screen is incomplete.
- Loading, error, and empty states MUST each be handled. A component that renders only the success path is incomplete.
- Every list query MUST be paginated. Enforcement is server-side (`../SKILL.md` §4) - the client cannot verify it, and MUST NOT work around it.
- A loop that exhausts `fetchNextPage` to assemble a whole collection is forbidden. ⚙ The page you were handed is not the data set, and treating it as one produces a UI that is quietly wrong at scale.
- Cursor-paged lists use `useInfiniteQuery`. Looping pages to reassemble a complete client-side collection is forbidden; if a screen needs a total or an aggregate, the server computes it.
- A mutation MUST NOT be retried automatically unless it carries an idempotency key. The key is generated once per user intent and reused across every retry of that intent - regenerating it per attempt defeats the mechanism completely.
- A `429` MUST be respected: honour `Retry-After`, back off, and never retry in a tight loop. The UI MUST show that it is throttled rather than appearing to hang.

## 5. Client State - Zustand

Client state is UI state the server does not own: selections, wizard step, panel open or closed, unsaved draft.

- Default to component state. Escalate to a feature store only when two or more components need it. Escalate to a global store only when two or more features need it, and justify it in the PR.
- Stores MUST be feature-scoped, in `features/<feature>/stores/`. ⚙
- Stores MUST NOT hold server data, form state, or anything derivable from either.

## 6. Forms and Validation

- React Hook Form + Zod through Shadcn's `<Form>`. No other form library. ⚙
- Every form MUST have a Zod schema. Ad-hoc validation inside handlers is forbidden.
- Schemas MUST mirror the backend Request DTO's constraints. Where they diverge, the backend is right.
- Client validation is UX. Server errors MUST still be handled and displayed - a `ProblemDetails` 400 is an expected outcome, not an edge case.
- Field-level errors from `ProblemDetails` MUST be mapped back onto the corresponding form fields, not shown only as a banner.

## 7. Components

- Shadcn primitives live in `shared/ui/` and MAY be modified - they are source, not a dependency. Any modification MUST be noted in the file.
- Components MUST NOT call `fetch` or the generated client directly. Data access goes through a hook in the feature's `api/`. ⚙
- Presentational components MUST NOT contain data fetching or business rules.
- Prefer composition over configuration. A component with more than roughly seven props, or with boolean props that change layout, SHOULD be split.
- Styling is Tailwind. A `.css` import outside `shared/ui/` and the global stylesheet is forbidden. ⚙
- A static object literal passed to `style` is forbidden. ⚙ A computed value is fine - that is what the prop is for.
- Every interactive element MUST be keyboard-operable and MUST have an accessible name.

## 8. Routing

- TanStack Router. Route params and search params MUST be typed and validated with Zod.
- Route-level data loading SHOULD use loaders integrated with TanStack Query.
- A route file MUST be thin: typed params and search, a loader, and composition of feature components. Business logic in a route file belongs in a feature.
- Authorization MUST NOT be inferred from route structure. Hiding UI is not access control; the server decides.

## 9. Authentication

Entra ID through MSAL. The browser proves who the user is. It decides nothing about what they may do - that is the server's answer, every time (`../SKILL.md` §7).

- Token acquisition MUST live in exactly one place in `shared/lib/`. No component, hook, or query calls MSAL directly. ⚙
- Access tokens MUST be acquired silently from MSAL's cache per request and MUST NOT be stored by the application. Writing a token to `localStorage`, `sessionStorage`, a cookie, or a store is forbidden. ⚙
- The token is attached by exactly one interceptor on the generated client. A hand-written `Authorization` header is a defect. ⚙
- A `401` MUST trigger one silent refresh, then an interactive sign-in. It MUST NOT produce a retry loop and MUST NOT surface as a generic error.
- A `403` is a final answer. It MUST be shown as a permission error and MUST NOT trigger a refresh - the token is fine, the permission is not. Retrying a `403` is how a UI turns a clear denial into a hang.
- Scopes MUST be declared in one place and MUST be the narrowest set the application needs.
- Sign-out MUST clear the MSAL cache, the TanStack Query cache, and every store. A previous user's data surviving a sign-out on a shared workstation is a disclosure, and shared workstations are the normal case in this domain.
- The frontend holds no secrets. Everything in the bundle is public: a client id belongs there, a client secret never does.

## 10. Errors

- `ProblemDetails` is the error shape. Exactly one parser lives in `shared/lib/` and is the only place that shape is read. ⚙
- A raw server error object or message MUST NOT be rendered to the user unmapped.
- The root route MUST declare an `errorComponent`, and a route SHOULD declare its own wherever a failure should not take down more than that screen. An unhandled render error MUST NOT blank the app.

## 11. Data Boundaries

`../SKILL.md` §8 governs, and its classes apply here unchanged. The browser is inside the authenticated boundary. Everything listed below is outside it.

- `Confidential` and `Restricted` values MUST NOT appear in a URL path, query string, route param, or search param. ⚙ They land in browser history, server access logs, and `Referer` headers sent to third parties.
- They MUST NOT be written to `localStorage`, `sessionStorage`, IndexedDB, or a cookie. ⚙ Server data belongs in the TanStack Query cache, which is memory-only and dies with the tab.
- Analytics, telemetry, session replay, and error reporting MUST scrub them. Breadcrumbs, form-field capture, and unredacted request bodies are the usual leak, and the default configuration of every one of these tools is wrong for this domain.
- The client MUST NOT assemble an export of sensitive data out of paged reads. An export comes from a server endpoint carrying the same policy as the read, and is audited (`../SKILL.md` §8).

## 12. Real-time

- SignalR messages update the TanStack Query cache. Components MUST NOT subscribe to hubs directly. ⚙
- The UI MUST be correct if no SignalR message ever arrives. Real-time is an optimization over refetch, never the only path to a state.

## 13. Testing

- Vitest + React Testing Library. Query by role and accessible name. `data-testid` is a last resort and requires a comment explaining why.
- MSW for all network mocking. `vi.mock` on the generated client is forbidden - it mocks past the contract. ⚙
- Every form MUST have a test for a validation failure and a test for a server-side failure.
- **Data boundaries**: telemetry, analytics, and error reporting MUST have a test proving a known classified field is scrubbed from a captured event.
- **Authentication**: a `401` MUST have a test proving exactly one silent refresh is attempted before interactive sign-in, and a `403` MUST have a test proving no refresh occurs.
- Playwright for critical user journeys only.
- Tests MUST NOT assert on class names, DOM structure, or component internals.

## 14. Verification ⚙

- `eslint-plugin-boundaries` enforces §2's dependency direction. A cross-feature or upward import fails the build.
- `typescript-eslint` runs type-aware.
- `pnpm lint && pnpm typecheck && pnpm test` MUST pass before a change is complete.
