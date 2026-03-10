using MediatR;

namespace CoachPlatform.Application.Features.Applications.Commands.ApproveApplication;

/// <summary>
/// Command to approve an application and create an Athlete.
/// </summary>
public record ApproveApplicationCommand : IRequest<ApproveApplicationResult>
{
    public Guid ApplicationId { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Result of approving an application.
/// </summary>
public record ApproveApplicationResult
{
    public Guid ApplicationId { get; init; }
    public Guid AthleteId { get; init; }
}
