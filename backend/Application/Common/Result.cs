namespace TutoringScheduling.Application.Common;

/// <summary>
/// Outcome of a use case that has no value to return on success, only whether
/// it succeeded and, if not, why.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);

    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>
/// Outcome of a use case that returns <typeparamref name="T"/> on success.
/// <see cref="Value"/> is only meaningful when <see cref="Result.IsSuccess"/>
/// is true.
/// </summary>
public sealed class Result<T> : Result
{
    internal Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static implicit operator Result<T>(T value) => Success(value);
}
