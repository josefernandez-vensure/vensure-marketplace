import type { ReactNode } from 'react';
import {
  IamProvider,
  StorageService,
  createFetchWithAuth,
  useAuth,
  type VenAuthOptions,
} from '@vensure-devops-qa/vensureone-iam-react';

// The one module that touches the identity library. The PrismOne shell signs the user in and keeps the session in
// the library's own store; this application never runs a sign-in flow, never refreshes a token, and never stores one.

const options: VenAuthOptions = {
  enterpriseSecurityUrl: import.meta.env.VITE_ENTERPRISE_SECURITY_URL,
  issuer: import.meta.env.VITE_IAM_ISSUER,
  orgId: import.meta.env.VITE_IAM_ORG_ID,
  destinationApp: import.meta.env.VITE_IAM_DESTINATION_APP,
  // What this application needs from Enterprise Security: its service name, declared here and nowhere else.
  serviceName: import.meta.env.VITE_IAM_SERVICE_NAME,
  userTypes: 'v1-user-int,v1-user-cli,v1-user-wse',
  enableAccessTokenGrant: true,
  // The shell owns the session: only it renews tokens.
  enableTokenRefreshInterval: false,
};

export function IdentityProvider({ children }: { children: ReactNode }) {
  return <IamProvider options={options}>{children}</IamProvider>;
}

/** The interceptor: the only place a token is attached to a request. It reads the session store on every call. */
export const authorizedFetch: typeof fetch = createFetchWithAuth(new StorageService());

/** Sign-out goes through the shell's session: the library clears its store and ends the provider session. */
export function useSignOut(): () => Promise<void> {
  const { signOut } = useAuth();
  return signOut;
}
