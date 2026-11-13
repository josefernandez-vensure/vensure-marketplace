import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { configureApiClient } from '../src/shared/lib/api-client';
import { clearAppStorage } from '../src/shared/lib/app-storage';
import { resetUnauthorized } from '../src/shared/lib/session';
import { apiUrl, server } from './server';

beforeAll(() => {
  // The real client configuration, interceptors included, against MSW: only the token is left out.
  configureApiClient(apiUrl, (input, init) => fetch(input, init));
  server.listen({ onUnhandledFrame: 'error' });
});
afterEach(() => {
  cleanup();
  server.resetHandlers();
  resetUnauthorized();
  clearAppStorage();
});
afterAll(() => server.close());
