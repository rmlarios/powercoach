using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Applications.Queries.GetApplicationById;

/// <summary>
/// Handler for GetApplicationByIdQuery.
/// </summary>
public class GetApplicationByIdQueryHandler : IRequestHandler<GetApplicationByIdQuery, ApplicationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetApplicationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationDto?> Handle(GetApplicationByIdQuery request, CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (application is null)
        {
            return null;
        }

        return new ApplicationDto
        {
            Id = application.Id,
            CoachId = application.CoachId,
            FirstName = application.ApplicantName.FirstName,
            LastName = application.ApplicantName.LastName,
            FullName = $"{application.ApplicantName.FirstName} {application.ApplicantName.LastName}",
            Email = application.Email.Value,
            Phone = application.Phone,
            Age = application.Age,
            Gender = application.Gender,
            Country = application.Country,
            TrainingExperience = application.TrainingExperience,
            CurrentSquat = application.CurrentSquat,
            CurrentBench = application.CurrentBench,
            CurrentDeadlift = application.CurrentDeadlift,
            Motivation = application.Motivation,
            Goals = application.Goals,
            Message = application.Message,
            ReferralSource = application.ReferralSource,
            Status = application.Status,
            CoachNotes = application.CoachNotes,
            ReviewedAt = application.ReviewedAt,
            RejectionReason = application.RejectionReason,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }
}
