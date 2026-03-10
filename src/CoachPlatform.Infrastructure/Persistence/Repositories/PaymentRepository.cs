using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Enums;
using CoachPlatform.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Payment entity.
/// </summary>
public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Payment>> GetBySubscriptionIdAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByStatusAsync(
        PaymentStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Athlete)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByExternalReferenceAsync(
        string externalReference,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.ExternalReference == externalReference, cancellationToken);
    }

    public async Task<Payment?> GetByTransactionIdAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Athlete)
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalRevenueAsync(
        Guid coachId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var payments = await DbSet
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Athlete)
            .Where(p => p.Subscription.Athlete.CoachId == coachId &&
                       p.Status == PaymentStatus.Completed &&
                       p.PaymentDate >= startDate &&
                       p.PaymentDate <= endDate)
            .ToListAsync(cancellationToken);

        return payments.Sum(p => p.Amount.Amount);
    }
}
