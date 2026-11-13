import { setupServer } from 'msw/node';

/** MSW stands in for the server: every network call a test makes is answered here, never by a module mock. */
export const apiUrl = 'http://api.test';
export const server = setupServer();
