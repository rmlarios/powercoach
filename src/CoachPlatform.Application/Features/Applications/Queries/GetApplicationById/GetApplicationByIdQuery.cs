using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Applications.Queries.GetApplicationById;

/// <summary>
/// Query to get an application by its ID.
/// </summary>
public record GetApplicationByIdQuery : IRequest<ApplicationDto?>
{
    public Guid Id { get; init; }
}
