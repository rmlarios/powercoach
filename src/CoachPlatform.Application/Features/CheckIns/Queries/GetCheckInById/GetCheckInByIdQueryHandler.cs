using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.CheckIns.Queries.GetCheckInById;

/// <summary>
/// Handler for GetCheckInByIdQuery.
/// </summary>
public class GetCheckInByIdQueryHandler : IRequestHandler<GetCheckInByIdQuery, CheckInDto>
{
    private readonly IApplicationDbContext _context;

    public GetCheckInByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckInDto> Handle(GetCheckInByIdQuery request, CancellationToken cancellationToken)
    {
        var checkIn = await _context.CheckIns
            .Include(c => c.Athlete)
            .FirstOrDefaultAsync(c => c.Id == request.CheckInId, cancellationToken);

        if (checkIn == null)
        {
            throw new NotFoundException(nameof(CheckIn), request.CheckInId);
        }

        // Authorization: Coach must be assigned
        if (checkIn.Athlete.CoachId != request.CoachId)
        {
            throw new UnauthorizedAccessException("You are not authorized to view this check-in.");
        }

        return new CheckInDto
        {
            Id = checkIn.Id,
            AthleteId = checkIn.AthleteId,
            AthleteName = checkIn.Athlete.Name.FirstName + " " + checkIn.Athlete.Name.LastName,
            CheckInDate = checkIn.CheckInDate,
            Weight = checkIn.Weight,
            WeightUnit = "kg",
            Notes = checkIn.Notes,
            PhotoUrls = checkIn.PhotoUrls,
            CoachFeedback = checkIn.CoachFeedback,
            ReviewedAt = checkIn.FeedbackDate,
            CreatedAt = checkIn.CreatedAt,
            UpdatedAt = checkIn.UpdatedAt,
            EnergyLevel = checkIn.EnergyLevel,
            SleepQuality = checkIn.SleepQuality,
            SleepHours = checkIn.SleepHours,
            StressLevel = checkIn.StressLevel,
            NutritionAdherence = checkIn.NutritionAdherence,
            TrainingAdherence = checkIn.TrainingAdherence
        };
    }
}
