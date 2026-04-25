using MediatR;

namespace CoachPlatform.Application.Features.CheckIns.Commands.ReviewCheckIn;

/// <summary>
/// Command for a coach to review and provide feedback on an athlete's check-in.
/// </summary>
public record ReviewCheckInCommand : IRequest<Unit>
{
    public Guid CheckInId { get; init; }
    public Guid CoachId { get; init; }
    public string Feedback { get; init; } = null!;
}
