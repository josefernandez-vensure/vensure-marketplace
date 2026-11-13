import { QueryClientProvider } from '@tanstack/react-query';
import { act, renderHook } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import type { ReactNode } from 'react';
import { describe, expect, it } from 'vitest';
import { testQueryClient } from '../../../../tests/render';
import { apiUrl, server } from '../../../../tests/server';
import { productId } from '../../../../tests/products';
import { useReceiveStock } from './product';

describe('useReceiveStock', () => {
  it('sends the same idempotency key on every attempt of one intent, and a new one for the next', async () => {
    const keys: (string | null)[] = [];
    server.use(http.post(`${apiUrl}/catalog/products/${productId}/stock-receipts`, ({ request }) => {
      keys.push(request.headers.get('Idempotency-Key'));
      return keys.length === 1
        ? new HttpResponse(null, { status: 503, headers: { 'Retry-After': '0' } })
        : HttpResponse.json({ etag: `"${keys.length}"` });
    }));
    const client = testQueryClient();
    const wrapper = ({ children }: { children: ReactNode }) => <QueryClientProvider client={client}>{children}</QueryClientProvider>;
    const { result } = renderHook(() => useReceiveStock(productId), { wrapper });

    await act(() => result.current.receive({ quantity: 5, etag: '"1"' }));
    expect(keys).toHaveLength(2);
    expect(keys[0]).toBeTruthy();
    expect(keys[1]).toBe(keys[0]);

    await act(() => result.current.receive({ quantity: 5, etag: '"2"' }));
    expect(keys[2]).not.toBe(keys[0]);
  });
});
