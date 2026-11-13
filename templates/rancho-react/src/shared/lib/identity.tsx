import { useQueryClient } from '@tanstack/react-query';
import { useEffect, type ReactNode } from 'react';
import {
  IamProvider,
  StorageService,
  createFetchWithAuth,
  useAuth,
  type VenAuthOptions,
} from '@vensure-devops-qa/vensureone-iam-react';
import { onUnauthorized } from './session';
import { signOut } from './sign-out';

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
  return (
    <IamProvider options={options}>
      <SignInOnUnauthorized />
      {children}
    </IamProvider>
  );
}

/** A 401 hands the user to the shell's sign-in page, once; the application never refreshes a token itself. */
function SignInOnUnauthorized() {
  const { signIn } = useAuth();
  useEffect(() => onUnauthorized(() => signIn()), [signIn]);
  return null;
}

/** The interceptor: the only place a token is attached to a request. It reads the session store on every call. */
export const authorizedFetch: typeof fetch = createFetchWithAuth(new StorageService());

export function useSignOut(): () => Promise<void> {
  const queryClient = useQueryClient();
  const { signOut: endSession } = useAuth();
  return () => signOut({ queryClient, endSession });
}
