using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetCheckInById;

/// <summary>
/// Query to get a single check-in by its ID.
/// </summary>
public record GetCheckInByIdQuery : IRequest<CheckInDto>
{
    public Guid CheckInId { get; init; }
    public Guid CoachId { get; init; }
}
