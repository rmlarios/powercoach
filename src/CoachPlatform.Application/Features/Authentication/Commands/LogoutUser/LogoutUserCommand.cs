using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Commands.LogoutUser;

public record LogoutUserCommand : IRequest<Unit>;

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    
    public LogoutUserCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }
    
    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new UnauthorizedAccessException("User not found.");
        
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");
        
        user.ClearRefreshToken();
        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
