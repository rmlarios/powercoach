using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Commands.UpdateAthlete;

/// <summary>
/// Command to update an existing Athlete's physical and training data.
/// </summary>
public record UpdateAthleteCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
    public string? Country { get; init; }
    public decimal? Height { get; init; }
    public decimal? Weight { get; init; }
    public string? ExperienceLevel { get; init; }
}
