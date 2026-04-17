using System.Security.Claims;

namespace CoachPlatform.Application.Shared.Interfaces;

public interface IJwtService
{
    /// <summary>
    /// Generate a JWT access token for the given user.
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, string role, Guid? coachId = null, Guid? athleteId = null);
    
    /// <summary>
    /// Generate a refresh token (long-lived, for token rotation).
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validate a JWT token and return claims if valid.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    
    /// <summary>
    /// Hash a refresh token for storage.
    /// </summary>
    string HashRefreshToken(string refreshToken);
    
    /// <summary>
    /// Verify a refresh token against its hash.
    /// </summary>
    bool VerifyRefreshToken(string refreshToken, string refreshTokenHash);
}

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string Role,
    Guid? CoachId,
    Guid? AthleteId,
    bool IsActive
);
