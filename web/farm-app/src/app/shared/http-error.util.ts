import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extracts a human-readable message from a backend error response so the UI never shows a raw
 * JSON dump. The API returns RFC 7807 ProblemDetails everywhere (doc 11): a 409 business-rule
 * failure carries `title`/`detail` (see ApiControllerBase.ErrorResult), and a 400 FluentValidation
 * failure carries an `errors` field map (ASP.NET's ValidationProblem shape, keyed by property name).
 * The client-side Validators mirror the backend's rules, so the 400 branch is mostly a safety net -
 * but it's still surfaced properly rather than assumed unreachable.
 */
export function extractErrorMessage(err: HttpErrorResponse): string {
  const body: unknown = err.error;

  if (body && typeof body === 'object') {
    const problem = body as { errors?: Record<string, string[]>; detail?: string; title?: string };

    if (problem.errors && typeof problem.errors === 'object') {
      const messages = Object.values(problem.errors).flat();
      if (messages.length) return messages.join(' ');
    }
    if (problem.detail) return problem.detail;
    if (problem.title) return problem.title;
  }

  return 'Something went wrong. Please try again.';
}
