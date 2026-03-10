using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Applications.Commands.CreateApplication;

/// <summary>
/// Command to create a new Application (athlete applying to a coach).
/// </summary>
public record CreateApplicationCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public int? Age { get; init; }
    public string? Gender { get; init; }
    public string? Country { get; init; }
    public string? TrainingExperience { get; init; }
    public decimal? CurrentSquat { get; init; }
    public decimal? CurrentBench { get; init; }
    public decimal? CurrentDeadlift { get; init; }
    public string? Motivation { get; init; }
    public string? Goals { get; init; }
    public string? Message { get; init; }
    public string? ReferralSource { get; init; }
}
