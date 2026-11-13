import { userFacingProblemTypes } from '../../generated/classes.gen';
import type { Problem } from './problem-details';

/**
 * How a failure is shown: a problem's title only for a type the document marks user-facing, never its detail, and
 * always the trace id, so a report can be joined to the server's trace.
 */
export function ProblemMessage({ problem }: { problem: Problem }) {
  const title = problem.type && userFacingProblemTypes.has(problem.type) ? problem.title : undefined;
  return (
    <p role="alert">
      {title ?? (problem.status === 403 ? 'You don\'t have permission to do this.' : 'Something went wrong.')}
      {problem.traceId && <> Reference: {problem.traceId}</>}
    </p>
  );
}
