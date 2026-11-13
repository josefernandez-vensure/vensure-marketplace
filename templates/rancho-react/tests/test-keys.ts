/** Query keys for tests that query outside any feature. */
export const testKeys = {
  retry: (status: number) => ['test', 'retry', status] as const,
};
