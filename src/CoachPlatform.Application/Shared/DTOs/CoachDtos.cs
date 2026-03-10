namespace CoachPlatform.Application.Shared.DTOs;

/// <summary>
/// Data Transfer Object for Coach entity.
/// </summary>
public record CoachDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? Specialization { get; init; }
    public string? ProfileImageUrl { get; init; }
    public bool IsActive { get; init; }
    public int ActiveAthletesCount { get; init; }
    public int TotalAthletesCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new coach.
/// </summary>
public record CreateCoachDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? Specialization { get; init; }
}

/// <summary>
/// DTO for updating coach profile.
/// </summary>
public record UpdateCoachDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Bio { get; init; }
    public string? Specialization { get; init; }
    public string? ProfileImageUrl { get; init; }
}

/// <summary>
/// DTO for coach dashboard summary.
/// </summary>
public record CoachDashboardDto
{
    public int ActiveAthletes { get; init; }
    public int PendingApplications { get; init; }
    public int PendingPayments { get; init; }
    public int CheckInsPendingReview { get; init; }
    public int ExpiringSubscriptions { get; init; }
    public decimal TotalRevenueThisMonth { get; init; }
}
