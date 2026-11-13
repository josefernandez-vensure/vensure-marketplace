// Server answers for the products feature's tests: the shapes the server's OpenAPI document declares.

export const productId = '0b6f4f2e-7c3d-4a51-9e2b-6f1a2c3d4e5f';

export const product = (name: string, etag: string) => ({
  id: { value: productId },
  name,
  isDiscontinued: false,
  stockOnHand: 3,
  etag,
});

export const page = (names: string[]) => ({
  items: names.map((name, i) => ({ id: { value: `00000000-0000-4000-8000-00000000000${i}` }, name })),
  nextCursor: null,
});

export const problem = (status: number, type: string, extra: Record<string, unknown> = {}) => ({
  type,
  title: 'A title for the logs, not the user',
  status,
  detail: 'Internal detail',
  traceId: '4bf92f3577b34da6a3ce929d0e0e4736',
  ...extra,
});
