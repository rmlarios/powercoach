using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.Athletes.Queries.GetExerciseHistory;

/// <summary>
/// Handler for GetExerciseHistoryQuery.
/// Returns comprehensive exercise history for an athlete.
/// </summary>
public class GetExerciseHistoryQueryHandler : IRequestHandler<GetExerciseHistoryQuery, ExerciseHistoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetExerciseHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExerciseHistoryDto?> Handle(GetExerciseHistoryQuery request, CancellationToken cancellationToken)
    {
        // Verify athlete exists
        var athlete = await _context.Athletes
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AthleteId, cancellationToken);

        if (athlete is null)
        {
            return null;
        }

        // Verify exercise exists
        var exercise = await _context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
        {
            return null;
        }

        var athleteName = $"{athlete.Name.FirstName} {athlete.Name.LastName}";

        // Get all max lifts for this exercise (sorted by date)
        var maxLifts = await _context.AthleteMaxLifts
            .Where(m => m.AthleteId == request.AthleteId && m.ExerciseId == request.ExerciseId)
            .OrderByDescending(m => m.RecordedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Current 1RM (most recent)
        var current1RM = maxLifts.FirstOrDefault();
        MaxLiftSummaryDto? current1RMDto = current1RM != null
            ? new MaxLiftSummaryDto
            {
                Weight = current1RM.Weight,
                RecordedAt = current1RM.RecordedAt,
                IsTested = current1RM.IsTested,
                Source = current1RM.IsTested ? "Tested" : "Estimated"
            }
            : null;

        // Personal Record (highest weight ever)
        var personalRecord = maxLifts.OrderByDescending(m => m.Weight).FirstOrDefault();
        MaxLiftSummaryDto? personalRecordDto = personalRecord != null
            ? new MaxLiftSummaryDto
            {
                Weight = personalRecord.Weight,
                RecordedAt = personalRecord.RecordedAt,
                IsTested = personalRecord.IsTested,
                Source = personalRecord.IsTested ? "Tested" : "Estimated"
            }
            : null;

        // Calculate trend (comparing current to 3 months ago)
        decimal? trendKg = null;
        if (maxLifts.Count >= 2)
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var oldRecord = maxLifts
                .Where(m => m.RecordedAt <= threeMonthsAgo)
                .OrderByDescending(m => m.RecordedAt)
                .FirstOrDefault();

            if (oldRecord != null && current1RM != null)
            {
                trendKg = current1RM.Weight - oldRecord.Weight;
            }
        }

        // Get last programmed prescription
        var lastProgrammed = await GetLastProgrammedAsync(request.AthleteId, request.ExerciseId, cancellationToken);

        // Get recent exercise logs (last 10 performance records)
        var recentLogs = await _context.AthleteExerciseLogs
            .Include(l => l.Workout)
            .ThenInclude(w => w.AthleteProgram)
            .Where(l => l.ExerciseId == request.ExerciseId && 
                       l.Workout.AthleteProgram.AthleteId == request.AthleteId)
            .OrderByDescending(l => l.Workout.CompletedDate ?? l.Workout.ScheduledDate)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var recentLogDtos = recentLogs
            .GroupBy(l => l.WorkoutId)
            .Select(g => new ExerciseLogSummaryDto
            {
                PerformedAt = g.First().Workout.CompletedDate ?? g.First().Workout.ScheduledDate,
                Sets = g.Count(),
                Reps = (int)g.Average(l => l.Reps),
                Weight = g.Max(l => l.Weight),
                Rpe = g.Average(l => l.Rpe),
                Notes = g.FirstOrDefault(l => !string.IsNullOrEmpty(l.Notes))?.Notes
            })
            .ToList();

        // Calculate suggested start percentage
        var (suggestedPercentage, suggestedWeight) = CalculateSuggestion(current1RMDto?.Weight, lastProgrammed);

        var hasHistory = current1RMDto != null || recentLogDtos.Count > 0 || lastProgrammed != null;

        return new ExerciseHistoryDto
        {
            AthleteId = request.AthleteId,
            AthleteName = athleteName,
            ExerciseId = request.ExerciseId,
            ExerciseName = exercise.Name,
            Current1RM = current1RMDto,
            PersonalRecord = personalRecordDto,
            LastProgrammed = lastProgrammed,
            TrendKg = trendKg,
            SuggestedStartPercentage = suggestedPercentage,
            SuggestedStartWeight = suggestedWeight,
            RecentLogs = recentLogDtos,
            HasHistory = hasHistory
        };
    }

    private async Task<LastProgrammedDto?> GetLastProgrammedAsync(
        Guid athleteId, 
        Guid exerciseId, 
        CancellationToken cancellationToken)
    {
        // Find athlete programs for this athlete
        var athletePrograms = await _context.AthletePrograms
            .Where(ap => ap.AthleteId == athleteId)
            .OrderByDescending(ap => ap.StartDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (athletePrograms.Count == 0)
        {
            return null;
        }

        var programTemplateIds = athletePrograms.Select(ap => ap.ProgramTemplateId).Distinct().ToList();

        // Find the most recent exercise template matching this exercise in assigned programs
        var lastExercise = await _context.ProgramExerciseTemplates
            .Include(e => e.DayTemplate)
                .ThenInclude(d => d.WeekTemplate)
                    .ThenInclude(w => w.ProgramTemplate)
            .Where(e => e.ExerciseId == exerciseId)
            .Where(e => programTemplateIds.Contains(e.DayTemplate.WeekTemplate.ProgramTemplateId))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (lastExercise == null)
        {
            return null;
        }

        var athleteProgram = athletePrograms
            .FirstOrDefault(ap => ap.ProgramTemplateId == lastExercise.DayTemplate.WeekTemplate.ProgramTemplateId);

        if (athleteProgram == null)
        {
            return null;
        }

        // Build prescription string
        var prescription = BuildPrescriptionString(lastExercise);

        return new LastProgrammedDto
        {
            ProgramName = lastExercise.DayTemplate.WeekTemplate.ProgramTemplate.Name,
            WeekNumber = lastExercise.DayTemplate.WeekTemplate.WeekNumber,
            DayNumber = lastExercise.DayTemplate.DayNumber,
            Prescription = prescription,
            ProgramDate = athleteProgram.StartDate
        };
    }

    private static string BuildPrescriptionString(Domain.Entities.ProgramExerciseTemplate exercise)
    {
        var parts = new List<string>();
        
        // Sets x Reps (Reps is a string like "8-12", "10", etc.)
        if (exercise.Sets > 0)
        {
            parts.Add($"{exercise.Sets}x{exercise.Reps}");
        }

        // Add RPE if available
        if (exercise.TargetRpe.HasValue)
        {
            parts.Add($"@RPE{exercise.TargetRpe.Value}");
        }

        // Add percentage if available
        if (exercise.PercentageRM.HasValue)
        {
            parts.Add($"@{exercise.PercentageRM.Value}%");
        }
        
        return string.Join(" ", parts);
    }

    private static (int? percentage, decimal? weight) CalculateSuggestion(
        decimal? current1RM, 
        LastProgrammedDto? lastProgrammed)
    {
        // Default starting percentage based on common periodization
        const int defaultStartPercentage = 65;

        if (current1RM == null || current1RM <= 0)
        {
            return (null, null);
        }

        // Use 65% as a conservative starting point
        var suggestedPercentage = defaultStartPercentage;
        var suggestedWeight = Math.Round(current1RM.Value * suggestedPercentage / 100 / 2.5m) * 2.5m;

        return (suggestedPercentage, suggestedWeight);
    }
}
