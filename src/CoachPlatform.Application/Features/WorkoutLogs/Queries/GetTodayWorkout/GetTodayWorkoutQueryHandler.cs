using CoachPlatform.Application.Shared.DTOs;
using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CoachPlatform.Application.Features.WorkoutLogs.Queries.GetTodayWorkout;

/// <summary>
/// Handler for GetTodayWorkoutQuery.
/// Finds the workout scheduled for today (or the nearest upcoming one if none today),
/// loads exercises with sets and previous performance data.
/// </summary>
public class GetTodayWorkoutQueryHandler : IRequestHandler<GetTodayWorkoutQuery, TodayWorkoutDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTodayWorkoutQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodayWorkoutDto?> Handle(GetTodayWorkoutQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        // Find the athlete's active or ready-to-start program
        var activeProgram = await _context.AthletePrograms
            .Include(ap => ap.ProgramTemplate)
            .Where(ap => ap.AthleteId == request.AthleteId
                         && (ap.Status == Domain.Enums.ProgramStatus.Active
                             || (ap.Status == Domain.Enums.ProgramStatus.NotStarted
                                 && ap.StartDate <= now)))
            .FirstOrDefaultAsync(cancellationToken);

        if (activeProgram is null)
            return null;

        // Auto-activate if still NotStarted and start date has arrived
        if (activeProgram.Status == Domain.Enums.ProgramStatus.NotStarted)
        {
            activeProgram.Start();
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Find today's workout (or the nearest future one not completed)
        var workout = await _context.AthleteWorkouts
            .AsNoTracking()
            .Include(w => w.ExerciseLogs)
                .ThenInclude(l => l.Exercise)
            .Where(w => w.AthleteProgramId == activeProgram.Id
                        && w.ScheduledDate.Date == today)
            .FirstOrDefaultAsync(cancellationToken);

        // If no workout today, try the next upcoming one
        if (workout is null)
        {
            workout = await _context.AthleteWorkouts
                .AsNoTracking()
                .Include(w => w.ExerciseLogs)
                    .ThenInclude(l => l.Exercise)
                .Where(w => w.AthleteProgramId == activeProgram.Id
                            && w.ScheduledDate.Date >= today
                            && !w.IsCompleted)
                .OrderBy(w => w.ScheduledDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (workout is null)
            return null;

        // Get the day template for day name and focus
        var dayTemplate = await _context.ProgramDayTemplates
            .AsNoTracking()
            .Include(d => d.WeekTemplate)
            .Include(d => d.Exercises)
                .ThenInclude(e => e.Exercise)
            .Where(d => d.WeekTemplate.ProgramTemplateId == activeProgram.ProgramTemplateId
                        && d.WeekTemplate.WeekNumber == workout.WeekNumber
                        && d.DayNumber == workout.DayNumber)
            .FirstOrDefaultAsync(cancellationToken);

        // Build exercise groups from the prescribed template + actual logs
        var exerciseGroups = new List<WorkoutExerciseGroupDto>();

        if (dayTemplate?.Exercises != null)
        {
            foreach (var prescribed in dayTemplate.Exercises.OrderBy(e => e.Order))
            {
                var logs = workout.ExerciseLogs
                    .Where(l => l.ExerciseId == prescribed.ExerciseId)
                    .OrderBy(l => l.SetNumber)
                    .ToList();

                // Fetch previous performance FIRST — we need estimated 1RM to calculate suggested weights
                var previousPerformance = await GetPreviousPerformanceAsync(
                    request.AthleteId, prescribed.ExerciseId, workout.Id, cancellationToken);

                // Parse compound notation for per-set targets (e.g. "1x1 91% + 3x4 80%")
                var perSetTargets = ParseCompoundNotation(prescribed.RawNotation, prescribed.Sets,
                    ParseTargetReps(prescribed.Reps), prescribed.Weight, prescribed.PercentageRM);

                // Build sets: use existing logs or create empty targets from template
                var sets = new List<WorkoutSetDto>();
                for (int s = 1; s <= prescribed.Sets; s++)
                {
                    var log = logs.FirstOrDefault(l => l.SetNumber == s);
                    var target = perSetTargets.Count >= s ? perSetTargets[s - 1] : null;

                    // Calculate suggested weight from %1RM × estimated 1RM
                    var pctRM = target?.PercentageRM ?? prescribed.PercentageRM;
                    var suggestedWeight = CalculateSuggestedWeight(pctRM, previousPerformance?.EstimatedOneRM);

                    sets.Add(new WorkoutSetDto
                    {
                        Id = log?.Id ?? Guid.Empty,
                        SetNumber = s,
                        TargetReps = log?.TargetReps ?? target?.Reps ?? ParseTargetReps(prescribed.Reps),
                        TargetWeight = log?.TargetWeight ?? target?.Weight ?? prescribed.Weight,
                        TargetRpe = target?.Rpe ?? prescribed.TargetRpe,
                        TargetPercentageRM = pctRM,
                        SuggestedWeight = suggestedWeight,
                        SetLabel = target?.Label,
                        ActualReps = log?.Reps ?? 0,
                        ActualWeight = log?.Weight ?? 0,
                        ActualRpe = log?.Rpe,
                        IsCompleted = log?.IsCompleted ?? false,
                        SkippedReason = log?.SkippedReason,
                        Notes = log?.Notes
                    });
                }

                // Parse JSON configs
                var emomConfig = ParseJson<EmomConfigDto>(prescribed.EmomConfigJson);
                var tempoConfig = ParseJson<TempoConfigDto>(prescribed.TempoConfigJson);
                var (supersetGroupId, supersetPosition) = ParseSupersetConfig(prescribed.SupersetConfigJson);

                exerciseGroups.Add(new WorkoutExerciseGroupDto
                {
                    ExerciseId = prescribed.ExerciseId,
                    ExerciseName = prescribed.Exercise?.Name ?? "Unknown",
                    Order = prescribed.Order,
                    PrescribedSets = prescribed.Sets,
                    PrescribedReps = prescribed.Reps,
                    PrescribedRpe = prescribed.TargetRpe,
                    RestSeconds = prescribed.RestSeconds,
                    ExerciseNotes = prescribed.Notes,
                    ExerciseType = prescribed.ExerciseType.ToString(),
                    PercentageRM = prescribed.PercentageRM,
                    RawNotation = prescribed.RawNotation,
                    PrescribedWeight = prescribed.Weight,
                    EmomConfig = emomConfig,
                    TempoConfig = tempoConfig,
                    SupersetGroupId = supersetGroupId,
                    SupersetPosition = supersetPosition,
                    Sets = sets,
                    PreviousPerformance = previousPerformance
                });
            }
        }

        return new TodayWorkoutDto
        {
            WorkoutId = workout.Id,
            AthleteProgramId = activeProgram.Id,
            ProgramName = activeProgram.ProgramTemplate?.Name ?? "Program",
            WeekNumber = workout.WeekNumber,
            DayNumber = workout.DayNumber,
            DayName = dayTemplate?.Name,
            Focus = dayTemplate?.Focus.ToString(),
            ScheduledDate = workout.ScheduledDate,
            Status = workout.Status,
            StartedAt = workout.StartedAt,
            CompletedDate = workout.CompletedDate,
            DurationMinutes = workout.DurationMinutes,
            FatigueRating = workout.FatigueRating,
            Notes = workout.Notes,
            WeekNotes = dayTemplate?.WeekTemplate?.Notes,
            Exercises = exerciseGroups
        };
    }

    /// <summary>
    /// Calculates suggested weight from %1RM × estimated 1RM, rounded to nearest 2.5 kg.
    /// Returns null if either %RM or estimated 1RM is unavailable.
    /// </summary>
    private static decimal? CalculateSuggestedWeight(decimal? percentageRM, decimal? estimated1RM)
    {
        if (percentageRM is null or <= 0 || estimated1RM is null or <= 0)
            return null;

        var raw = estimated1RM.Value * percentageRM.Value / 100m;
        // Round to nearest 2.5 kg (standard plate increment)
        return Math.Round(raw / 2.5m) * 2.5m;
    }

    /// <summary>
    /// Gets the previous performance for an exercise (from the most recent completed workout).
    /// </summary>
    private async Task<PreviousPerformanceDto?> GetPreviousPerformanceAsync(
        Guid athleteId, Guid exerciseId, Guid currentWorkoutId, CancellationToken cancellationToken)
    {
        var previousLogs = await _context.AthleteExerciseLogs
            .AsNoTracking()
            .Where(l => l.Exercise.Id == exerciseId
                        && l.Workout.AthleteProgram.AthleteId == athleteId
                        && l.WorkoutId != currentWorkoutId
                        && l.IsCompleted)
            .OrderByDescending(l => l.Workout.ScheduledDate)
            .Take(20) // Get enough to find the last workout's sets
            .ToListAsync(cancellationToken);

        if (previousLogs.Count == 0)
            return null;

        // Group by workout to get only the most recent workout's logs
        var lastWorkoutLogs = previousLogs
            .GroupBy(l => l.WorkoutId)
            .First()
            .ToList();

        var maxWeight = lastWorkoutLogs.Max(l => l.Weight);
        var maxReps = lastWorkoutLogs.Max(l => l.Reps);
        var bestRpe = lastWorkoutLogs.Where(l => l.Rpe.HasValue).MinBy(l => l.Rpe)?.Rpe;

        // Estimated 1RM using Epley formula: weight * (1 + reps/30)
        var bestSet = lastWorkoutLogs.OrderByDescending(l => l.Weight).First();
        decimal? estimated1RM = bestSet.Weight > 0 && bestSet.Reps > 0
            ? Math.Round(bestSet.Weight * (1 + (decimal)bestSet.Reps / 30), 1)
            : null;

        return new PreviousPerformanceDto
        {
            Date = previousLogs.First().CreatedAt,
            MaxWeight = maxWeight,
            MaxReps = maxReps,
            BestRpe = bestRpe,
            EstimatedOneRM = estimated1RM,
            TotalSets = lastWorkoutLogs.Count
        };
    }

    /// <summary>
    /// Parses target reps from the template's Reps string (e.g., "8-12" → 8, "5" → 5).
    /// </summary>
    private static int? ParseTargetReps(string? reps)
    {
        if (string.IsNullOrWhiteSpace(reps))
            return null;

        // Handle range format "8-12" → take the lower bound
        if (reps.Contains('-'))
        {
            var parts = reps.Split('-');
            if (int.TryParse(parts[0].Trim(), out var lower))
                return lower;
        }

        // Handle single number "5"
        if (int.TryParse(reps.Trim(), out var single))
            return single;

        return null;
    }

    /// <summary>
    /// Parses a JSON string into the specified DTO type.
    /// </summary>
    private static T? ParseJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses superset config JSON into group ID and position.
    /// Expected format: { "groupId": "A", "position": 1 }
    /// </summary>
    private static (string? GroupId, int? Position) ParseSupersetConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return (null, null);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var groupId = root.TryGetProperty("groupId", out var gid) ? gid.GetString() : null;
            var position = root.TryGetProperty("position", out var pos) ? pos.GetInt32() : (int?)null;
            return (groupId, position);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// Parses compound notation into per-set targets.
    /// Supported formats (all may use "+" or space as separators):
    ///   "1x1 3x4"              — basic compound
    ///   "1x1 @9 3x4 @8"       — compound with RPE
    ///   "1x1 91% + 3x4 80%"   — compound with per-segment %1RM
    ///   "1x1 84% 3x4 74%"     — compound with %1RM, no separator
    ///   "3x8 @65%"             — single group with @NN%
    ///   "1x3@9 2x3@8"         — compact with RPE
    ///   "1x1@9 91% 3x4@8 80%" — RPE + %1RM mixed
    ///   "3x8-12 @8"            — range reps (uses lower bound)
    /// Uses two-pass parsing: split by SxR boundaries, then parse modifiers.
    /// </summary>
    private record SetTarget(int Reps, decimal? Weight, decimal? Rpe, decimal? PercentageRM, string? Label);

    // Regex to find SetsxReps groups
    private static readonly Regex SxRPattern = new(
        @"(\d+)\s*x\s*(\d+)(?:-(\d+))?",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Regex to find @RPE (@ followed by a number NOT followed by %)
    // Uses atomic group (?>...) to prevent backtracking so @65% won't match as RPE=6
    private static readonly Regex RpePattern = new(
        @"@\s*(?>(\d+(?:\.\d+)?))(?!\s*%)",
        RegexOptions.Compiled);

    // Regex to find %1RM (a number followed by %)
    private static readonly Regex PctPattern = new(
        @"(\d+(?:\.\d+)?)\s*%",
        RegexOptions.Compiled);

    private static List<SetTarget> ParseCompoundNotation(
        string? rawNotation, int totalSets, int? defaultReps, decimal? defaultWeight, decimal? defaultPercentageRM)
    {
        var results = new List<SetTarget>();

        if (string.IsNullOrWhiteSpace(rawNotation))
        {
            for (int i = 0; i < totalSets; i++)
                results.Add(new SetTarget(defaultReps ?? 0, defaultWeight, null, defaultPercentageRM, null));
            return results;
        }

        // Strip "+" separators
        var cleaned = rawNotation.Trim().Replace("+", " ");

        // Phase 1: Find all SxR groups and their positions
        var sxrMatches = SxRPattern.Matches(cleaned);
        if (sxrMatches.Count == 0)
        {
            for (int i = 0; i < totalSets; i++)
                results.Add(new SetTarget(defaultReps ?? 0, defaultWeight, null, defaultPercentageRM, null));
            return results;
        }

        // Phase 2: For each SxR, extract trailing text up to the next SxR,
        // then independently parse RPE and % from that text.
        var isCompound = sxrMatches.Count > 1;

        for (int idx = 0; idx < sxrMatches.Count; idx++)
        {
            var sxr = sxrMatches[idx];
            int sets = int.Parse(sxr.Groups[1].Value);
            int reps = int.Parse(sxr.Groups[2].Value);

            // Trailing text: from end of SxR to start of next SxR (or end of string)
            int trailStart = sxr.Index + sxr.Length;
            int trailEnd = idx + 1 < sxrMatches.Count ? sxrMatches[idx + 1].Index : cleaned.Length;
            var trailing = cleaned.Substring(trailStart, trailEnd - trailStart);

            // Parse modifiers independently
            decimal? rpe = null;
            var rpeMatch = RpePattern.Match(trailing);
            if (rpeMatch.Success)
                rpe = decimal.Parse(rpeMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

            decimal? pctRM = null;
            var pctMatch = PctPattern.Match(trailing);
            if (pctMatch.Success)
                pctRM = decimal.Parse(pctMatch.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);

            for (int s = 0; s < sets; s++)
            {
                string? label = null;
                if (isCompound)
                {
                    label = idx == 0 && sets == 1
                        ? "Top Set"
                        : idx == 0
                            ? $"Top Set {s + 1}"
                            : sets == 1
                                ? $"Backoff {idx}"
                                : $"Backoff {idx}.{s + 1}";
                }

                results.Add(new SetTarget(reps, defaultWeight, rpe, pctRM ?? defaultPercentageRM, label));
            }
        }

        while (results.Count < totalSets)
        {
            var last = results.LastOrDefault();
            results.Add(new SetTarget(last?.Reps ?? defaultReps ?? 0, defaultWeight, last?.Rpe, last?.PercentageRM ?? defaultPercentageRM, null));
        }

        return results;
    }
}
