using CoachPlatform.Domain.Common;

namespace CoachPlatform.Domain.Entities;

/// <summary>
/// User aggregate root for authentication and authorization.
/// A User can be a Coach, Athlete, or Admin.
/// Coaches own data; Athletes see their assigned data; Admins manage users.
/// </summary>
public class User : AuditableEntity, IAggregateRoot
{
    public string Email { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    
    // Polymorphic relationship
    public Guid? CoachId { get; private set; }      // Set if Role == Coach
    public Guid? AthleteId { get; private set; }    // Set if Role == Athlete
    
    // Navigation properties
    public Coach? Coach { get; set; }
    public Athlete? Athlete { get; set; }
    
    // Audit trail
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    
    // Refresh token tracking (for logout/revocation)
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    
    private User() { }  // EF Core
    
    /// <summary>
    /// Factory method to create a Coach user.
    /// </summary>
    public static User CreateCoach(Guid coachId, string email, string username, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Username = username.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Coach,
            CoachId = coachId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Factory method to create an Athlete user.
    /// </summary>
    public static User CreateAthlete(Guid athleteId, string email, string username, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Username = username.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Athlete,
            AthleteId = athleteId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Factory method to create an Admin user.
    /// </summary>
    public static User CreateAdmin(string email, string username, string passwordHash)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            Username = username.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Update last login timestamp.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Update refresh token hash and expiration.
    /// </summary>
    public void SetRefreshToken(string refreshTokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiresAt = expiresAt;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Clear refresh token (logout).
    /// </summary>
    public void ClearRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Deactivate user.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Reactivate user.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }
    
    /// <summary>
    /// Update password hash.
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangeAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}

public enum UserRole
{
    Coach,
    Athlete,
    Admin
}
