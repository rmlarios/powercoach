using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Authentication.Queries.GetUsers;

public record GetUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenAccessException("Only admins can list users.");

        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(u => new UserDto(
            u.Id, u.Email, u.Username, u.Role.ToString(),
            u.CoachId, u.AthleteId, u.IsActive));
    }
}
