using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.CheckIns.Commands.CreateCheckIn;

/// <summary>
/// Command to create a new Check-In.
/// </summary>
public record CreateCheckInCommand : IRequest<Guid>, IAthleteOwnedRequest
{
    public Guid AthleteId { get; init; }
    public decimal? Weight { get; init; }
    public int? FatigueLevel { get; init; }
    public int? SleepQuality { get; init; }
    public int? MotivationLevel { get; init; }
    public string? Notes { get; init; }
}
