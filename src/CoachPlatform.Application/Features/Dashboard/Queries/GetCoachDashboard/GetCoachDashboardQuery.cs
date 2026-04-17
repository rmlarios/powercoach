using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;

namespace CoachPlatform.Application.Features.Dashboard.Queries.GetCoachDashboard;

/// <summary>
/// Query to get the coach's intelligent dashboard — stats, alerts, athlete statuses, activity feed.
/// </summary>
public record GetCoachDashboardQuery : IRequest<CoachDashboardDto>, ITenantRequest
{
    public Guid CoachId { get; init; }
}
