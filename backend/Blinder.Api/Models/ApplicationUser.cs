using Microsoft.AspNetCore.Identity;

namespace Blinder.Api.Models;

/// <summary>
/// Application user entity extending ASP.NET Core Identity.
/// Uses <c>IdentityUser&lt;Guid&gt;</c> for Guid-keyed primary key, consistent with
/// <c>IdentityRole&lt;Guid&gt;</c> and <c>IdentityDbContext&lt;..., Guid&gt;</c>.
/// All date-related properties use <see cref="DateTimeOffset"/> — never <see cref="System.DateTime"/>.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// The user's self-identified gender, used for matching rules.
    /// </summary>
    public UserGender Gender { get; set; }

    /// <summary>
    /// Timestamp when the user completed the values/personality quiz.
    /// Null until the quiz is submitted. Uses <c>timestamptz</c> in PostgreSQL.
    /// </summary>
    public DateTimeOffset? QuizCompletedAt { get; set; }

    /// <summary>
    /// FK placeholder referencing the invite link used during registration.
    /// The actual <c>InviteLink</c> entity and FK constraint are added in Story 2.5.
    /// </summary>
    public Guid? InviteLinkId { get; set; }

    /// <summary>
    /// Indicates whether the user has completed all onboarding steps
    /// (quiz + photo upload + preferences). Defaults to <c>false</c>.
    /// </summary>
    public bool IsOnboardingComplete { get; set; } = false;

    /// <summary>
    /// Timestamp when the user accepted the over-18 age declaration during registration.
    /// Null if not yet set. Mapped to <c>age_declaration_accepted_at timestamptz</c> in PostgreSQL.
    /// </summary>
    public DateTimeOffset? AgeDeclarationAcceptedAt { get; set; }
}

/// <summary>
/// Possible gender values for a Blinder user.
/// Stored as an integer column via EF Core default enum mapping.
/// <c>Unspecified = 0</c> is the sentinel for "not yet set", ensuring an uninitialized
/// <see cref="ApplicationUser.Gender"/> is always distinguishable from a real gender choice.
/// This prevents unset users from silently satisfying Female-invite enforcement rules.
/// </summary>
public enum UserGender
{
    /// <summary>Gender has not been set. Default enum value — treated as incomplete onboarding.</summary>
    Unspecified = 0,
    Male = 1,
    Female = 2,
    NonBinary = 3
}
