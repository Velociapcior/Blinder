namespace Blinder.SharedKernel;

/// <summary>
/// Discriminated union for operation outcomes. Carries either a success value or a structured error.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value
    {
        get
        {
            if (IsFailure) throw new InvalidOperationException("Cannot access Value on a failed result.");
            return _value!;
        }
    }

    public Error Error
    {
        get
        {
            if (IsSuccess) throw new InvalidOperationException("Cannot access Error on a successful result.");
            return _error!;
        }
    }

    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
