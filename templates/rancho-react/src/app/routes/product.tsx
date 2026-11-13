import { useParams } from 'react-router';
import { zCatalogGetProductPath } from '../../generated/classes.gen';
import { RenameProductForm } from '../../features/products/components/RenameProductForm';

/** Route params are parsed through the operation's own parameter schema before anything uses them. */
export function ProductRoute() {
  const { id } = zCatalogGetProductPath.parse(useParams());
  return (
    <>
      <h1>Product</h1>
      <RenameProductForm id={id} />
    </>
  );
}
