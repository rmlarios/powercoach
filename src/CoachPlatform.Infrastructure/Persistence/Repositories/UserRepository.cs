using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using CoachPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Coach)
            .Include(u => u.Athlete)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
    
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users
            .Include(u => u.Coach)
            .Include(u => u.Athlete)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
    
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.ToLowerInvariant();
        return await _context.Users
            .Include(u => u.Coach)
            .Include(u => u.Athlete)
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);
    }
    
    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
    
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Username == normalizedUsername, cancellationToken);
    }
    
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Coach)
            .Include(u => u.Athlete)
            .ToListAsync(cancellationToken);
    }
    
    public void Add(User user)
    {
        _context.Users.Add(user);
    }
    
    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}
