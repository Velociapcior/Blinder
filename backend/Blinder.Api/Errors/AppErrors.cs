namespace Blinder.Api.Errors;

/// <summary>
/// Single source of truth for all RFC 7807 Problem Details type URIs.
/// Every 4xx/5xx response in the application must reference a constant from this class.
/// Never define error type strings inline in controllers or services.
/// </summary>
/// <example>
/// <code>
/// return Problem(
///     title: "User not found",
///     type: AppErrors.UserNotFound,
///     statusCode: StatusCodes.Status404NotFound);
/// </code>
/// </example>
public static class AppErrors
{
    private const string Base = "https://blinder.app/errors";

    /// <summary>The requested user does not exist.</summary>
    public const string UserNotFound = $"{Base}/user-not-found";

    /// <summary>A user with this email address already exists.</summary>
    public const string DuplicateEmail = $"{Base}/duplicate-email";

    /// <summary>The invite token is invalid, expired, or already used.</summary>
    public const string InvalidInviteToken = $"{Base}/invalid-invite-token";

    /// <summary>
    /// The mutual reveal threshold has not yet been met for this conversation.
    /// </summary>
    public const string RevealThresholdNotMet = $"{Base}/reveal-threshold-not-met";

    /// <summary>
    /// The user has reached the maximum number of active conversations allowed
    /// on their current subscription tier.
    /// </summary>
    public const string ConversationLimitReached = $"{Base}/conversation-limit-reached";

    /// <summary>The request is unauthenticated — caller must log in first (HTTP 401).</summary>
    public const string Unauthorized = $"{Base}/unauthorized";

    /// <summary>The authenticated caller lacks permission to perform this action (HTTP 403).</summary>
    public const string Forbidden = $"{Base}/forbidden";

    /// <summary>The caller does not have sufficient permissions for this action.</summary>
    public const string InsufficientPermissions = $"{Base}/insufficient-permissions";

    /// <summary>The uploaded photo failed automatic content scanning.</summary>
    public const string PhotoScanFailed = $"{Base}/photo-scan-failed";

    /// <summary>Female user registration requires a valid invite link.</summary>
    public const string InviteRequiredForFemale = $"{Base}/invite-required-for-female";

    /// <summary>The inbound webhook signature verification failed.</summary>
    public const string WebhookVerificationFailed = $"{Base}/webhook-verification-failed";
}
