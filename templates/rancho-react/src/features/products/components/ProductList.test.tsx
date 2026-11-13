import { screen } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { renderWithQuery } from '../../../../tests/render';
import { apiUrl, server } from '../../../../tests/server';
import { ProductList } from './ProductList';

const page = (names: string[]) => ({
  items: names.map((name, i) => ({ id: { value: `00000000-0000-4000-8000-00000000000${i}` }, name })),
  nextCursor: null,
});

describe('ProductList', () => {
  it('shows the loading state, then the products', async () => {
    server.use(http.get(`${apiUrl}/catalog/products`, () => HttpResponse.json(page(['Anvil', 'Bucket']))));
    renderWithQuery(<ProductList nameStartsWith="" />);
    expect(screen.getByLabelText('Loading')).toBeInTheDocument();
    expect(await screen.findByText('Anvil')).toBeInTheDocument();
    expect(screen.getByText('Bucket')).toBeInTheDocument();
  });

  it('shows the error state', async () => {
    server.use(http.get(`${apiUrl}/catalog/products`, () => HttpResponse.json({ title: 'Boom' }, { status: 500 })));
    renderWithQuery(<ProductList nameStartsWith="" />);
    expect(await screen.findByRole('alert')).toHaveTextContent('could not be loaded');
  });

  it('shows the empty state', async () => {
    server.use(http.get(`${apiUrl}/catalog/products`, () => HttpResponse.json(page([]))));
    renderWithQuery(<ProductList nameStartsWith="" />);
    expect(await screen.findByText('No products match.')).toBeInTheDocument();
  });
});
