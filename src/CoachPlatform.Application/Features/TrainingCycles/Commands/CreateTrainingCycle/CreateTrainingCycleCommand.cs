using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingCycles.Commands.CreateTrainingCycle;

/// <summary>
/// Command to create a new TrainingCycle for an athlete.
/// </summary>
public record CreateTrainingCycleCommand : IRequest<Guid>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public string Name { get; init; } = null!;
    public int DurationWeeks { get; init; }
    public DateTime StartDate { get; init; }
}
