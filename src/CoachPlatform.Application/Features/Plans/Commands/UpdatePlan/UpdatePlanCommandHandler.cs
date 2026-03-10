using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Plans.Commands.UpdatePlan;

/// <summary>
/// Handler for UpdatePlanCommand.
/// </summary>
public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdatePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(nameof(Plan), request.Id);
        }

        // Convert months to days
        var durationDays = request.DurationInMonths * 30;

        plan.Update(
            name: request.Name,
            description: request.Description,
            price: request.Price,
            currency: request.Currency,
            durationDays: durationDays);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
