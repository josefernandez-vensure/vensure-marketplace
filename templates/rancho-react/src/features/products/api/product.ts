import { queryOptions, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { zProductResponse, zReceiveStockResponse, zRenameProductResponse } from '../../../generated/classes.gen';
import { catalogGetProduct, catalogReceiveStock, catalogRenameProduct } from '../../../generated/sdk.gen';
import { ApiError } from '../../../shared/lib/api-error';
import { retryDelay } from '../../../shared/lib/query-client';
import { productKeys } from './product-keys';

export function productQuery(id: string) {
  return queryOptions({
    queryKey: productKeys.detail(id),
    queryFn: async ({ signal }) => {
      const { data } = await catalogGetProduct({ path: { id }, signal });
      return zProductResponse.parse(data);
    },
  });
}

/** For an edit form: the product as loaded, never refreshed behind the user's back while they edit. */
export function useProductForEditing(id: string) {
  return useQuery({ ...productQuery(id), refetchOnWindowFocus: false, refetchOnReconnect: false });
}

export function isStale(error: unknown): boolean {
  return error instanceof ApiError && error.status === 412;
}

/** A versioned write: it sends the etag of the values the user edited. A 412 is the form's to show; never retried. */
export function useRenameProduct(id: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async ({ name, etag }: { name: string; etag: string }) => {
      const { data } = await catalogRenameProduct({ path: { id }, body: { name }, headers: { 'If-Match': etag } });
      return zRenameProductResponse.parse(data);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productKeys.all }),
  });
}

/**
 * A keyed write that may be retried: the key is made once per intent (one submission) and every attempt sends it, so
 * the server runs it once.
 */
export function useReceiveStock(id: string) {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: async ({ quantity, etag, idempotencyKey }: { quantity: number; etag: string; idempotencyKey: string }) => {
      const { data } = await catalogReceiveStock({
        path: { id },
        body: { quantity },
        headers: { 'If-Match': etag, 'Idempotency-Key': idempotencyKey },
      });
      return zReceiveStockResponse.parse(data);
    },
    retry: (failureCount, error) => failureCount < 3 && error instanceof ApiError && (error.status === undefined || error.status >= 500 || error.status === 429),
    retryDelay,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: productKeys.all }),
  });
  return {
    ...mutation,
    /** One intent, one key: every retry of this call sends the same one. */
    receive: (values: { quantity: number; etag: string }) => mutation.mutateAsync({ ...values, idempotencyKey: crypto.randomUUID() }),
  };
}
