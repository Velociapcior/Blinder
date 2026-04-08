namespace Blinder.SharedKernel;

/// <summary>
/// Structured error descriptor carrying a machine-readable code and a human-readable message.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static readonly Error None = new(string.Empty, string.Empty);

    public override string ToString() => $"{Code}: {Message}";
}
