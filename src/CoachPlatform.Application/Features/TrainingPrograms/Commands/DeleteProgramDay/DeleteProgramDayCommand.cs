using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.DeleteProgramDay;

/// <summary>
/// Command to delete a day from a program week.
/// </summary>
public record DeleteProgramDayCommand : IRequest<Unit>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid DayId { get; init; }
}
