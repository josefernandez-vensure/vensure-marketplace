// Everything this application writes to browser storage is keyed under one prefix, so sign-out can clear exactly
// that and leave the shell's shared session store to the identity library.

const PREFIX = 'rancho-react:';

export function appStorageKey(name: string): string {
  return PREFIX + name;
}

export function clearAppStorage(): void {
  for (const storage of [localStorage, sessionStorage]) {
    for (const key of Array.from({ length: storage.length }, (_, i) => storage.key(i))) {
      if (key?.startsWith(PREFIX)) storage.removeItem(key);
    }
  }
}
