using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;

namespace CoachPlatform.Application.Features.TrainingPrograms.Commands.AddProgramDay;

/// <summary>
/// Command to add a training day to a program week.
/// </summary>
public record AddProgramDayCommand : IRequest<Guid>, ITenantRequest
{
    public Guid CoachId { get; init; }
    public Guid WeekTemplateId { get; init; }
    public int DayNumber { get; init; }
    public string? Name { get; init; }
    public DayFocus? Focus { get; init; }
    public string? Notes { get; init; }
}
