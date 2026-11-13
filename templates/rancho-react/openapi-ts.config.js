import { defineConfig } from '@hey-api/openapi-ts';

// The server's committed OpenAPI document is the only source of the API's types. Never hand-edit src/generated/.
export default defineConfig({
  input: './openapi/v1.json',
  output: { path: './src/generated', postProcess: [] },
  plugins: ['@hey-api/client-fetch', '@hey-api/typescript', '@hey-api/sdk', 'zod'],
});