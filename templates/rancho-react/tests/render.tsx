import { QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import type { ReactElement } from 'react';
import { createMemoryRouter, RouterProvider, type RouteObject } from 'react-router';
import { IdentityProvider } from '../src/shared/lib/identity';
import { createQueryClient } from '../src/shared/lib/query-client';

/** The application's own query client, without its retry delay, so a test sees each state at once. */
export function testQueryClient() {
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { ...client.getDefaultOptions().queries, retry: false } });
  return client;
}

export function renderWithQuery(ui: ReactElement, client = testQueryClient()) {
  return { client, ...render(<QueryClientProvider client={client}>{ui}</QueryClientProvider>) };
}

/** Renders the given routes at a path, as the shell would mount them. */
export function renderRoutes(routes: RouteObject[], path: string, client = testQueryClient(), { identity = false } = {}) {
  const router = createMemoryRouter(routes, { initialEntries: [path] });
  const app = <RouterProvider router={router} />;
  return { client, router, ...render(<QueryClientProvider client={client}>{identity ? <IdentityProvider>{app}</IdentityProvider> : app}</QueryClientProvider>) };
}
