import { PhrButtonBase, PhrSpinner } from '@prismhr/components-react';
import { useProductList } from '../api/products';
import { useProductSelection } from '../stores/selection';

export function ProductList({ nameStartsWith }: { nameStartsWith: string }) {
  const { data, error, isPending, hasNextPage, fetchNextPage, isFetchingNextPage } = useProductList(nameStartsWith);
  const { selected, toggle } = useProductSelection();

  if (isPending) return <PhrSpinner />;
  if (error) return <p role="alert">The products could not be loaded.</p>;

  const products = data.pages.flatMap((page) => page.items);
  if (products.length === 0) return <p>No products match.</p>;

  return (
    <section aria-label="Products">
      <ul>
        {products.map((product) => (
          <li key={product.id.value}>
            <label>
              <input
                type="checkbox"
                checked={selected.has(product.id.value ?? '')}
                onChange={() => toggle(product.id.value ?? '')}
              />
              {product.name}
            </label>
          </li>
        ))}
      </ul>
      {hasNextPage && !isFetchingNextPage && (
        <PhrButtonBase buttonConfig={{ title: 'More', type: 'button' }} onClick={() => void fetchNextPage()} />
      )}
    </section>
  );
}
