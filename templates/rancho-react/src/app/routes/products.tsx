import { useSearchParams } from 'react-router';
import { ProductList } from '../../features/products/components/ProductList';

/** A route composes features; it holds no logic of its own. */
export function ProductsRoute() {
  const [search] = useSearchParams();
  return (
    <>
      <h1>Products</h1>
      <ProductList nameStartsWith={search.get('name') ?? ''} />
    </>
  );
}
