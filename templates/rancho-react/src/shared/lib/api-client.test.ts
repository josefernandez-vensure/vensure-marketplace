import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import { apiUrl, server } from '../../../tests/server';
import { productId, problem } from '../../../tests/products';
import { catalogGetProduct, catalogListProducts } from '../../generated/sdk.gen';
import { ApiError } from './api-error';
import { onUnauthorized } from './session';

describe('the API client', () => {
  it('sends a traceparent on every call to the server', async () => {
    const seen: (string | null)[] = [];
    server.use(
      http.get(`${apiUrl}/catalog/products`, ({ request }) => {
        seen.push(request.headers.get('traceparent'));
        return HttpResponse.json({ items: [], nextCursor: null });
      }),
      http.get(`${apiUrl}/catalog/products/:id`, ({ request }) => {
        seen.push(request.headers.get('traceparent'));
        return HttpResponse.json(problem(404, 'urn:problem:not-found'), { status: 404 });
      }),
    );
    await catalogListProducts({});
    await catalogGetProduct({ path: { id: productId } }).catch(() => undefined);
    expect(seen).toHaveLength(2);
    for (const header of seen) expect(header).toMatch(/^00-[0-9a-f]{32}-[0-9a-f]{16}-01$/);
    expect(new Set(seen).size).toBe(2);
  });

  it('asks the shell to sign the user in once on a 401, and never retries the call', async () => {
    let calls = 0;
    server.use(http.get(`${apiUrl}/catalog/products`, () => {
      calls += 1;
      return new HttpResponse(null, { status: 401 });
    }));
    const signIn = vi.fn();
    const stop = onUnauthorized(signIn);
    try {
      const results = await Promise.allSettled([catalogListProducts({}), catalogListProducts({})]);
      expect(results.map((r) => r.status)).toEqual(['rejected', 'rejected']);
      expect(signIn).toHaveBeenCalledTimes(1);
      expect(calls).toBe(2);
    } finally {
      stop();
    }
  });

  it('treats a 403 as a final answer: no sign-in, a permission error', async () => {
    server.use(http.get(`${apiUrl}/catalog/products`, () => HttpResponse.json(problem(403, 'urn:problem:forbidden'), { status: 403 })));
    const signIn = vi.fn();
    const stop = onUnauthorized(signIn);
    try {
      const error: unknown = await catalogListProducts({}).catch((e: unknown) => e);
      expect(error).toBeInstanceOf(ApiError);
      expect((error as ApiError).status).toBe(403);
      expect(signIn).not.toHaveBeenCalled();
    } finally {
      stop();
    }
  });
});
