using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Commands.DeactivateAthlete;

/// <summary>
/// Command to deactivate an Athlete (mark as inactive without deleting).
/// </summary>
public record DeactivateAthleteCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
