using System.Text.Json;

namespace Platform.Application.Abstractions;

/// <summary>Whether a dispatch succeeded, for a caller that knows the message only at run time, such as the outbox dispatcher.</summary>
public interface IDispatchResult
{
    bool IsSuccess { get; }

    Problem? Problem { get; }
}

/// <summary>What every dispatch returns. A behavior short-circuits, records, and replays it without knowing the value type.</summary>
public interface IResult<TSelf> : IDispatchResult
    where TSelf : IResult<TSelf>
{
    static abstract TSelf Failure(Problem problem);

    static abstract TSelf Replay(RecordedResult recorded);

    RecordedResult Record(string fingerprint);
}

public sealed class Result : IResult<Result>
{
    private static readonly Result Ok = new(null);

    private Result(Problem? problem) => Problem = problem;

    public bool IsSuccess => Problem is null;

    public Problem? Problem { get; }

    public static Result Success() => Ok;

    public static Result Failure(Problem problem) => new(problem);

    public static Result Replay(RecordedResult recorded) =>
        recorded.Succeeded ? Ok : Failure(new Problem(recorded.ProblemType!, recorded.Detail, recorded.Errors));

    public static implicit operator Result(Problem problem) => Failure(problem);

    public RecordedResult Record(string fingerprint) =>
        new(fingerprint, IsSuccess, null, Problem?.Type, Problem?.Detail, Problem?.Errors);
}

public sealed class Result<T> : IResult<Result<T>>
{
    private readonly T? _value;

    private Result(T? value, Problem? problem)
    {
        _value = value;
        Problem = problem;
    }

    public bool IsSuccess => Problem is null;

    public Problem? Problem { get; }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(value, null);

    public static Result<T> Failure(Problem problem) => new(default, problem);

    public static Result<T> Replay(RecordedResult recorded) =>
        recorded.Succeeded
            ? Success(JsonSerializer.Deserialize<T>(recorded.ValueJson!)!)
            : Failure(new Problem(recorded.ProblemType!, recorded.Detail, recorded.Errors));

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Problem problem) => Failure(problem);

    public RecordedResult Record(string fingerprint) =>
        new(fingerprint, IsSuccess, IsSuccess ? JsonSerializer.Serialize(_value) : null, Problem?.Type, Problem?.Detail, Problem?.Errors);
}
