namespace Platform.Application.Abstractions;

/// <summary>What every dispatch returns. A behavior short-circuits with <see cref="Failure"/> without knowing the value type.</summary>
public interface IResult<out TSelf>
    where TSelf : IResult<TSelf>
{
    bool IsSuccess { get; }

    Problem? Problem { get; }

    static abstract TSelf Failure(Problem problem);
}

public sealed class Result : IResult<Result>
{
    private static readonly Result Ok = new(null);

    private Result(Problem? problem) => Problem = problem;

    public bool IsSuccess => Problem is null;

    public Problem? Problem { get; }

    public static Result Success() => Ok;

    public static Result Failure(Problem problem) => new(problem);

    public static implicit operator Result(Problem problem) => Failure(problem);
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

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Problem problem) => Failure(problem);
}
