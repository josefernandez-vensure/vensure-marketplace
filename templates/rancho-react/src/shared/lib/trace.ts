function hex(bytes: number): string {
  return Array.from(crypto.getRandomValues(new Uint8Array(bytes)), (b) => b.toString(16).padStart(2, '0')).join('');
}

/** A W3C traceparent for a new trace: each call to the server starts one, which its error report can name. */
export function newTraceparent(): string {
  return `00-${hex(16)}-${hex(8)}-01`;
}
