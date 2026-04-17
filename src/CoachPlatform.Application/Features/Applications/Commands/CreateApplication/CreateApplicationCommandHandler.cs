using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Applications.Commands.CreateApplication;

/// <summary>
/// Handler for CreateApplicationCommand.
/// </summary>
public class CreateApplicationCommandHandler : IRequestHandler<CreateApplicationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateApplicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateApplicationCommand request, CancellationToken cancellationToken)
    {
        // Verify coach exists
        var coachExists = await _context.Coaches
            .AnyAsync(c => c.Id == request.CoachId, cancellationToken);

        if (!coachExists)
        {
            throw new NotFoundException(nameof(Coach), request.CoachId);
        }

        // Check if email has already applied to this coach
        var email = Email.Create(request.Email);
        var existingEmailsForCoach = await _context.Applications
            .Where(a => a.CoachId == request.CoachId)
            .Select(a => a.Email.Value)
            .ToListAsync(cancellationToken);

        var existingApplication = existingEmailsForCoach
            .Any(value => string.Equals(value, email.Value, StringComparison.OrdinalIgnoreCase));

        if (existingApplication)
        {
            throw new ConflictException(nameof(Domain.Entities.Application), "Email", request.Email);
        }

        // Create the application
        var application = Domain.Entities.Application.Create(
            coachId: request.CoachId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            phone: request.Phone,
            age: request.Age,
            gender: request.Gender,
            country: request.Country,
            trainingExperience: request.TrainingExperience,
            currentSquat: request.CurrentSquat,
            currentBench: request.CurrentBench,
            currentDeadlift: request.CurrentDeadlift,
            motivation: request.Motivation,
            goals: request.Goals,
            message: request.Message,
            referralSource: request.ReferralSource);

        _context.Applications.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
