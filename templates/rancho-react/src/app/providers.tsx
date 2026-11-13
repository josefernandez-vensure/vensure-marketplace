import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { RouterProvider } from 'react-router';
import { IdentityProvider } from '../shared/lib/identity';
import { createRouter } from './router';

export interface AppProps {
  /** The path the shell mounts this application under. */
  basename: string;
}

export function App({ basename }: AppProps) {
  const [queryClient] = useState(() => new QueryClient());
  const [router] = useState(() => createRouter(basename));
  return (
    <IdentityProvider>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </IdentityProvider>
  );
}
