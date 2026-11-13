import { describe, expect, it, vi } from 'vitest';
import { testQueryClient } from '../../../tests/render';
import { testKeys } from '../../../tests/test-keys';
import { appStorageKey } from './app-storage';
import { signOut } from './sign-out';

describe('sign-out', () => {
  it('clears the caches and what the application stored, then hands over to the shell\'s sign-out', async () => {
    const queryClient = testQueryClient();
    queryClient.setQueryData(testKeys.retry(200), 'cached');
    localStorage.setItem(appStorageKey('products.view'), '{}');
    sessionStorage.setItem(appStorageKey('draft'), '{}');
    localStorage.setItem('the-shells-own', 'kept');
    const endSession = vi.fn(() => Promise.resolve());

    await signOut({ queryClient, endSession });

    expect(queryClient.getQueryCache().getAll()).toHaveLength(0);
    expect(localStorage.getItem(appStorageKey('products.view'))).toBeNull();
    expect(sessionStorage.getItem(appStorageKey('draft'))).toBeNull();
    expect(localStorage.getItem('the-shells-own')).toBe('kept');
    expect(endSession).toHaveBeenCalledTimes(1);
    localStorage.removeItem('the-shells-own');
  });
});
