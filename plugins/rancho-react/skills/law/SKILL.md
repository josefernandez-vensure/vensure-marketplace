---
name: law
description: The React client stack law - structure and feature boundaries, generated API types, server and client state, forms, components and styling, routing, authentication, errors, data boundaries in the browser, real-time, and testing. Subordinate to the core law in /rancho:law. Read in full, after the core law, before writing, reviewing, or placing code in a tree whose CLAUDE.md names `Stack law: /rancho-react:law`.
---

# React Stack Law

The core law, `/rancho:law`, governs: read it first. This is a stack law under core §2: it adds the React rules, and carries the core law's mechanism in every tree bound to it. Core §N is §N of the core law. `⚙` = can be made to fail the build (core §1). Every `⚙` here, and every core rule this role carries, is inventoried in `references/enforcement.md`: consult it by rule id rather than reading it whole.

A tree is bound to this stack law by one line in the `CLAUDE.md` at its root:

```
Stack law: /rancho-react:law
```

If `/rancho:law` is not available in the session, stop and say so. This file alone is not the law.

## Role

`client`. A tree bound here consumes the API contract a server stack law publishes (core §4), and owns what reaches the user's device. What the user may do is the server's answer, every time.

## Core map

| Core | Carried here by |
|---|---|
| §3 Module Boundary Law | Not bound: core §3 governs server modules. Client boundaries are §2 Structure. |
| §4 API Contract | §3 API Types · §4 Server State |
| §5 Validation | §6 Forms and Validation |
| §6 Error Contract | §10 Errors · §6 Forms and Validation |
| §7 Security Baseline | §8 Routing · §9 Authentication |
| §8 Data Boundaries | §3 API Types · §11 Data Boundaries |
| §9 Observability | §11 Data Boundaries, for what telemetry may carry · §9 Authentication (the interceptor sends `traceparent`) |
| §10 Verification | §13 Testing · §14 Verification · `references/enforcement.md` |

Gaps: none.

## 1. Stack

React · Vite · TypeScript (strict) · the organization's UI library and the component library it is built on (`DECISIONS.md`) · TanStack Query · TanStack Router · Zustand · React Hook Form + Zod · the organization's identity library (`DECISIONS.md`) · Vitest + React Testing Library · Playwright · MSW · ESLint 9 flat config, type-aware · Prettier

A UI-class package - one whose purpose is presentation, such as a component, styling, icon, animation or class-name package - is a dependency only if the UI library depends on it, and application code imports none but the UI library and the component library it is built on. ⚙ FE-32

TypeScript `strict` MUST stay on. `any` is forbidden; use `unknown` and narrow. Every type-checker suppression directive - `@ts-ignore`, `@ts-nocheck`, or any other - is forbidden but `@ts-expect-error`, permitted only with a reason comment and an issue reference - or, where it suppresses a check of a rule of the law, the register entry's id (core §11). ⚙ FE-01, FE-02

A lint suppression MUST name the rules it suppresses and give a reason. ⚙ FE-38 Where a suppressed rule enforces a rule of the law, the reason is its register entry's id (core §11).

Generated files - `src/generated/` and `src/app/routeTree.gen.ts` - are outside these suppression rules: the generator writes their headers, and lint does not read them.

## 2. Structure

```
src/
  app/                  composition root. Nothing outside it may import from it.
    main.tsx            the entry module index.html loads
    routes/             the route tree. A route file composes features.
    routeTree.gen.ts    TanStack Router output. Never hand-edited.
    router.tsx
    providers.tsx       query client, router, theme, and the identity provider (§9)
  features/<feature>/   one user-facing capability
    api/                query options, key factories, mutations, use of the generated client
    components/
    hooks/
    stores/
    schemas/
    lib/                feature-local helpers
    types/              feature-local types
  shared/               used by two or more features
    lib/
    stores/             stores two or more features use (§5)
  generated/            OpenAPI output. Never hand-edited.
```

When a host shell mounts the application, its lifecycle entry - such as `bootstrap`, `mount` and `unmount` - is part of `app/`, a composition root like any other.

Dependencies flow one way: `shared` -> `features` -> `app`.

