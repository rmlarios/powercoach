using CoachPlatform.Domain.Entities;
using CoachPlatform.Domain.Interfaces;
using CoachPlatform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Coach entity.
/// </summary>
public class CoachRepository : BaseRepository<Coach>, ICoachRepository
{
    public CoachRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Coach?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailVo = Email.Create(email);
        return await DbSet
            .FirstOrDefaultAsync(c => c.Email == emailVo, cancellationToken);
    }

    public async Task<Coach?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Athletes)
            .Include(c => c.Applications)
            .Include(c => c.Plans.Where(p => p.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailVo = Email.Create(email);
        return await DbSet.AnyAsync(c => c.Email == emailVo, cancellationToken);
    }
}
