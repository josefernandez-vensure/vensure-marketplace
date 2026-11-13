import { QueryClient } from '@tanstack/react-query';
import { ApiError } from './api-error';

const MAX_RETRIES = 3;

/** One retry rule for every query: never a 4xx but 408 and 429, and never before the server's Retry-After. */
export function shouldRetry(failureCount: number, error: unknown): boolean {
  if (failureCount >= MAX_RETRIES) return false;
  if (!(error instanceof ApiError) || error.status === undefined) return true;
  if (error.status === 408 || error.status === 429) return true;
  return error.status < 400 || error.status >= 500;
}

export function retryDelay(failureCount: number, error: unknown): number {
  if (error instanceof ApiError && error.retryAfterMs !== undefined) return error.retryAfterMs;
  return Math.min(1000 * 2 ** failureCount, 30_000);
}

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: shouldRetry, retryDelay },
      // A mutation is retried only when it carries an idempotency key, and then by its own options.
      mutations: { retry: false },
    },
  });
}
