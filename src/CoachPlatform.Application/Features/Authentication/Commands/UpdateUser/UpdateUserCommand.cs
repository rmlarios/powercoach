using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string? Username = null,
    bool? IsActive = null
) : IRequest<UserDto>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenAccessException("Only admins can update users.");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId.ToString());

        if (request.Username != null)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
                throw new ConflictException($"Username '{request.Username}' is already taken.");
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value) user.Activate();
            else user.Deactivate();
        }

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Email, user.Username, user.Role.ToString(), user.CoachId, user.AthleteId, user.IsActive);
    }
}
