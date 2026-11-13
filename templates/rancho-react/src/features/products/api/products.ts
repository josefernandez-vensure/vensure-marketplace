import { infiniteQueryOptions, useInfiniteQuery } from '@tanstack/react-query';
import { zCatalogListProductsQuery, zPageOfProductSummary } from '../../../generated/classes.gen';
import { catalogListProducts } from '../../../generated/sdk.gen';
import { productKeys } from './product-keys';

const pageSize = 50;

/** The list's filter, parsed through the operation's own parameter schema, so it carries its data class. */
export const productFilter = zCatalogListProductsQuery.pick({ nameStartsWith: true });

export function productListQuery(nameStartsWith: string) {
  return infiniteQueryOptions({
    queryKey: productKeys.list(nameStartsWith),
    queryFn: async ({ pageParam, signal }) => {
      const { data } = await catalogListProducts({
        query: { nameStartsWith, pageSize, ...(pageParam ? { cursor: pageParam } : {}) },
        signal,
        throwOnError: true,
      });
      // Every response is parsed through its generated schema: what the application holds carries its data class.
      return zPageOfProductSummary.parse(data);
    },
    initialPageParam: '',
    getNextPageParam: (page) => page.nextCursor ?? undefined,
  });
}

export function useProductList(nameStartsWith: string) {
  return useInfiniteQuery(productListQuery(nameStartsWith));
}
