namespace Platform.Application.Abstractions;

/// <summary>
/// A problem a server can return. Its URI is its identity: clients decide by type and status, never by title.
/// Every module declares its own; <see cref="PlatformProblems"/> declares the pipeline's and the framework's.
/// </summary>
public sealed record ProblemType(string Uri, int Status, string Title, bool UserFacing)
{
    public Problem With(string? detail = null) => new(this, detail);
}

/// <summary>One occurrence of a problem type. Errors are keyed by request path, such as <c>address.lines[0]</c>.</summary>
public sealed record Problem(ProblemType Type, string? Detail = null, IReadOnlyDictionary<string, string[]>? Errors = null);

public static class PlatformProblems
{
    public static readonly ProblemType Validation = new("/problems/validation", 400, "The request is not valid.", UserFacing: true);
    public static readonly ProblemType Unauthenticated = new("/problems/unauthenticated", 401, "Authentication is required.", UserFacing: true);
    public static readonly ProblemType Forbidden = new("/problems/forbidden", 403, "You are not allowed to do this.", UserFacing: true);
    public static readonly ProblemType NotFound = new("/problems/not-found", 404, "Nothing was found at this address.", UserFacing: true);
    public static readonly ProblemType Conflict = new("/problems/conflict", 409, "Someone else changed this first. Reload and try again.", UserFacing: true);
    public static readonly ProblemType IdempotencyKeyRequired = new("/problems/idempotency-key-required", 400, "This operation needs an Idempotency-Key header.", UserFacing: false);
    public static readonly ProblemType IdempotencyKeyReused = new("/problems/idempotency-key-reused", 422, "This Idempotency-Key was already used with a different request.", UserFacing: false);
    public static readonly ProblemType RateLimited = new("/problems/rate-limited", 429, "Too many requests.", UserFacing: true);
    public static readonly ProblemType Unexpected = new("/problems/unexpected", 500, "Something went wrong.", UserFacing: false);

    /// <summary>Every platform problem type. For a status the framework answers on its own, the first one listed is used.</summary>
    public static IReadOnlyList<ProblemType> All { get; } =
        [Validation, Unauthenticated, Forbidden, NotFound, Conflict, IdempotencyKeyRequired, IdempotencyKeyReused, RateLimited, Unexpected];
}
