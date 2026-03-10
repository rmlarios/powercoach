using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetAthleteCheckIns;

/// <summary>
/// Query to get all check-ins for a specific athlete.
/// </summary>
public record GetAthleteCheckInsQuery : IRequest<IEnumerable<CheckInListItemDto>>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
}
