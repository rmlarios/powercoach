using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Queries.GetAthleteProgram;

/// <summary>
/// Query to get an athlete's current program.
/// </summary>
public record GetAthleteProgramQuery : IRequest<AthleteCurrentProgramDto?>
{
    public Guid AthleteId { get; init; }
}
