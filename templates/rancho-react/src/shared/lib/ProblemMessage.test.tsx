import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { problem } from '../../../tests/products';
import { ApiError } from './api-error';
import { readProblem } from './problem-details';
import { ProblemMessage } from './ProblemMessage';

describe('ProblemMessage', () => {
  it('shows neither the title nor the detail of a problem type not marked user-facing, but offers the trace id', () => {
    render(<ProblemMessage problem={readProblem(new ApiError(409, problem(409, 'urn:problem:internal-conflict'), undefined))} />);
    const alert = screen.getByRole('alert');
    expect(alert).toHaveTextContent('Something went wrong.');
    expect(alert).not.toHaveTextContent('A title for the logs');
    expect(alert).not.toHaveTextContent('Internal detail');
    expect(alert).toHaveTextContent('4bf92f3577b34da6a3ce929d0e0e4736');
  });
});
