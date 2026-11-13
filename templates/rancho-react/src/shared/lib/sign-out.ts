import type { QueryClient } from '@tanstack/react-query';
import { clearAppStorage } from './app-storage';

/**
 * Sign-out: this application clears its caches and everything it wrote to browser storage, then hands over to the
 * shell's session, whose sign-out clears the identity library's store and ends the provider session.
 */
export async function signOut({ queryClient, endSession }: { queryClient: QueryClient; endSession: () => Promise<void> }) {
  queryClient.clear();
  clearAppStorage();
  await endSession();
}
