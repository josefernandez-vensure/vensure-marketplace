import { createBrowserRouter, type RouteObject } from 'react-router';
import { ProductRoute } from './routes/product';
import { ProductsRoute } from './routes/products';
import { RootError, RootLayout } from './routes/root';

/** The route tree. Every route is listed here, so the accessibility test visits each one. */
export const routes: RouteObject[] = [
  {
    path: '/',
    element: <RootLayout />,
    errorElement: <RootError />,
    children: [
      { path: 'products', element: <ProductsRoute /> },
      { path: 'products/:id', element: <ProductRoute /> },
    ],
  },
];

/** The shell mounts this application under its own path, its basename. */
export function createRouter(basename: string) {
  return createBrowserRouter(routes, { basename });
}
