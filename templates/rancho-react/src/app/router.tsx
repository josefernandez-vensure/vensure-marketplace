import { createBrowserRouter } from 'react-router';
import { RootError, RootLayout } from './routes/root';
import { ProductsRoute } from './routes/products';

/** The route tree. The shell mounts this application under its own path, its basename. */
export function createRouter(basename: string) {
  return createBrowserRouter(
    [
      {
        path: '/',
        element: <RootLayout />,
        errorElement: <RootError />,
        children: [{ path: 'products', element: <ProductsRoute /> }],
      },
    ],
    { basename },
  );
}
