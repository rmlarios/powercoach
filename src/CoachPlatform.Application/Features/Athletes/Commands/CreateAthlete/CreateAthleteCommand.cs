using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Commands.CreateAthlete;

/// <summary>
/// Command to create a new Athlete.
/// </summary>
public record CreateAthleteCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public string? Goals { get; init; }
    public string? Country { get; init; }
    public string? Gender { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public decimal? Height { get; init; }
    public decimal? Weight { get; init; }
    public string? ExperienceLevel { get; init; }
    public Guid? ApplicationId { get; init; }
}
