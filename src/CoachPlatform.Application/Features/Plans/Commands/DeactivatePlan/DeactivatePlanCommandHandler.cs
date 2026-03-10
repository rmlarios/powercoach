using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Plans.Commands.DeactivatePlan;

/// <summary>
/// Handler for DeactivatePlanCommand.
/// </summary>
public class DeactivatePlanCommandHandler : IRequestHandler<DeactivatePlanCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeactivatePlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeactivatePlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException(nameof(Plan), request.Id);
        }

        if (!plan.IsActive)
        {
            throw new InvalidOperationException("Plan is already inactive.");
        }

        plan.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
