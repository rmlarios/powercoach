using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Applications.Commands.RejectApplication;

/// <summary>
/// Handler for RejectApplicationCommand.
/// </summary>
public class RejectApplicationCommandHandler : IRequestHandler<RejectApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RejectApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RejectApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Application), request.ApplicationId);
        }

        // Validate application can be rejected
        if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
        {
            throw new ConflictException(
                nameof(Domain.Entities.Application), 
                "Status", 
                $"Cannot reject an application with status '{application.Status}'");
        }

        // Reject the application
        application.Reject(request.Reason, request.Notes);
        
        await _context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
