import { z } from 'zod';

/** Mirrors the server's RenameProductRequest; the server is right where they differ. The etag rides with the values. */
export const renameProductForm = z.object({
  name: z.string().trim().min(1, 'Enter a name.').max(200, 'Use at most 200 characters.'),
  etag: z.string(),
});

export type RenameProductForm = z.infer<typeof renameProductForm>;
