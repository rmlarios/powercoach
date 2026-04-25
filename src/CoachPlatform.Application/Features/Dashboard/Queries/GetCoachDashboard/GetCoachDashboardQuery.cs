using CoachPlatform.Application.Shared.DTOs;
using MediatR;

namespace CoachPlatform.Application.Features.Dashboard.Queries.GetCoachDashboard;

/// <summary>
/// Query to get the coach's intelligent dashboard — stats, alerts, athlete statuses, activity feed.
/// The coachId is taken directly from the request param — no tenant pipeline validation needed.
/// </summary>
public record GetCoachDashboardQuery : IRequest<CoachDashboardDto>
{
    public Guid CoachId { get; init; }
}
