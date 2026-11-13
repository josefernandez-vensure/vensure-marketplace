import { client } from '../../generated/client.gen';
import { authorizedFetch } from './identity';

/** Points the generated client at the server, through the one interceptor. Called once, by the composition root. */
export function configureApiClient(baseUrl: string): void {
  client.setConfig({ baseUrl, fetch: authorizedFetch });
}
