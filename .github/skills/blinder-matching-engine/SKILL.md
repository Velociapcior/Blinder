---
name: blinder-matching-engine
description: |
  Implements the Blinder matching algorithm: rules-based filtering with values-weighted scoring,
  triggered by Coravel background jobs on onboarding completion. Use this skill when: implementing
  or modifying the match generation logic, adding new matching criteria, working on the Coravel
  scheduler, debugging "empty match screen on restart" edge cases, or tuning scoring weights.
  Triggers: "matching", "match algorithm", "Coravel", "match score", "onboarding complete",
  "match generation", "compatibility score".
---

# Blinder Matching Engine

## Overview

Matching runs as a **Coravel background job** triggered immediately on onboarding completion
and periodically for unmatched users. The algorithm has two phases:

1. **Hard filter** — mandatory criteria that eliminate candidates (age range, location radius, gender preference)
2. **Weighted scoring** — values-based compatibility score across multiple dimensions

## Architecture

```
OnboardingCompletedEvent
        ↓
MatchGenerationJob (Coravel Invocable)
        ↓
IMatchingService.GenerateMatchesAsync(userId)
        ↓
    [Hard Filter] → eliminates ineligible candidates
        ↓
    [Score Engine] → ranks remaining candidates
        ↓
    Top N candidates → saved to Matches table
        ↓
SignalR notification → "You have a new match"
```

## Coravel Job Setup

```csharp
// Program.cs
builder.Services.AddQueue();
builder.Services.AddScheduler();
builder.Services.AddTransient<MatchGenerationJob>();

// Scheduler for periodic rematch of unmatched users
app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<MatchGenerationJob>()
        .EveryThirtyMinutes()
        .PreventOverlapping(nameof(MatchGenerationJob));
});
```

```csharp
// Triggered immediately on onboarding complete
public class OnboardingCompletedHandler : INotificationHandler<OnboardingCompletedEvent>
{
    private readonly IQueue _queue;

    public OnboardingCompletedHandler(IQueue queue) => _queue = queue;

    public Task Handle(OnboardingCompletedEvent notification, CancellationToken ct)
    {
        _queue.QueueInvocableWithPayload<MatchGenerationJob, MatchJobPayload>(
            new MatchJobPayload(notification.UserId));
        return Task.CompletedTask;
    }
}
```

## Job Implementation

```csharp
public class MatchGenerationJob : IInvocable, IInvocableWithPayload<MatchJobPayload>
{
    public MatchJobPayload Payload { get; set; } = default!;

    private readonly IMatchingService _matchingService;
    private readonly ILogger<MatchGenerationJob> _logger;

    public MatchGenerationJob(IMatchingService matchingService,
        ILogger<MatchGenerationJob> logger)
    {
        _matchingService = matchingService;
        _logger = logger;
    }

    public async Task Invoke()
    {
        try
        {
            var userId = Payload?.UserId;

            // If no payload (scheduled run), process all unmatched users
            if (userId is null)
            {
                await _matchingService.ProcessUnmatchedUsersAsync();
                return;
            }

            await _matchingService.GenerateMatchesAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Match generation failed for user {UserId}", Payload?.UserId);
            // Don't rethrow — Coravel will retry via queue
        }
    }
}

public record MatchJobPayload(string? UserId);
```

## Hard Filter (Phase 1)

```csharp
public IQueryable<UserProfile> ApplyHardFilters(
    IQueryable<UserProfile> candidates,
    UserProfile seeker)
{
    return candidates
        .Where(c => c.Id != seeker.Id)
        .Where(c => c.IsOnboardingComplete)
        .Where(c => !c.IsDeleted && !c.IsBanned)
        // Age range filter (bidirectional respect)
        .Where(c =>
            c.Age >= seeker.PreferredAgeMin &&
            c.Age <= seeker.PreferredAgeMax &&
            seeker.Age >= c.PreferredAgeMin &&
            seeker.Age <= c.PreferredAgeMax)
        // Gender preference
        .Where(c =>
            seeker.PreferredGenders.Contains(c.Gender) &&
            c.PreferredGenders.Contains(seeker.Gender))
        // Not already matched or previously rejected
        .Where(c => !_context.Matches.Any(m =>
            (m.User1Id == seeker.Id && m.User2Id == c.Id) ||
            (m.User1Id == c.Id && m.User2Id == seeker.Id)));
}
```

## Weighted Score Engine (Phase 2)

