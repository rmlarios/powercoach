using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoachPlatform.Application.Shared.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CoachPlatform.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;
    
    public JwtService(IConfiguration config)
    {
        _config = config;
        _secret = config["TokenSettings:Secret"] ?? throw new InvalidOperationException("TokenSettings:Secret not configured");
        _issuer = config["TokenSettings:Issuer"] ?? "CoachPlatform";
        _audience = config["TokenSettings:Audience"] ?? "CoachPlatformAPI";
        _accessTokenExpirationMinutes = config.GetValue("TokenSettings:AccessTokenExpirationMinutes", 15);
        _refreshTokenExpirationDays = config.GetValue("TokenSettings:RefreshTokenExpirationDays", 7);
    }
    
    public string GenerateAccessToken(Guid userId, string email, string role, Guid? coachId = null, Guid? athleteId = null)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        
        // Add role-specific claims
        if (coachId.HasValue)
            claims.Add(new("coach_id", coachId.Value.ToString()));
        if (athleteId.HasValue)
            claims.Add(new("athlete_id", athleteId.Value.ToString()));
        
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken()
    {
        // Use cryptographically secure random bytes
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }
    
    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        try
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public string HashRefreshToken(string refreshToken)
    {
        // Use SHA256 for refresh token hashing
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }
    }
    
    public bool VerifyRefreshToken(string refreshToken, string refreshTokenHash)
    {
        var hash = HashRefreshToken(refreshToken);
        return hash == refreshTokenHash;
    }
}
