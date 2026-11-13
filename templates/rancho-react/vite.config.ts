import { prismaticAssets } from '@prismhr/components-react/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

// The shell loads src/app/main.tsx as an ES module and calls its lifecycles.
export default defineConfig({
  plugins: [react(), prismaticAssets()],
  build: {
    // The UI library's stylesheet has rules lightningcss rejects.
    cssMinify: 'esbuild',
    rolldownOptions: {
      input: 'src/app/main.tsx',
      preserveEntrySignatures: 'strict',
      output: { format: 'es', entryFileNames: 'main.js' },
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['tests/setup.ts'],
    css: false,
    include: ['src/**/*.test.{ts,tsx}', 'tests/**/*.test.{ts,tsx}'],
  },
});
