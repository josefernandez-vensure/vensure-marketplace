"""Prove the rancho-react template's checks bite.

Seeds one violation per check into a copy of the template and confirms the lint, the type check, the check script
or the tests fail on it, naming the check. Run it after changing a check.

Usage, from the repository root:  python -I templates/check-rancho-react.py

Run `npm install` in templates/rancho-react first (the org's packages need NODE_AUTH_TOKEN, a token that reads
GitHub Packages). Each seed gets a copy of its own, sharing the template's node_modules through a directory junction
or symlink. SEEDS=<name>,<name> reruns only those seeds; WORKERS=<n> runs that many at once (default 6).
"""
import concurrent.futures
import functools
import json
import os
import shutil
import subprocess
import sys
import tempfile
import threading
import time
from pathlib import Path

print = functools.partial(print, flush=True)

SRC = Path(__file__).resolve().parent / "rancho-react"
WORKERS = int(os.environ.get("WORKERS", "6"))
ONLY = [n for n in os.environ.get("SEEDS", "").split(",") if n]
_local = threading.local()

PRODUCTS = "src/features/products/"
LIST = PRODUCTS + "components/ProductList.tsx"
API = PRODUCTS + "api/products.ts"
FILTER = PRODUCTS + "components/ProductFilter.tsx"
VIEW = PRODUCTS + "stores/view.ts"
IDENTITY = "src/shared/lib/identity.tsx"
CLIENT = "src/shared/lib/api-client.ts"
QUERY_CLIENT = "src/shared/lib/query-client.ts"
FORM = PRODUCTS + "components/RenameProductForm.tsx"
PRODUCT_API = PRODUCTS + "api/product.ts"


def work():
    return _local.work


def fresh():
    _local.work = Path(tempfile.mkdtemp(prefix="rancho-react-seeded-"))
    shutil.copytree(SRC, _local.work, dirs_exist_ok=True, ignore=shutil.ignore_patterns("node_modules", "dist", "coverage"))
    link = _local.work / "node_modules"
    if os.name == "nt":
        subprocess.run(["cmd", "/c", "mklink", "/J", str(link), str(SRC / "node_modules")], check=True, capture_output=True)
    else:
        link.symlink_to(SRC / "node_modules", target_is_directory=True)


def done():
    link = _local.work / "node_modules"
    if os.name == "nt":
        subprocess.run(["cmd", "/c", "rmdir", str(link)], capture_output=True)
    else:
        link.unlink(missing_ok=True)
    shutil.rmtree(_local.work, ignore_errors=True)


def edit(path, old, new):
    p = work() / path
    s = p.read_text(encoding="utf-8")
    assert old in s, f"{old!r} not in {path}"
    p.write_text(s.replace(old, new, 1), encoding="utf-8")


def write(path, text):
    p = work() / path
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding="utf-8")


def remove(path):
    (work() / path).unlink()


def edit_json(path, change):
    p = work() / path
    data = json.loads(p.read_text(encoding="utf-8"))
    change(data)
    p.write_text(json.dumps(data, indent=2), encoding="utf-8")


def classify(schema, prop, data_class):
    """Changes a property's class in the server's document (None removes it) and regenerates the classes."""
    def change(d):
        p = d["components"]["schemas"][schema]["properties"][prop]
        if data_class:
            p["x-data-class"] = data_class
        else:
            p.pop("x-data-class", None)
    edit_json("openapi/v1.json", change)
    code, out = node("scripts/generate-classes.mjs")
    assert code == 0, out


