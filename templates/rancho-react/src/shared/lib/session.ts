// The session's events. The API client raises `unauthorized` on a 401; the identity module answers it by asking the
// shell to sign the user in. It is raised once until the session is renewed, so a burst of 401s asks once.

const events = new EventTarget();
let unauthorizedPending = false;

export function raiseUnauthorized(): void {
  if (unauthorizedPending) return;
  unauthorizedPending = true;
  events.dispatchEvent(new Event('unauthorized'));
}

export function onUnauthorized(listener: () => void): () => void {
  events.addEventListener('unauthorized', listener);
  return () => events.removeEventListener('unauthorized', listener);
}

/** Called once the session is renewed, or by a test between cases. */
export function resetUnauthorized(): void {
  unauthorizedPending = false;
}
