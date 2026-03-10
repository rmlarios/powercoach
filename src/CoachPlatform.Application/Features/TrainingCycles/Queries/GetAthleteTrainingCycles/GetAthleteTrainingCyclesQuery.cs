using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingCycles.Queries.GetAthleteTrainingCycles;

/// <summary>
/// Query to get all training cycles for a specific athlete.
/// </summary>
public record GetAthleteTrainingCyclesQuery : IRequest<IEnumerable<TrainingCycleListItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
