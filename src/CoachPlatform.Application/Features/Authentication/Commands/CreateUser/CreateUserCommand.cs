using CoachPlatform.Application.Shared.Exceptions;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

// Alias to avoid ambiguity between Application and FluentValidation ValidationException
using AppValidationException = CoachPlatform.Application.Shared.Exceptions.ValidationException;

namespace CoachPlatform.Application.Features.Authentication.Commands.CreateUser;

public record CreateUserCommand(
    string Email,
    string Username,
    string Password,
    string Role,            // "Coach", "Athlete", "Admin"
    Guid? CoachId = null,   // Required if Role == Coach
    Guid? AthleteId = null  // Required if Role == Athlete
) : IRequest<UserDto>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsInRole("Admin"))
            throw new ForbiddenAccessException("Only admins can create users.");

        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException($"User with email '{request.Email}' already exists.");

        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
            throw new ConflictException($"Username '{request.Username}' is already taken.");

        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Role", "Invalid role. Must be 'Coach', 'Athlete', or 'Admin'.")
            });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        User user = role switch
        {
            UserRole.Coach => await CreateCoachUserAsync(request, passwordHash, cancellationToken),
            UserRole.Athlete => await CreateAthleteUserAsync(request, passwordHash, cancellationToken),
            UserRole.Admin => User.CreateAdmin(request.Email, request.Username, passwordHash),
            _ => throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Role", "Invalid role.")
            })
        };

        _userRepository.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Email, user.Username, user.Role.ToString(), user.CoachId, user.AthleteId, user.IsActive);
    }

    private async Task<User> CreateCoachUserAsync(CreateUserCommand request, string passwordHash, CancellationToken cancellationToken)
    {
        Guid coachId;

        if (request.CoachId.HasValue)
        {
            // Link to existing Coach entity
            var coachExists = await _context.Coaches.AnyAsync(c => c.Id == request.CoachId.Value, cancellationToken);
            if (!coachExists)
                throw new NotFoundException("Coach", request.CoachId.ToString()!);
            coachId = request.CoachId.Value;
        }
        else
        {
            // Auto-create a new Coach entity from user's email and username
            var nameParts = request.Username.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : request.Username;
            var coach = Coach.Create(firstName, lastName, request.Email);
            _context.Coaches.Add(coach);
            coachId = coach.Id;
        }

        return User.CreateCoach(coachId, request.Email, request.Username, passwordHash);
    }

    private async Task<User> CreateAthleteUserAsync(CreateUserCommand request, string passwordHash, CancellationToken cancellationToken)
    {
        if (!request.AthleteId.HasValue)
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("AthleteId", "AthleteId is required for Athlete role.")
            });

        var athleteExists = await _context.Athletes.AnyAsync(a => a.Id == request.AthleteId.Value, cancellationToken);
        if (!athleteExists)
            throw new NotFoundException("Athlete", request.AthleteId.ToString()!);

        return User.CreateAthlete(request.AthleteId.Value, request.Email, request.Username, passwordHash);
    }
}
