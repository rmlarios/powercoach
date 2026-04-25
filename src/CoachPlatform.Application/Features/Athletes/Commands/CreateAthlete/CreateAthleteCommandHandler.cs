using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Commands.CreateAthlete;

/// <summary>
/// Handler for CreateAthleteCommand.
/// </summary>
public class CreateAthleteCommandHandler : IRequestHandler<CreateAthleteCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAthleteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAthleteCommand request, CancellationToken cancellationToken)
    {
        // Verify coach exists
        var coachExists = await _context.Coaches
            .AnyAsync(c => c.Id == request.CoachId, cancellationToken);

        if (!coachExists)
        {
            throw new NotFoundException(nameof(Coach), request.CoachId);
        }

        // Check if email is already in use by another athlete
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();
        var emailExists = await _context.Athletes
            .AnyAsync(a => a.Email.Value == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(nameof(Athlete), "Email", request.Email);
        }

        // Create the athlete using the factory method
        var athlete = Athlete.Create(
            coachId: request.CoachId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            phone: request.Phone,
            goals: request.Goals,
            country: request.Country,
            gender: request.Gender,
            dateOfBirth: request.DateOfBirth,
            height: request.Height,
            weight: request.Weight,
            experienceLevel: request.ExperienceLevel,
            applicationId: request.ApplicationId);

        _context.Athletes.Add(athlete);
        await _context.SaveChangesAsync(cancellationToken);

        return athlete.Id;
    }
}
