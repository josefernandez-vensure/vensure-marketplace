import { QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { RouterProvider } from 'react-router';
import { IdentityProvider } from '../shared/lib/identity';
import { createQueryClient } from '../shared/lib/query-client';
import { createRouter } from './router';

export interface AppProps {
  /** The path the shell mounts this application under. */
  basename: string;
}

export function App({ basename }: AppProps) {
  const [queryClient] = useState(createQueryClient);
  const [router] = useState(() => createRouter(basename));
  return (
    <QueryClientProvider client={queryClient}>
      <IdentityProvider>
        <RouterProvider router={router} />
      </IdentityProvider>
    </QueryClientProvider>
  );
}
