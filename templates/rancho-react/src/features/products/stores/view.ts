import type { z } from 'zod';
import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import { productListDensity } from '../../../shared/lib/client-state';

type Density = z.output<typeof productListDensity>;

interface ProductListView {
  density: Density;
  setDensity: (density: string) => void;
}

/** A layout preference kept across visits. Only what partialize names is stored, and it carries its data class. */
export const useProductListView = create<ProductListView>()(
  persist(
    (set) => ({
      density: productListDensity.parse('comfortable'),
      setDensity: (density) => set({ density: productListDensity.parse(density) }),
    }),
    {
      name: 'products.view',
      storage: createJSONStorage(() => localStorage),
      partialize: ({ density }) => ({ density }),
    },
  ),
);
