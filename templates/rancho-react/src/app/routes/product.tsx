import { useParams } from 'react-router';
import { zCatalogGetProductPath } from '../../generated/classes.gen';
import { RenameProductForm } from '../../features/products/components/RenameProductForm';

/**
 * Route params are parsed through the operation's own parameter schema before anything uses them. The schema carries the server's
 * constraints, so an id the server could never find, such as the nil GUID, is not found here, without a call.
 */
export function ProductRoute() {
  const params = zCatalogGetProductPath.safeParse(useParams());
  return (
    <>
      <h1>Product</h1>
      {params.success ? <RenameProductForm id={params.data.id} /> : <p role="alert">This product does not exist.</p>}
    </>
  );
}
