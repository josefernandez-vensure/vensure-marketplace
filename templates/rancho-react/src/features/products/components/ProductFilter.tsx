import { PhrInputText } from '@prismhr/components-react';
import { useSearchParams } from 'react-router';
import { productFilter } from '../api/products';

/** The filter lives in the URL: the value written there is parsed first, so it carries its data class. */
export function ProductFilter() {
  const [search, setSearch] = useSearchParams();
  return (
    <PhrInputText
      label="Name starts with"
      value={search.get('name') ?? ''}
      onValueChange={(text) => {
        const { nameStartsWith } = productFilter.parse({ nameStartsWith: text });
        setSearch(nameStartsWith ? { name: nameStartsWith } : {});
      }}
    />
  );
}
