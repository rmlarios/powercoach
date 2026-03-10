using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Plans.Commands.CreatePlan;

/// <summary>
/// Handler for CreatePlanCommand.
/// </summary>
public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        // Verify coach exists
        var coachExists = await _context.Coaches
            .AnyAsync(c => c.Id == request.CoachId, cancellationToken);

        if (!coachExists)
        {
            throw new NotFoundException(nameof(Coach), request.CoachId);
        }

        // Convert months to days and determine plan type
        var durationDays = request.DurationInMonths * 30;
        var planType = GetPlanType(request.DurationInMonths);

        // Create the plan using the factory method
        var plan = Plan.Create(
            coachId: request.CoachId,
            name: request.Name,
            price: request.Price,
            currency: request.Currency,
            durationDays: durationDays,
            planType: planType,
            description: request.Description);

        _context.Plans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }

    private static PlanType GetPlanType(int durationInMonths)
    {
        return durationInMonths switch
        {
            1 => PlanType.Monthly,
            3 => PlanType.Quarterly,
            >= 12 => PlanType.Annual,
            _ => PlanType.Monthly
        };
    }
}
