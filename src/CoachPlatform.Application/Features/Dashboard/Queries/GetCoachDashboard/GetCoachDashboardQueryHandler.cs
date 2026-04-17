using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Dashboard.Queries.GetCoachDashboard;

/// <summary>
/// Handler for GetCoachDashboardQuery.
/// Aggregates stats, alerts, athlete statuses, and recent activity in a single query.
/// </summary>
public class GetCoachDashboardQueryHandler : IRequestHandler<GetCoachDashboardQuery, CoachDashboardDto>
{
    private readonly IApplicationDbContext _context;

    public GetCoachDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CoachDashboardDto> Handle(
        GetCoachDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var coachId = request.CoachId;
        var now = DateTime.UtcNow;
        var today = now.Date;
        var twoDaysAgo = today.AddDays(-2);
        var sevenDaysAgo = today.AddDays(-7);
        var thirtyDaysAgo = today.AddDays(-30);
        var sixtyDaysAgo = today.AddDays(-60);
        var sevenDaysFromNow = today.AddDays(7);
        var fortyEightHoursAgo = now.AddHours(-48);

        // ── Pre-load coach's athlete IDs (used by all sections) ──
        var activeAthletes = await _context.Athletes
            .AsNoTracking()
            .Where(a => a.CoachId == coachId && a.Status == AthleteStatus.Active)
            .Select(a => new { a.Id, FullName = a.Name.FirstName + " " + a.Name.LastName, a.ProfilePictureUrl })
            .ToListAsync(cancellationToken);

        var activeAthleteIds = activeAthletes.Select(a => a.Id).ToHashSet();

        // ═══════════════════════════════════════════
        // 1. STATS
        // ═══════════════════════════════════════════

        var activeAthletesCount = activeAthletes.Count;

        var pendingApplications = await _context.Applications
            .AsNoTracking()
            .CountAsync(a => a.CoachId == coachId && a.Status == ApplicationStatus.Pending, cancellationToken);

        var activePrograms = await _context.AthletePrograms
            .AsNoTracking()
            .CountAsync(ap => activeAthleteIds.Contains(ap.AthleteId) && ap.Status == ProgramStatus.Active, cancellationToken);

        // Completion rate: last 7 days — completed vs total scheduled (past due)
        var recentWorkouts = await _context.AthleteWorkouts
            .AsNoTracking()
            .Where(w => _context.AthletePrograms
                .Where(ap => activeAthleteIds.Contains(ap.AthleteId))
                .Select(ap => ap.Id)
                .Contains(w.AthleteProgramId))
            .Where(w => w.ScheduledDate >= sevenDaysAgo && w.ScheduledDate <= today)
            .Select(w => w.Status)
            .ToListAsync(cancellationToken);

        var totalScheduled = recentWorkouts.Count;
        var completedCount = recentWorkouts.Count(s => s == WorkoutStatus.Completed || s == WorkoutStatus.PartiallyCompleted);
        var completionRate = totalScheduled > 0 ? (int)Math.Round(100.0 * completedCount / totalScheduled) : 0;

        // Pending check-in reviews
        var pendingCheckIns = await _context.CheckIns
            .AsNoTracking()
            .CountAsync(c => activeAthleteIds.Contains(c.AthleteId) && !c.IsReviewed, cancellationToken);

        // Athletes trend: active now vs active 30 days ago
        var athletesLastMonth = await _context.Athletes
            .AsNoTracking()
            .CountAsync(a => a.CoachId == coachId && a.Status == AthleteStatus.Active && a.StartDate <= thirtyDaysAgo, cancellationToken);

        var athletesTwoMonthsAgo = await _context.Athletes
            .AsNoTracking()
            .CountAsync(a => a.CoachId == coachId && a.Status == AthleteStatus.Active && a.StartDate <= sixtyDaysAgo, cancellationToken);

        var athletesTrend = athletesLastMonth > 0 && athletesTwoMonthsAgo > 0
            ? activeAthletesCount - athletesLastMonth
            : 0;

        var stats = new DashboardStatsDto
        {
            ActiveAthletes = activeAthletesCount,
            PendingApplications = pendingApplications,
            ActivePrograms = activePrograms,
            CompletionRate = completionRate,
            PendingCheckIns = pendingCheckIns,
            AthletesTrend = athletesTrend,
        };

        // ═══════════════════════════════════════════
        // 2. ALERTS
        // ═══════════════════════════════════════════

        var alerts = new List<DashboardAlertDto>();

        // -- High Fatigue: workouts in last 2 days with FatigueRating >= 8 --
        var highFatigueWorkouts = await _context.AthleteWorkouts
            .AsNoTracking()
            .Where(w => _context.AthletePrograms
                .Where(ap => activeAthleteIds.Contains(ap.AthleteId))
                .Select(ap => ap.Id)
                .Contains(w.AthleteProgramId))
            .Where(w => w.CompletedDate != null && w.CompletedDate >= twoDaysAgo && w.FatigueRating >= 8)
            .Select(w => new
            {
                AthleteId = _context.AthletePrograms
                    .Where(ap => ap.Id == w.AthleteProgramId)
                    .Select(ap => ap.AthleteId)
                    .FirstOrDefault(),
                w.FatigueRating,
                w.CompletedDate,
            })
            .ToListAsync(cancellationToken);

        foreach (var hf in highFatigueWorkouts)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == hf.AthleteId);
            if (athlete != null)
            {
                alerts.Add(new DashboardAlertDto
                {
                    AlertType = "HighFatigue",
                    Severity = "warning",
                    AthleteId = hf.AthleteId,
                    AthleteName = athlete.FullName,
                    Message = $"{athlete.FullName} reportó fatiga {hf.FatigueRating}/10 en su último entrenamiento",
                    CreatedAt = hf.CompletedDate ?? now,
                });
            }
        }

        // -- Missed Workouts: active athletes with programs but no completed workout in 2 days --
        var athletesWithPrograms = await _context.AthletePrograms
            .AsNoTracking()
            .Where(ap => activeAthleteIds.Contains(ap.AthleteId) && ap.Status == ProgramStatus.Active)
            .Select(ap => ap.AthleteId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var athletesWithRecentWorkouts = await _context.AthleteWorkouts
            .AsNoTracking()
            .Where(w => _context.AthletePrograms
                .Where(ap => activeAthleteIds.Contains(ap.AthleteId) && ap.Status == ProgramStatus.Active)
                .Select(ap => ap.Id)
                .Contains(w.AthleteProgramId))
            .Where(w => w.CompletedDate != null && w.CompletedDate >= twoDaysAgo)
            .Select(w => _context.AthletePrograms
                .Where(ap => ap.Id == w.AthleteProgramId)
                .Select(ap => ap.AthleteId)
                .FirstOrDefault())
            .Distinct()
            .ToListAsync(cancellationToken);

        var missedWorkoutAthleteIds = athletesWithPrograms.Except(athletesWithRecentWorkouts).ToList();
        foreach (var athleteId in missedWorkoutAthleteIds)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == athleteId);
            if (athlete != null)
            {
                alerts.Add(new DashboardAlertDto
                {
                    AlertType = "MissedWorkout",
                    Severity = "danger",
                    AthleteId = athleteId,
                    AthleteName = athlete.FullName,
                    Message = $"{athlete.FullName} no ha entrenado en más de 2 días",
                    CreatedAt = now,
                });
            }
        }

        // -- Missed Check-ins: active athletes without check-in in 7 days --
        var athletesWithRecentCheckIn = await _context.CheckIns
            .AsNoTracking()
            .Where(c => activeAthleteIds.Contains(c.AthleteId) && c.CheckInDate >= sevenDaysAgo)
            .Select(c => c.AthleteId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var missedCheckInIds = activeAthleteIds.Except(athletesWithRecentCheckIn).ToList();
        foreach (var athleteId in missedCheckInIds)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == athleteId);
            if (athlete != null)
            {
                alerts.Add(new DashboardAlertDto
                {
                    AlertType = "MissedCheckIn",
                    Severity = "warning",
                    AthleteId = athleteId,
                    AthleteName = athlete.FullName,
                    Message = $"{athlete.FullName} no ha enviado check-in en más de 7 días",
                    CreatedAt = now,
                });
            }
        }

        // -- Expiring Subscriptions: ending within 7 days --
        var expiringSubscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Where(s => activeAthleteIds.Contains(s.AthleteId)
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= today
                && s.EndDate <= sevenDaysFromNow)
            .Select(s => new { s.AthleteId, s.EndDate })
            .ToListAsync(cancellationToken);

        foreach (var sub in expiringSubscriptions)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == sub.AthleteId);
            if (athlete != null)
            {
                var daysLeft = (sub.EndDate - today).Days;
                alerts.Add(new DashboardAlertDto
                {
                    AlertType = "ExpiringSubscription",
                    Severity = "warning",
                    AthleteId = sub.AthleteId,
                    AthleteName = athlete.FullName,
                    Message = $"La suscripción de {athlete.FullName} expira en {daysLeft} día{(daysLeft != 1 ? "s" : "")}",
                    CreatedAt = now,
                });
            }
        }

        // -- Pending Applications older than 48h --
        var staleApplications = await _context.Applications
            .AsNoTracking()
            .Where(a => a.CoachId == coachId && a.Status == ApplicationStatus.Pending && a.CreatedAt <= fortyEightHoursAgo)
            .Select(a => new { a.Id, Name = a.ApplicantName.FirstName + " " + a.ApplicantName.LastName, a.CreatedAt })
            .ToListAsync(cancellationToken);

        foreach (var app in staleApplications)
        {
            alerts.Add(new DashboardAlertDto
            {
                AlertType = "PendingApplication",
                Severity = "warning",
                AthleteName = app.Name,
                Message = $"Solicitud de {app.Name} pendiente hace más de 48 horas",
                CreatedAt = app.CreatedAt,
            });
        }

        // Sort alerts: danger first, then by date desc
        var sortedAlerts = alerts
            .OrderByDescending(a => a.Severity == "danger" ? 1 : 0)
            .ThenByDescending(a => a.CreatedAt)
            .ToList();

        // ═══════════════════════════════════════════
        // 3. ATHLETE STATUSES
        // ═══════════════════════════════════════════

        // Last workout per athlete (via AthleteProgram → AthleteWorkout)
        var lastWorkouts = await _context.AthleteWorkouts
            .AsNoTracking()
            .Where(w => _context.AthletePrograms
                .Where(ap => activeAthleteIds.Contains(ap.AthleteId))
                .Select(ap => ap.Id)
                .Contains(w.AthleteProgramId))
            .Where(w => w.CompletedDate != null)
            .GroupBy(w => _context.AthletePrograms
                .Where(ap => ap.Id == w.AthleteProgramId)
                .Select(ap => ap.AthleteId)
                .FirstOrDefault())
            .Select(g => new
            {
                AthleteId = g.Key,
                LastDate = g.Max(w => w.CompletedDate),
                LastDayNumber = g.OrderByDescending(w => w.CompletedDate).Select(w => w.DayNumber).FirstOrDefault(),
                LastWeekNumber = g.OrderByDescending(w => w.CompletedDate).Select(w => w.WeekNumber).FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var lastWorkoutsDict = lastWorkouts.ToDictionary(w => w.AthleteId);

        // Last check-in per athlete
        var lastCheckIns = await _context.CheckIns
            .AsNoTracking()
            .Where(c => activeAthleteIds.Contains(c.AthleteId))
            .GroupBy(c => c.AthleteId)
            .Select(g => new
            {
                AthleteId = g.Key,
                LastDate = g.Max(c => c.CheckInDate),
                IsReviewed = g.OrderByDescending(c => c.CheckInDate).Select(c => c.IsReviewed).FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var lastCheckInsDict = lastCheckIns.ToDictionary(c => c.AthleteId);

        // Active programs per athlete
        var activeProgramsData = await _context.AthletePrograms
            .AsNoTracking()
            .Where(ap => activeAthleteIds.Contains(ap.AthleteId) && ap.Status == ProgramStatus.Active)
            .Select(ap => new
            {
                ap.AthleteId,
                ProgramName = _context.ProgramTemplates
                    .Where(pt => pt.Id == ap.ProgramTemplateId)
                    .Select(pt => pt.Name)
                    .FirstOrDefault(),
                ap.CurrentWeek,
            })
            .ToListAsync(cancellationToken);

        var activeProgramsDict = activeProgramsData
            .GroupBy(p => p.AthleteId)
            .ToDictionary(g => g.Key, g => g.First());

        // Fatigue alert athlete IDs for red coloring
        var highFatigueAthleteIds = highFatigueWorkouts.Select(hf => hf.AthleteId).ToHashSet();

        var athleteStatuses = activeAthletes.Select(a =>
        {
            lastWorkoutsDict.TryGetValue(a.Id, out var lastWk);
            lastCheckInsDict.TryGetValue(a.Id, out var lastCi);
            activeProgramsDict.TryGetValue(a.Id, out var program);

            var hasRecentWorkout = lastWk?.LastDate >= twoDaysAgo;
            var hasRecentCheckIn = lastCi?.LastDate >= sevenDaysAgo;
            var hasFatigue = highFatigueAthleteIds.Contains(a.Id);

            string statusColor;
            if (!hasRecentWorkout || hasFatigue)
                statusColor = "red";
            else if (lastCi != null && !lastCi.IsReviewed)
                statusColor = "yellow";
            else if (!hasRecentCheckIn)
                statusColor = "yellow";
            else
                statusColor = "green";

            return new AthleteStatusRowDto
            {
                AthleteId = a.Id,
                Name = a.FullName,
                ProfilePictureUrl = a.ProfilePictureUrl,
                Status = statusColor == "green" ? "OK" : statusColor == "yellow" ? "Atención" : "Problema",
                StatusColor = statusColor,
                LastWorkoutDate = lastWk?.LastDate,
                LastWorkoutName = lastWk != null ? $"Sem {lastWk.LastWeekNumber} · Día {lastWk.LastDayNumber}" : null,
                LastCheckInDate = lastCi?.LastDate,
                IsCheckInReviewed = lastCi?.IsReviewed,
                ActiveProgramName = program?.ProgramName,
                CurrentWeek = program?.CurrentWeek,
            };
        })
        .OrderByDescending(a => a.StatusColor == "red" ? 2 : a.StatusColor == "yellow" ? 1 : 0)
        .ThenBy(a => a.Name)
        .ToList();

        // ═══════════════════════════════════════════
        // 4. RECENT ACTIVITY
        // ═══════════════════════════════════════════

        var threeDaysAgo = today.AddDays(-3);

        // Completed / started workouts in last 3 days
        var recentWorkoutActivity = await _context.AthleteWorkouts
            .AsNoTracking()
            .Where(w => _context.AthletePrograms
                .Where(ap => activeAthleteIds.Contains(ap.AthleteId))
                .Select(ap => ap.Id)
                .Contains(w.AthleteProgramId))
            .Where(w => (w.CompletedDate != null && w.CompletedDate >= threeDaysAgo)
                     || (w.StartedAt != null && w.StartedAt >= threeDaysAgo))
            .Select(w => new
            {
                AthleteId = _context.AthletePrograms
                    .Where(ap => ap.Id == w.AthleteProgramId)
                    .Select(ap => ap.AthleteId)
                    .FirstOrDefault(),
                w.CompletedDate,
                w.StartedAt,
                w.Status,
                w.WeekNumber,
                w.DayNumber,
                w.DurationMinutes,
            })
            .ToListAsync(cancellationToken);

        var activityItems = new List<ActivityFeedItemDto>();

        foreach (var w in recentWorkoutActivity)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == w.AthleteId);
            if (athlete == null) continue;

            if (w.Status == WorkoutStatus.Completed || w.Status == WorkoutStatus.PartiallyCompleted)
            {
                activityItems.Add(new ActivityFeedItemDto
                {
                    Type = "WorkoutCompleted",
                    AthleteName = athlete.FullName,
                    AthleteId = w.AthleteId,
                    Description = $"{athlete.FullName} completó entrenamiento Sem {w.WeekNumber} · Día {w.DayNumber}"
                        + (w.DurationMinutes.HasValue ? $" ({w.DurationMinutes}min)" : ""),
                    Timestamp = w.CompletedDate ?? now,
                });
            }
            else if (w.Status == WorkoutStatus.InProgress && w.StartedAt != null)
            {
                activityItems.Add(new ActivityFeedItemDto
                {
                    Type = "WorkoutStarted",
                    AthleteName = athlete.FullName,
                    AthleteId = w.AthleteId,
                    Description = $"{athlete.FullName} inició entrenamiento Sem {w.WeekNumber} · Día {w.DayNumber}",
                    Timestamp = w.StartedAt.Value,
                });
            }
        }

        // Check-ins in last 3 days
        var recentCheckIns = await _context.CheckIns
            .AsNoTracking()
            .Where(c => activeAthleteIds.Contains(c.AthleteId) && c.CheckInDate >= threeDaysAgo)
            .Select(c => new { c.AthleteId, c.CheckInDate, c.Weight })
            .ToListAsync(cancellationToken);

        foreach (var ci in recentCheckIns)
        {
            var athlete = activeAthletes.FirstOrDefault(a => a.Id == ci.AthleteId);
            if (athlete == null) continue;

            activityItems.Add(new ActivityFeedItemDto
            {
                Type = "CheckInSubmitted",
                AthleteName = athlete.FullName,
                AthleteId = ci.AthleteId,
                Description = $"{athlete.FullName} envió check-in"
                    + (ci.Weight.HasValue ? $" ({ci.Weight:F1}kg)" : ""),
                Timestamp = ci.CheckInDate,
            });
        }

        // Applications in last 3 days
        var recentApplications = await _context.Applications
            .AsNoTracking()
            .Where(a => a.CoachId == coachId && a.CreatedAt >= threeDaysAgo)
            .Select(a => new { Name = a.ApplicantName.FirstName + " " + a.ApplicantName.LastName, a.CreatedAt })
            .ToListAsync(cancellationToken);

        foreach (var app in recentApplications)
        {
            activityItems.Add(new ActivityFeedItemDto
            {
                Type = "ApplicationReceived",
                AthleteName = app.Name,
                Description = $"Nueva solicitud de {app.Name}",
                Timestamp = app.CreatedAt,
            });
        }

        var recentActivity = activityItems
            .OrderByDescending(a => a.Timestamp)
            .Take(20)
            .ToList();

        // ═══════════════════════════════════════════
        // ASSEMBLE RESULT
        // ═══════════════════════════════════════════

        return new CoachDashboardDto
        {
            Stats = stats,
            Alerts = sortedAlerts,
            AthleteStatuses = athleteStatuses,
            RecentActivity = recentActivity,
        };
    }
}