- A feature MUST NOT import from another feature. ⚙ FE-03
- `shared/` MUST NOT import from `features/` or `app/`, and a feature MUST NOT import from `app/`. ⚙ FE-04
- `app/` MAY import from anything.
- Cross-feature composition happens in `app/routes/`. A screen that needs two features is a route that renders both, never a feature that reaches into another.
- Promote to `shared/` on the second consumer, never in anticipation of one - except the modules this law places in `shared/lib/` from the start: the identity module (§9), the problem-details parser (§10) and the client-state registry (§3).
- A feature's folders are those listed above and no others, and it contains only the ones it needs. An empty folder created for symmetry is a defect.

### Feature names

- Every feature MUST be declared in `src/features/REGISTRY.md`: its folder name and one line naming the capability it covers. ⚙ FE-28
- Where a feature covers the same concept as a server module, it SHOULD reuse that module's name.
- The two trees share vocabulary, not shape, and MUST NOT be forced into one decomposition. A server module MAY have no feature; a feature MAY read several modules' contracts, and a UI-only feature - a dashboard, onboarding, global search - MAY map to none.

### Routing

Route files live in `app/routes/`. Configure the generator once, in `vite.config.ts`, and do not work around it:

```ts
tanstackRouter({
  routesDirectory: './src/app/routes',
  generatedRouteTree: './src/app/routeTree.gen.ts',
})
```

The generated route tree imports every route file, so it lives in `app/` with them.

## 3. API Types

- All request and response types are generated from the server's OpenAPI document into `src/generated/`. ⚙ FE-05
- `src/generated/` MUST NOT be hand-edited. ⚙ FE-06
- A response from the generated client MUST NOT be cast, type-asserted, or re-declared by a locally written type. ⚙ FE-07
- Generated types MUST carry the class of every property and parameter in the OpenAPI document (`x-data-class`, core §4 *Wire contract*; core §8) in a form the type checker can read, such as a branded type per class; a property or parameter the document leaves without `x-data-class` carries the `Restricted` brand. ⚙ FE-22 At each boundary §11 names - the URL, browser storage, client telemetry - a lint reads that type at the call that crosses it, and refuses a value whose class the boundary does not permit or that carries no brand: an unbranded value is unclassified, which is `Restricted` (core §8). A value whose type is a literal type or a union of literal types is written in the source, carries no data, and passes. ⚙ FE-33, FE-16, FE-23 A value the user enters takes its brand only by being parsed through the generated schema for the field or parameter it fills, such as a Zod schema whose output carries the class brand. ⚙ FE-22 Client-only state - a value no contract field holds, such as a selected tab, a sort order or a saved grid layout - has no class from the contract, and unclassified is `Restricted` (core §8). Before it crosses a boundary §11 names, it is declared in the client-state registry: one module in `shared/lib/` holding, for each such value, its class by core §8's test and a schema whose output carries that class's brand. Each schema accepts only a closed set of values - an enum, a literal, a bounded number, or an object composed of them - and the registry exports nothing else that brands. The value takes its brand by being parsed through that schema. A type assertion to a class brand, or a schema's `.brand()` naming one, outside generated code and the client-state registry is forbidden. ⚙ FE-34 A value rebuilt with string operations loses its brand and is refused.
- Regenerate whenever the server's contract changes, and commit the output.

## 4. Server State - TanStack Query

