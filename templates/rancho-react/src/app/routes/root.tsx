import { Outlet, isRouteErrorResponse, useRouteError } from 'react-router';

export function RootLayout() {
  return (
    <main className="app-layout">
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
