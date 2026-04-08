namespace Blinder.SharedKernel;

/// <summary>
/// Base exception type for domain-level errors within the Blinder application.
/// All domain-specific exceptions should inherit from this type.
/// </summary>
public abstract class BlinderException : Exception
{
    public string ErrorCode { get; }

    protected BlinderException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected BlinderException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
