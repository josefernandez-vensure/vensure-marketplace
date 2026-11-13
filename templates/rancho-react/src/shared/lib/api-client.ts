import { client } from '../../generated/client.gen';
import { ApiError, retryAfterMs } from './api-error';
import { raiseUnauthorized } from './session';
import { newTraceparent } from './trace';

/**
 * Points the generated client at the server, through the one interceptor. Called once, by the composition root, with
 * the identity module's authorized fetch; tests pass plain fetch.
 */
export function configureApiClient(baseUrl: string, fetch: typeof globalThis.fetch): void {
  client.setConfig({ baseUrl, fetch, throwOnError: true });
  // Every call to the server carries a traceparent.
  client.interceptors.request.use((request) => {
    if (!request.headers.has('traceparent')) request.headers.set('traceparent', newTraceparent());
    return request;
  });
  // A failure becomes an ApiError. A 401 asks the shell to sign the user in; a 403 is a final answer.
  client.interceptors.error.use((error, response) => {
    if (response?.status === 401) raiseUnauthorized();
    return new ApiError(response?.status, error, retryAfterMs(response?.headers.get('Retry-After') ?? null));
  });
}