def node(*args):
    r = subprocess.run(["node", *args], cwd=work(), capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.returncode, r.stdout + r.stderr


RUNNERS = {
    "lint": lambda: node("node_modules/eslint/bin/eslint.js", "."),
    "types": lambda: node("node_modules/typescript/bin/tsc", "-p", "tsconfig.json"),
    "check": lambda: node("scripts/check.mjs"),
    "test": lambda: node("node_modules/vitest/vitest.mjs", "run"),
}

# name, runner, what the failure names, seed
SEEDS = [
    # TypeScript stays strict.
    ("Any_is_refused", "lint", "@typescript-eslint/no-explicit-any",
     lambda: edit(PRODUCTS + "api/product-keys.ts", "detail: (id: string)", "detail: (id: any)")),
    ("Ts_ignore_is_refused", "lint", "@typescript-eslint/ban-ts-comment",
     lambda: edit(PRODUCTS + "api/product-keys.ts", "export const productKeys", "// @ts-ignore\nexport const productKeys")),
    ("Ts_expect_error_needs_an_issue", "lint", "@typescript-eslint/ban-ts-comment",
     lambda: edit(PRODUCTS + "api/product-keys.ts", "export const productKeys", "// @ts-expect-error: it works\nconst broken: number = 'x';\nexport const productKeys")),
    ("Strict_stays_on", "check", "typescript: tsconfig.json must set \"strict\": true",
     lambda: edit_json("tsconfig.json", lambda d: d["compilerOptions"].update(strict=False))),
    ("No_strict_option_is_turned_off", "check", "typescript: tsconfig.json turns off strictNullChecks",
     lambda: edit_json("tsconfig.json", lambda d: d["compilerOptions"].update(strictNullChecks=False))),
    # A lint suppression names its rules and says why.
    ("A_blanket_disable_is_refused", "lint", "eslint-comments/no-unlimited-disable",
     lambda: edit(PRODUCTS + "api/product-keys.ts", "export const productKeys", "/* eslint-disable */\nexport const productKeys")),
    ("A_disable_gives_its_reason", "lint", "eslint-comments/require-description",
     lambda: edit(PRODUCTS + "api/product-keys.ts", "  all: ['products'] as const,", "  // eslint-disable-next-line @typescript-eslint/no-explicit-any\n  all: ['products'] as any,")),
    # Dependencies flow shared -> features -> app.
    ("A_feature_imports_no_other_feature", "lint", "boundaries/dependencies",
     lambda: (write("src/features/orders/components/OrderList.tsx",
                    "import { ProductList } from '../../products/components/ProductList';\n\nexport function OrderList() {\n  return <ProductList nameStartsWith=\"\" />;\n}\n"),
              edit("src/features/REGISTRY.md", "- `products`", "- `orders`: place orders.\n- `products`"))),
    ("Shared_imports_no_feature", "lint", "boundaries/dependencies",
     lambda: write("src/shared/lib/products.ts", "export { productKeys } from '../../features/products/api/product-keys';\n")),
    ("A_feature_imports_nothing_from_app", "lint", "boundaries/dependencies",
     lambda: edit(LIST, "import { useProductList }", "import { createRouter } from '../../../app/router';\nvoid createRouter;\nimport { useProductList }")),
    ("Every_feature_is_registered", "check", "registry: src/features/orders isn't declared",
     lambda: write("src/features/orders/lib/orders.ts", "export const orders = 1;\n")),
    # The API's types are generated, never edited.
    ("Generated_code_is_not_edited", "check", "generated: src/generated/types.gen.ts differs",
     lambda: edit("src/generated/types.gen.ts", "export type ProductSummary = {", "export type ProductSummary = {\n    price?: number;")),
    ("Generated_code_is_complete", "check", "generated: src/generated/zod.gen.ts is missing",
     lambda: remove("src/generated/zod.gen.ts")),
    # Server data goes through TanStack Query, through the generated client, in a feature's api/.
    ("A_component_calls_no_fetch", "lint", "no-restricted-globals",
     lambda: edit(LIST, "  if (isPending)", "  void fetch('/catalog/products');\n  if (isPending)")),
    ("Fetch_is_not_reached_through_window", "lint", "no-restricted-properties",
     lambda: edit(API, "const pageSize = 50;", "const pageSize = 50;\nvoid window.fetch('/catalog/products');")),
    ("An_effect_loads_no_server_data", "lint", "rancho/no-fetch-in-effect",
     lambda: write(PRODUCTS + "api/use-product-count.ts",
                   "import { useEffect, useState } from 'react';\nimport { catalogListProducts } from '../../../generated/sdk.gen';\n\n"
                   "export function useProductCount() {\n  const [count, setCount] = useState(0);\n  useEffect(() => {\n"
                   "    void catalogListProducts({}).then((r) => setCount(r.data?.items.length ?? 0));\n  }, []);\n  return count;\n}\n")),
    ("A_component_calls_no_generated_client", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import { catalogListProducts } from '../../../generated/sdk.gen';\nvoid catalogListProducts;\nimport { useProductList }")),
    ("A_query_key_comes_from_its_factory", "lint", "rancho/query-key-from-factory",
     lambda: edit(API, "queryKey: productKeys.list(nameStartsWith),", "queryKey: ['products', nameStartsWith],")),
    ("A_cache_key_comes_from_its_factory", "lint", "rancho/query-key-from-factory",
     lambda: write(PRODUCTS + "api/forget.ts", "import type { QueryClient } from '@tanstack/react-query';\n\nexport function forget(client: QueryClient) {\n  client.setQueryData(['products'], undefined);\n}\n")),
    ("A_list_is_not_paged_in_a_loop", "lint", "no-restricted-syntax",
     lambda: write(PRODUCTS + "api/all-products.ts", "export async function all(next: { fetchNextPage: () => Promise<{ hasNextPage: boolean }> }) {\n"
                   "  for (;;) {\n    const page = await next.fetchNextPage();\n    if (!page.hasNextPage) return;\n  }\n}\n")),
    ("A_list_is_not_paged_to_the_end", "lint", "rancho/no-paging-to-the-end",
     lambda: write(PRODUCTS + "api/all-products.ts", "export async function all(next: { fetchNextPage: () => Promise<{ hasNextPage: boolean }> }): Promise<void> {\n"
                   "  const page = await next.fetchNextPage();\n  if (page.hasNextPage) await all(next);\n}\n")),
    # Client state.
    ("A_store_lives_in_stores", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import { create } from 'zustand';\nexport const useOpen = create(() => ({ open: false }));\nimport { useProductList }")),
    # Forms.
    ("Forms_use_no_other_form_library", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import { Formik } from 'formik';\nvoid Formik;\nimport { useProductList }")),
    ("Forms_use_no_UI_library_validator", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { PhrButtonBase, PhrSpinner } from '@prismhr/components-react';", "import { PhrButtonBase, PhrSpinner, Validators } from '@prismhr/components-react';\nvoid Validators;")),
    # Components and styling.
    ("Shared_uses_no_UI_library", "lint", "no-restricted-imports",
     lambda: write("src/shared/lib/spinner.tsx", "import { PhrSpinner } from '@prismhr/components-react';\n\nexport function Busy() {\n  return <PhrSpinner />;\n}\n")),
    ("Nothing_re_exports_the_UI_library", "lint", "rancho/no-ui-reexport",
     lambda: write(PRODUCTS + "components/Button.ts", "export { PhrButtonBase as Button } from '@prismhr/components-react';\n")),
    ("Nothing_re_exports_a_UI_component_by_name", "lint", "rancho/no-ui-reexport",
     lambda: write(PRODUCTS + "components/Button.ts", "import { PhrButtonBase } from '@prismhr/components-react';\n\nexport { PhrButtonBase };\n")),
    ("No_other_presentation_package", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import { useEditor } from '@tiptap/react';\nvoid useEditor;\nimport { useProductList }")),
    ("No_other_presentation_dependency", "check", "ui-dependencies: styled-components",
     lambda: edit_json("package.json", lambda d: d["dependencies"].update({"styled-components": "^6.1.0"}))),
    ("A_component_imports_no_stylesheet", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import './ProductList.css';\nimport { useProductList }")),
    ("No_static_style_object", "lint", "rancho/no-static-style",
     lambda: edit(LIST, "<section aria-label=\"Products\">", "<section aria-label=\"Products\" style={{ padding: 8, color: 'red' }}>")),
    ("The_global_stylesheet_imports_only_the_UI_library_and_layout", "check", "styles: src/app/styles.css imports ./extra.css",
     lambda: edit("src/app/styles.css", "@import './layout.css';", "@import './layout.css';\n@import './extra.css';")),
    ("The_layout_partial_never_restyles_a_UI_component", "check", "styles: src/app/layout.css styles a UI-library component",
     lambda: edit("src/app/layout.css", ".app-layout {", ".app-layout phr-spinner {\n  color: red;\n}\n\n.app-layout {")),
    # Routing.
    ("Routing_is_react_router", "lint", "no-restricted-imports",
     lambda: edit("src/app/routes/products.tsx", "import { useSearchParams } from 'react-router';", "import { useSearchParams } from 'react-router-dom';")),
    # Identity.
    ("Only_the_identity_module_uses_the_identity_library", "lint", "no-restricted-imports",
     lambda: edit(LIST, "import { useProductList }", "import { useAuth } from '@vensure-devops-qa/vensureone-iam-react';\nvoid useAuth;\nimport { useProductList }")),
    ("Nothing_touches_the_identity_session_store", "lint", "no-restricted-syntax",
     lambda: edit(API, "const pageSize = 50;", "const pageSize = 50;\nexport const session = localStorage.getItem('prismone-iam-storage');")),
    ("Nothing_clears_the_shared_storage", "lint", "no-restricted-syntax",
     lambda: edit(API, "const pageSize = 50;", "const pageSize = 50;\nexport const forgetAll = () => localStorage.clear();")),
    ("Only_the_interceptor_attaches_the_token", "lint", "no-restricted-syntax",
     lambda: edit(API, "        signal,", "        signal,\n        headers: { Authorization: 'Bearer x' },")),
    # Tests mock the network, not the client.
    ("Tests_mock_no_generated_client", "lint", "no-restricted-syntax",
     lambda: edit(LIST.replace(".tsx", ".test.tsx"), "import { ProductList } from './ProductList';", "import { ProductList } from './ProductList';\n\nvi.mock('../../../generated/sdk.gen');")),
    # Data classes (increment 2): the generated schemas classify every value, unclassified is restricted, and only
    # public and internal data reach the URL, browser storage or telemetry.
    ("Generated_classes_are_not_edited", "check", "generated: src/generated/classes.gen.ts differs",
     lambda: edit("src/generated/classes.gen.ts", '"name": classified("public", z.string()),', '"name": classified("internal", z.string()),')),
    ("Unparsed_input_goes_into_no_URL", "lint", "rancho/classified-at-boundary",
     lambda: edit(FILTER, "setSearch(nameStartsWith ? { name: nameStartsWith } : {});", "setSearch({ name: text });")),
    ("Unparsed_input_goes_into_no_link", "lint", "rancho/classified-at-boundary",
     lambda: (edit(FILTER, "    <PhrInputText", "    <>\n    <a href={`/products?name=${search.get('name') ?? ''}`}>Share</a>\n    <PhrInputText"),
              edit(FILTER, "    />\n  );", "    />\n    </>\n  );"))),
    ("Confidential_data_goes_into_no_URL", "lint", "rancho/classified-at-boundary",
     lambda: (classify("ProductSummary", "name", "confidential"),
              edit(LIST, "              {product.name}", "              <Link to={`/products?name=${product.name}`}>{product.name}</Link>"),
              edit(LIST, "import { useProductList }", "import { Link } from 'react-router';\nimport { useProductList }"))),
    ("Unclassified_data_is_restricted", "lint", "rancho/classified-at-boundary",
     lambda: (classify("ProductSummary", "name", None),
              edit(LIST, "              {product.name}", "              <Link to={`/products?name=${product.name}`}>{product.name}</Link>"),
              edit(LIST, "import { useProductList }", "import { Link } from 'react-router';\nimport { useProductList }"))),
    ("Unparsed_input_goes_into_no_storage", "lint", "rancho/classified-at-boundary",
     lambda: edit(FILTER, "        const { nameStartsWith }", "        localStorage.setItem('products.filter', JSON.stringify({ name: text }));\n        const { nameStartsWith }")),
    ("Unparsed_input_goes_into_no_cookie", "lint", "rancho/classified-at-boundary",
     lambda: edit(FILTER, "        const { nameStartsWith }", "        document.cookie = `filter=${text}`;\n        const { nameStartsWith }")),
    ("A_persisted_store_names_what_it_persists", "lint", "rancho/classified-at-boundary",
     lambda: edit(VIEW, "      partialize: ({ density }) => ({ density }),\n", "")),
    ("A_persisted_store_persists_only_classified_state", "lint", "rancho/classified-at-boundary",
     lambda: edit(VIEW, "partialize: ({ density }) => ({ density }),", "partialize: ({ density }) => ({ density, note: String(density).toUpperCase() }),")),
    ("Unparsed_input_goes_into_no_telemetry", "lint", "rancho/classified-at-boundary",
     lambda: edit(FILTER, "        const { nameStartsWith }", "        console.info('filtered', text);\n        const { nameStartsWith }")),
    ("Nothing_asserts_a_data_class", "lint", "rancho/no-class-assertion",
     lambda: (edit(FILTER, "        const { nameStartsWith } = productFilter.parse({ nameStartsWith: text });", "        const nameStartsWith = text as Classified<string, 'public'>;"),
              edit(FILTER, "import { productFilter } from '../api/products';", "import type { Classified } from '../../../generated/classes.gen';"))),
    ("Nothing_asserts_a_response_type", "lint", "rancho/no-response-assertion",
     lambda: (edit(API, "return zPageOfProductSummary.parse(data);", "return data as PageOfProductSummary;"),
              edit(API, "import { productKeys } from './product-keys';", "import type { PageOfProductSummary } from '../../../generated/types.gen';\nimport { productKeys } from './product-keys';"))),
    ("Only_the_parser_reads_a_problem", "lint", "rancho/one-problem-parser",
     lambda: write(PRODUCTS + "api/product-title.ts", "import { catalogGetProduct } from '../../../generated/sdk.gen';\n\n"
                   "export async function failure(id: string) {\n  const { error } = await catalogGetProduct({ path: { id } });\n  return error?.title;\n}\n")),
    ("A_token_never_leaves_the_identity_module", "lint", "rancho/identity-exports-no-token",
     lambda: edit(IDENTITY, "export function useSignOut", "export function useAccessToken() {\n  return useAuth().getAccessToken;\n}\n\nexport function useSignOut")),
    ("The_identity_module_exports_no_token_store", "lint", "rancho/identity-exports-no-token",
     lambda: edit(IDENTITY, "export function useSignOut", "export const session = () => new StorageService().getTokens();\n\nexport function useSignOut")),
    ("No_nested_credential_is_read", "lint", "no-restricted-syntax",
     lambda: edit(IDENTITY, "export function useSignOut", "export const upstream = (claims: Record<string, string>) => claims.idp_access_token;\n\nexport function useSignOut")),
    # The required behaviour tests (increment 3): each fails when its behaviour regresses.
    ("Every_call_carries_a_traceparent", "test", "sends a traceparent on every call",
     lambda: edit(CLIENT, "if (!request.headers.has('traceparent')) request.headers.set('traceparent', newTraceparent());", "void newTraceparent;")),
    ("A_401_asks_for_sign_in_once", "test", "asks the shell to sign the user in once",
     lambda: edit("src/shared/lib/session.ts", "  if (unauthorizedPending) return;\n", "")),
    ("A_403_never_asks_for_sign_in", "test", "treats a 403 as a final answer",
     lambda: edit(CLIENT, "if (response?.status === 401) raiseUnauthorized();", "if (response?.status === 401 || response?.status === 403) raiseUnauthorized();")),
    ("A_403_is_never_retried", "test", "never retries a 403",
     lambda: edit(QUERY_CLIENT, "return error.status < 400 || error.status >= 500;", "return true;")),
    ("A_429_waits_for_Retry_After", "test", "waits for a 429",
     lambda: edit(QUERY_CLIENT, "if (error instanceof ApiError && error.retryAfterMs !== undefined) return error.retryAfterMs;", "")),
    ("A_form_refuses_invalid_values", "test", "refuses an empty name",
     lambda: edit(PRODUCTS + "schemas/rename-product.ts", ".min(1, 'Enter a name.')", "")),
    ("A_form_shows_server_field_errors_on_their_fields", "test", "shows the server's field error",
     lambda: edit(FORM, "if (field === 'name') form.setError('name', { message: messages.join(' ') });", "void field; void messages;")),
    ("An_edit_form_resets_to_what_it_wrote", "test", "sends the etag of the values it was last reset from",
     lambda: edit(FORM, "      form.reset({ name: values.name, etag: written.etag });", "      void written;")),
    ("A_412_is_not_retried", "test", "refetches on a 412",
     lambda: edit(PRODUCT_API, "      return zRenameProductResponse.parse(data);\n    },\n    onSuccess", "      return zRenameProductResponse.parse(data);\n    },\n    retry: 1,\n    onSuccess")),
    ("A_refetch_never_resets_an_edit_in_progress", "test", "keeps the user's edits",
     lambda: edit(FORM, "<RenameProductFields id={id}", "<RenameProductFields key={product.data.etag} id={id}")),
    ("A_retried_mutation_keeps_its_key", "test", "sends the same idempotency key",
     lambda: edit(PRODUCT_API, "headers: { 'If-Match': etag, 'Idempotency-Key': idempotencyKey },", "headers: { 'If-Match': etag, 'Idempotency-Key': `${idempotencyKey}-${crypto.randomUUID()}` },")),
    ("Sign_out_clears_what_the_application_stored", "test", "clears the caches and what the application stored",
     lambda: edit("src/shared/lib/sign-out.ts", "  clearAppStorage();\n", "  void clearAppStorage;\n")),
    ("An_unknown_problem_shows_no_title", "test", "shows neither the title nor the detail",
     lambda: edit("src/shared/lib/ProblemMessage.tsx", "problem.type && userFacingProblemTypes.has(problem.type) ? problem.title : undefined", "problem.title")),
    ("Every_route_is_checked_for_accessibility", "test", "lists every route",
     lambda: edit("src/app/router.tsx", "      { path: 'products/:id', element: <ProductRoute /> },", "      { path: 'products/:id', element: <ProductRoute /> },\n      { path: 'about', element: <h1>About</h1> },")),
    ("A_route_with_an_accessibility_violation_fails", "test", "passes an automated accessibility check",
     lambda: edit("src/app/routes/products.tsx", "      <h1>Products</h1>", "      <h1>Products</h1>\n      <input type=\"text\" />")),
    ("The_root_route_shows_its_error_element", "test", "shows the root error element",
     lambda: edit("src/app/routes/root.tsx", "<h1>{status === 404 ? 'Page not found' : 'Something went wrong'}</h1>", "<h1>{status === 404 ? 'Page not found' : 'Oops'}</h1>")),
    # The template's own tests run.
    ("The_tests_run", "test", "ProductList",
     lambda: edit(LIST, "No products match.", "Nothing here.")),
]


def run_seed(name, runner, expected, seed):
    fresh()
    try:
        try:
            seed()
        except AssertionError as broken:
            return name, False, runner, f"  SEED BROKEN: {broken}"
        code, out = RUNNERS[runner]()
        ok = code != 0 and expected in out
        return name, ok, runner, "" if ok else "\n    " + "\n    ".join(out.strip().splitlines()[-12:])
    finally:
        done()


if not (SRC / "node_modules").exists():
    sys.exit("Run npm install in templates/rancho-react first.")

began = time.monotonic()
jobs = [s for s in SEEDS if not ONLY or s[0] in ONLY]
failures = []


def timed(job):
    clock = time.monotonic()
    return run_seed(*job), time.monotonic() - clock


# The unseeded template must pass every runner first, or no failure below means anything.
_local.work = None
fresh()
try:
    for runner in ("lint", "types", "check", "test"):
        code, out = RUNNERS[runner]()
        if code != 0:
            sys.exit(f"The unseeded template fails {runner}:\n{out[-3000:]}")
finally:
    done()

with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
    for (name, ok, runner, detail), seconds in pool.map(timed, jobs):
        print(f"{'FAILS AS EXPECTED' if ok else 'NOT CAUGHT':18} {runner:6} {name}  ({seconds:.0f}s){detail}")
        if not ok:
            failures.append(name)

print(f"\n{len(jobs) - len(failures)} of {len(jobs)} seeds caught in {(time.monotonic() - began) / 60:.1f} minutes")
sys.exit(1 if failures else 0)
