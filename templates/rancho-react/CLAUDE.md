Stack law: /rancho-react:law

## Reference

- Feature layout and registry: src/features/products/ and src/features/REGISTRY.md
- Query and key factory: src/features/products/api/products.ts and src/features/products/api/product-keys.ts
- Store: src/features/products/stores/selection.ts
- Component rendering a query, with its loading, error and empty states: src/features/products/components/ProductList.tsx
- Route: src/app/routes/products.tsx, in the route tree src/app/router.tsx
- Identity, the interceptor and sign-out: src/shared/lib/identity.tsx
- Component test against MSW: src/features/products/components/ProductList.test.tsx

## Verification

```bash
npm run verify
```

`verify` runs `lint`, `typecheck`, `test` and `check`. A failing check is the law speaking: fix the code, not the check. The checks are `eslint.config.js` (with `eslint/rancho-plugin.js`) and `scripts/check.mjs`. After the server's OpenAPI document changes, copy it to `openapi/v1.json` and run `npm run generate`.