```csharp
public class MatchScoreEngine
{
    // Weights must sum to 1.0
    private static readonly Dictionary<string, double> Weights = new()
    {
        ["Values"]       = 0.35,
        ["Lifestyle"]    = 0.25,
        ["Relationship"] = 0.20,
        ["Interests"]    = 0.15,
        ["Location"]     = 0.05,
    };

    public double Calculate(UserProfile seeker, UserProfile candidate)
    {
        var scores = new Dictionary<string, double>
        {
            ["Values"]       = ScoreValues(seeker, candidate),
            ["Lifestyle"]    = ScoreLifestyle(seeker, candidate),
            ["Relationship"] = ScoreRelationshipGoals(seeker, candidate),
            ["Interests"]    = ScoreInterests(seeker, candidate),
            ["Location"]     = ScoreLocation(seeker, candidate),
        };

        return scores.Sum(kv => kv.Value * Weights[kv.Key]);
    }

    private double ScoreValues(UserProfile a, UserProfile b)
    {
        // Jaccard similarity on values arrays
        var intersection = a.Values.Intersect(b.Values).Count();
        var union = a.Values.Union(b.Values).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    private double ScoreInterests(UserProfile a, UserProfile b)
    {
        var intersection = a.Interests.Intersect(b.Interests).Count();
        var union = a.Interests.Union(b.Interests).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    private double ScoreRelationshipGoals(UserProfile a, UserProfile b)
        => a.RelationshipGoal == b.RelationshipGoal ? 1.0 : 0.0;

    private double ScoreLifestyle(UserProfile a, UserProfile b)
    {
        // Normalize lifestyle dimension mismatches
        var dimensions = new[]
        {
            Math.Abs(a.ActivityLevel - b.ActivityLevel),
            Math.Abs(a.SocialPreference - b.SocialPreference),
        };
        var avgMismatch = dimensions.Average();
        return 1.0 - (avgMismatch / 4.0); // assuming 0–4 scale
    }

    private double ScoreLocation(UserProfile a, UserProfile b)
    {
        var distanceKm = Haversine(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
        var maxKm = 100.0;
        return Math.Max(0, 1.0 - (distanceKm / maxKm));
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
```

## Match Persistence

```csharp
public async Task GenerateMatchesAsync(string userId, CancellationToken ct = default)
{
    var seeker = await _context.UserProfiles
        .Include(u => u.Values)
        .Include(u => u.Interests)
        .FirstOrDefaultAsync(u => u.Id == userId, ct);

    if (seeker is null) return;

    var candidates = await ApplyHardFilters(
        _context.UserProfiles
            .Include(u => u.Values)
            .Include(u => u.Interests),
        seeker)
        .ToListAsync(ct);

    var scored = candidates
        .Select(c => (Profile: c, Score: _scoreEngine.Calculate(seeker, c)))
        .OrderByDescending(x => x.Score)
        .Take(10) // Top 10 candidates per run
        .ToList();

    foreach (var (profile, score) in scored)
    {
        var match = new Match
        {
            Id = Guid.NewGuid().ToString(),
            User1Id = userId,
            User2Id = profile.Id,
            CompatibilityScore = score,
            CreatedAt = DateTime.UtcNow,
            Status = MatchStatus.Active,
        };
        _context.Matches.Add(match);
    }

    await _context.SaveChangesAsync(ct);

    // Notify via SignalR if new matches were created
    if (scored.Any())
        await _matchHub.Clients.User(userId).SendAsync("NewMatchesAvailable", ct);
}
```

## Empty Screen Edge Case (Coravel Startup Guard)

```csharp
// Runs at startup to catch users who completed onboarding but have no matches
// (e.g., app crashed after onboarding, before job ran)
public async Task ProcessUnmatchedUsersAsync(CancellationToken ct = default)
{
    var unmatchedUserIds = await _context.UserProfiles
        .Where(u => u.IsOnboardingComplete && !u.IsDeleted)
        .Where(u => !_context.Matches.Any(m =>
            m.User1Id == u.Id || m.User2Id == u.Id))
        .Select(u => u.Id)
        .ToListAsync(ct);

    foreach (var userId in unmatchedUserIds)
        await GenerateMatchesAsync(userId, ct);
}
```

## Key Rules

- Hard filters run in SQL (EF LINQ) — NEVER load all users into memory for filtering
- Score engine runs in-memory on the filtered candidate set
- Matching is always bidirectional — both users must satisfy each other's hard filters
- Max 10 matches per run to avoid overwhelming new users
- The startup idempotent guard (ProcessUnmatchedUsers) prevents empty screens after crashes
- NEVER expose raw compatibility scores to users in the UI
