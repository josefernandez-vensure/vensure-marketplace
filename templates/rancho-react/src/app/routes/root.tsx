import { PhrButtonBase } from '@prismhr/components-react';
import { Outlet, isRouteErrorResponse, useRouteError } from 'react-router';
import { useSignOut } from '../../shared/lib/identity';

export function RootLayout() {
  const signOut = useSignOut();
  return (
    <main className="app-layout">
      <PhrButtonBase buttonConfig={{ title: 'Sign out', type: 'button' }} onClick={() => void signOut()} />
      <Outlet />
    </main>
  );
}

/** The root route's error element: a failure shows here and never blanks the application. */
export function RootError() {
  const error = useRouteError();
  const status = isRouteErrorResponse(error) ? error.status : undefined;
  return (
    <main className="app-layout" role="alert">
      <h1>{status === 404 ? 'Page not found' : 'Something went wrong'}</h1>
    </main>
  );
}
