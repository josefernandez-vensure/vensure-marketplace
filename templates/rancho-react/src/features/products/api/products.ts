import { infiniteQueryOptions, useInfiniteQuery } from '@tanstack/react-query';
import { catalogListProducts } from '../../../generated/sdk.gen';
import { productKeys } from './product-keys';

const pageSize = 50;

export function productListQuery(nameStartsWith: string) {
  return infiniteQueryOptions({
    queryKey: productKeys.list(nameStartsWith),
    queryFn: async ({ pageParam, signal }) => {
      const { data } = await catalogListProducts({
        query: { nameStartsWith, pageSize, ...(pageParam ? { cursor: pageParam } : {}) },
        signal,
        throwOnError: true,
      });
      return data;
    },
    initialPageParam: '',
    getNextPageParam: (page) => page.nextCursor ?? undefined,
  });
}

export function useProductList(nameStartsWith: string) {
  return useInfiniteQuery(productListQuery(nameStartsWith));
}