- All server data MUST go through TanStack Query. `useEffect` plus `fetch` for data loading is forbidden. ⚙ FE-08 The TanStack Query cache is memory-only: it is never persisted, and dies with the tab.
- Server data MUST NOT be copied into Zustand or component state. Derive from the query result. An edit form is the exception: it takes the query's result as its starting values, through React Hook Form's `values` option, or through `defaultValues` from a component mounted only after the query resolves. A dialog holds the id of what it opened, never the record.
- Query keys MUST come from a key factory, one per server resource. An array literal as a key is forbidden outside a key factory. ⚙ FE-09
- A resource's key factory lives in the `api/` of the feature that reads it, and moves to `shared/lib/` when a second feature reads that resource or invalidates it.
- Mutations MUST declare their invalidations explicitly, through the key factory of every resource they change. A mutation that leaves stale data on screen is incomplete.
- Loading, error, and empty states MUST each be handled.
- A list query against a paginated endpoint pages with the page shape (core §4 *Wire contract*); a collection the server bounds by construction arrives whole.
- Cursor-paged lists use `useInfiniteQuery`. A loop that exhausts `fetchNextPage` to assemble a whole collection is forbidden. ⚙ FE-21 Nor may pages be looped any other way to reassemble a complete client-side collection. If a screen needs a total or an aggregate, the server computes it.
- A mutation MUST NOT be retried automatically unless it carries an idempotency key, generated once per user intent and reused across every retry of that intent.
- A mutation of a versioned resource sends as `If-Match` the `etag` it read: from the resource's `ETag` or `etag` field, from a list item's `etag` field, or from the last write's response. A `412` refetches the resource and tells the user it changed; it MUST NOT be retried as it was.
- A `429` MUST be respected: honour `Retry-After`, back off, and never retry in a tight loop. The UI MUST show that it is throttled rather than appearing to hang.
- One client-wide retry rule, configured once on the query client, governs every query: it never retries a `4xx` except `408` and `429`, and its delay honours `Retry-After`. The interceptor's one replay after a silent refresh (§9), and a mutation's replay under its idempotency key, are not retries under this rule.

## 5. Client State - Zustand

Client state is UI state the server does not own: selections, wizard step, panel open or closed, unsaved draft.

