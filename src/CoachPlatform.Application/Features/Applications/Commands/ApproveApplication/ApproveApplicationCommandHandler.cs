using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Applications.Commands.ApproveApplication;

/// <summary>
/// Handler for ApproveApplicationCommand.
/// Approves the application and creates an Athlete from the applicant's data.
/// </summary>
public class ApproveApplicationCommandHandler : IRequestHandler<ApproveApplicationCommand, ApproveApplicationResult>
{
    private readonly IApplicationDbContext _context;

    public ApproveApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApproveApplicationResult> Handle(
        ApproveApplicationCommand request, 
        CancellationToken cancellationToken)
    {
        // Get the application
        var application = await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application is null)
        {
            throw new NotFoundException(nameof(Domain.Entities.Application), request.ApplicationId);
        }

        // Validate application can be approved
        if (application.Status != ApplicationStatus.Pending && application.Status != ApplicationStatus.UnderReview)
        {
            throw new ConflictException(
                nameof(Domain.Entities.Application), 
                "Status", 
                $"Cannot approve an application with status '{application.Status}'");
        }

        // Check if an athlete with this email already exists for this coach
        var emailExists = await _context.Athletes
            .AnyAsync(a => a.CoachId == application.CoachId && 
                          a.Email == application.Email, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(nameof(Athlete), "Email", application.Email.Value);
        }

        // Approve the application
        application.Accept(request.Notes);

        // Create the Athlete from application data
        var athlete = Athlete.Create(
            coachId: application.CoachId,
            firstName: application.ApplicantName.FirstName,
            lastName: application.ApplicantName.LastName,
            email: application.Email.Value,
            phone: null,
            goals: application.Goals,
            applicationId: application.Id);

        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync(cancellationToken);

        return new ApproveApplicationResult
        {
            ApplicationId = application.Id,
            AthleteId = athlete.Id
        };
    }
}
