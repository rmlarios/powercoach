using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetExerciseLiftHistory;

/// <summary>
/// Handler for GetExerciseLiftHistoryQuery.
/// Returns exercise details + session-by-session lift data with PRs and e1RM calculations.
/// </summary>
public class GetExerciseLiftHistoryQueryHandler : IRequestHandler<GetExerciseLiftHistoryQuery, ExerciseLiftHistoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetExerciseLiftHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExerciseLiftHistoryDto?> Handle(GetExerciseLiftHistoryQuery request, CancellationToken cancellationToken)
    {
        // Load exercise details
        var exercise = await _context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExerciseId, cancellationToken);

        if (exercise is null)
            return null;

        // Get all completed sets for this exercise by this athlete (via their workouts)
        var athleteProgramIds = await _context.AthletePrograms
            .AsNoTracking()
            .Where(ap => ap.AthleteId == request.AthleteId)
            .Select(ap => ap.Id)
            .ToListAsync(cancellationToken);

        var logs = await _context.AthleteExerciseLogs
            .AsNoTracking()
            .Include(l => l.Workout)
            .Where(l => l.ExerciseId == request.ExerciseId
                        && l.IsCompleted
                        && l.Weight > 0
                        && l.Reps > 0
                        && athleteProgramIds.Contains(l.Workout.AthleteProgramId))
            .OrderByDescending(l => l.Workout.ScheduledDate)
            .ToListAsync(cancellationToken);

        // Group by workout session (date)
        var sessionGroups = logs
            .GroupBy(l => l.WorkoutId)
            .Take(request.Limit)
            .ToList();

        // Build lift entries per session
        var entries = new List<LiftEntryDto>();
        decimal globalMaxWeight = 0;
        DateTime globalMaxWeightDate = default;
        int globalMaxReps = 0;
        DateTime globalMaxRepsDate = default;
        decimal globalMaxE1RM = 0;
        DateTime globalMaxE1RMDate = default;
        decimal globalMaxVolume = 0;
        DateTime globalMaxVolumeDate = default;

        foreach (var session in sessionGroups)
        {
            var sessionLogs = session.ToList();
            var workout = sessionLogs.First().Workout;
            var date = workout.ScheduledDate;

            var maxWeight = sessionLogs.Max(l => l.Weight);
            var bestRepsAtMaxWeight = sessionLogs
                .Where(l => l.Weight == maxWeight)
                .Max(l => l.Reps);
            var bestRpe = sessionLogs
                .Where(l => l.Rpe.HasValue)
                .OrderByDescending(l => l.Weight)
                .FirstOrDefault()?.Rpe;

            // Epley e1RM from best set
            var bestSet = sessionLogs
                .OrderByDescending(l => CalculateE1RM(l.Weight, l.Reps))
                .First();
            var e1RM = CalculateE1RM(bestSet.Weight, bestSet.Reps);

            // Volume = sum of (weight × reps) for all sets
            var totalVolume = sessionLogs.Sum(l => l.Weight * l.Reps);
            var totalSets = sessionLogs.Count;

            entries.Add(new LiftEntryDto
            {
                Date = date,
                WeekNumber = workout.WeekNumber,
                DayNumber = workout.DayNumber,
                MaxWeight = maxWeight,
                BestReps = bestRepsAtMaxWeight,
                Rpe = bestRpe,
                Estimated1RM = Math.Round(e1RM, 1),
                TotalVolume = totalVolume,
                TotalSets = totalSets
            });

            // Track PRs across all sessions
            if (maxWeight > globalMaxWeight || (maxWeight == globalMaxWeight && date > globalMaxWeightDate))
            {
                globalMaxWeight = maxWeight;
                globalMaxWeightDate = date;
            }
            var sessionMaxReps = sessionLogs.Max(l => l.Reps);
            if (sessionMaxReps > globalMaxReps || (sessionMaxReps == globalMaxReps && date > globalMaxRepsDate))
            {
                globalMaxReps = sessionMaxReps;
                globalMaxRepsDate = date;
            }
            if (e1RM > globalMaxE1RM || (e1RM == globalMaxE1RM && date > globalMaxE1RMDate))
            {
                globalMaxE1RM = e1RM;
                globalMaxE1RMDate = date;
            }
            if (totalVolume > globalMaxVolume || (totalVolume == globalMaxVolume && date > globalMaxVolumeDate))
            {
                globalMaxVolume = totalVolume;
                globalMaxVolumeDate = date;
            }
        }

        // Current e1RM = most recent session
        var currentE1RM = entries.FirstOrDefault()?.Estimated1RM;

        // Build PR record (only if we have data)
        ExerciseLiftPRDto? prs = entries.Count > 0
            ? new ExerciseLiftPRDto
            {
                MaxWeight = globalMaxWeight,
                MaxWeightDate = globalMaxWeightDate,
                MaxReps = globalMaxReps,
                MaxRepsDate = globalMaxRepsDate,
                MaxEstimated1RM = Math.Round(globalMaxE1RM, 1),
                MaxEstimated1RMDate = globalMaxE1RMDate,
                MaxVolume = globalMaxVolume,
                MaxVolumeDate = globalMaxVolumeDate
            }
            : null;

        return new ExerciseLiftHistoryDto
        {
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            Category = exercise.Category.ToString(),
            PrimaryMuscleGroup = exercise.PrimaryMuscleGroup.ToString(),
            Equipment = exercise.Equipment,
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            Description = exercise.Description,
            IsCompound = exercise.IsCompound,
            Instructions = exercise.Instructions,
            CoachingCues = exercise.CoachingCues,
            CurrentEstimated1RM = currentE1RM,
            PersonalRecords = prs,
            Entries = entries
        };
    }

    /// <summary>
    /// Epley formula: e1RM = weight × (1 + reps / 30)
    /// For 1RM (reps == 1), just returns the weight.
    /// </summary>
    private static decimal CalculateE1RM(decimal weight, int reps)
    {
        if (reps <= 0 || weight <= 0) return 0;
        if (reps == 1) return weight;
        return weight * (1m + reps / 30m);
    }
}
