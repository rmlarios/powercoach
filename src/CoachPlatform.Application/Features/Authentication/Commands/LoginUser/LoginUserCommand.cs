using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password
) : IRequest<LoginResponse>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    
    public LoginUserCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IJwtService jwtService)
    {
        _context = context;
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
    
    public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");
        
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
        
        var accessToken = _jwtService.GenerateAccessToken(
            user.Id, user.Email, user.Role.ToString(), user.CoachId, user.AthleteId);
        
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        
        user.SetRefreshToken(refreshTokenHash, DateTime.UtcNow.AddDays(7));
        user.UpdateLastLogin();
        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        return new LoginResponse(
            accessToken,
            refreshToken,
            new UserDto(user.Id, user.Email, user.Username, user.Role.ToString(), user.CoachId, user.AthleteId, user.IsActive)
        );
    }
}
