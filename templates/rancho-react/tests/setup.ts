import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { client } from '../src/generated/client.gen';
import { server, apiUrl } from './server';

beforeAll(() => {
  client.setConfig({ baseUrl: apiUrl });
  server.listen({ onUnhandledFrame: 'error' });
});
afterEach(() => {
  cleanup();
  server.resetHandlers();
});
afterAll(() => server.close());
