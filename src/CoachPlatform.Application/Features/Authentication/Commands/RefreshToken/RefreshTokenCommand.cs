using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<TokenResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ICurrentUserService _currentUserService;
    
    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IJwtService jwtService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _currentUserService = currentUserService;
    }
    
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User not found in token.");
        
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User is not active.");
        
        if (user.RefreshTokenHash == null || user.RefreshTokenExpiresAt == null)
            throw new UnauthorizedAccessException("No refresh token found.");
        
        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token has expired.");
        
        if (!_jwtService.VerifyRefreshToken(request.RefreshToken, user.RefreshTokenHash))
            throw new UnauthorizedAccessException("Invalid refresh token.");
        
        var newAccessToken = _jwtService.GenerateAccessToken(
            user.Id, user.Email, user.Role.ToString(), user.CoachId, user.AthleteId);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newHash = _jwtService.HashRefreshToken(newRefreshToken);
        
        user.SetRefreshToken(newHash, DateTime.UtcNow.AddDays(7));
        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new TokenResponse(newAccessToken, newRefreshToken, DateTime.UtcNow.AddMinutes(15));
    }
}
