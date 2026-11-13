import { z } from 'zod';
import type { Classified, DataClass } from '../../generated/classes.gen';

// The client-state registry: state no contract field holds, such as a layout preference, gets its data class here
// before it crosses a boundary (the URL, browser storage, telemetry). Each schema accepts only a closed set of values,
// and this module exports nothing else that classifies.

function clientState<C extends DataClass, S extends z.ZodType>(dataClass: C, schema: S) {
  return schema.transform((value) => value as Classified<z.output<S>, C>);
}

/** How densely the product list is laid out. */
export const productListDensity = clientState('internal', z.enum(['compact', 'comfortable']));