- Default to component state. Escalate to a feature store when two or more components need it, and to a store in `shared/stores/` when two or more features need it (§2's second-consumer rule).
- A store MUST live in `features/<feature>/stores/` or `shared/stores/`. ⚙ FE-11
- Stores MUST NOT hold server data, form state, or anything derivable from either.

## 6. Forms and Validation

- React Hook Form + Zod. No other form library, and no validation through the UI library's own validator. ⚙ FE-12 A UI-library input is bound in the feature with React Hook Form's `Controller`, and receives its error text through the input's own error property.
- Every form MUST have a Zod schema. Ad-hoc validation inside handlers is forbidden.
- Schemas MUST mirror the server's Request DTO constraints. Where they diverge, the server is right.
- Server errors, a `ProblemDetails` 400 included, MUST still be handled and displayed. (core §5)
- Field-level errors - the `errors` member of `ProblemDetails` (core §4 *Wire contract*) - MUST be mapped onto their form fields, not shown only as a banner.
- An edit form of a versioned resource carries the `etag` as one of its form values, so every reset updates it with the values it belongs to, and sends that one as `If-Match` (§4) - never the query's current `etag`. Its resets MUST NOT use `keepValues` or `keepDirtyValues`: either keeps edits made against older values while taking a newer `etag`, which turns a concurrent change into a silent overwrite. A form fed through `values` MUST NOT be reset by a background refetch while the user is editing: it passes `values` only while the form is not dirty, or its query does not refetch in the background while the form is mounted. After its own successful write, the form resets to the written values and the `etag` the write returned.

## 7. Components

- Features and `app/` use the UI library and the component library it is built on directly. `shared/` MUST NOT import either, and no module re-exports a UI-library component. ⚙ FE-31 A component MUST NOT wrap a UI-library component to forward its props or restyle it: there is no second design system. A feature component that composes library components to express domain meaning is a feature component, not a wrapper.
- A gap in the UI library - a component, variant or behaviour it lacks - that the wrapper rule above or FE-14's styling rule would leave unfilled is a conflict entry in the exceptions register against that rule, and the upstream issue is its amendment request (core §11). Until the library fills it, the feature composes the library's public components and tokens, and MUST NOT reach into its internals, such as private classes or undocumented exports.
- Components MUST NOT call `fetch` or the generated client directly. Data access goes through a hook in the feature's `api/`. ⚙ FE-13
- Presentational components MUST NOT contain data fetching or business rules.
- Prefer composition over configuration. A component with more than roughly seven props, or with boolean props that change layout, SHOULD be split.
- Styling is the UI library's. The global stylesheet imports, once, the UI library's stylesheet entry, the theme of the component library it is built on, and the library's icon stylesheet. Layout the library has no component for lives in one layout partial the global stylesheet imports, which uses only the library's own tokens and mixins, styles only the application's own markup, and MUST NOT target or restyle a library component. Any other stylesheet import, utility-class system or CSS-in-JS is forbidden. ⚙ FE-14
- A static object literal passed to `style` is forbidden. ⚙ FE-14 A computed value is permitted.
- Every interactive element MUST be keyboard-operable and MUST have an accessible name.

## 8. Routing

- TanStack Router. Route params and search params MUST be typed and validated with Zod, through schemas whose output carries a class brand: a generated schema for a value a contract field holds, and the registry's for client-only state (§3).
- Route-level data loading SHOULD use loaders integrated with TanStack Query.
- A route file MUST be thin: typed params and search, a loader, and composition of feature components. Business logic in a route file belongs in a feature.
- Authorization MUST NOT be inferred from route structure; hiding UI is not access control.

## 9. Authentication

The organization's identity provider, through its identity library, both recorded in `DECISIONS.md`. The browser proves who the user is.

- An application mounted in a host shell uses the shell's session through the identity library and MUST NOT run a sign-in flow of its own.
- Token acquisition MUST live in exactly one module in `shared/lib/`, which also exports the identity provider component that `app/providers.tsx` renders. Nothing else imports the identity library, or reads or writes its session store. ⚙ FE-24
- A token is obtained from the identity library per request and MUST NOT be retained by the application beyond that request - in browser storage of any kind (§11), a store, or a module-level cache. ⚙ FE-25 The identity library's own session store is the one place a token may live. Where the library offers a choice of store, a standalone application MUST configure the shortest-lived one it supports; an application in a host shell uses the store the shell configures.
- A token is opaque to the application. Its claims, and the signed-in user's profile as the identity library returns it, MAY be read for display within the user's own session and MUST NOT be used for any other purpose.
- A credential nested inside a token - an embedded access or refresh token - MUST NOT be extracted, stored, forwarded, or logged. ⚙ FE-30 The claim names to flag live in the project's lint configuration.
- The token is attached by exactly one interceptor on the generated client. A hand-written `Authorization` header is a defect. ⚙ FE-26
- The same interceptor sends `traceparent`, and `tracestate` when present, on every call to the server, and the client attaches its trace id to its own telemetry and error reports.
- A `401` MUST trigger one silent refresh, then an interactive sign-in, and MUST NOT produce a retry loop or surface as a generic error. In a host shell both are the shell's: the application asks the identity library once and never refreshes a token itself, and the interactive step is the shell's sign-in page.
- A `403` is a final answer: it MUST be shown as a permission error and MUST NOT trigger a refresh.
- What the application requests from the provider - scopes, a service name, or any other grant - MUST be declared in one place and MUST be the narrowest the application needs. In a host shell the shell requests the grant: the application declares what it needs to the shell's owners, in that one place, and requests nothing itself.
- Sign-out MUST have the identity library clear its session store - in a host shell, through the shell's sign-out (below) - and MUST clear the TanStack Query cache, every store, and everything the application wrote to browser storage.
- Sign-out MUST also end the identity-provider session, including any upstream session the provider federates to. A standalone application calls the provider's end-session endpoint itself; an application in a host shell clears its own caches and stores, then delegates sign-out to the shell's sign-out, which MUST end the provider session. The next sign-in MUST prompt.
- The frontend holds no secrets: a client id may be in the bundle, a client secret never.

## 10. Errors

- `ProblemDetails` is the error shape, and exactly one parser, in `shared/lib/`, reads it; nothing else does. ⚙ FE-15
- A server error is mapped by its `type` and `status`. The parser reads which types are user-facing from the document's `x-problem-types` (core §4 *Wire contract*), generated with the types (§3). The UI MAY render a problem's `title` and its field messages only for a type the OpenAPI document marks user-facing, and MUST NOT render `detail`, or anything of a type it does not recognise, unmapped.
- An error shown to the user offers its `traceId`, so a report can be joined to the server's trace.
- The root route MUST declare an `errorComponent`, and a route SHOULD declare its own wherever a failure should not take down more than that screen. An unhandled render error MUST NOT blank the app.

## 11. Data Boundaries

Core §8 governs, and its classes apply here unchanged. The browser's URLs and storage are inside the authenticated boundary (core §8), so `Internal` data MAY appear in them; `Confidential` and `Restricted` MUST NOT, because a URL is kept in history and server logs, and storage outlives the session. They stay inside only while the document is served with a referrer policy of `strict-origin-when-cross-origin` or stricter, and no element, request or `<meta>` tag sets a weaker one, so a URL's path never reaches another origin; and while sign-out clears everything the application wrote to browser storage (§9). Analytics, telemetry, session replay and error reporting are core §8's telemetry boundary.

- `Confidential` and `Restricted` values MUST NOT appear in any part of a URL - path, query string, fragment, route param, or search param. ⚙ FE-16
- They MUST NOT be written to browser storage of any kind - such as `localStorage`, `sessionStorage`, IndexedDB, Cache Storage, or a cookie. ⚙ FE-23 The identity library's session store (§9) MAY hold the signed-in user's own profile as the provider returns it. The application MUST NOT write to that store.
- Analytics, telemetry, session replay, and error reporting MUST scrub them, from breadcrumbs, form-field capture and request bodies too: the default configuration of each is wrong for this domain.
- The client MUST NOT assemble an export of sensitive data out of paged reads. (core §8)

## 12. Real-time

§12 is in `references/annex-realtime.md`, which binds once the tree meets its trigger: the first real-time channel the client opens - SignalR, `WebSocket`, `EventSource`, or any other.

## 13. Testing

- Vitest + React Testing Library. Query by role and accessible name. `data-testid` is a last resort, and requires a comment stating which element has no usable role or accessible name.
- MSW for all network mocking. Module-mocking the generated client - `vi.mock`, `vi.doMock`, or any other - is forbidden. ⚙ FE-19
- **Forms**: every form MUST have a test for a validation failure, and a test for a server-side failure proving each field error appears on its field; every edit form of a versioned resource, a test proving it sends the `etag` of the values it was last reset from, and that after its own successful write a second save is not refused with `412`. (FE-41)
- **Data boundaries**: telemetry, analytics, and error reporting MUST have a test proving a known classified field is scrubbed from a captured event (FE-17).
- **Authentication**: a `401` MUST have a test proving exactly one silent refresh precedes interactive sign-in - the provider's, or the host shell's sign-in page - and a `403` a test proving no refresh occurs (FE-27). Sign-out MUST have a test proving the local caches and everything the application wrote to browser storage are cleared and the provider's end-session endpoint is called - or, in a host shell, that sign-out is delegated to the shell's sign-out (FE-29).
- **Wire contract**: the client MUST have a test proving every call to the server carries `traceparent` (FE-35), and a test proving a `412` on a mutation refetches the resource and does not retry the write (FE-36).
- **Retries**: the query client MUST have a test proving a `403` is not retried and a `429` waits for its `Retry-After` before retrying. (FE-37)
- **Query states**: every component that renders a query's result MUST have a test for its loading state, its error state and, for a list, its empty state. Where a route's loader and a suspense query move those states to the route, the route's pending and error components carry the tests. (FE-39)
- **Idempotency**: every mutation that is retried MUST have a test proving each attempt carries the same idempotency key. (FE-40)
- **Accessibility**: every route MUST have a test that runs axe against it - a component test with axe-core, or `@axe-core/playwright` in a journey that visits it - and fails on any violation. A passing check supports §7's accessibility rule and does not satisfy it: review still carries it. (FE-42)
- **Errors**: the root route MUST have a test proving a render error shows its `errorComponent`, and the problem-details parser a test proving a problem of a type not marked user-facing renders neither its `title` nor its `detail`. (FE-43)
- Playwright for critical user journeys only.
- Tests MUST NOT assert on class names, DOM structure, or component internals.

## 14. Verification

- `eslint-plugin-boundaries` enforces §2's dependency direction, failing the build on a cross-feature or upward import.
- `typescript-eslint` runs type-aware.
- `pnpm lint && pnpm typecheck && pnpm test` MUST pass before a change is complete. ⚙ FE-20
