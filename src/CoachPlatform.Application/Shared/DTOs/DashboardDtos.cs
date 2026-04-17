namespace CoachPlatform.Application.Shared.DTOs;

// ========================
// Coach Dashboard DTOs
// ========================

/// <summary>
/// Top-level DTO returned by GET /api/dashboard/coach.
/// Contains everything a coach needs to see at a glance.
/// </summary>
public record CoachDashboardDto
{
    public DashboardStatsDto Stats { get; init; } = new();
    public IReadOnlyList<DashboardAlertDto> Alerts { get; init; } = [];
    public IReadOnlyList<AthleteStatusRowDto> AthleteStatuses { get; init; } = [];
    public IReadOnlyList<ActivityFeedItemDto> RecentActivity { get; init; } = [];
}

/// <summary>
/// Aggregate stats for the top cards.
/// </summary>
public record DashboardStatsDto
{
    public int ActiveAthletes { get; init; }
    public int PendingApplications { get; init; }
    public int ActivePrograms { get; init; }
    /// <summary>Workout completion rate (0-100) for the last 7 days.</summary>
    public int CompletionRate { get; init; }
    public int PendingCheckIns { get; init; }
    /// <summary>Change in active athletes vs previous month (can be negative).</summary>
    public int AthletesTrend { get; init; }
}

/// <summary>
/// A single alert that demands coach attention.
/// </summary>
public record DashboardAlertDto
{
    public string AlertType { get; init; } = null!;
    /// <summary>"warning" or "danger"</summary>
    public string Severity { get; init; } = "warning";
    public Guid? AthleteId { get; init; }
    public string? AthleteName { get; init; }
    public string Message { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// One row in the Athlete Status Table.
/// </summary>
public record AthleteStatusRowDto
{
    public Guid AthleteId { get; init; }
    public string Name { get; init; } = null!;
    public string? ProfilePictureUrl { get; init; }
    public string Status { get; init; } = null!;
    /// <summary>"green", "yellow", or "red"</summary>
    public string StatusColor { get; init; } = "green";
    public DateTime? LastWorkoutDate { get; init; }
    public string? LastWorkoutName { get; init; }
    public DateTime? LastCheckInDate { get; init; }
    public bool? IsCheckInReviewed { get; init; }
    public string? ActiveProgramName { get; init; }
    public int? CurrentWeek { get; init; }
}

/// <summary>
/// A single item in the activity feed / timeline.
/// </summary>
public record ActivityFeedItemDto
{
    /// <summary>WorkoutCompleted, WorkoutStarted, CheckInSubmitted, ApplicationReceived</summary>
    public string Type { get; init; } = null!;
    public string? AthleteName { get; init; }
    public Guid? AthleteId { get; init; }
    public string Description { get; init; } = null!;
    public DateTime Timestamp { get; init; }
}
