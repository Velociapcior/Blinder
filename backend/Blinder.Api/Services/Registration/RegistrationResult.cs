namespace Blinder.Api.Services.Registration;

/// <summary>
/// Outcome of a registration attempt via <see cref="IRegistrationService"/>.
/// Construct only via <see cref="Success"/> or <see cref="Failure"/> factory methods.
/// The private constructor enforces that <see cref="UserId"/> is always non-null when
/// <see cref="Succeeded"/> is <c>true</c>, preventing a null-forgiving dereference at call sites.
/// </summary>
public sealed class RegistrationResult
{
    private RegistrationResult(
        bool succeeded,
        IReadOnlyList<string> errors,
        string? errorType,
        Guid? userId)
    {
        Succeeded = succeeded;
        Errors = errors;
        ErrorType = errorType;
        UserId = userId;
    }

    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }
    public string? ErrorType { get; }

    /// <summary>Non-null only when <see cref="Succeeded"/> is <c>true</c>.</summary>
    public Guid? UserId { get; }

    public static RegistrationResult Success(Guid userId) =>
        new(true, [], null, userId);

    public static RegistrationResult Failure(IEnumerable<string> errors, string? errorType = null) =>
        new(false, errors.ToList(), errorType, null);
}
