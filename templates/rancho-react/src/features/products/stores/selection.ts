import { create } from 'zustand';

/** Which products the user has selected: their ids, never the records. */
interface ProductSelection {
  selected: ReadonlySet<string>;
  toggle: (id: string) => void;
  clear: () => void;
}

export const useProductSelection = create<ProductSelection>()((set) => ({
  selected: new Set(),
  toggle: (id) =>
    set(({ selected }) => {
      const next = new Set(selected);
      if (!next.delete(id)) next.add(id);
      return { selected: next };
    }),
  clear: () => set({ selected: new Set() }),
}));
