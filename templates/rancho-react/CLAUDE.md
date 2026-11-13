Stack law: /rancho-react:law

## Reference

- Feature layout and registry: src/features/products/ and src/features/REGISTRY.md
- Query and key factory: src/features/products/api/products.ts and src/features/products/api/product-keys.ts
- Store: src/features/products/stores/selection.ts; a persisted store: src/features/products/stores/view.ts
- A response parsed through its generated schema, so it carries its data class: src/features/products/api/products.ts
- User input parsed before it goes into the URL: src/features/products/components/ProductFilter.tsx
- Client-only state that crosses a boundary: src/shared/lib/client-state.ts
- Problem-details parser: src/shared/lib/problem-details.ts
- Component rendering a query, with its loading, error and empty states: src/features/products/components/ProductList.tsx
- Route: src/app/routes/products.tsx, in the route tree src/app/router.tsx
- Identity, the interceptor and sign-out: src/shared/lib/identity.tsx
- Component test against MSW: src/features/products/components/ProductList.test.tsx
- Edit form of a versioned resource, and its tests: src/features/products/components/RenameProductForm.tsx, with its schema in src/features/products/schemas/rename-product.ts and its mutation in src/features/products/api/product.ts
- A keyed mutation that is retried, and its test: useReceiveStock in src/features/products/api/product.ts and product.test.tsx
- Route with parsed params: src/app/routes/product.tsx
- The API client's interceptors (traceparent, 401, errors) and the query client's retry rule: src/shared/lib/api-client.ts and src/shared/lib/query-client.ts, with their tests
- Showing a failure: src/shared/lib/ProblemMessage.tsx
- Sign-out: src/shared/lib/sign-out.ts, wired to the shell in src/shared/lib/identity.tsx
- Every route's accessibility check, and the root error element's test: tests/routes.test.tsx

## Verification

```bash
npm run verify
```

`verify` runs `lint`, `typecheck`, `test` and `check`. A failing check is the law speaking: fix the code, not the check. The checks are `eslint.config.js` (with `eslint/rancho-plugin.js`) and `scripts/check.mjs`. After the server's OpenAPI document changes, copy it to `openapi/v1.json` and run `npm run generate`.
