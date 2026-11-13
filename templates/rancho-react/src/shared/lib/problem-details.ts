import { zHttpValidationProblemDetails } from '../../generated/classes.gen';
import { ApiError } from './api-error';

/** What the application knows of a failed call: the only reading of a problem-details body. */
export interface Problem {
  status: number | undefined;
  type: string | undefined;
  /** Rendered only for a user-facing type (see ProblemMessage). */
  title: string | undefined;
  /** The server's trace id, which an error shown to the user offers for a report. */
  traceId: string | undefined;
  /** Field errors by request path. */
  fieldErrors: Record<string, string[]>;
}

/** Reads a problem-details body from whatever a failed call threw. Anything else is an unknown problem. */
export function readProblem(error: unknown): Problem {
  const status = error instanceof ApiError ? error.status : undefined;
  const parsed = zHttpValidationProblemDetails.loose().safeParse(error instanceof ApiError ? error.body : error);
  if (!parsed.success) return { status, type: undefined, title: undefined, traceId: undefined, fieldErrors: {} };
  const { type, title, errors } = parsed.data;
  const traceId = (parsed.data as Record<string, unknown>)['traceId'];
  return {
    status: status ?? (parsed.data.status === null || parsed.data.status === undefined ? undefined : Number(parsed.data.status)),
    type: type ?? undefined,
    title: title ?? undefined,
    traceId: typeof traceId === 'string' ? traceId : undefined,
    fieldErrors: errors ?? {},
  };
}
