import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { apiUrl, server } from '../../../tests/server';
import { problem } from '../../../tests/products';
import { testKeys } from '../../../tests/test-keys';
import { catalogListProducts } from '../../generated/sdk.gen';
import { createQueryClient } from './query-client';

const listProducts = async () => (await catalogListProducts({})).data;

describe('the query client\'s retry rule', () => {
  it('never retries a 403', async () => {
    let calls = 0;
    server.use(http.get(`${apiUrl}/catalog/products`, () => {
      calls += 1;
      return HttpResponse.json(problem(403, 'urn:problem:forbidden'), { status: 403 });
    }));
    await expect(createQueryClient().fetchQuery({ queryKey: testKeys.retry(403), queryFn: listProducts })).rejects.toThrow();
    expect(calls).toBe(1);
  });

  it.each([
    ['0', 0, 500],
    ['1', 950, 1900],
  ])('waits for a 429\'s Retry-After (%s s) before it retries, and no longer', async (retryAfter, atLeast, below) => {
    const at: number[] = [];
    server.use(http.get(`${apiUrl}/catalog/products`, () => {
      at.push(Date.now());
      return at.length === 1
        ? HttpResponse.json(problem(429, 'urn:problem:rate-limited'), { status: 429, headers: { 'Retry-After': retryAfter } })
        : HttpResponse.json({ items: [], nextCursor: null });
    }));
    await createQueryClient().fetchQuery({ queryKey: testKeys.retry(429), queryFn: listProducts });
    expect(at).toHaveLength(2);
    expect(at[1]! - at[0]!).toBeGreaterThanOrEqual(atLeast);
    expect(at[1]! - at[0]!).toBeLessThan(below);
  });
});
