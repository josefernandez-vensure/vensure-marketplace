/** The key factory for the server's products: every query key for a product comes from here. */
export const productKeys = {
  all: ['products'] as const,
  lists: () => [...productKeys.all, 'list'] as const,
  list: (nameStartsWith: string) => [...productKeys.lists(), { nameStartsWith }] as const,
  detail: (id: string) => [...productKeys.all, 'detail', id] as const,
};
