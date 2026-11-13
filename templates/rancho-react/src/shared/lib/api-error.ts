/** A call the server refused or failed: its status, when it may be retried, and its body for the problem parser. */
export class ApiError extends Error {
  constructor(
    readonly status: number | undefined,
    readonly body: unknown,
    /** From Retry-After, in milliseconds, when the server sent one. */
    readonly retryAfterMs: number | undefined,
  ) {
    super(status === undefined ? 'The server could not be reached' : `The server answered ${status}`);
    this.name = 'ApiError';
  }
}

export function retryAfterMs(header: string | null): number | undefined {
  if (!header) return undefined;
  const seconds = Number(header);
  if (Number.isFinite(seconds)) return Math.max(0, seconds * 1000);
  const date = Date.parse(header);
  return Number.isNaN(date) ? undefined : Math.max(0, date - Date.now());
}
