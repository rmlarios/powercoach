using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Commands.ResetPassword;

public record ResetPasswordCommand(
    Guid UserId,
    string NewPassword
) : IRequest<Unit>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenAccessException("Only admins can reset passwords.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId.ToString());

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
