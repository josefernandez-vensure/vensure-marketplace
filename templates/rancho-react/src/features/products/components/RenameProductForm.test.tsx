import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { renderWithQuery } from '../../../../tests/render';
import { apiUrl, server } from '../../../../tests/server';
import { problem, product, productId } from '../../../../tests/products';
import { productKeys } from '../api/product-keys';
import { RenameProductForm } from './RenameProductForm';

const productUrl = `${apiUrl}/catalog/products/${productId}`;

/** A server that keeps one product, versioned: a rename must name its current etag. */
function versionedProduct(initial: { name: string; etag: string }) {
  const state = { ...initial, renames: [] as { name: string; ifMatch: string | null }[], reads: 0 };
  server.use(
    http.get(productUrl, () => {
      state.reads += 1;
      return HttpResponse.json(product(state.name, state.etag));
    }),
    http.post(`${productUrl}/rename`, async ({ request }) => {
      const { name } = (await request.json()) as { name: string };
      const ifMatch = request.headers.get('If-Match');
      state.renames.push({ name, ifMatch });
      if (ifMatch !== state.etag) return HttpResponse.json(problem(412, 'urn:problem:precondition-failed'), { status: 412 });
      state.name = name;
      state.etag = `"${state.renames.length + 1}"`;
      return HttpResponse.json({ etag: state.etag });
    }),
  );
  return state;
}

async function rename(to: string) {
  const input = await screen.findByLabelText('Name');
  await userEvent.clear(input);
  if (to) await userEvent.type(input, to);
  await userEvent.click(screen.getByRole('button', { name: 'Save' }));
}

describe('RenameProductForm', () => {
  it('refuses an empty name before calling the server', async () => {
    const server = versionedProduct({ name: 'Anvil', etag: '"1"' });
    renderWithQuery(<RenameProductForm id={productId} />);
    await rename('');
    expect(await screen.findByText('Enter a name.')).toBeInTheDocument();
    expect(server.renames).toHaveLength(0);
  });

  it('shows the server\'s field error on its field', async () => {
    server.use(
      http.get(productUrl, () => HttpResponse.json(product('Anvil', '"1"'))),
      http.post(`${productUrl}/rename`, () => HttpResponse.json(
        problem(400, 'urn:problem:validation', { errors: { name: ['That name is taken.'] } }),
        { status: 400 },
      )),
    );
    renderWithQuery(<RenameProductForm id={productId} />);
    await rename('Bucket');
    expect(await screen.findByText('That name is taken.')).toBeInTheDocument();
  });

  it('sends the etag of the values it was last reset from, so a second save after its own isn\'t refused', async () => {
    const state = versionedProduct({ name: 'Anvil', etag: '"1"' });
    renderWithQuery(<RenameProductForm id={productId} />);
    await rename('Bucket');
    await waitFor(() => expect(state.renames).toHaveLength(1));
    await rename('Crate');
    await waitFor(() => expect(state.renames).toHaveLength(2));
    expect(state.renames).toEqual([{ name: 'Bucket', ifMatch: '"1"' }, { name: 'Crate', ifMatch: '"2"' }]);
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('keeps the user\'s edits, and the etag they came with, when the product is refetched behind them', async () => {
    const state = versionedProduct({ name: 'Anvil', etag: '"1"' });
    const { client } = renderWithQuery(<RenameProductForm id={productId} />);
    const input = await screen.findByLabelText('Name');
    await userEvent.clear(input);
    await userEvent.type(input, 'Bucket');
    state.etag = '"7"'; // someone else saved
    await client.refetchQueries({ queryKey: productKeys.all });
    await waitFor(() => expect(state.reads).toBe(2));
    expect(input).toHaveValue('Bucket');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await waitFor(() => expect(state.renames).toEqual([{ name: 'Bucket', ifMatch: '"1"' }]));
    expect(await screen.findByText(/Someone changed this product/)).toBeInTheDocument();
  });

  it('refetches on a 412 and tells the user, without retrying the write', async () => {
    const state = versionedProduct({ name: 'Anvil', etag: '"1"' });
    renderWithQuery(<RenameProductForm id={productId} />);
    await screen.findByLabelText('Name');
    state.etag = '"7"'; // someone else saved
    await rename('Bucket');
    expect(await screen.findByText(/Someone changed this product/)).toBeInTheDocument();
    await waitFor(() => expect(state.reads).toBe(2));
    expect(state.renames).toHaveLength(1);
  });
});
