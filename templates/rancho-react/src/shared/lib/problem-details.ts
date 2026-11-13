import { zHttpValidationProblemDetails } from '../../generated/classes.gen';

/** What the application knows of a failed call: the only reading of a problem-details body. */
export interface Problem {
  status: number | undefined;
  type: string | undefined;
  /** The server's trace id, which an error shown to the user offers for a report. */
  traceId: string | undefined;
  /** Field errors by request path. */
  fieldErrors: Record<string, string[]>;
}

/** Reads a problem-details body from whatever a failed call threw or returned. Anything else is an unknown problem. */
export function readProblem(error: unknown): Problem {
  const parsed = zHttpValidationProblemDetails.passthrough().safeParse(error);
  if (!parsed.success) return { status: undefined, type: undefined, traceId: undefined, fieldErrors: {} };
  const { status, type, errors } = parsed.data;
  const traceId = (parsed.data as Record<string, unknown>)['traceId'];
  return {
    status: status === null || status === undefined ? undefined : Number(status),
    type: type ?? undefined,
    traceId: typeof traceId === 'string' ? traceId : undefined,
    fieldErrors: errors ?? {},
  };
}
