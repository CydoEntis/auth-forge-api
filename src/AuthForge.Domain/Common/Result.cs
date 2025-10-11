using AuthForge.Domain.Errors;

namespace AuthForge.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value
    {
        get
        {
            if (IsFailure)
                throw new InvalidOperationException("Cannot access value of a failed result");

            return _value!;
        }
    }

    public static Result<T> Success(T value) => new(value, true, Error.None);
    public new static Result<T> Failure(Error error) => new(default, false, error);
    public static implicit operator Result<T>(T value) => Success(value);
}