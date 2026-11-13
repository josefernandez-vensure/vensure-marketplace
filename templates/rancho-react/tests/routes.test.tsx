import { screen } from '@testing-library/react';
import axe from 'axe-core';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { RootError } from '../src/app/routes/root';
import { routes } from '../src/app/router';
import { page, product, productId } from './products';
import { renderRoutes } from './render';
import { apiUrl, server } from './server';

// Every route in the tree, at a path that renders it with data. A new route fails here until it is listed.
const visits: Record<string, string> = {
  products: '/products',
  'products/:id': `/products/${productId}`,
};

function leafPaths(list = routes): string[] {
  return list.flatMap((r) => (r.children ? leafPaths(r.children) : r.path ? [r.path] : []));
}


describe('the routes', () => {
  it('lists every route for the accessibility check', () => {
    expect(leafPaths().sort()).toEqual(Object.keys(visits).sort());
  });

  it.each(Object.entries(visits))('%s passes an automated accessibility check', async (_, path) => {
    server.use(
      http.get(`${apiUrl}/catalog/products`, () => HttpResponse.json(page(['Anvil']))),
      http.get(`${apiUrl}/catalog/products/:id`, () => HttpResponse.json(product('Anvil', '"1"'))),
    );
    const { container } = renderRoutes(routes, path, undefined, { identity: true });
    await screen.findByRole('heading', { level: 1 });
    await screen.findByText('Anvil').catch(() => screen.findByLabelText('Name'));
    const { violations } = await axe.run(container, { rules: { 'color-contrast': { enabled: false } } });
    expect(violations.map((v) => `${v.id}: ${v.help}`)).toEqual([]);
  });

  it('shows the root error element when a route fails to render', async () => {
    function Broken(): never {
      throw new Error('render failed');
    }
    renderRoutes([{ path: '/', element: <p>root</p>, errorElement: <RootError />, children: [] }, { path: '/broken', element: <Broken />, errorElement: <RootError /> }], '/broken');
    expect(await screen.findByRole('heading', { name: 'Something went wrong' })).toBeInTheDocument();
  });
});
