using MediatR;

namespace CoachPlatform.Application.Features.Applications.Commands.RejectApplication;

/// <summary>
/// Command to reject an application.
/// </summary>
public record RejectApplicationCommand : IRequest<Guid>
{
    public Guid ApplicationId { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}
