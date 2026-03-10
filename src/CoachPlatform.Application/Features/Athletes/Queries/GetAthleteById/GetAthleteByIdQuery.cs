using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetAthleteById;

/// <summary>
/// Query to get an athlete by their ID with detailed information.
/// </summary>
public record GetAthleteByIdQuery : IRequest<AthleteDetailDto?>
{
    public Guid Id { get; init; }
}
